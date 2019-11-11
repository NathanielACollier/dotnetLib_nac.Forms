using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;

using Avalonia.Threading;
using Avalonia.Reactive;
using System.Linq;

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
        private Application app;
        private Window win;
        private bool isMainForm;
        private bool isDisplayed;

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
            this.Host = new StackPanel();
            
            this.Host.Orientation = Orientation.Vertical;
        }

        public Form(Form _parentForm) : this(__app: _parentForm.app, _model: _parentForm.Model)
        {
            
        }


        private void AddRowToHost(IControl ctrl)
        {
            DockPanel row = new DockPanel();

            row.Children.Add(ctrl);

            this.Host.Children.Add(row);
        }


        private void FireOnNext<T>(Subject<T> bindingSource, string modelFieldName)
        {
            // field value has changed
            if (this.Model[modelFieldName] is T newVal)
            {
                bindingSource.OnNext(newVal);
            }
            else
            {
                if (lib.Util.CanChangeType<T>(
                    this.Model[modelFieldName], out T newVal2))
                {
                    bindingSource.OnNext(newVal2);
                }
            }
        }


        private void AddVisibilityTrigger(Visual control, string isVisibleModelName)
        {
            notifyOnModelChange(isVisibleModelName, (val) =>
            {
                if( val is bool isVisible)
                {
                    control.IsVisible = isVisible;
                }
            });
        }



        private void notifyOnModelChange(string modelFieldName, Action<object> codeToRunOnChange)
        {
            // need to fire what it is now if there is anything there
            if (this.Model.GetDynamicMemberNames().Contains(modelFieldName))
            {
                // fire OnNext
                codeToRunOnChange(this.Model[modelFieldName]);
            }


            // Default we grab all changes to model field and apply them to property
            this.Model.PropertyChanged += (_s, _args) =>
            {
                if (string.Equals(_args.PropertyName, modelFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    codeToRunOnChange(this.Model[modelFieldName]);
                }
            };
        }



        private void AddBinding<T>(string modelFieldName,
            AvaloniaObject control,
            AvaloniaProperty property,
            bool isTwoWayDataBinding = false)
        {
            // (ideas from here)[http://avaloniaui.net/docs/binding/binding-from-code]
            var bindingSource = new Subject<T>();
            var bindingSourceObservable = bindingSource.AsObservable()
                .Select(i =>
                {
                    return (object)i;
                });
            control.Bind(property, bindingSourceObservable);

            notifyOnModelChange(modelFieldName, (val) =>
            {
                FireOnNext<T>(bindingSource, modelFieldName);
            });

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
        

        public void DisplayChildForm(Action<Form> setupChildForm, int height = 600, int width = 800,
            Action onClosing = null)
        {
            var childForm = new Form(this.app, this.Model);

            setupChildForm(childForm);

            childForm.Display_Internal(height: height, width: width, onClosing: onClosing);
        }

        private void Display_Internal(int height, int width,
            Action onClosing = null)
        {
            win.Height = height;
            win.Width = width;
            win.Content = this.Host;
            win.Closing += (_sender, _args) =>
            {
                log("Window is closing");
                if( onClosing != null)
                {
                    onClosing();
                }
            };
            win.Show();
        }

        public void Display(int height = 600, int width = 800,
            Action onClosing = null)
        {
            if( this.isDisplayed)
            {
                throw new Exception("Cannot call Display twice on Form.  Display has already been called on this form.");
            }

            if(!this.isMainForm)
            {
                throw new Exception("Cannot call Display on child form.  If you already have a main form, you must call DisplayChildForm.  Main form manages the avalonia app.");
            }

            this.Display_Internal(height: height, width: width, onClosing: onClosing);

            this.app.Run(win);
        }


    }
}
