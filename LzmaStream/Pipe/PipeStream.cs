using System;
using LzmaStream.Streams;

namespace LzmaStream.Pipe
{
    /// <summary>
    /// Uses a buffer. It is strongly advised to read and write from different threads
    /// </summary>
    internal class PipeStream : SimpleStream
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        private bool _disposed;
        private readonly CircularBuffer _buffer;

        public PipeStream()
        {
            _buffer = new CircularBuffer();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _buffer.Dispose();
            _disposed = true;
        }

        public override int Read(byte[] buffer, int offset, int count) => StreamUtility.ReadAll(_buffer.Read, buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("this");
            for (int left = count; left > 0;)
            {
                var written = _buffer.Write(buffer, offset, left);
                offset += written;
                left -= written;
            }
        }
    }
}
