using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.Threading;
using Avalonia.Reactive;

namespace dotnetCoreAvaloniaNCForms
{
    public partial class Form
    {
        static void log(string message)
        {
            Debug.WriteLine($"[{DateTime.Now:hh_mm_tt]}:{message}");
        }
        private StackPanel Host { get; set; }
        public lib.BindableDynamicDictionary Model { get; set; }

        public Form()
        {
            this.Host = new StackPanel();
            this.Model = new lib.BindableDynamicDictionary();
            this.Host.Orientation = Orientation.Vertical;
        }


        private void AddRowToHost(IControl ctrl)
        {
            DockPanel row = new DockPanel();

            row.Children.Add(ctrl);

            this.Host.Children.Add(row);
        }

        private void AddBinding<T>(string modelFieldName,
            AvaloniaObject control,
            AvaloniaProperty property,
            bool isTwoWayDataBinding = false)
        {
            // (ideas from here)[http://avaloniaui.net/docs/binding/binding-from-code]
            var bindingSource = new Subject<T>();
            control.Bind(property, bindingSource);
            // Default we grab all changes to model field and apply them to property
            this.Model.PropertyChanged += (_s, _args) =>
            {
                if( string.Equals(_args.PropertyName, modelFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    // field value has changed
                    if( this.Model[modelFieldName] is T newVal)
                    {
                        bindingSource.OnNext(newVal);
                    }
                    
                }
            };
            // If they say two way then we setup a watch on the property observable and apply the values back to the model
            if(isTwoWayDataBinding)
            {
                // monitor for Property changes on control
                var controlValueChangesObservable = control.GetObservable(property);

                controlValueChangesObservable.Subscribe(newVal =>
                {
                    this.Model[modelFieldName] = newVal;
                });
            }
        }
        
        static AppBuilder BuildAvaloniaApp()
            => AppBuilder
            .Configure<App>()
            .LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
            .UsePlatformDetect()
            .SetupWithoutStarting()
            ;


        public Task<Form> DisplayNoThread(int height = 600, int width = 800)
        {
            var promise = new TaskCompletionSource<Form>();
            log("Starting with no thread");

            var builder = BuildAvaloniaApp();
            var win = new Window();
            win.Height = height;
            win.Width = width;
            win.Content = this.Host;
            win.Closed += (_sender, _args) =>
            {
                log("Window closed");
                promise.SetResult(this);
            };
            builder.Start(win);

            return promise.Task;
        }

        public Task<Form> Display(int height = 600, int width = 800)
        {
            var promise = new TaskCompletionSource<Form>();

            var t = new Thread(() =>
            {
                try
                {
                    log("Starting NCForm Display");
                    var appBuilder = BuildAvaloniaApp();

                    log("Constructing window");
                    var win = new Window();
                    win.Height = height;
                    win.Width = width;
                    win.Content = this.Host;
                    var cancelToken = new CancellationToken();
                    win.Closed += (_sender, _args) =>
                    {
                        log("Window closed");
                        promise.SetResult(this);
                    };
                    win.Show();

                    Avalonia.Threading.Dispatcher.UIThread.MainLoop(cancelToken);
                }
                catch(Exception ex)
                {
                    // save this error where the user can retrieve it from Form
                    promise.SetException(ex);
                }
            });
            t.Start();

            return promise.Task;
        }
    }
}
