using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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
    private static nac.Logging.Logger log = new();
    public static nac.Forms.Form.ConfigureAppBuilder GlobalAppBuilderConfigurFunction;
    private static Avalonia.Application app;
    private static CancellationTokenSource appCancelSource = new();
    private static bool appIsShutdown = false; 
    
    private static Task<bool> StartAvaloniaApplication()
    {
        var promise = new System.Threading.Tasks.TaskCompletionSource<bool>();

        var mainThread = new Thread(async () =>
        {
            app = nac.Forms.Form.SetupAvaloniaApp(beforeAppBuilderInit: GlobalAppBuilderConfigurFunction);

            promise.SetResult(true);
            app.Run(token: appCancelSource.Token);
        });
        // configure the thread
        
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            mainThread.SetApartmentState(ApartmentState.STA);
        }
        
        mainThread.Start();

        return promise.Task;
    }

    private static async Task<bool> DisplayFormWithExistingApp(Avalonia.Application app,
        Action<nac.Forms.Form> buildFormFunction,
        int height = 600,
        int width = 800,
        Func<Form, Task<bool?>> onClosing = null,
        Func<Form, Task> onDisplay = null)
    {
        
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            log.Info("Inside UIThread InvokeAsync");
            // Sending a null model means it will be a parent form
            var form = new nac.Forms.Form(__app: app,
                _model: null);

            log.Info("Building Form");
            buildFormFunction(form);

            log.Info("DIsplay Internal Form");
            await form.Display_Internal(height: height,
                width: width,
                onClosing: async (_f) =>
                {
                    if (onClosing != null)
                    {
                        bool stopClose = await onClosing(_f) ?? false;
                        return stopClose;
                    }

                    return false;
                }, onDisplay: onDisplay);
        });

        return true;
    }

    public static async Task<bool> DisplayForm(Action<nac.Forms.Form> buildFormFunction,
        int height = 600,
        int width = 800,
        Func<Form, Task<bool?>> onClosing = null,
        Func<Form, Task> onDisplay = null)
    {
        log.Info("Starting display of form");
        if (appIsShutdown)
        {
            throw new Exception("Avalonia App Manager is shutdown.  You cannot start it back in this process");
        }

        if (Avalonia.Application.Current == null)
        {
            log.Info("Creating new Avalonia Application");
            await StartAvaloniaApplication();
        }

        log.Info("Avalonia Application Should Exist now");
        return await DisplayFormWithExistingApp(app: Avalonia.Application.Current,
            buildFormFunction: buildFormFunction,
            height: height,
            width: width,
            onClosing: onClosing,
            onDisplay: onDisplay);
    }


    /*
     Shutdown is a permanent action.  You cannot start the Avalonia thread back up again
     */
    public static void Shutdown()
    {
        if(app == null)
        {
            return; // if app is null then we didn't setup Avalonia so we won't be doing anything with it
        }

        log.Info("Starting Avalonia App shutdown");

        appCancelSource.Cancel();

        log.Info("Cleaning up variables");
        app = null;
        appCancelSource.Dispose();
        appIsShutdown = true;

        log.Info("Shutdown is finished");
        
    }

}
