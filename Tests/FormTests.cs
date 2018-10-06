using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class FormTests
    {
        [TestMethod]
        public void TestDisplay()
        {
            new dotnetCoreAvaloniaNCForms.Form()
                .Display();
        }
    }
}
