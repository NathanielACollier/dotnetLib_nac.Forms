using System;
using System.Threading.Tasks;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public class Events
{
    
    public static void OnDisplay(Form parentForm)
    {
        parentForm.DisplayChildForm(f =>
        {
            f.TextFor("message");
        }, onDisplay: async (f) =>
        {
            f.Model["message"] = "Form is displayed";
        }, useIsolatedModelForThisChildForm: true);
    }
    
    
    public static void OnDisplayLongRunning(Form parentForm)
    {
        parentForm.DisplayChildForm(f =>
        {
            f.Text("OnDisplay update current clock for 1 minute")
                .TextFor("currentTime");
        }, onDisplay: async (f) =>
        {
            int secondsOfRuntime = 0;
            await Task.Run(async () =>
            {
                while (secondsOfRuntime < 60 * 1)
                {
                    f.Model["currentTime"] = DateTime.Now.ToLongTimeString();
                    await Task.Delay(millisecondsDelay: 1000);
                    ++secondsOfRuntime;
                }
            });
        }, useIsolatedModelForThisChildForm: true)
         .ContinueWith(t =>
         {
             Console.WriteLine("Form is complete");
         });
    }
    
    
    
    public static void OnClosingButtonClick(Form parentForm)
    {
        parentForm.DisplayChildForm(f =>
        {
            f.Model["closeCount"] = 0;
            f.Model["isQuit"] = false;
            f.Text("Clicking ok will close this form")
                .HorizontalGroup(hg =>
                {
                    hg.Text("Close count: ")
                        .TextFor("closeCount");
                })
                .HorizontalGroup(hg =>
                {
                    hg.Button("Quit", async () =>
                    {
                        f.Close();
                    }).Button("Force Quit", async () =>
                    {
                        f.Model["isQuit"] = true;
                        f.Close();
                    });
                });
        }, onClosing: async (f) =>
        {
            dynamic closeCount = f.Model["closeCount"];
            f.Model["closeCount"] = ++closeCount;

            if (f.Model["isQuit"] as bool? == true)
            {
                return false; // don't cancel
            }
            else
            {
                return true; // prevent closing the window (return if cancel or not)
            }
            
        }, useIsolatedModelForThisChildForm: true);
    }
    
    
    
    
    
    
    
}