using System;
using Java.Lang;
using Java.Nio;

namespace Steepshot.Utils.GifDecoder
{
    class GifHeaderParser
    {
        static readonly int MIN_FRAME_DELAY = 2;
        static readonly int DEFAULT_FRAME_DELAY = 10;

        private static readonly int MAX_BLOCK_SIZE = 256;
        private readonly byte[] block = new byte[MAX_BLOCK_SIZE];

        private ByteBuffer rawData;
        private GifHeader header;
        private int blockSize = 0;

        public GifHeaderParser SetData(ByteBuffer data)
        {
            Reset();
            rawData = data.AsReadOnlyBuffer();
            rawData.Position(0);
            rawData.Order(ByteOrder.LittleEndian);
            return this;
        }

        public GifHeaderParser SetData(byte[] data)
        {
            if (data != null)
            {
                SetData(ByteBuffer.Wrap(data));
            }
            else
            {
                rawData = null;
                header.Status = GifDecoder.STATUS_OPEN_ERROR;
            }
            return this;
        }

        public void Clear()
        {
            rawData = null;
            header = null;
        }

        private void Reset()
        {
            rawData = null;
            Array.Fill(block, (byte)0);
            header = new GifHeader();
            blockSize = 0;
        }

        public GifHeader ParseHeader()
        {
            if (rawData == null)
            {
                throw new IllegalStateException("You must call setData() before parseHeader()");
            }
            if (Err())
            {
                return header;
            }

            ReadHeader();
            if (!Err())
            {
                ReadContents();
                if (header.FrameCount < 0)
                {
                    header.Status = GifDecoder.STATUS_FORMAT_ERROR;
                }
            }

            return header;
        }

        public bool IsAnimated()
        {
            ReadHeader();
            if (!Err())
            {
                ReadContents(2);
            }
            return header.FrameCount > 1;
        }

        private void ReadContents()
        {
            ReadContents(Integer.MaxValue);
        }

        private void ReadContents(int maxFrames)
        {
            bool done = false;
            while (!(done || Err() || header.FrameCount > maxFrames))
            {
                int code = Read();
                switch (code)
                {
                    case 0x2C:
                        if (header.CurrentFrame == null)
                        {
                            header.CurrentFrame = new GifFrame();
                        }
                        ReadBitmap();
                        break;
                    // Extension.
                    case 0x21:
                        code = Read();
                        switch (code)
                        {
                            // Graphics control extension.
                            case 0xf9:
                                // Start a new frame.
                                header.CurrentFrame = new GifFrame();
                                ReadGraphicControlExt();
                                break;
                            case 0xff:
                                ReadBlock();
                                string app = "";
                                for (int i = 0; i < 11; i++)
                                {
                                    app += (char)block[i];
                                }
                                if (app.Equals("NETSCAPE2.0"))
                                {
                                    ReadNetscapeExt();
                                }
                                else
                                {
                                    Skip();
                                }
                                break;
                            case 0xfe:
                                Skip();
                                break;
                            case 0x01:
                                Skip();
                                break;
                            default:
                                Skip();
                                break;
                        }
                        break;
                    case 0x3b:
                        done = true;
                        break;
                    case 0x00:
                    default:
                        header.Status = GifDecoder.STATUS_FORMAT_ERROR;
                        break;
                }
            }
        }

        private void ReadGraphicControlExt()
        {
            Read();
            int packed = Read();
            header.CurrentFrame.Dispose = (packed & 0x1c) >> 2;
            if (header.CurrentFrame.Dispose == 0)
            {
                header.CurrentFrame.Dispose = 1;
            }
            header.CurrentFrame.Transparency = (packed & 1) != 0;
            int delayInHundredthsOfASecond = ReadShort();
            if (delayInHundredthsOfASecond < MIN_FRAME_DELAY)
            {
                delayInHundredthsOfASecond = DEFAULT_FRAME_DELAY;
            }
            header.CurrentFrame.Delay = delayInHundredthsOfASecond * 10;
            header.CurrentFrame.TransIndex = Read();
            Read();
        }

