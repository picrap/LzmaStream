namespace Lzma
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using Pipe;
    using SevenZip;
    using SevenZip.Compression.LZMA;
    using Streams;

    public class LzmaStream : SimpleStream
    {
        private readonly Thread _coderThread;
        private readonly Stream _innerStream;
        private readonly bool _ownsStream;
        private readonly Stream _pipeStream;

        public LzmaStream(Stream stream, CompressionMode compressionMode, bool ownsStream = true, int bufferSize = 1 << 20)
        : this(stream, compressionMode, LzmaCompressionParameters.Default, ownsStream, bufferSize)
        { }

        public LzmaStream(Stream stream, LzmaCompressionParameters compressionParameters, bool ownsStream = true, int bufferSize = 1 << 20)
        : this(stream, CompressionMode.Compress, compressionParameters, ownsStream, bufferSize)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LzmaStream" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="compressionMode">The compression mode.</param>
        /// <param name="compressionParameters">The compression parameters.</param>
        /// <param name="ownsStream">if set to <c>true</c> [owns stream].</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">compressionMode - null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">compressionMode - null</exception>
        private LzmaStream(Stream stream, CompressionMode compressionMode, LzmaCompressionParameters compressionParameters = null,
            bool ownsStream = true, int bufferSize = 1 << 20)
        {
            _innerStream = stream;
            _ownsStream = ownsStream;
            _pipeStream = new PipeStream(bufferSize);
            switch (compressionMode)
            {
                case CompressionMode.Compress:
                    CanWrite = true;
                    _coderThread = StartThread(() => CreateEncoder(_innerStream, compressionParameters).Code(_pipeStream, _innerStream, -1, -1, null),
                        "LZMA compress");
                    break;
                case CompressionMode.Decompress:
                    CanRead = true;
                    _coderThread = StartThread(() =>
                        {
                            var decoder = CreateDecoder(_innerStream, out var length);
                            var inputLength = _innerStream.CanSeek ? _innerStream.Length : -1;
                            decoder.Code(_innerStream, _pipeStream, inputLength, length, null);
                            _pipeStream.Dispose();
                        },
                        "LZMA decompress");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionMode), compressionMode, null);
            }
        }

        public override bool CanRead { get; }
        public override bool CanWrite { get; }

        private static Thread StartThread(ThreadStart action, string name)
        {
            var thread = new Thread(action) { Name = name };
            thread.Start();
            return thread;
        }

        private static ICoder CreateDecoder(Stream innerStream, out long outputLength)
        {
            var decoder = new Decoder();
            decoder.SetDecoderProperties(innerStream.ReadBytes(5));
            outputLength = innerStream.ReadLong();
            return decoder;
        }

        private static ICoder CreateEncoder(Stream innerStream, LzmaCompressionParameters compressionParameters)
        {
            var encoder = new Encoder();
            compressionParameters.SetEncoderProperties(encoder);
            encoder.WriteCoderProperties(innerStream);
            innerStream.WriteLong(-1);
            return encoder;
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