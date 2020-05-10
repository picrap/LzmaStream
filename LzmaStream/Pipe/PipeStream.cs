namespace Lzma.Pipe
{
    using System;
    using Streams;

    /// <summary>
    ///     Uses a buffer. It is strongly advised to read and write from different threads
    /// </summary>
    internal class PipeStream : SimpleStream
    {
        private readonly CircularBuffer _buffer;

        private bool _disposed;

        public PipeStream(int bufferSize = 1 << 20)
        {
            _buffer = new CircularBuffer(bufferSize);
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed)
                return;
            _buffer.Dispose();
            _disposed = true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return StreamUtility.ReadAll(_buffer.Read, buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("this");
            for (var left = count; left > 0;)
            {
                var written = _buffer.Write(buffer, offset, left);
                offset += written;
                left -= written;
            }
        }
    }
}