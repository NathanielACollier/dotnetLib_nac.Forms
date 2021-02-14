using Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;

using forms=NC.Forms.Avalonia;
using NC.Forms.Avalonia; // bring in the extensions

namespace Tests
{
    [TestClass]
    public class FormTests
    {
        [TestMethod]
        public void TestDisplay()
        {
            Avalonia.AppBuilder.Configure<forms.App>()
                .NewForm()
                .Display();
        }

        [TestMethod]
        public void FormWithButtonClickCount()
        {
            var f = Avalonia.AppBuilder.Configure<forms.App>()
                .NewForm();

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
            var f = Avalonia.AppBuilder.Configure<forms.App>()
                .NewForm();

            f.TextFor("txt2", "Type here")
                .TextBoxFor("txt2")
                .Display();
        }



        [TestMethod]
        public void LayoutVerticalSplitTest(){
            var f = Avalonia.AppBuilder.Configure<forms.App>()
                .NewForm();

            f.VerticalGroupSplit(grp=>{
                grp.Text("Text Above")
                    .Text("Text Below");
            });
        }
    }
}
