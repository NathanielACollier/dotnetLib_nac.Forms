using Avalonia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using nac.Forms;
using Avalonia.Logging;
using System;

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
            nac.Logging.Appenders.Debug.Setup();

            nac.Forms.lib.AvaloniaAppManager.GlobalAppBuilderConfigurFunction = (appBuilder) =>
            {
                appBuilder.LogToTrace(LogEventLevel.Debug);
                System.Diagnostics.Debug.WriteLine("[nac.Forms.Testing] - Avalonia LogToTrace Setup");
            };

            await nac.Forms.lib.AvaloniaAppManager.DisplayForm(async f =>
            {
                f.Text("Special UI Thread.  Never use this unless you know why you used it.  Will not work on macos where UI must be on the main thread");
            });

            await nac.Forms.lib.AvaloniaAppManager.DisplayForm(async f =>
            {
                f.Text("A second form on the avalonia App");
            });

            // shut it down then test creating a form after shutdown
            nac.Forms.lib.AvaloniaAppManager.Shutdown();

            try
            {
                await nac.Forms.lib.AvaloniaAppManager.DisplayForm(async f =>
                {
                    Assert.Fail("You cannot create a form after the window has shutdown");
                    f.Text("Window After shutdown.  This is not possible");
                });

            }
            catch (Exception ex) { }


            System.Diagnostics.Debug.WriteLine("Test finished");
        }



        [TestMethod]
        public async Task InterruptFormThatisRunningViaAppShutdown()
        {
            await nac.Forms.lib.AvaloniaAppManager.DisplayForm(async f =>
            {
                f.HorizontalGroup(hg =>
                {
                    hg.Text("Current Time is: ")
                    .TextFor("CurrentTime");
                })
                .Button("stop", async () =>
                {
                    nac.Forms.lib.AvaloniaAppManager.Shutdown();
                });
            }, onDisplay: async f=>
            {
                await Task.Run(async () =>
                {
                    while (1 == 1)
                    {
                        await f.InvokeAsync(async () =>
                        {
                            f.Model["CurrentTime"] = DateTime.Now.ToString();
                        });
                        await Task.Delay(200);
                    }
                });
            });
        }
        
    }
}
