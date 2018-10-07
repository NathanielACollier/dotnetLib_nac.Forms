using Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class FormTests
    {
        [TestMethod]
        public async Task TestDisplay()
        {
            await new dotnetCoreAvaloniaNCForms.Form()
                .Display();
        }


        [TestMethod]
        public void TestAvalonia()
        {
            var app = new Application();
            AppBuilder.Configure(app)
                .UsePlatformDetect()
                .SetupWithoutStarting();

            var dialog = new Avalonia.Controls.Window();
            dialog.Show();
            app.Run(dialog);
        }


        [TestMethod]
        public async Task TestDisplayFormWithNoThread()
        {
            await new dotnetCoreAvaloniaNCForms.Form()
                .DisplayNoThread();
        }
    }
}
