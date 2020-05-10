namespace LzmaStream.Streams
{
    public delegate int ReadDelegate(byte[] buffer, int offset, int count);

    public static class StreamUtility
    {
        public static int ReadAll(this System.IO.Stream stream, byte[] buffer, int offset, int count) => ReadAll(stream.Read, buffer, offset, count);

        public static int ReadAll(this ReadDelegate readDelegate, byte[] buffer, int offset, int count)
        {
            var totalRead = 0;
            for (int left = count; left > 0;)
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
    }
}
