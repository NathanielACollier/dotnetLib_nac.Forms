using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nac.Forms.lib;

/*
 THis avalonia app manager is a special thing that is used for launching avalonia windows from a WPF app
  - It will keep a reference to a "Main" form an spin off child forms for each new thing needed
    !!REMEMBER!!
    + On some operating systems like MACOS you cannot run the UI on a seperate thread than the main thread
    + Allways try and use Display() directly first, and if you run into some situation you can use StartUI
    + You cannot call this from an existing avalonia app.
*/

public static class AvaloniaAppManager
{
    public static nac.Forms.Form.ConfigureAppBuilder GlobalAppBuilderConfigurFunction;
    private static nac.Forms.Form mainForm;
    private static System.Threading.Thread mainThread;

    private static Task EnsureMainForm()
    {
        var promise = new System.Threading.Tasks.TaskCompletionSource<bool>();

        if (mainForm != null)
        {
            return Task.FromResult(true);
        }

        // mainForm isn't setup yet
        mainThread = new Thread(async () =>
        {
            mainForm = nac.Forms.Form.NewForm(beforeAppBuilderInit: GlobalAppBuilderConfigurFunction);

            mainForm.Display(height: 0,
                width: 0,
                onDisplay: async (_f) =>
                {
                    promise.SetResult(true);
                });
        });
        // configure the thread
        mainThread.SetApartmentState(ApartmentState.STA);
        mainThread.Start();

        return promise.Task;
    }



    public static async Task<bool> DisplayForm(Action<nac.Forms.Form> buildFormFunction,
                                    int height=600,
                                    int width = 800,
                                    Func<Form, Task<bool?>> onClosing = null,
                                    Func<Form, Task> onDisplay = null)
    {
        await EnsureMainForm();
        await mainForm.InvokeAsync(async () =>
        {
            await mainForm.DisplayChildForm(setupChildForm: buildFormFunction,
            height: height,
            width: width,
            onClosing: onClosing,
            onDisplay: onDisplay,
            useIsolatedModelForThisChildForm: true,
            isDialog: true);

        });

        return true;
    }


}
