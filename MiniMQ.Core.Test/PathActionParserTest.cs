using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiniMQ.Core.Test
{
    using MiniMQ.Core.Routing;

    [TestClass]
    public class PathActionParserTest
    {
        [TestMethod]
        public void GetPathActionTest()
        {
            var parser = new PathActionParser(PathActionMap.Items);

            var item = parser.GetPathAction("/snd/123");
            Assert.AreEqual(PathAction.SendMessage, item.PathAction);

            item = parser.GetPathAction("/snd/");
            Assert.AreEqual(PathAction.SendMessage, item.PathAction);

            item = parser.GetPathAction("/rcv/");
            Assert.AreEqual(PathAction.ReceiveMessage, item.PathAction);

            item = parser.GetPathAction("/rcw/");
            Assert.AreEqual(PathAction.ReceiveMessageWait, item.PathAction);

            item = parser.GetPathAction("/srw/flaskhals");
            Assert.AreEqual(PathAction.SendAndReceiveMessageWait, item.PathAction);

        }
    }
}
