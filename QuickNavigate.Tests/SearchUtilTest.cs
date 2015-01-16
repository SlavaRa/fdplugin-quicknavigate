using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickNavigate.Tests
{
    [TestClass]
    public class SearchUtilTest
    {
        [TestMethod]
        public void TestSimpleSearchMatchWholeWord()
        {
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash", true, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display", true, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display.Sprite", true, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display", true, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display.Sprite", true, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "Sprite", true, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.DisplayObject", "Object", true, false));
        }

        [TestMethod]
        public void TestSimpleSearchMatch()
        {
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash", false, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display", false, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display.Sprite", false, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display", false, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display.Sprite", false, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "Sprite", false, false));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.DisplayObject", "Object", false, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display.sprite", false, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display.sprite", false, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "sprite", false, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.DisplayObject", "object", false, false));
            Assert.IsFalse(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.Sprite", false, false));
        }

        [TestMethod]
        public void TestSimpleSearchMatchNoCase()
        {
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "flash.display.sprite", false, true));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "display.sprite", false, true));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.Sprite", "sprite", false, true));
            Assert.IsTrue(SearchUtil.SimpleSearchMatch("flash.display.DisplayObject", "object", false, true));
        }

        [TestMethod]
        public void TestGetParts()
        {
            List<string> parts = SearchUtil.GetParts("DisplayObjectContainer");
            Assert.AreEqual("Display", parts[0]);
            Assert.AreEqual("Object", parts[1]);
            Assert.AreEqual("Container", parts[2]);
            parts = SearchUtil.GetParts("TTClass1");
            Assert.AreEqual("TT", parts[0]);
            Assert.AreEqual("Class1", parts[1]);
            parts = SearchUtil.GetParts("TT12Class1");
            Assert.AreEqual("TT12", parts[0]);
            Assert.AreEqual("Class1", parts[1]);
            parts = SearchUtil.GetParts("Sprite3D");
            Assert.AreEqual("Sprite3", parts[0]);
            Assert.AreEqual("D", parts[1]);
        }

        [TestMethod]
        public void TestAdvancedSearchMatch()
        {
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "DOC", false));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "DisplayOC", false));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "DisObjCont", false));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "Object", false));
            Assert.IsFalse(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "doc", false));
            Assert.IsFalse(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "displayoc", false));
            Assert.IsFalse(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "DISPLAYOC", false));
            Assert.IsFalse(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "displayocontainer", false));
            Assert.IsFalse(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "Disobj", false));
        }

        [TestMethod]
        public void TestAdvancedSearchMatchNoCase()
        {
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "doc", true));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "displayoc", true));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "DISPLAYOC", true));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "displayocontainer", true));
            Assert.IsTrue(SearchUtil.AdvancedSearchMatch("DisplayObjectContainer", "Disobj", true));
        }

        [TestMethod]
        public void TestSearchTypeMatchCase()
        {
            List<string> source = new List<string>()
            {
                "flash.display.DisplayObject",
                "flash.display.DisplayObjectContainer",
                "flash.display.Shape",
                "flash.display.Sprite",
                "flash.display.MovieClip"
            };
            List<string> matches = SearchUtil.Matches(source, "D", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "d", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 0);
            matches = SearchUtil.Matches(source, "Display", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "display", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 0);
            matches = SearchUtil.Matches(source, "DisplayObject", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            Assert.AreEqual(0, SearchUtil.Matches(source, "displayobject", ".", 100, false, true).Count);
            Assert.AreEqual(2, SearchUtil.Matches(source, "Object", ".", 100, false, true).Count);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "object", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 0);
            matches = SearchUtil.Matches(source, "DO", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "do", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 0);
            matches = SearchUtil.Matches(source, "DOC", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 1);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "doc", ".", 100, false, true);
            Assert.AreEqual(matches.Count, 0);
        }

        [TestMethod]
        public void TestSearchType()
        {
            List<string> source = new List<string>()
            {
                "flash.display.DisplayObject",
                "flash.display.DisplayObjectContainer",
                "flash.display.Shape",
                "flash.display.Sprite",
                "flash.display.MovieClip"
            };
            List<string> matches = SearchUtil.Matches(source, "d", ".", 100, false, false);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "display", ".", 100, false, false);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "displayobject", ".", 100, false, false);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "object", ".", 100, false, false);
            Assert.AreEqual(matches.Count, 2);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            Assert.AreEqual(0, SearchUtil.Matches(source, "do", ".", 100, false, false).Count);
            Assert.AreEqual(0, SearchUtil.Matches(source, "doc", ".", 100, false, false).Count);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "flash.", ".", 100, false, false);
            Assert.AreEqual(source.Count, matches.Count);
            foreach (string m in matches) Assert.IsTrue(source.Contains(m));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            matches = SearchUtil.Matches(source, "display.", ".", 0, false, false);
            Assert.AreEqual(source.Count, matches.Count);
            foreach (string m in matches) Assert.IsTrue(source.Contains(m));
        }

        [TestMethod]
        public void TestSearchTypeWholeWordMatchCase()
        {
            List<string> source = new List<string>()
            {
                "flash.display.DisplayObject",
                "flash.display.DisplayObjectContainer",
                "flash.display.Shape",
                "flash.display.Sprite",
                "flash.display.MovieClip"
            };
            Assert.AreEqual(source.Count, SearchUtil.Matches(source, "flash.", ".", 0, true, true).Count);
            Assert.AreEqual(0, SearchUtil.Matches(source, "FLASH.", ".", 0, true, true).Count);
            Assert.AreEqual(source.Count, SearchUtil.Matches(source, "flash.display", ".", 0, true, true).Count);
            Assert.AreEqual(0, SearchUtil.Matches(source, "FLASH.DISPLAY", ".", 0, true, true).Count);
            List<string> matches = SearchUtil.Matches(source, "flash.display.Display", ".", 0, true, true);
            Assert.AreEqual(2, matches.Count);
            Assert.IsTrue(matches.Contains("flash.display.DisplayObject"));
            Assert.IsTrue(matches.Contains("flash.display.DisplayObjectContainer"));
            Assert.AreEqual(0, SearchUtil.Matches(source, "flash.display.display", ".", 0, true, true).Count);
        }

        [TestMethod]
        public void TestSearchTypeWholeWordNoCase()
        {
            List<string> source = new List<string>()
            {
                "flash.display.DisplayObject",
                "flash.display.DisplayObjectContainer",
                "flash.display.Shape",
                "flash.display.Sprite",
                "flash.display.MovieClip"
            };
            Assert.AreEqual(source.Count, SearchUtil.Matches(source, "FLASH.", ".", 0, true, false).Count);
            Assert.AreEqual(source.Count, SearchUtil.Matches(source, "FLASH.DISPLAY", ".", 0, true, false).Count);
            Assert.AreEqual(2, SearchUtil.Matches(source, "FLASH.DISPLAY.Display", ".", 0, true, false).Count);
            Assert.AreEqual(2, SearchUtil.Matches(source, "flash.display.display", ".", 0, true, false).Count);
            Assert.AreEqual(2, SearchUtil.Matches(source, "Display", ".", 0, true, false).Count);
            Assert.AreEqual(0, SearchUtil.Matches(source, "Object", ".", 0, true, false).Count);
            Assert.AreEqual(0, SearchUtil.Matches(source, "do", ".", 100, true, false).Count);
            Assert.AreEqual(0, SearchUtil.Matches(source, "doc", ".", 100, true, false).Count);
        }

        [TestMethod]
        public void TestGetIsAbbreviation()
        {
            Assert.IsFalse(SearchUtil.GetIsAbbreviation("test"));
            Assert.IsFalse(SearchUtil.GetIsAbbreviation("Test"));
            Assert.IsTrue(SearchUtil.GetIsAbbreviation("TEST"));
            Assert.IsFalse(SearchUtil.GetIsAbbreviation("TEST."));
        }

        [TestMethod]
        public void TestGetAbbreviation()
        {
            List<char> abbreviation = SearchUtil.GetAbbreviation("DisplayObjectContainer");
            Assert.AreEqual('D', abbreviation[0]);
            Assert.AreEqual('O', abbreviation[1]);
            Assert.AreEqual('C', abbreviation[2]);
        }

        [TestMethod]
        public void TestAbbreviationSearchMatch()
        {
            Assert.IsTrue(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "D"));
            Assert.IsTrue(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "DO"));
            Assert.IsTrue(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "DOC"));
            Assert.IsTrue(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "O"));
            Assert.IsTrue(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "OC"));
            Assert.IsTrue(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "C"));
            Assert.IsFalse(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "DOCC"));
            Assert.IsFalse(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "doc"));
            Assert.IsFalse(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "doC"));
            Assert.IsFalse(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "dOC"));
            Assert.IsFalse(SearchUtil.AbbreviationSearchMatch("DisplayObjectContainer", "DC"));
        }

        [TestMethod]
        public void TestSearchTypeAbbreviation()
        {
            List<string> source = new List<string> { "flash.display.GraphicsPathWinding" };
            Assert.AreEqual(0, SearchUtil.Matches(source, "GW", ".", 0, false, false).Count);
            Assert.AreEqual(0, SearchUtil.Matches(source, "WP", ".", 0, false, false).Count);
            Assert.AreEqual(1, SearchUtil.Matches(source, "GP", ".", 0, false, false).Count);
            Assert.AreEqual(1, SearchUtil.Matches(source, "GPW", ".", 0, false, false).Count);
            Assert.AreEqual(1, SearchUtil.Matches(source, "PW", ".", 0, false, false).Count);
        }
    }
}