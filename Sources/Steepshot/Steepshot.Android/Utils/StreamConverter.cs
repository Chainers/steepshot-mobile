using System;
using System.IO;
using Steepshot.Core.Utils;

namespace Steepshot.Utils
{

    public sealed class StreamConverter : System.IO.Stream, IDisposable
    {
        private readonly Java.IO.FileInputStream _fileInputStream;
        private readonly Java.IO.FileOutputStream _fileOutputStream;
        private readonly Action<int> _progressAction;
        private long _totalReaded = 0;

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        public StreamConverter(Java.IO.FileInputStream fileInputStream, Action<int> progress)
        {
            _fileInputStream = fileInputStream;
            CanRead = true;
            CanSeek = false;
            CanWrite = false;
            Length = fileInputStream.Available();
            _progressAction = progress;
        }

        public StreamConverter(Java.IO.FileOutputStream fileOutputStream)
        {
            _fileOutputStream = fileOutputStream;
            CanRead = false;
            CanSeek = false;
            CanWrite = true;
            Length = 0;
        }

        public override void Flush()
        {
            _fileOutputStream?.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                var l = _fileInputStream.Read(buffer, offset, count);
                _totalReaded += l;

                if (l < 0)
                {
                    _progressAction?.Invoke(100);
                    return 0;
                }
                _progressAction?.Invoke((int)(_totalReaded * 100 / Length));
                return l;
            }
            catch (System.Exception ex)
            {
                AppSettings.Logger.WarningAsync(ex);
            }
            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _fileOutputStream.Write(buffer, offset, count);
        }


        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _fileInputStream?.Close();
                    Flush();
                    _fileInputStream?.Dispose();
                    _fileOutputStream?.Close();
                    _fileOutputStream?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BasePostPresenter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
