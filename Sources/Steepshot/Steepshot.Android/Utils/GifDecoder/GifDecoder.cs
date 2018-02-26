using System;
using Android.Graphics;
using Java.IO;
using Java.Lang;
using Java.Nio;

namespace Steepshot.Utils.GifDecoder
{
    public class GifDecoder
    {
        public static readonly int STATUS_OK = 0;
        public static readonly int STATUS_FORMAT_ERROR = 1;
        public static readonly int STATUS_OPEN_ERROR = 2;
        public static readonly int STATUS_PARTIAL_DECODE = 3;
        private static readonly int MAX_STACK_SIZE = 4096;
        private static readonly int DISPOSAL_UNSPECIFIED = 0;
        private static readonly int DISPOSAL_NONE = 1;
        private static readonly int DISPOSAL_BACKGROUND = 2;
        private static readonly int DISPOSAL_PREVIOUS = 3;
        private static readonly int NULL_CODE = -1;
        private static readonly int INITIAL_FRAME_POINTER = -1;
        public static readonly int LOOP_FOREVER = -1;
        private static readonly int BYTES_PER_INTEGER = 4;

        private int[] act;
        private readonly int[] pct = new int[256];

        private ByteBuffer rawData;

        private byte[] block;

        // Temporary buffer for block reading. Reads 16k chunks from the native buffer for processing,
        // to greatly reduce JNI overhead.
        private static readonly int WORK_BUFFER_SIZE = 16384;
        private byte[] workBuffer;
        private int workBufferSize = 0;
        private int workBufferPosition = 0;

        private GifHeaderParser parser;

        private short[] prefix;
        private byte[] suffix;
        private byte[] pixelStack;
        private byte[] mainPixels;
        private int[] mainScratch;

        private int framePointer;
        private int loopIndex;
        private GifHeader header;
        private IBitmapProvider bitmapProvider;
        private Bitmap previousImage;
        private bool savePrevious;
        private int status;
        private int sampleSize;
        private int downsampledHeight;
        private int downsampledWidth;
        private bool isFirstFrameTransparent;

        public GifDecoder(IBitmapProvider provider, GifHeader gifHeader, ByteBuffer rawData) : this(provider, gifHeader, rawData, 1)
        {

        }

        public GifDecoder(IBitmapProvider provider, GifHeader gifHeader, ByteBuffer rawData, int sampleSize) : this(provider)
        {
            SetData(gifHeader, rawData, sampleSize);
        }

        public GifDecoder(IBitmapProvider provider)
        {
            bitmapProvider = provider;
            header = new GifHeader();
        }

        public GifDecoder() : this(new SimpleBitmapProvider())
        {
        }

        public int Width => header.Width;

        public int Height => header.Height;

        ByteBuffer Data => rawData;

        int Status => status;

        public bool Advance()
        {
            if (header.FrameCount <= 0)
            {
                return false;
            }

            if (framePointer == FrameCount - 1)
            {
                loopIndex++;
            }

            if (header.LoopCount != LOOP_FOREVER && loopIndex > header.LoopCount)
            {
                return false;
            }

            framePointer = (framePointer + 1) % header.FrameCount;
            return true;
        }

        int GetDelay(int n)
        {
            int delay = -1;
            if ((n >= 0) && (n < header.FrameCount))
            {
                delay = header.Frames[n].Delay;
            }
            return delay;
        }

        public int GetNextDelay()
        {
            if (header.FrameCount <= 0 || framePointer < 0)
            {
                return 0;
            }

            return GetDelay(framePointer);
        }

        public int FrameCount => header.FrameCount;

        public int CurrentFrameIndex => framePointer;

        public bool SetFrameIndex(int frame)
        {
            if (frame < INITIAL_FRAME_POINTER || frame >= FrameCount)
            {
                return false;
            }
            framePointer = frame;
            return true;
        }

        void ResetFrameIndex()
        {
            framePointer = INITIAL_FRAME_POINTER;
        }

        public void ResetLoopIndex() { loopIndex = 0; }

        int GetLoopCount() { return header.LoopCount; }

        int LoopIndex => loopIndex;

        int ByteSize => rawData.Limit() + mainPixels.Length + (mainScratch.Length * BYTES_PER_INTEGER);

