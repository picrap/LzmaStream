#region Arx One

// Arx One
// The ass kicking online backup
// © Arx One 2009-2019

#endregion

namespace Lzma.Pipe
{
    using System;
    using System.Threading;

    /// <summary>
    ///     Circular byte buffer, as fast as possible.
    ///     Not thread-safe.
    /// </summary>
    public class CircularBuffer : IDisposable
    {
        private readonly ManualResetEvent _availableData = new ManualResetEvent(false);

        private readonly ManualResetEvent _availableSpace = new ManualResetEvent(true);

        private readonly byte[] _buffer;

        private readonly object _lock = new object();

        private bool _ended;

        /// <summary>
        ///     Index from where next data will be read
        /// </summary>
        private int _readIndex;

        /// <summary>
        ///     Index to where next data will be written
        /// </summary>
        private int _writeIndex;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CircularBuffer" /> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public CircularBuffer(int size = 1 << 20)
        {
            // +1 because the buffer can not be totally filled (see remark in Write() method)
            _buffer = new byte[size + 1];
        }

        /// <summary>
        ///     Gets the capacity.
        /// </summary>
        /// <value>The capacity.</value>
        public int Capacity => _buffer.Length - 1;

        /// <summary>
        ///     Gets the current size.
        /// </summary>
        /// <value>The count.</value>
        public int Size => (_writeIndex - _readIndex + _buffer.Length) % _buffer.Length;

        public void Dispose()
        {
            _ended = true;
            _availableData.Set();
        }

        /// <summary>
        ///     Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public int Write(byte[] buffer, int offset, int count)
        {
            _availableSpace.WaitOne();
            lock (_lock)
            {
                // minus 1 here because we can not totally fill the buffer
                // (or we won't be able to make a difference between a full and an empty buffer)
                var available = _buffer.Length - Size - 1;
                if (count > available)
                    count = available;

                if (count == 0)
                    return count;

                // we have two possibilities:
                // 1. there is enough room at buffer end
                // 2. there is not, and we restart to buffer start

                // possibility 1: enough room
                if (_writeIndex + count <= _buffer.Length)
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, _writeIndex, count);
                    _writeIndex += count;
                }
                // possibility 2: not enough, so we split
                else
                {
                    // first part, from write point to end
                    var toBufferEnd = _buffer.Length - _writeIndex;
                    Buffer.BlockCopy(buffer, offset, _buffer, _writeIndex, toBufferEnd);
                    // second part, from buffer start to end
                    var fromBufferStart = count - toBufferEnd;
                    Buffer.BlockCopy(buffer, offset + toBufferEnd, _buffer, 0, fromBufferStart);
                    _writeIndex = fromBufferStart;
                }

                if (Size == Capacity)
                    _availableSpace.Reset();
                _availableData.Set();

                return count;
            }
        }

        /// <summary>
        ///     Reads to specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (!_ended)
                _availableData.WaitOne();
            lock (_lock)
            {
                var size = Size;
                if (count > size)
                    count = size;

                if (count == 0)
                    return count;

                // same thing as write:
                // if there is enough room at buffer end, we read only one thing
                // otherwise, we restart

                if (_readIndex + count <= _buffer.Length)
                {
                    Buffer.BlockCopy(_buffer, _readIndex, buffer, offset, count);
                    _readIndex += count;
                }
                else
                {
                    // first part
                    var toBufferEnd = _buffer.Length - _readIndex;
                    Buffer.BlockCopy(_buffer, _readIndex, buffer, offset, toBufferEnd);
                    // second part
                    var fromBufferStart = count - toBufferEnd;
                    Buffer.BlockCopy(_buffer, 0, buffer, offset + toBufferEnd, fromBufferStart);
                    _readIndex = fromBufferStart;
                }

                if (Size == 0)
                    _availableData.Reset();
                _availableSpace.Set();

                return count;
            }
        }
    }
}