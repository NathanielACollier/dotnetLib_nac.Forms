using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        private Dictionary<string, IControl> controlsIndex;
        public lib.BindableDynamicDictionary Model { get; set; }
        private Application app;
        private Window win;
        private bool isMainForm;
        private bool isDisplayed;

        public object DataContext
        {
            get { return this.Model[model.SpecialModelKeys.DataContext]; }
            set { this.Model[model.SpecialModelKeys.DataContext] = value; }
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
            
            this.controlsIndex = new Dictionary<string, IControl>();
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
            if (this.Model.HasKey(modelFieldName))
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


        private object getDataContextValue(object dataContext, string modelFieldName){
            if (dataContext == null)
            {
                throw new Exception($"Error accessing [field: {modelFieldName}]. DataContext special field in model is null");
            }

            var fieldPath = new model.ModelFieldNamePathInfo(modelFieldName);

            if( dataContext is lib.BindableDynamicDictionary dynDict){
                if (!dynDict.HasKey(fieldPath.Current))
                {
                    throw new Exception($"Field [{fieldPath.Current}] does not exist on DataContext object");
                }

                var val = dynDict[fieldPath.Current];
                if (fieldPath.ChildPath.Length > 0)
                {
                    return getDataContextValue(val, fieldPath.ChildPath);
                }
                else
                {
                    return val;
                }
            }else
            {
                Type dcType = dataContext.GetType();
                var prop = dcType.GetProperty(fieldPath.Current);
                if (prop == null)
                {
                    throw new Exception(
                        $"Field [{fieldPath.Current}] does not exist on DataContext object of [type: {dcType.Name}]");
                }
                
                var val = prop.GetValue(dataContext);

                if (fieldPath.ChildPath.Length > 0)
                {
                    if (val == null)
                    {
                        // warn that we have a child path but a parent is null
                        log.Warn($"DataContext warning.  Child Path found [FullPath: {modelFieldName}] but current [Path: {fieldPath.Current}] is null.  Would not be able to find [child path: {fieldPath.ChildPath}] while parent is null.");
                    }
                    return getDataContextValue(val, fieldPath.ChildPath);
                }
                else
                {
                    return val;
                }
                
            }
        }
        
        private void setDataContextValue(object dataContext, string modelFieldName,  object val)
        {

            var fieldPath = new model.ModelFieldNamePathInfo(modelFieldName);

            if (fieldPath.ChildPath.Length > 0)
            {
                if (dataContext is lib.BindableDynamicDictionary dyncDict)
                {
                    // dynDict must contain this part of the path, because it's a path
                    if (!dyncDict.HasKey(fieldPath.Current))
                    {
                        throw new Exception($"Field [{fieldPath.Current}] does not exist on DataContext object");
                    }

                    var currentPathVal = dyncDict[fieldPath.Current];
                    setDataContextValue(currentPathVal, fieldPath.ChildPath, val); 
                }
                else
                {
                    Type dcType = dataContext.GetType();
                    var prop = dcType.GetProperty(fieldPath.Current);
                    if (prop == null)
                    {
                        throw new Exception(
                            $"Field [{fieldPath.Current}] does not exist on DataContext object of [type: {dcType.Name}]");
                    }

                    var currentPathVal = prop.GetValue(dataContext);
                    setDataContextValue(currentPathVal, fieldPath.ChildPath, val);
                }
            }
            else
            {
                // no child path so do the normal
                if( dataContext is lib.BindableDynamicDictionary dynDict){
                    dynDict[fieldPath.Current] = val;
                }else {
                    Type dcType = dataContext.GetType();
                    var prop = dcType.GetProperty(fieldPath.Current);
                    if (prop == null)
                    {
                        throw new Exception(
                            $"Field [{fieldPath.Current}] does not exist on DataContext object of [type: {dcType.Name}]");
                    }
                    prop.SetValue(dataContext, val);
                }
            }
            

        }


        /*
         This is used at least by the nac.Forms.Table project to get access to the List itemsource even if it's on the DataContext
         */
        public object getModelValue(string modelFieldName)
        {
            if (this.Model.HasKey(model.SpecialModelKeys.DataContext))
            {
                var dataContext = this.Model[SpecialModelKeys.DataContext];
                return getDataContextValue(dataContext, modelFieldName:  modelFieldName);
            }
            else
            {
                if (this.Model.HasKey(modelFieldName))
                {
                    return this.Model[modelFieldName];
                }
                else
                {
                    throw new Exception($"Form Model does not contain [key={modelFieldName}]");
                }
            }
        }


        public void setModelValue(string modelFieldName, object val)
        {
            if (this.Model.HasKey(model.SpecialModelKeys.DataContext))
            {
                var dataContext = this.Model[SpecialModelKeys.DataContext];
                setDataContextValue(dataContext, modelFieldName, val);
            }
            else
            {
                this.Model[modelFieldName] = val;
            }
        }
        
        
        private void AddBinding<T>(string modelFieldName,
            AvaloniaObject control,
            AvaloniaProperty<T> property,
            bool isTwoWayDataBinding = false)
        {
            // (ideas from here)[http://avaloniaui.net/docs/binding/binding-from-code]
            var bindingSource = new Subject<T>();
            control.Bind<T>(property, bindingSource.AsObservable());

            bool bindingIsDataContext = false;
            object dataContext = null;

            // does model contain a datacontext???
            if( this.Model.HasKey(SpecialModelKeys.DataContext)){
                // bind to the data context
                dataContext = this.Model[SpecialModelKeys.DataContext];

                if( dataContext is System.ComponentModel.INotifyPropertyChanged prop){
                    // It's INotifyPropertyChanged so set this as handled
                    bindingIsDataContext = true;

                    // need to fire it's current value.  Then start watching for changes
                    var currentValue = getDataContextValue(dataContext, modelFieldName);
                    log.Debug(
                        $"AddBinding-Model Value Change [*Initial* Field: {modelFieldName}; New Value: {currentValue}]");
                    FireOnNextWithValue<T>(bindingSource, currentValue );

                    prop.PropertyChanged += (_s,_args) => {
                        if( string.Equals(_args.PropertyName, modelFieldName, StringComparison.OrdinalIgnoreCase))
                        {
                            var newCurrentValue = getDataContextValue(dataContext, modelFieldName);
                            log.Debug($"AddBinding-Model Value Change [Field: {modelFieldName}; New Value: {currentValue}]");
                            FireOnNextWithValue<T>(bindingSource, newCurrentValue );
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
                    if( newVal == null)
                    {
                        // there is never a situation where the UI would need to make a model property null right???
                        return;
                    }

                    log.Debug($"AddBinding-TwoWay-Control Value Change [Control Property: {property.Name}; Field: {modelFieldName}; New Value: {newVal}]");
                    setModelValue(modelFieldName, newVal);
                });
            }
            
            
            // end of AddBinding
        }
        

        public void DisplayChildForm(Action<Form> setupChildForm, int height = 600, int width = 800,
            Func<Form,bool?> onClosing = null,
            Action<Form> onDisplay = null,
            bool useIsolatedModelForThisChildForm = false)
        {
            // default to use the parent's model, but some child will use a DataContext and need an isolated model
            var childFormModel = this.Model;
            if (useIsolatedModelForThisChildForm == true)
            {
                childFormModel = new BindableDynamicDictionary();
            }
            var childForm = new Form(this.app, childFormModel);

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


        public async Task InvokeAsync(Func<Task> codeToRun)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await codeToRun();
            });
        }
    }
}