        public Bitmap GetNextFrame()
        {
            if (header.FrameCount <= 0 || framePointer < 0)
            {
                status = STATUS_FORMAT_ERROR;
            }
            if (status == STATUS_FORMAT_ERROR || status == STATUS_OPEN_ERROR)
            {
                return null;
            }
            status = STATUS_OK;

            GifFrame currentFrame = header.Frames[framePointer];
            GifFrame previousFrame = null;
            int previousIndex = framePointer - 1;
            if (previousIndex >= 0)
            {
                previousFrame = header.Frames[previousIndex];
            }

            act = currentFrame.Lct != null ? currentFrame.Lct : header.Gct;
            if (act == null)
            {
                status = STATUS_FORMAT_ERROR;
                return null;
            }

            if (currentFrame.Transparency)
            {
                Array.Copy(act, 0, pct, 0, act.Length);
                act = pct;
                act[currentFrame.TransIndex] = 0;
            }

            return SetPixels(currentFrame, previousFrame);
        }

        public int Read(InputStream input, int contentLength)
        {
            if (input != null)
            {
                try
                {
                    int capacity = (contentLength > 0) ? (contentLength + 4096) : 16384;
                    ByteArrayOutputStream buffer = new ByteArrayOutputStream(capacity);
                    int nRead;
                    byte[] data = new byte[16384];
                    while ((nRead = input.Read(data, 0, data.Length)) != -1)
                    {
                        buffer.Write(data, 0, nRead);
                    }
                    buffer.Flush();

                    Read(buffer.ToByteArray());
                }
                catch
                {

                }
            }
            else
            {
                status = STATUS_OPEN_ERROR;
            }

            try
            {
                if (input != null)
                {
                    input.Close();
                }
            }
            catch
            {
            }

            return status;
        }

        public void Clear()
        {
            header = null;
            if (mainPixels != null)
            {
                bitmapProvider.Release(mainPixels);
            }
            if (mainScratch != null)
            {
                bitmapProvider.Release(mainScratch);
            }
            if (previousImage != null)
            {
                bitmapProvider.Release(previousImage);
            }
            previousImage = null;
            rawData = null;
            isFirstFrameTransparent = false;
            if (block != null)
            {
                bitmapProvider.Release(block);
            }
            if (workBuffer != null)
            {
                bitmapProvider.Release(workBuffer);
            }
        }

        void SetData(GifHeader header, byte[] data)
        {
            SetData(header, ByteBuffer.Wrap(data));
        }

        void SetData(GifHeader header, ByteBuffer buffer)
        {
            SetData(header, buffer, 1);
        }

        void SetData(GifHeader header, ByteBuffer buffer, int sampleSize)
        {
            if (sampleSize <= 0)
            {
                throw new IllegalArgumentException("Sample size must be >=0, not: " + sampleSize);
            }
            sampleSize = Integer.HighestOneBit(sampleSize);
            this.status = STATUS_OK;
            this.header = header;
            isFirstFrameTransparent = false;
            framePointer = INITIAL_FRAME_POINTER;
            ResetLoopIndex();
            rawData = buffer.AsReadOnlyBuffer();
            rawData.Position(0);
            rawData.Order(ByteOrder.LittleEndian);

            savePrevious = false;
            foreach (GifFrame frame in header.Frames)
            {
                if (frame.Dispose == DISPOSAL_PREVIOUS)
                {
                    savePrevious = true;
                    break;
                }
            }

            this.sampleSize = sampleSize;
            downsampledWidth = header.Width / sampleSize;
            downsampledHeight = header.Height / sampleSize;

            mainPixels = bitmapProvider.ObtainByteArray(header.Width * header.Height);
            mainScratch = bitmapProvider.ObtainIntArray(downsampledWidth * downsampledHeight);
        }

        private GifHeaderParser GetHeaderParser()
        {
            if (parser == null)
            {
                parser = new GifHeaderParser();
            }
            return parser;
        }

        public int Read(byte[] data)
        {
            header = GetHeaderParser().SetData(data).ParseHeader();
            if (data != null)
            {
                SetData(header, data);
            }

            return status;
        }

