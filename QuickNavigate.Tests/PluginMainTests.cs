using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickNavigate.Tests
{
    [TestClass]
    public class PluginMainTests
    {
        [TestMethod]
        public void TestNew()
        {
            PluginMain plugin = new PluginMain();
            Assert.AreEqual(1, plugin.Api);
            Assert.AreEqual("QuickNavigate", plugin.Name);
            Assert.AreEqual("5e256956-8f0d-4f2b-9548-08673c0adefd", plugin.Guid);
            Assert.AreEqual("Canab, SlavaRa", plugin.Author);
            Assert.AreEqual("QuickNavigate plugin", plugin.Description);
            Assert.AreEqual("http://www.flashdevelop.org/community/", plugin.Help);
            Assert.IsNull(plugin.Settings);
        }
    }
}