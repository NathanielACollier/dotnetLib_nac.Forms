using Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;

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

        [TestMethod]
        public void FormWithButtonClickCount()
        {
            var f = new dotnetCoreAvaloniaNCForms.Form();
                f.TextFor("txt1")
                .Button("Click Me!", arg =>
                {
                    var current = f.Model.GetOrDefault<int>("txt1", 0);
                    ++current;
                    f.Model["txt1"] = current;
                })
                .Display();
        }
    }
}
