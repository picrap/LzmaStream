using LzmaStream.Pipe;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LzmaStreamTest
{
    [TestClass]
    public class CircularBufferTest
    {
        [TestMethod]
        public void SimpleReadWrite()
        {
            var circularBuffer = new CircularBuffer();
            var referenceData = new byte[] { 1 };
            circularBuffer.Write(referenceData, 0, referenceData.Length);
            var readData = new byte[10];
            Assert.AreEqual(1, circularBuffer.Read(readData, 0, readData.Length));
            Assert.AreEqual(referenceData[0], readData[0]);
            circularBuffer.Dispose();
            Assert.AreEqual(0, circularBuffer.Read(readData, 0, readData.Length));
        }
    }
}
