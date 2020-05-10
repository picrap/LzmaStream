namespace LzmaStreamTest
{
    using System.IO;
    using System.IO.Compression;
    using Lzma;
    using Lzma.Streams;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SevenZip.Compression.LZMA;

    [TestClass]
    public class LzmaStreamTest
    {
        private string GetReference()
        {
            using var t = GetType().OpenSiblingResourceStream("r.raw");
            using var tr = new StreamReader(t);
            return tr.ReadToEnd();
        }

        [TestMethod]
        public void ReferenceRead()
        {
            using var packStream = GetType().OpenSiblingResourceStream("r.lzma");
            using var unpackTarget = new MemoryStream();
            var decoder = new Decoder();
            decoder.SetDecoderProperties(packStream.ReadBytes(5));
            var outSize = packStream.ReadLong();
            decoder.Code(packStream, unpackTarget, -1, outSize, null);

            unpackTarget.Seek(0, SeekOrigin.Begin);
            using var unpackReader = new StreamReader(unpackTarget);
            var unpacked = unpackReader.ReadToEnd();
            Assert.AreEqual(GetReference(), unpacked);
        }

        [TestMethod]
        public void SimpleRead()
        {
            using var packStream = GetType().OpenSiblingResourceStream("r.lzma");
            using var unpackTarget = new MemoryStream();
            using (var lzmaStream = new LzmaStream(packStream, CompressionMode.Decompress))
                lzmaStream.CopyTo(unpackTarget);

            unpackTarget.Seek(0, SeekOrigin.Begin);
            using var unpackReader = new StreamReader(unpackTarget);
            var unpacked = unpackReader.ReadToEnd();
            Assert.AreEqual(GetReference(), unpacked);
        }

        [TestMethod]
        public void SimpleReadWithSmallBuffer()
        {
            using var packStream = GetType().OpenSiblingResourceStream("r.lzma");
            using var unpackTarget = new MemoryStream();
            using (var lzmaStream = new LzmaStream(packStream, CompressionMode.Decompress, bufferSize: 10))
                lzmaStream.CopyTo(unpackTarget);

            unpackTarget.Seek(0, SeekOrigin.Begin);
            using var unpackReader = new StreamReader(unpackTarget);
            var unpacked = unpackReader.ReadToEnd();
            Assert.AreEqual(GetReference(), unpacked);
        }
    }
}
