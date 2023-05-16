using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            this.parentForm = _parentForm;
        }

        public Form(Form _parentForm, lib.BindableDynamicDictionary _model) : this(__app: _parentForm.app,
            _model: _model)
        {
            this.parentForm = _parentForm;
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

        private INotifyPropertyChanged getBindingSource()
        {
            // does model contain a datacontext???
            if (this.Model.HasKey(SpecialModelKeys.DataContext))
            {
                // bind to the data context
                var dataContext = this.Model[SpecialModelKeys.DataContext];

                if (dataContext is System.ComponentModel.INotifyPropertyChanged prop)
                {
                    return prop;
                }
                else
                {
                    /*
                     !!REMEMBER!! You can use the bindabledictionary
                     If you don't want to go to the trouble of implementing INotifyPropertyChanged just have your items use BindableDictionary and it's pretty easy
                     */
                    throw new Exception(
                        $"Special DataContext used in model, but DataContext is not INotifyPropertyChanged. Type given is {dataContext?.GetType().Name ?? "null"}.  There is no way to read properties without INotifyPropertyChanged");
                }
            }
            else // need to fire what it is now if there is anything there
            {
                return this.Model;
            }
        }


        private void notifyOnRootModelChange(string modelFieldName, CodeToRunOnModelChange codeToRunOnModelChange)
        {
            var bindingSource = getBindingSource();
            
            var valueResult = getDataContextValue(parentContext:null,
                datacontext: bindingSource,
                modelFieldName: modelFieldName);

            WatchPathForModelChanges(modelFieldName, valueResult);
            
            notifyOnModelChange(context: valueResult,
                codeToRunOnChange: codeToRunOnModelChange);
        }

        private void WatchPathForModelChanges(string modelFieldName, DataContextValueResult valueResult)
        {
            if (valueResult.ParentContext == null)
            {
                return; // no path if parent is null
            }
            
            /*
             We need to mnitor for changes on every part of the parent tree
             */
            var parentContext = valueResult.ParentContext;
            do
            {
                notifyOnModelChange(context: parentContext,
                    codeToRunOnChange: (context, value) =>
                    {
                        log.Info(
                            $"Parent of root path [{modelFieldName}] with current path [{context.FieldName}] and current field [{context.CurrentFieldName}] has value change to: [{value}]");
                    });

                parentContext = parentContext.ParentContext;
            } while (parentContext != null);
        }

        private delegate void CodeToRunOnModelChange(model.DataContextValueResult bindingSource, object value);

        private void notifyOnModelChange( model.DataContextValueResult context , CodeToRunOnModelChange codeToRunOnChange)
        {
            log.Info($"AddBinding-Initial value [property: {context.FieldName}; value: {context.Value}]");
            codeToRunOnChange(context, context.Value);

            context.DataContext.PropertyChanged += (_s, args) =>
            {
                if (!string.Equals(context.CurrentFieldName, args.PropertyName))
                {
                    return; // ignore this
                }
                
                var changedValue = getDataContextValue(parentContext:null,
                    datacontext: context.DataContext,
                    modelFieldName: args.PropertyName);

                if (changedValue.invalid)
                {
                    log.Warn($"PropertyChanged: Field: [{args.PropertyName}] is not valid for datacontext");
                    return;
                }
                
                log.Debug($"AddBinding-Model Value Change [Field: {context.FieldName}; New Value: {changedValue.Value}]");
                codeToRunOnChange(changedValue, changedValue.Value);
            };
        }
        
        
        private model.DataContextValueResult getDataContextValue(model.DataContextValueResult parentContext, INotifyPropertyChanged datacontext, string modelFieldName)
        {
            DataContextValueResult result = new();
            result.ParentContext = parentContext;
            result.DataContext = datacontext;
            result.FieldName = modelFieldName;
            result.CurrentFieldName = modelFieldName;
            
            if (datacontext == null)
            {
                result.invalid = true;
                log.Warn($"Error accessing [field: {modelFieldName}]. DataContext is null, or is not INotifyPropertyChanged");
                return result;
            }

            var fieldPath = new model.ModelFieldNamePathInfo(modelFieldName);
            result.CurrentFieldName = fieldPath.Current;

            if( datacontext is lib.BindableDynamicDictionary dynDict){
                if (!dynDict.HasKey(fieldPath.Current))
                {
                    result.invalid = true;
                    log.Warn($"Field [{fieldPath.Current}] does not exist on DataContext object");
                    return result;
                }
                
                result.Value = dynDict[fieldPath.Current];
                if (fieldPath.ChildPath.Length > 0)
                {
                    return getDataContextValue( parentContext: result,
                        datacontext: result.Value as INotifyPropertyChanged,
                        modelFieldName: fieldPath.ChildPath);
                }
                
                return result;
            }
            
            Type dcType = datacontext.GetType();
            var prop = dcType.GetProperty(fieldPath.Current);
            if (prop == null)
            {
                result.invalid = true;
                log.Warn($"Field [{fieldPath.Current}] does not exist on DataContext object of [type: {dcType.Name}]");
                return result;
            }
                
            result.Value = prop.GetValue(datacontext);

            if (fieldPath.ChildPath.Length > 0)
            {
                if (result.Value == null)
                {
                    result.invalid = true;
                    // warn that we have a child path but a parent is null
                    log.Warn($"DataContext warning.  Child Path found [FullPath: {modelFieldName}] but current [Path: {fieldPath.Current}] is null.  Would not be able to find [child path: {fieldPath.ChildPath}] while parent is null.");
                    return result;
                }
                
                return getDataContextValue(parentContext: result,
                    datacontext: result.Value as INotifyPropertyChanged,
                    modelFieldName: fieldPath.ChildPath);
            }

            return result;
        }
        
        private void setDataContextValue(INotifyPropertyChanged dataContext, string modelFieldName,  object val)
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

                    var currentPathVal = dyncDict[fieldPath.Current] as INotifyPropertyChanged;
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

                    var currentPathVal = prop.GetValue(dataContext) as INotifyPropertyChanged;
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
        public model.DataContextValueResult getModelValue(string modelFieldName)
        {
            var datacontext = getBindingSource();

            return getDataContextValue(parentContext: null,
                datacontext: datacontext,
                modelFieldName: modelFieldName);
        }


        public void setModelValue(string modelFieldName, object val)
        {
            var datacontext = getBindingSource();
            
            setDataContextValue(datacontext, modelFieldName, val);
        }


        public void setModelIfNull(string modelFieldName, object val)
        {
            var result = getModelValue(modelFieldName);

            if (result.invalid)
            {
                log.Warn($"Trying to set model field: [{modelFieldName}] but it is invalid for DataContext");
                return;
            }
            
            if (result.Value is null)
            {
                setDataContextValue(result.DataContext, modelFieldName, val);
            }
        }
        
        
        private void AddBinding<T>(string modelFieldName,
            AvaloniaObject control,
            AvaloniaProperty<T> property,
            bool isTwoWayDataBinding = false,
            Func<object, T> convertFromModelToUI = null,
            Func<T, object> convertFromUIToModel = null)
        {
            // (ideas from here)[http://avaloniaui.net/docs/binding/binding-from-code]
            var bindingSource = new Subject<T>();
            control.Bind<T>(property, bindingSource.AsObservable());

            notifyOnRootModelChange(modelFieldName, (context, val) =>
            {
                if (convertFromModelToUI != null)
                {
                    log.Debug($"ConvertFromModelToUI [model val: {val}]");
                    val = convertFromModelToUI(val);
                    log.Debug($"ConvertFromModelToUI [ui val: {val}]");
                }
                FireOnNextWithValue<T>(bindingSource, val);
            });
            
            // If they say two way then we setup a watch on the property observable and apply the values back to the model
            if(isTwoWayDataBinding)
            {
                // monitor for Property changes on control
                var controlValueChangesObservable = control.GetObservable(property);

                controlValueChangesObservable.Subscribe(newVal =>
                {
                    object objVal = newVal;
                    if (convertFromUIToModel != null)
                    {
                        objVal = convertFromUIToModel(newVal);
                        log.Debug($"ConvertFromUIToModel [ui val: {newVal}; converted val: {objVal}]");
                    }
                    if( objVal == null)
                    {
                        // there is never a situation where the UI would need to make a model property null right???
                        return;
                    }

                    log.Debug($"AddBinding-TwoWay-Control Value Change [Control Property: {property.Name}; Field: {modelFieldName}; New Value: {objVal}]");
                    setModelValue(modelFieldName, objVal);
                });
            }
            
            
            // end of AddBinding
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
        
        private async Task<bool> Display_Internal(int height, int width,
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
