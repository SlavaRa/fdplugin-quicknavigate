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
            ClassModel model = new ClassModel()
            {
                InFile = new FileModel()
                {
                    FileName = "test/TestClass.as",
                    Package = "test"
                },
                Name = "TestClass"
            };
            TypeNode node = new TypeNode(model, 0);
            Assert.AreEqual(model, node.Model);
            Assert.IsFalse(node.IsPrivate);
            Assert.IsNull(node.Module);
            Assert.AreEqual("TestClass (test)", node.Text);
        }

        [TestMethod]
        public void TestNewTypeNodeWithClassFromSWC()
        {
            ClassModel model = new ClassModel()
            {
                InFile = new FileModel()
                {
                    FileName = "../libs/playerglobal.swc/flash/display/DisplayObject",
                    Package = "flash.display"
                },
                Name = "DisplayObject"
            };
            TypeNode node = new TypeNode(model, 0);
            Assert.AreEqual("playerglobal.swc", node.Module);
            Assert.AreEqual("DisplayObject (flash.display)", node.Text);
        }

        [TestMethod]
        public void TestNewTypeNodeWithPrivateClass()
        {
            ClassModel model = new ClassModel()
            {
                InFile = new FileModel()
                {
                    FileName = "test/TestClass.as",
                    Package = "test"
                },
                Name = "TestClass2",
                Access = Visibility.Private
            };
            TypeNode node = new TypeNode(model, 0);
            Assert.IsTrue(node.IsPrivate);
            Assert.AreEqual("TestClass2 (test.TestClass)", node.Text);
        }

    }
}