        private Bitmap SetPixels(GifFrame currentFrame, GifFrame previousFrame)
        {
            int[] dest = mainScratch;
            int downsampledIH;
            int downsampledIY;
            int downsampledIW;
            int downsampledIX;

            if (previousFrame == null)
            {
                Array.Fill(dest, 0);
            }

            if (previousFrame != null && previousFrame.Dispose > DISPOSAL_UNSPECIFIED)
            {
                if (previousFrame.Dispose == DISPOSAL_BACKGROUND)
                {
                    int c = 0;
                    if (!currentFrame.Transparency)
                    {
                        c = header.BgColor;
                        if (currentFrame.Lct != null && header.BgIndex == currentFrame.TransIndex)
                        {
                            c = 0;
                        }
                    }
                    else if (framePointer == 0)
                    {
                        isFirstFrameTransparent = true;
                    }
                    FillRect(dest, previousFrame, c);
                }
                else if (previousFrame.Dispose == DISPOSAL_PREVIOUS)
                {
                    if (previousImage == null)
                    {
                        FillRect(dest, previousFrame, 0);
                    }
                    else
                    {
                        downsampledIH = previousFrame.Ih / sampleSize;
                        downsampledIY = previousFrame.Iy / sampleSize;
                        downsampledIW = previousFrame.Iw / sampleSize;
                        downsampledIX = previousFrame.Ix / sampleSize;
                        var topLeft = downsampledIY * downsampledWidth + downsampledIX;
                        previousImage.GetPixels(dest, topLeft, downsampledWidth,
                            downsampledIX, downsampledIY, downsampledIW, downsampledIH);
                    }
                }
            }

            DecodeBitmapData(currentFrame);

            downsampledIH = currentFrame.Ih / sampleSize;
            downsampledIY = currentFrame.Iy / sampleSize;
            downsampledIW = currentFrame.Iw / sampleSize;
            downsampledIX = currentFrame.Ix / sampleSize;

            int pass = 1;
            int inc = 8;
            int iline = 0;
            bool isFirstFrame = framePointer == 0;
            for (int i = 0; i < downsampledIH; i++)
            {
                int line = i;
                if (currentFrame.Interlace)
                {
                    if (iline >= downsampledIH)
                    {
                        pass++;
                        switch (pass)
                        {
                            case 2:
                                iline = 4;
                                break;
                            case 3:
                                iline = 2;
                                inc = 4;
                                break;
                            case 4:
                                iline = 1;
                                inc = 2;
                                break;
                            default:
                                break;
                        }
                    }
                    line = iline;
                    iline += inc;
                }
                line += downsampledIY;
                if (line < downsampledHeight)
                {
                    int k = line * downsampledWidth;
                    int dx = k + downsampledIX;
                    int dlim = dx + downsampledIW;
                    if (k + downsampledWidth < dlim)
                    {
                        dlim = k + downsampledWidth;
                    }
                    int sx = i * sampleSize * currentFrame.Iw;
                    int maxPositionInSource = sx + ((dlim - dx) * sampleSize);
                    while (dx < dlim)
                    {
                        int averageColor;
                        if (sampleSize == 1)
                        {
                            int currentColorIndex = ((int)mainPixels[sx]) & 0x000000ff;
                            averageColor = act[currentColorIndex];
                        }
                        else
                        {
                            averageColor = AverageColorsNear(sx, maxPositionInSource, currentFrame.Iw);
                        }
                        if (averageColor != 0)
                        {
                            dest[dx] = averageColor;
                        }
                        else if (!isFirstFrameTransparent && isFirstFrame)
                        {
                            isFirstFrameTransparent = true;
                        }
                        sx += sampleSize;
                        dx++;
                    }
                }
            }

            if (savePrevious && (currentFrame.Dispose == DISPOSAL_UNSPECIFIED
                || currentFrame.Dispose == DISPOSAL_NONE))
            {
                if (previousImage == null)
                {
                    previousImage = GetNextBitmap();
                }
                previousImage.SetPixels(dest, 0, downsampledWidth, 0, 0, downsampledWidth,
                    downsampledHeight);
            }

            Bitmap result = GetNextBitmap();
            result.SetPixels(dest, 0, downsampledWidth, 0, 0, downsampledWidth, downsampledHeight);
            return result;
        }

