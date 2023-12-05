using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using nac.Forms.lib;
using nac.Forms.model;

namespace nac.Forms
{
    public partial class Form
    {
        private static lib.Log log = new lib.Log();

        private Grid Host { get; set; }
        private Dictionary<string, Control> controlsIndex;
        public lib.BindableDynamicDictionary Model { get; set; }
        private Application app;
        private Window win;
        private Form parentForm;
        private bool isMainForm;
        private bool isDisplayed;

        public object DataContext
        {
            get { return this.Model[model.SpecialModelKeys.DataContext]; }
            set { this.Model[model.SpecialModelKeys.DataContext] = value; }
        }

        public string Title
        {
            get { return this.win.Title; }
            set { this.win.Title = value; }
        }

        public Form(Application __app, lib.BindableDynamicDictionary _model=null)
        {
            if( _model == null)
            {
                // parent form
                this.Model = new lib.BindableDynamicDictionary();
                this.isMainForm = true;
            }
            else
            {
                // child form
                this.Model = _model;
                this.isMainForm = false;
            }
            this.app = __app;
            this.isDisplayed = false;
            this.win = new Window();

            var g = new Grid();
            var gridCol = new ColumnDefinition();
            g.ColumnDefinitions.Add(gridCol);

            this.Host = g;
            
            this.controlsIndex = new Dictionary<string, Control>();
        }

        public Form DebugAvalonia()
        {
            // documentation on how the dev tools work:
            //    + https://docs.avaloniaui.net/docs/getting-started/developer-tools
            this.win.AttachDevTools();
            return this;
        }

        public Form(Form _parentForm) : this(__app: _parentForm.app, _model: _parentForm.Model)
        {
            this.parentForm = _parentForm;
        }

        public Form(Form _parentForm, lib.BindableDynamicDictionary _model) : this(__app: _parentForm.app,
            _model: _model)
        {
            this.parentForm = _parentForm;
        }


        public delegate void ConfigureAppBuilder(Avalonia.AppBuilder appBuilder);

        public static Form NewForm(ConfigureAppBuilder beforeAppBuilderInit = null)
        {
            var appBuilder = Avalonia.AppBuilder.Configure<nac.Forms.App>();
            beforeAppBuilderInit?.Invoke(appBuilder);
            var builder = appBuilder
                //.LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
                .UsePlatformDetect()
                .SetupWithoutStarting();

            var f = new Form(builder.Instance);
            return f;
        }

        private void FireOnNextWithValue<T>(nac.Forms.Reactive.Subject<T> bindingSource, object value)
        {
            // field value has changed
            if (value is T newVal)
            {
                bindingSource.OnNext(newVal);
            }
            else
            {
                if (lib.Util.CanChangeType<T>(value, out T newVal2))
                {
                    bindingSource.OnNext(newVal2);
                }
            }
        }


        internal void AddVisibilityTrigger(Visual control, 
                                    string isVisibleModelName, 
                                    bool trueResultMeansVisible)
        {
            notifyOnRootModelChange(isVisibleModelName, (context, val) =>
            {
                if( val is bool isVisible)
                {
                    if (trueResultMeansVisible)
                    {
                        control.IsVisible = isVisible;
                    }
                    else
                    {
                        control.IsVisible = !isVisible;
                    }
                    
                }
            });
        }

        public async Task<bool> DisplayChildForm(Action<Form> setupChildForm, int height = 600, int width = 800,
            Func<Form, Task<bool?>> onClosing = null,
            Func<Form, Task> onDisplay = null,
            bool useIsolatedModelForThisChildForm = false,
            bool isDialog = false)
        {
            // default to use the parent's model, but some child will use a DataContext and need an isolated model
            var childFormModel = this.Model;
            if (useIsolatedModelForThisChildForm == true)
            {
                childFormModel = new BindableDynamicDictionary();
            }
            var childForm = new Form(_parentForm: this, _model: childFormModel);

            setupChildForm(childForm);

            return await childForm.Display_Internal(height: height, width: width, onClosing: onClosing,
                onDisplay: onDisplay,
                isDialog: isDialog
                );
        }

        public Task _Internal_ShowDialog(Window subWindow)
        {
            return subWindow.ShowDialog(win);
        }
        
        internal async Task<bool> Display_Internal(int height, int width,
            Func<Form, Task<bool?>> onClosing = null,
            Func<Form, Task> onDisplay = null,
            bool isDialog = false)
        {
            var promise = new System.Threading.Tasks.TaskCompletionSource<bool>();
            win.Height = height;
            win.Width = width;
            win.Content = this.Host;
            win.Closing += async (_sender, _args) =>
            {
                log.Debug("Window is closing");
                /*
                 _args.Cancel = true => stops the window from closing
                    + So what we do if they didn't specif an onClosing, is we set cancel to false
                 */
                if (onClosing != null)
                {
                    bool? stopCancel = await onClosing.Invoke(this);
                    _args.Cancel = stopCancel ?? false;
                }

                if (_args.Cancel == false && promise.Task.IsCompleted == false)
                {
                    promise.SetResult(true); // window is closed
                }
                
            };

            if (onDisplay != null)
            {
                // showing the form, so notify people if they wanted notification
                onDisplay.Invoke(this);
            }

            if (isDialog)
            {
                if (parentForm != null)
                {
                    await parentForm._Internal_ShowDialog(win);
                }
                else
                {
                    throw new Exception(
                        "Cannot show window as ShowDialog unless you have a parent form.  parentForm was null");
                }
            }
            else
            {
                win.Show();
            }

            return await promise.Task;
        }

        public void Display(int height = 600, int width = 800,
            Func<Form, Task<bool?>> onClosing = null,
            Func<Form, Task> onDisplay = null)
        {
            if( this.isDisplayed)
            {
                throw new Exception("Cannot call Display twice on Form.  Display has already been called on this form.");
            }

            if(!this.isMainForm)
            {
                throw new Exception("Cannot call Display on child form.  If you already have a main form, you must call DisplayChildForm.  Main form manages the avalonia app.");
            }

            this.Display_Internal(height: height, 
                            width: width, 
                            onClosing: onClosing,
                            onDisplay: onDisplay
                            );

            this.app.Run(win);
        }

        public void Close()
        {
            this.win.Close();
        }


        public async Task InvokeAsync(Func<Task> codeToRun)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await codeToRun();
            });
        }

        internal Grid getBoundControlFromPopulateForm(Action<Form> buildFormAction)
        {
            var rowForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
            // this has to have a unique model
            rowForm.DataContext = getBindingSource();
            buildFormAction(rowForm);

            rowForm.Host.DataContext = getBindingSource();

            return rowForm.Host;
        }
    }
}
