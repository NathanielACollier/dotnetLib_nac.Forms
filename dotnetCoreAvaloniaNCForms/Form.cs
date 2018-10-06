using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace dotnetCoreAvaloniaNCForms
{
    public class Form
    {
        private StackPanel Host { get; set; }

        public Form()
        {
            this.Host = new StackPanel();
        }


        public Task<Form> Display(int height = 600, int width = 800)
        {
            var promise = new TaskCompletionSource<Form>();

            var t = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    AppBuilder.Configure(app)
                        .UsePlatformDetect()
                        .SetupWithoutStarting();

                    var win = new Window();
                    win.Height = height;
                    win.Width = width;
                    win.Content = this.Host;
                    win.Closed += (_sender, _args) =>
                    {
                        promise.SetResult(this);
                    };
                    win.Show();
                    app.Run(win);
                    
                }
                catch(Exception ex)
                {
                    // save this error where the user can retrieve it from Form
                    promise.SetException(ex);
                }
            });
            t.ApartmentState = ApartmentState.STA;
            t.Start();

            return promise.Task;
        }
    }
}