        private void ReadBitmap()
        {
            header.CurrentFrame.Ix = ReadShort();
            header.CurrentFrame.Iy = ReadShort();
            header.CurrentFrame.Iw = ReadShort();
            header.CurrentFrame.Ih = ReadShort();

            int packed = Read();
            bool lctFlag = (packed & 0x80) != 0;
            int lctSize = (int)System.Math.Pow(2, (packed & 0x07) + 1);
            header.CurrentFrame.Interlace = (packed & 0x40) != 0;
            if (lctFlag)
            {
                header.CurrentFrame.Lct = ReadColorTable(lctSize);
            }
            else
            {
                header.CurrentFrame.Lct = null;
            }

            header.CurrentFrame.BufferFrameStart = rawData.Position();

            SkipImageData();

            if (Err())
            {
                return;
            }

            header.FrameCount++;
            header.Frames.Add(header.CurrentFrame);
        }

        private void ReadNetscapeExt()
        {
            do
            {
                ReadBlock();
                if (block[0] == 1)
                {
                    int b1 = ((int)block[1]) & 0xff;
                    int b2 = ((int)block[2]) & 0xff;
                    header.LoopCount = (b2 << 8) | b1;
                    if (header.LoopCount == 0)
                    {
                        header.LoopCount = GifDecoder.LOOP_FOREVER;
                    }
                }
            } while ((blockSize > 0) && !Err());
        }

        private void ReadHeader()
        {
            string id = "";
            for (int i = 0; i < 6; i++)
            {
                id += (char)Read();
            }
            if (!id.StartsWith("GIF"))
            {
                header.Status = GifDecoder.STATUS_FORMAT_ERROR;
                return;
            }
            ReadLSD();
            if (header.GctFlag && !Err())
            {
                header.Gct = ReadColorTable(header.GctSize);
                header.BgColor = header.Gct[header.BgIndex];
            }
        }

        private void ReadLSD()
        {
            header.Width = ReadShort();
            header.Height = ReadShort();
            int packed = Read();
            header.GctFlag = (packed & 0x80) != 0;
            header.GctSize = 2 << (packed & 7);
            header.BgIndex = Read();
            header.PixelAspect = Read();
        }

        private int[] ReadColorTable(int ncolors)
        {
            int nbytes = 3 * ncolors;
            int[] tab = null;
            byte[] c = new byte[nbytes];

            try
            {
                rawData.Get(c);

                tab = new int[MAX_BLOCK_SIZE];
                int i = 0;
                int j = 0;
                while (i < ncolors)
                {
                    int r = c[j++] & 0xff;
                    int g = c[j++] & 0xff;
                    int b = c[j++] & 0xff;
                    tab[i++] = 0xff00000 | (r << 16) | (g << 8) | b;
                }
            }
            catch (BufferUnderflowException e)
            {
                header.Status = GifDecoder.STATUS_FORMAT_ERROR;
            }

            return tab;
        }

        private void SkipImageData()
        {
            Read();
            Skip();
        }

        private void Skip()
        {
            try
            {
                int blockSize;
                do
                {
                    blockSize = Read();
                    rawData.Position(rawData.Position() + blockSize);
                } while (blockSize > 0);
            }
            catch (IllegalArgumentException ex)
            {
            }
        }

        private int ReadBlock()
        {
            blockSize = Read();
            int n = 0;
            if (blockSize > 0)
            {
                int count = 0;
                try
                {
                    while (n < blockSize)
                    {
                        count = blockSize - n;
                        rawData.Get(block, n, count);

                        n += count;
                    }
                }
                catch
                {
                    header.Status = GifDecoder.STATUS_FORMAT_ERROR;
                }
            }
            return n;
        }

        private int Read()
        {
            int curByte = 0;
            try
            {
                curByte = rawData.Get() & 0xFF;
            }
            catch
            {
                header.Status = GifDecoder.STATUS_FORMAT_ERROR;
            }
            return curByte;
        }

        private int ReadShort()
        {
            return rawData.Short;
        }

        private bool Err()
        {
            return header.Status != GifDecoder.STATUS_OK;
        }
    }
}