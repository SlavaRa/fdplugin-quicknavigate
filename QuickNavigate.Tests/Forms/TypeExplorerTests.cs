using ASCompletion.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickNavigate.Forms;

namespace QuickNavigate.Tests.Forms
{
    [TestClass]
    public class TypeExplorerTests
    {
        [TestMethod]
        public void TestNewTypeNode()
        {
            var model = new ClassModel
            {
                InFile = new FileModel
                {
                    FileName = "test/TestClass.as",
                    Package = "test"
                },
                Name = "TestClass"
            };
            var node = new ClassNode(model, 0);
            Assert.AreEqual(model, node.Model);
            Assert.IsFalse(node.IsPrivate);
            Assert.IsNull(node.Module);
            Assert.AreEqual("TestClass", node.Text);
            Assert.AreEqual("test", node.In);
        }

        [TestMethod]
        public void TestNewTypeNodeWithClassFromSWC()
        {
            var model = new ClassModel
            {
                InFile = new FileModel
                {
                    FileName = "../libs/playerglobal.swc/flash/display/DisplayObject",
                    Package = "flash.display"
                },
                Name = "DisplayObject"
            };
            var node = new ClassNode(model, 0);
            Assert.AreEqual("playerglobal.swc", node.Module);
            Assert.AreEqual("DisplayObject", node.Text);
            Assert.AreEqual("flash.display", node.In);
        }

        [TestMethod]
        public void TestNewTypeNodeWithPrivateClass()
        {
            var model = new ClassModel
            {
                InFile = new FileModel

                {
                    FileName = "test/TestClass.as",
                    Package = "test"
                },
                Name = "TestClass2",
                Access = Visibility.Private
            };
            var node = new ClassNode(model, 0);
            Assert.IsTrue(node.IsPrivate);
            Assert.AreEqual("TestClass2", node.Text);
            Assert.AreEqual("test.TestClass", node.In);
        }

    }
}