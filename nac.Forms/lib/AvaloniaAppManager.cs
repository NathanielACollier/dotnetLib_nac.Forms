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
    
    private static Task<bool> DisplayFormWithNewAvaloniaApp(Action<nac.Forms.Form> buildFormFunction,
                                                        int height=600,
                                                        int width = 800,
                                                        Func<Form, Task<bool?>> onClosing = null,
                                                        Func<Form, Task> onDisplay = null)
    {
        var promise = new System.Threading.Tasks.TaskCompletionSource<bool>();
        
        var mainThread = new Thread(async () =>
        {
            var mainForm = nac.Forms.Form.NewForm(beforeAppBuilderInit: GlobalAppBuilderConfigurFunction);

            buildFormFunction(mainForm);
            
            mainForm.Display(height: height,
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
        // configure the thread
        mainThread.SetApartmentState(ApartmentState.STA);
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
            var form = new nac.Forms.Form(__app: app,
                _model: new nac.Forms.lib.BindableDynamicDictionary());

            buildFormFunction(form);
            
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
        if (Avalonia.Application.Current == null)
        {
            return await DisplayFormWithNewAvaloniaApp(buildFormFunction,
                height: height,
                width: width,
                onClosing: onClosing,
                onDisplay: onDisplay);
        }

        return await DisplayFormWithExistingApp(app: Avalonia.Application.Current,
            buildFormFunction: buildFormFunction,
            height: height,
            width: width,
            onClosing: onClosing,
            onDisplay: onDisplay);
    }


}
