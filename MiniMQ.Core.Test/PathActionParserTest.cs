using System;
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

            var item = parser.GetPathAction("/add/123");
            Assert.AreEqual(PathAction.SendMessage, item.PathAction);

            item = parser.GetPathAction("/add/");
            Assert.AreEqual(PathAction.SendMessage, item.PathAction);

            item = parser.GetPathAction("/get/");
            Assert.AreEqual(PathAction.ReceiveMessage, item.PathAction);

            item = parser.GetPathAction("/getw/");
            Assert.AreEqual(PathAction.ReceiveMessageWait, item.PathAction);

            item = parser.GetPathAction("/getw/flaskhals");
            Assert.AreEqual(PathAction.ReceiveMessageWait, item.PathAction);

        }
    }
}
