using Android.Media;
using ApxLabs.FastAndroidCamera;
using Java.Lang;
using Java.Nio;

namespace Steepshot.CameraGL
{
    public class CircularEncoderBuffer
    {
        private readonly ByteBuffer _dataBufferWrapper;
        private readonly FastJavaByteArray _dataBuffer;

        private readonly int[] _packetFlags;
        private readonly long[] _packetPtsUsec;
        private readonly int[] _packetStart;
        private readonly int[] _packetLength;

        private int _metaHead;
        private int _metaTail;

        public CircularEncoderBuffer(int bitRate, int fps, int desiredSpanSec)
        {
            var dataBufferSize = bitRate * desiredSpanSec / 8;
            _dataBuffer = new FastJavaByteArray(dataBufferSize);
            _dataBufferWrapper = ByteBufferExtensions.Wrap(_dataBuffer);

            var packetSize = bitRate / fps / 8;
            var estimatedPacketCount = dataBufferSize / packetSize + 1;

            var metaBufferCount = estimatedPacketCount * 2;
            _packetFlags = new int[metaBufferCount];
            _packetPtsUsec = new long[metaBufferCount];
            _packetStart = new int[metaBufferCount];
            _packetLength = new int[metaBufferCount];
        }

        public void Add(ByteBuffer buf, int flags, long ptsUsec)
        {
            var size = buf.Limit() - buf.Position();

            while (!CanAdd(size))
            {
                RemoveTail();
            }

            var dataLen = _dataBuffer.Count;
            var metaLen = _packetStart.Length;
            var packetStart = GetHeadStart();
            _packetFlags[_metaHead] = flags;
            _packetPtsUsec[_metaHead] = ptsUsec;
            _packetStart[_metaHead] = packetStart;
            _packetLength[_metaHead] = size;

            if (packetStart + size < dataLen)
            {
                buf.Get(_dataBuffer, packetStart, size);
            }
            else
            {
                var firstSize = dataLen - packetStart;
                buf.Get(_dataBuffer, packetStart, firstSize);
                buf.Get(_dataBuffer, 0, size - firstSize);
            }

            _metaHead = (_metaHead + 1) % metaLen;
        }

        public int GetFirstIndex()
        {
            var metaLen = _packetStart.Length;

            var index = _metaTail;
            while (index != _metaHead)
            {
                if ((_packetFlags[index] & (int)MediaCodecBufferFlags.SyncFrame) != 0)
                {
                    break;
                }
                index = (index + 1) % metaLen;
            }

            if (index == _metaHead)
            {
                index = _metaTail;
            }
            return index;
        }

        public int GetNextIndex(int index)
        {
            var metaLen = _packetStart.Length;
            var next = (index + 1) % metaLen;
            if (next == _metaHead)
            {
                next = -1;
            }
            return next;
        }

        public ByteBuffer GetChunk(int index, MediaCodec.BufferInfo info)
        {
            var dataLen = _dataBuffer.Count;
            var packetStart = _packetStart[index];
            var length = _packetLength[index];

            info.Flags = (MediaCodecBufferFlags)_packetFlags[index];
            info.Offset = packetStart;
            info.PresentationTimeUs = _packetPtsUsec[index];
            info.Size = length;

            if (packetStart + length <= dataLen)
            {
                return _dataBufferWrapper;
            }

            var tempBuf = ByteBuffer.AllocateDirect(length);
            var firstSize = dataLen - packetStart;
            tempBuf.Put(_dataBuffer, _packetStart[index], firstSize);
            tempBuf.Put(_dataBuffer, 0, length - firstSize);
            info.Offset = 0;
            return tempBuf;
        }

        private int GetHeadStart()
        {
            if (_metaHead == _metaTail)
            {
                return 0;
            }

            var dataLen = _dataBuffer.Count;
            var metaLen = _packetStart.Length;

            var beforeHead = (_metaHead + metaLen - 1) % metaLen;
            return (_packetStart[beforeHead] + _packetLength[beforeHead] + 1) % dataLen;
        }

        private bool CanAdd(int size)
        {
            var dataLen = _dataBuffer.Count;
            var metaLen = _packetStart.Length;

            if (size > dataLen)
            {
                throw new RuntimeException("Enormous packet: " + size + " vs. buffer " +
                        dataLen);
            }

            if (_metaHead == _metaTail)
            {
                return true;
            }

            var nextHead = (_metaHead + 1) % metaLen;
            if (nextHead == _metaTail)
            {
                return false;
            }

            var headStart = GetHeadStart();
            var tailStart = _packetStart[_metaTail];
            var freeSpace = (tailStart + dataLen - headStart) % dataLen;
            if (size > freeSpace)
            {
                return false;
            }

            return true;
        }

        private void RemoveTail()
        {
            if (_metaHead == _metaTail)
            {
                throw new RuntimeException("Can't removeTail() in empty buffer");
            }
            var metaLen = _packetStart.Length;
            _metaTail = (_metaTail + 1) % metaLen;
        }

        public void Release()
        {
            _dataBuffer.Dispose();
            _dataBufferWrapper.Dispose();
        }
    }
}