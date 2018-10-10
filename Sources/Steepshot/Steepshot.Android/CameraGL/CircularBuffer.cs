using Android.Media;
using Java.Lang;
using Java.Nio;

namespace Steepshot.CameraGL
{
    public class CircularBuffer
    {
        private readonly ByteBuffer[] _dataBuffer;
        private ByteOrder _order;
        private readonly int _bufferSize;
        private readonly int _totalBufferSize;

        private readonly int[] _packetFlags;
        private readonly long[] _packetPtsUs;
        private readonly int[] _packetStart;
        private readonly int[] _packetLength;
        private readonly int _metaLength;

        private int _metaHead;
        private int _metaTail;

        public CircularBuffer(MediaFormat mediaFormat, int desiredSpanMs)
        {
            _dataBuffer = new ByteBuffer[1];

            var bitrate = mediaFormat.GetInteger(MediaFormat.KeyBitRate);
            _bufferSize = (int)((long)bitrate * desiredSpanMs / (8 * 1000));

            _dataBuffer[0] = ByteBuffer.AllocateDirect(_bufferSize);
            _bufferSize = _dataBuffer[0].Capacity();
            _totalBufferSize = _bufferSize;

            var mimeType = mediaFormat.GetString(MediaFormat.KeyMime);
            var isVideo = mimeType.Equals(MediaFormat.MimetypeVideoAvc);
            var isAudio = mimeType.Equals(MediaFormat.MimetypeAudioAac);
            double packetsPerSecond;
            if (isVideo)
            {
                packetsPerSecond = mediaFormat.GetInteger(MediaFormat.KeyFrameRate);
            }
            else if (isAudio)
            {
                var sampleRate = mediaFormat.GetInteger(MediaFormat.KeySampleRate);
                packetsPerSecond = sampleRate / 1024d;
            }
            else
            {
                throw new RuntimeException("Media format provided is neither AVC nor AAC");
            }

            var packetSize = bitrate / packetsPerSecond / 8;
            var estimatedPacketCount = (int)(_bufferSize / packetSize + 1);
            _metaLength = estimatedPacketCount * 2;
            _packetFlags = new int[_metaLength];
            _packetPtsUs = new long[_metaLength];
            _packetStart = new int[_metaLength];
            _packetLength = new int[_metaLength];
        }

        private bool IsEmpty()
        {
            return _metaHead == _metaTail;
        }

        private int GetFirstIndex()
        {
            if (IsEmpty())
            {
                return -1;
            }
            return _metaTail;
        }

        private int GetLastIndex()
        {
            if (IsEmpty())
            {
                return -1;
            }
            return (_metaHead + _metaLength - 1) % _metaLength;
        }

        private int GetHeadStart()
        {
            if (IsEmpty())
            {
                return 0;
            }
            var beforeHead = GetLastIndex();
            return (_packetStart[beforeHead] + _packetLength[beforeHead]) % _totalBufferSize;
        }

        private int GetFreeSpace(int headStart)
        {
            if (IsEmpty())
            {
                return _totalBufferSize;
            }
            var tailStart = _packetStart[_metaTail];
            var freeSpace = (tailStart + _totalBufferSize - headStart) % _totalBufferSize;
            return freeSpace;
        }

        public int Add(ByteBuffer buf, MediaCodec.BufferInfo info)
        {
            var size = info.Size;

            if (_order == null)
            {
                _order = buf.Order();
                foreach (var buff in _dataBuffer)
                {
                    buff.Order(_order);
                }
            }
            if (_order != buf.Order())
            {
                throw new RuntimeException("Byte ordering changed");
            }

            if (!CanAdd(size))
            {
                return -1;
            }

            var headStart = GetHeadStart();
            var bufferStart = headStart / _bufferSize * _bufferSize;
            var bufferEnd = bufferStart + _bufferSize - 1;
            if (headStart + size - 1 > bufferEnd)
            {
                headStart = (bufferStart + _bufferSize) % _totalBufferSize;
            }

            var packetStart = headStart % _bufferSize;
            var bufferId = headStart / _bufferSize;

            buf.Limit(info.Offset + info.Size);
            buf.Position(info.Offset);
            _dataBuffer[bufferId].Limit(packetStart + info.Size);
            _dataBuffer[bufferId].Position(packetStart);
            _dataBuffer[bufferId].Put(buf);

            _packetFlags[_metaHead] = (int)info.Flags;
            _packetPtsUs[_metaHead] = info.PresentationTimeUs;
            _packetStart[_metaHead] = headStart;
            _packetLength[_metaHead] = size;

            var currentIndex = _metaHead;
            _metaHead = (_metaHead + 1) % _metaLength;

            return currentIndex;
        }

        private bool CanAdd(int size)
        {
            if (size > _bufferSize)
            {
                throw new RuntimeException("Enormous packet: " + size + " vs. buffer " +
                        _bufferSize);
            }
            if (IsEmpty())
            {
                return true;
            }

            var nextHead = (_metaHead + 1) % _metaLength;
            if (nextHead == _metaTail)
            {
                return false;
            }

            var headStart = GetHeadStart();
            var freeSpace = GetFreeSpace(headStart);
            if (size > freeSpace)
            {
                return false;
            }

            var bufferStart = headStart / _bufferSize * _bufferSize;
            var bufferEnd = bufferStart + _bufferSize - 1;
            if (headStart + size - 1 > bufferEnd)
            {
                headStart = (bufferStart + _bufferSize) % _totalBufferSize;
                freeSpace = GetFreeSpace(headStart);
                if (size > freeSpace)
                {
                    return false;
                }
            }
            return true;
        }

        private ByteBuffer GetChunk(int index, MediaCodec.BufferInfo info)
        {
            if (IsEmpty())
            {
                throw new RuntimeException("Can't return chunk of empty buffer");
            }

            var packetStart = _packetStart[index] % _bufferSize;
            var bufferId = _packetStart[index] / _bufferSize;

            info.Flags = (MediaCodecBufferFlags)_packetFlags[index];
            info.PresentationTimeUs = _packetPtsUs[index];
            info.Offset = packetStart;
            info.Size = _packetLength[index];

            var byteBuffer = _dataBuffer[bufferId].Duplicate();
            byteBuffer.Order(_order);
            byteBuffer.Limit(info.Offset + info.Size);
            byteBuffer.Position(info.Offset);

            return byteBuffer;
        }

        public ByteBuffer GetTailChunk(MediaCodec.BufferInfo info)
        {
            var index = GetFirstIndex();
            return GetChunk(index, info);
        }

        public void RemoveTail()
        {
            if (IsEmpty())
            {
                throw new RuntimeException("Can't removeTail() in empty buffer");
            }
            _metaTail = (_metaTail + 1) % _metaLength;
        }
    }
}