namespace MiniMQ.Client.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MiniMQ.Client.Implementation;

    [TestClass]
    public class BufferTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var buffer = Buffer.CreateDefault();
            Assert.IsNotNull(buffer.Contents);
        }

        [TestMethod]
        public void AsBufferSegmentTest()
        {
            var buffer = Buffer.CreateDefault();
            var segment = buffer.AsArraySegment();

            Assert.AreSame(buffer.Contents, segment.Array);

            Assert.AreEqual(0, segment.Offset);
            Assert.AreEqual(buffer.Length, segment.Count);

        }
    }
}