        private void FillRect(int[] dest, GifFrame frame, int bgColor)
        {
            int downsampledIH = frame.Ih / sampleSize;
            int downsampledIY = frame.Iy / sampleSize;
            int downsampledIW = frame.Iw / sampleSize;
            int downsampledIX = frame.Ix / sampleSize;
            int topLeft = downsampledIY * downsampledWidth + downsampledIX;
            int bottomLeft = topLeft + downsampledIH * downsampledWidth;
            for (int left = topLeft; left < bottomLeft; left += downsampledWidth)
            {
                int right = left + downsampledIW;
                for (int pointer = left; pointer < right; pointer++)
                {
                    dest[pointer] = bgColor;
                }
            }
        }

        private int AverageColorsNear(int positionInMainPixels, int maxPositionInMainPixels,
            int currentFrameIw)
        {
            int alphaSum = 0;
            int redSum = 0;
            int greenSum = 0;
            int blueSum = 0;

            int totalAdded = 0;
            for (int i = positionInMainPixels;
                i < positionInMainPixels + sampleSize && i < mainPixels.Length
                    && i < maxPositionInMainPixels; i++)
            {
                int currentColorIndex = ((int)mainPixels[i]) & 0xff;
                int currentColor = act[currentColorIndex];
                if (currentColor != 0)
                {
                    alphaSum += currentColor >> 24 & 0x000000ff;
                    redSum += currentColor >> 16 & 0x000000ff;
                    greenSum += currentColor >> 8 & 0x000000ff;
                    blueSum += currentColor & 0x000000ff;
                    totalAdded++;
                }
            }
            for (int i = positionInMainPixels + currentFrameIw;
                i < positionInMainPixels + currentFrameIw + sampleSize && i < mainPixels.Length
                    && i < maxPositionInMainPixels; i++)
            {
                int currentColorIndex = ((int)mainPixels[i]) & 0xff;
                int currentColor = act[currentColorIndex];
                if (currentColor != 0)
                {
                    alphaSum += currentColor >> 24 & 0x000000ff;
                    redSum += currentColor >> 16 & 0x000000ff;
                    greenSum += currentColor >> 8 & 0x000000ff;
                    blueSum += currentColor & 0x000000ff;
                    totalAdded++;
                }
            }
            if (totalAdded == 0)
            {
                return 0;
            }
            else
            {
                return ((alphaSum / totalAdded) << 24)
                    | ((redSum / totalAdded) << 16)
                    | ((greenSum / totalAdded) << 8)
                    | (blueSum / totalAdded);
            }
        }

