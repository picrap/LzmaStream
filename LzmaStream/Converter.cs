namespace Lzma
{
    using System;
    using System.Linq;

    internal static class Converter
    {
        public static long ToLong(this byte[] bytes)
        {
            if (bytes.Length != 8)
                throw new FormatException();
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt64(bytes, 0);
            return BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
        }

        public static byte[] ToBytes(this long value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                return bytes;
            return bytes.Reverse().ToArray();
        }
    }
}
