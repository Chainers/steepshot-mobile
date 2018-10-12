using System;
using System.IO;
using Foundation;

namespace Steepshot.iOS.Helpers
{
    public class CustomInputStream : Stream
    {
        private readonly NSInputStream input_stream;

        public CustomInputStream(NSInputStream str)
        {
            input_stream = str;
            input_stream.Open();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
                throw new NotSupportedException();

            return (int)input_stream.Read(buffer, (nuint)count);
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        protected override void Dispose(bool disposing)
        {
            input_stream.Close();
            input_stream.Dispose();
            base.Dispose(disposing);
        }
    }
}
