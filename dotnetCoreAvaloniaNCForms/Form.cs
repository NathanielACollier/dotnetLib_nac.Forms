using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.Threading;

namespace dotnetCoreAvaloniaNCForms
{
    public class Form
    {
        static void log(string message)
        {
            Debug.WriteLine($"[{DateTime.Now:hh_mm_tt]}:{message}");
        }
        private StackPanel Host { get; set; }

        public Form()
        {
            this.Host = new StackPanel();
        }
        
        static AppBuilder BuildAvaloniaApp()
            => AppBuilder
            .Configure<App>()
            .LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
            .UsePlatformDetect()
            .SetupWithoutStarting()
            ;

        public Task<Form> Display(int height = 600, int width = 800)
        {
            var promise = new TaskCompletionSource<Form>();

            var t = new Thread(() =>
            {
                try
                {
                    log("Starting NCForm Display");
                    Avalonia.Threading.AvaloniaSynchronizationContext.InstallIfNeeded();
                    
                    Avalonia.Threading.Dispatcher.UIThread.VerifyAccess();

                    var appBuilder = BuildAvaloniaApp();

                    log("Constructing window");
                    var win = new Window();
                    win.Height = height;
                    win.Width = width;
                    win.Content = this.Host;
                    win.Closed += (_sender, _args) =>
                    {
                        promise.SetResult(this);
                    };
                    win.Show();

                    appBuilder.Start(win);
                    
                }
                catch(Exception ex)
                {
                    // save this error where the user can retrieve it from Form
                    promise.SetException(ex);
                }
            });
            t.TrySetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();

            return promise.Task;
        }
    }
}
