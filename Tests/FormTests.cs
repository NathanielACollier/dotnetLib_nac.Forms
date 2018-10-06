using Avalonia;
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
    }
}
