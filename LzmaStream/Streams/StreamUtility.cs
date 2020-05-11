namespace Lzma.Streams
{
    using System;
    using System.IO;

    public delegate int ReadDelegate(byte[] buffer, int offset, int count);

    public static class StreamUtility
    {
        public static int ReadAll(this Stream stream, byte[] buffer, int offset, int count)
        {
            return ReadAll(stream.Read, buffer, offset, count);
        }

        public static int ReadAll(this ReadDelegate readDelegate, byte[] buffer, int offset, int count)
        {
            var totalRead = 0;
            for (var left = count; left > 0;)
            {
                var stepRead = readDelegate(buffer, offset, left);
                if (stepRead == 0)
                    break;
                totalRead += stepRead;
                offset += stepRead;
                left -= stepRead;
            }

            return totalRead;
        }

        public static bool TryReadBytes(this Stream stream, int length, out byte[] bytes)
        {
            var buffer = new byte[length];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                bytes = null;
                return false;
            }

            bytes = buffer;
            return true;
        }

        public static byte[] ReadBytes(this Stream stream, int length)
        {
            if (!stream.TryReadBytes(length, out var bytes))
                throw new InvalidOperationException("Stream too short");
            return bytes;
        }

        public static bool TryReadLong(this Stream stream, out long value)
        {
            if (!stream.TryReadBytes(8, out var bytes))
            {
                value = 0;
                return false;
            }

            value = bytes.ToLong();
            return true;
        }

        public static long ReadLong(this Stream stream)
        {
            if (!stream.TryReadLong(out var value))
                throw new InvalidOperationException("Stream too short");
            return value;
        }

        public static void WriteBytes(this Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

        public static void WriteLong(this Stream stream, long value) => stream.WriteBytes(value.ToBytes());
    }
}