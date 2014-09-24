using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace QuickNavigate.Test
{
    [TestClass]
    public class SearchUtilTest
    {
        [TestMethod]
        public void SimpleSearchMatch_WholeWord()
        {
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash", true));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display", true));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display.Sprite", true));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display", true));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display.Sprite", true));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "Sprite", true));
        }

        [TestMethod]
        public void SimpleSearchMatch_NoWholeWord()
        {
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash", false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display", false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display.Sprite", false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display", false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display.Sprite", false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "Sprite", false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.Sprite", false));
        }

        [TestMethod]
        public void GetParts_MathCase()
        {
            List<string> parts = SearchUtil.GetParts("DisplayObjectContainer", false);
            Assert.AreEqual(parts[0], "Display");
            Assert.AreEqual(parts[1], "Object");
            Assert.AreEqual(parts[2], "Container");
        }

        [TestMethod]
        public void GetParts_noCase()
        {
            List<string> parts = SearchUtil.GetParts("DisplayObjectContainer", true);
            Assert.AreEqual(parts[0], "display");
            Assert.AreEqual(parts[1], "object");
            Assert.AreEqual(parts[2], "container");
        }

        [TestMethod]
        public void AdvancedSearchMatch_MathCase()
        {
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "DOC", false));
        }

        [TestMethod]
        public void AdvancedSearchMatch_noCase()
        {
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "DOC", true));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "doc", true));
        }
    }
}