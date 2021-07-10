using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using nac.Forms.model;

namespace nac.Forms
{
    public partial class Form
    {
        private static lib.Log log = new lib.Log();

        private Grid Host { get; set; }
        private Dictionary<string, IControl> controlsIndex;
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

            var g = new Grid();
            var gridCol = new ColumnDefinition();
            g.ColumnDefinitions.Add(gridCol);

            this.Host = g;
            
            this.controlsIndex = new Dictionary<string, IControl>();
        }

        public Form(Form _parentForm) : this(__app: _parentForm.app, _model: _parentForm.Model)
        {
            
        }


        private void FireOnNext<T>(Subject<T> bindingSource, string modelFieldName)
        {
            FireOnNextWithValue<T>(bindingSource, this.Model[modelFieldName]);
        }


        private void FireOnNextWithValue<T>(Subject<T> bindingSource, object value)
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


        internal void AddVisibilityTrigger(Visual control, string isVisibleModelName)
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

            bool bindingIsDataContext = false;
            object dataContext = null;
            object getDataContextValue(){
                if( dataContext is lib.BindableDynamicDictionary dynDict){
                    return dynDict[modelFieldName];
                }else{
                    return dataContext.GetType().GetProperty(modelFieldName).GetValue(dataContext);
                }
                
            }
            void setDataContextValue(object val){
                if( dataContext is lib.BindableDynamicDictionary dynDict){
                    dynDict[modelFieldName] = val;
                }else {
                    dataContext.GetType().GetProperty(modelFieldName).SetValue(dataContext, val);
                }
            }

            // does model contain a datacontext???
            if( this.Model.GetDynamicMemberNames().Any(key=> string.Equals(key, SpecialModelKeys.DataContext, StringComparison.OrdinalIgnoreCase))){
                // bind to the data context
                dataContext = this.Model[SpecialModelKeys.DataContext];

                if( dataContext is System.ComponentModel.INotifyPropertyChanged prop){
                    // It's INotifyPropertyChanged so set this as handled
                    bindingIsDataContext = true;

                    // need to fire it's current value.  Then start watching for changes
                    FireOnNextWithValue<T>(bindingSource, getDataContextValue());

                    prop.PropertyChanged += (_s,_args) => {
                        if( string.Equals(_args.PropertyName, modelFieldName, StringComparison.OrdinalIgnoreCase)){
                            
                            FireOnNextWithValue<T>(bindingSource, getDataContextValue());
                        }
                    };
                }
                else
                {
                    /*
                     !!REMEMBER!! You can use the bindabledictionary
                     If you don't want to go to the trouble of implementing INotifyPropertyChanged just have your items use BindableDictionary and it's pretty easy
                     */
                    throw new Exception(
                        $"Special DataContext used in model, but DataContext is not INotifyPropertyChanged.  Type given is {dataContext.GetType().Name}.  There is no way to read properties without INotifyPropertyChanged");
                }
            }

            if(!bindingIsDataContext){
                notifyOnModelChange(modelFieldName, (val) =>
                {
                    FireOnNext<T>(bindingSource, modelFieldName);
                });
            }


            // If they say two way then we setup a watch on the property observable and apply the values back to the model
            if(isTwoWayDataBinding)
            {
                // monitor for Property changes on control
                var controlValueChangesObservable = control.GetObservable(property);

                controlValueChangesObservable.Subscribe(newVal =>
                {
                    if( bindingIsDataContext){
                        // set the property
                        setDataContextValue(newVal);
                    }else {
                        this.Model[modelFieldName] = newVal;
                    }
                    
                });
            }
        }
        

        public void DisplayChildForm(Action<Form> setupChildForm, int height = 600, int width = 800,
            Func<Form,bool?> onClosing = null,
            Action<Form> onDisplay = null)
        {
            var childForm = new Form(this.app, this.Model);

            setupChildForm(childForm);

            childForm.Display_Internal(height: height, width: width, onClosing: onClosing,
                onDisplay: onDisplay);
        }

        private void Display_Internal(int height, int width,
            Func<Form,bool?> onClosing = null,
            Action<Form> onDisplay = null)
        {
            win.Height = height;
            win.Width = width;
            win.Content = this.Host;
            win.Closing += (_sender, _args) =>
            {
                log.Debug("Window is closing");
                /*
                 _args.Cancel = true => stops the window from closing
                    + So what we do if they didn't specif an onClosing, is we set cancel to false
                 */
                _args.Cancel = onClosing?.Invoke(this) ?? false; 
            };
            
            onDisplay?.Invoke(this); // showing the form, so notify people if they wanted notification
            win.Show();
        }

        public void Display(int height = 600, int width = 800,
            Func<Form, bool?> onClosing = null,
            Action<Form> onDisplay = null)
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

        public void Close()
        {
            this.win.Close();
        }
    }
}
