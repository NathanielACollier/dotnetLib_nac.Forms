using Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using nac.Forms;

// bring in the extensions

namespace Tests
{
    [TestClass]
    public class FormTests
    {
        [TestMethod]
        public void TestDisplay()
        {
            Avalonia.AppBuilder.Configure<App>()
                .NewForm()
                .Display();
        }

        [TestMethod]
        public void FormWithButtonClickCount()
        {
            var f = Avalonia.AppBuilder.Configure<App>()
                .NewForm();

            f.TextFor("txt1", "When you click button I'll change to count!")
                .Button("Click Me!", async () =>
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
            var f = Avalonia.AppBuilder.Configure<App>()
                .NewForm();

            f.TextFor("txt2", "Type here")
                .TextBoxFor("txt2")
                .Display();
        }



        [TestMethod]
        public void LayoutVerticalSplitTest(){
            var f = Avalonia.AppBuilder.Configure<App>()
                .NewForm();

            f.VerticalGroup(grp=>{
                grp.Text("Text Above")
                       .Text("Text Below");
             }, isSplit: true)
                .Display();
        }
        
        
        
    }
}
