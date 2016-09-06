using NUnit.Framework;
using QuickNavigate.Helpers;

namespace QuickNavigate.Tests.Herlps
{
    [TestFixture]
    public class FormHelperTests
    {
        [Test]
        public void Trascriptor()
        {
            Assert.AreEqual("main", FormHelper.Transcriptor("ьфшт"));
            Assert.AreEqual("TestClass", FormHelper.Transcriptor("еуыеСдфыы"));
        }
    }
}