using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using LzmaStream.Pipe;
using LzmaStream.Streams;
using SevenZip.Compression.LZMA;

namespace LzmaStream
{
    public class LzmaStream : SimpleStream
    {
        private readonly bool _ownsStream;
        private readonly Stream _pipeStream;
        private readonly Stream _innerStream;
        private readonly Thread _coderThread;

        public override bool CanRead { get; }
        public override bool CanWrite { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LzmaStream" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="compressionMode">The compression mode.</param>
        /// <param name="ownsStream">if set to <c>true</c> [owns stream].</param>
        /// <exception cref="System.ArgumentOutOfRangeException">compressionMode - null</exception>
        public LzmaStream(System.IO.Stream stream, CompressionMode compressionMode, bool ownsStream = true)
        {
            _innerStream = stream;
            _ownsStream = ownsStream;
            _pipeStream = new PipeStream();
            switch (compressionMode)
            {
                case CompressionMode.Compress:
                    CanWrite = true;
                    _coderThread = new Thread(() => new Encoder().Code(_pipeStream, _innerStream, -1, -1, null));
                    break;
                case CompressionMode.Decompress:
                    CanRead = true;
                    _coderThread = new Thread(() => new Decoder().Code(_innerStream, _pipeStream, -1, -1, null));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionMode), compressionMode, null);
            }
            _coderThread.Start();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _pipeStream.Dispose();
                _coderThread.Join();
                if (_ownsStream)
                    _innerStream.Dispose();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new InvalidOperationException();
            return _pipeStream.ReadAll(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new InvalidOperationException();
            _pipeStream.Write(buffer, offset, count);
        }
    }
}
