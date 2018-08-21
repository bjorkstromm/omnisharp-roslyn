using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace OmniSharp.WebSocket
{
    public class BlockingMemoryStream : Stream
    {
        private readonly BlockingCollection<byte> _buffer = new BlockingCollection<byte>();
        private readonly ManualResetEvent _isReading = new ManualResetEvent(false);
        private readonly ManualResetEvent _isWriting = new ManualResetEvent(false);
        private bool _disposed = false;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _buffer.Count;

        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            // noop
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var tempBuffer = new byte[count];
            var bytesRead = 0;

            _isReading.Set();
            while(bytesRead < count)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(BlockingMemoryStream));
                }

                if(_buffer.TryTake(out var @byte, Timeout.Infinite))
                {
                    tempBuffer[bytesRead] = @byte;
                    bytesRead++;
                }
            }
            _isReading.Reset();

            tempBuffer.CopyTo(buffer, offset);

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            // noop
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _isWriting.Set();
            for (var i = 0; i < count; i++)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(BlockingMemoryStream));
                }

                _buffer.TryAdd(buffer[offset + i]);
            }
            _isWriting.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _buffer?.Dispose();
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
