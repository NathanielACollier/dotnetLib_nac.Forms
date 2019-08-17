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
        public void TestDisplay()
        {
            new dotnetCoreAvaloniaNCForms.Form()
                .Display();
        }

        [TestMethod]
        public void FormWithButtonClickCount()
        {
            var f = new dotnetCoreAvaloniaNCForms.Form();
            f.TextFor("txt1", "When you click button I'll change to count!")
                .Button("Click Me!", arg =>
                {
                    var current = f.Model.GetOrDefault<int>("txt1", 0);
                    ++current;
                    f.Model["txt1"] = current;
                })
                .Display();
        }


        [TestMethod]
        public void FormThatDisplaysTypedText()
        {
            var f = new dotnetCoreAvaloniaNCForms.Form();
            f.TextFor("txt2", "Type here")
                .TextBoxFor("txt2")
                .Display();
        }
    }
}
