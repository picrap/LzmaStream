namespace LzmaStreamTest
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
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

        [TestMethod]
        public void DefaultPack() => Pack(LzmaCompressionParameters.Default);

        [TestMethod]
        public void OptimalPack() => Pack(LzmaCompressionParameters.Optimal);

        [TestMethod]
        public void FastPack() => Pack(LzmaCompressionParameters.Fast);

        private void Pack(LzmaCompressionParameters lzmaCompressionParameters)
        {
            using var rawStream = GetType().OpenSiblingResourceStream("flowers.bmp");
            using var packTarget = new MemoryStream();
            using (var lzmaStream = new LzmaStream(packTarget, lzmaCompressionParameters))
                rawStream.CopyTo(lzmaStream);

            using var packSource = new MemoryStream(packTarget.ToArray());
            using var rawTarget = new MemoryStream();
            using (var unpackLzmaStream = new LzmaStream(packSource, CompressionMode.Decompress))
                unpackLzmaStream.CopyTo(rawTarget);

            Assert.IsTrue(IsEqual(rawStream, rawTarget));
        }

        [TestMethod]
        public void GZipPack()
        {
            using var rawStream = GetType().OpenSiblingResourceStream("flowers.bmp");
            using var packTarget = new MemoryStream();
            using (var lzmaStream = new GZipStream(packTarget, CompressionLevel.Optimal))
                rawStream.CopyTo(lzmaStream);

            using var packSource = new MemoryStream(packTarget.ToArray());
            using var rawTarget = new MemoryStream();
            using (var unpackLzmaStream = new GZipStream(packSource, CompressionMode.Decompress))
                unpackLzmaStream.CopyTo(rawTarget);

            Assert.IsTrue(IsEqual(rawStream, rawTarget));
        }

        private static bool IsEqual(Stream a, Stream b)
        {
            if (a.Length != b.Length)
                return false;
            var aBuffer = new byte[10000];
            var bBuffer = new byte[aBuffer.Length];
            a.Seek(0, SeekOrigin.Begin);
            b.Seek(0, SeekOrigin.Begin);
            for (; ; )
            {
                var bytesRead = a.Read(aBuffer, 0, aBuffer.Length);
                if (bytesRead == 0)
                    return true;
                b.Read(bBuffer, 0, bBuffer.Length);
                if (!aBuffer.SequenceEqual(bBuffer)) // don’t care about comparing correct size (and too lazy to make it clean)
                    return false;
            }
        }
    }
}
