using Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using util = dotnetCoreAvaloniaNCForms.lib.Util;
namespace Tests
{
    [TestClass]
    public class UtilTests
    {
        [TestMethod]
        public async Task TestConversionToString()
        {
            Assert.IsTrue(
                util.CanChangeType<string>(1, out string result));

            Assert.IsTrue(result == "1");

            Assert.IsTrue(
                util.CanChangeType<string>(false, out string result2));

            Assert.IsTrue(result2 == "False");
        }

    }
}