        private void DecodeBitmapData(GifFrame frame)
        {
            workBufferSize = 0;
            workBufferPosition = 0;
            if (frame != null)
            {
                rawData.Position(frame.BufferFrameStart);
            }

            int npix = (frame == null) ? header.Width * header.Height : frame.Iw * frame.Ih;
            int available, clear, codeMask, codeSize, endOfInformation, inCode, oldCode, bits, code, count,
                i, datum,
                dataSize, first, top, bi, pi;

            if (mainPixels == null || mainPixels.Length < npix)
            {
                mainPixels = bitmapProvider.ObtainByteArray(npix);
            }
            if (prefix == null)
            {
                prefix = new short[MAX_STACK_SIZE];
            }
            if (suffix == null)
            {
                suffix = new byte[MAX_STACK_SIZE];
            }
            if (pixelStack == null)
            {
                pixelStack = new byte[MAX_STACK_SIZE + 1];
            }

            dataSize = ReadByte();
            clear = 1 << dataSize;
            endOfInformation = clear + 1;
            available = clear + 2;
            oldCode = NULL_CODE;
            codeSize = dataSize + 1;
            codeMask = (1 << codeSize) - 1;
            for (code = 0; code < clear; code++)
            {
                prefix[code] = 0;
                suffix[code] = (byte)code;
            }

            datum = bits = count = first = top = pi = bi = 0;
            for (i = 0; i < npix;)
            {
                if (count == 0)
                {
                    count = ReadBlock();
                    if (count <= 0)
                    {
                        status = STATUS_PARTIAL_DECODE;
                        break;
                    }
                    bi = 0;
                }

                datum += (((int)block[bi]) & 0xff) << bits;
                bits += 8;
                bi++;
                count--;

                while (bits >= codeSize)
                {
                    code = datum & codeMask;
                    datum >>= codeSize;
                    bits -= codeSize;

                    if (code == clear)
                    {
                        codeSize = dataSize + 1;
                        codeMask = (1 << codeSize) - 1;
                        available = clear + 2;
                        oldCode = NULL_CODE;
                        continue;
                    }

                    if (code > available)
                    {
                        status = STATUS_PARTIAL_DECODE;
                        break;
                    }

                    if (code == endOfInformation)
                    {
                        break;
                    }

                    if (oldCode == NULL_CODE)
                    {
                        pixelStack[top++] = suffix[code];
                        oldCode = code;
                        first = code;
                        continue;
                    }
                    inCode = code;
                    if (code >= available)
                    {
                        pixelStack[top++] = (byte)first;
                        code = oldCode;
                    }
                    while (code >= clear)
                    {
                        pixelStack[top++] = suffix[code];
                        code = prefix[code];
                    }
                    first = ((int)suffix[code]) & 0xff;
                    pixelStack[top++] = (byte)first;

                    if (available < MAX_STACK_SIZE)
                    {
                        prefix[available] = (short)oldCode;
                        suffix[available] = (byte)first;
                        available++;
                        if (((available & codeMask) == 0) && (available < MAX_STACK_SIZE))
                        {
                            codeSize++;
                            codeMask += available;
                        }
                    }
                    oldCode = inCode;

                    while (top > 0)
                    {
                        mainPixels[pi++] = pixelStack[--top];
                        i++;
                    }
                }
            }

            for (i = pi; i < npix; i++)
            {
                mainPixels[i] = 0;
            }
        }

        private void ReadChunkIfNeeded()
        {
            if (workBufferSize > workBufferPosition)
            {
                return;
            }
            if (workBuffer == null)
            {
                workBuffer = bitmapProvider.ObtainByteArray(WORK_BUFFER_SIZE);
            }
            workBufferPosition = 0;
            workBufferSize = System.Math.Min(rawData.Remaining(), WORK_BUFFER_SIZE);
            rawData.Get(workBuffer, 0, workBufferSize);
        }

        private int ReadByte()
        {
            try
            {
                ReadChunkIfNeeded();
                return workBuffer[workBufferPosition++] & 0xFF;
            }
            catch
            {
                status = STATUS_FORMAT_ERROR;
                return 0;
            }
        }

        private int ReadBlock()
        {
            int blockSize = ReadByte();
            if (blockSize > 0)
            {
                try
                {
                    if (block == null)
                    {
                        block = bitmapProvider.ObtainByteArray(255);
                    }
                    int remaining = workBufferSize - workBufferPosition;
                    if (remaining >= blockSize)
                    {
                        Array.Copy(workBuffer, workBufferPosition, block, 0, blockSize);
                        workBufferPosition += blockSize;
                    }
                    else if (rawData.Remaining() + remaining >= blockSize)
                    {
                        Array.Copy(workBuffer, workBufferPosition, block, 0, remaining);
                        workBufferPosition = workBufferSize;
                        ReadChunkIfNeeded();
                        int secondHalfRemaining = blockSize - remaining;
                        Array.Copy(workBuffer, 0, block, remaining, secondHalfRemaining);
                        workBufferPosition += secondHalfRemaining;
                    }
                    else
                    {
                        status = STATUS_FORMAT_ERROR;
                    }
                }
                catch
                {
                    status = STATUS_FORMAT_ERROR;
                }
            }
            return blockSize;
        }

        private Bitmap GetNextBitmap()
        {
            Bitmap.Config config = isFirstFrameTransparent
                ? Bitmap.Config.Argb8888 : Bitmap.Config.Rgb565;
            Bitmap result = bitmapProvider.Obtain(downsampledWidth, downsampledHeight, config);
            SetAlpha(result);
            return result;
        }

        private static void SetAlpha(Bitmap bitmap)
        {
            bitmap.HasAlpha = true;
        }
    }
}
