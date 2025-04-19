using Avalonia;
using Avalonia.Logging;

namespace nac.Forms.UITesterApp;

public static class TestApp
{

    public static async Task Run(Type targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList)
    {
        var f = nac.Forms.Form
            .NewForm(beforeAppBuilderInit: (appBuilder) =>
            {
                appBuilder.LogToTrace(LogEventLevel.Debug);
                Console.WriteLine("Logging Setup");
            })
            .DebugAvalonia();

        repos.MainWindowUI.Run(f, targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList);
        nac.Forms.lib.AvaloniaAppManager.Shutdown(); // this isn't needed, and is a test to make sure it's safe to call it
        
    }
    
}