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
            nac.Forms.Form
                .NewForm()
                .Display();
        }

        [TestMethod]
        public void FormWithButtonClickCount()
        {
            var f = nac.Forms.Form
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
            var f = nac.Forms.Form
                .NewForm();

            f.TextFor("txt2", "Type here")
                .TextBoxFor("txt2")
                .Display();
        }



        [TestMethod]
        public void LayoutVerticalSplitTest(){
            var f = nac.Forms.Form
                .NewForm();

            f.VerticalGroup(grp=>{
                grp.Text("Text Above")
                       .Text("Text Below");
             }, isSplit: true)
                .Display();
        }



        [TestMethod]
        public async Task StartUI_SetupSpecialUIThread()
        {
            await nac.Forms.lib.AvaloniaAppManager.DisplayForm(async f =>
            {
                f.Text("Special UI Thread.  Never use this unless you know why you used it.  Will not work on macos where UI must be on the main thread");
            });

            await nac.Forms.lib.AvaloniaAppManager.DisplayForm(async f =>
            {
                f.Text("A second form on the avalonia App");
            });

            System.Diagnostics.Debug.WriteLine("Test finished");
        }
        
        
        
    }
}
