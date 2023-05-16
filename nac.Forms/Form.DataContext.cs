using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using nac.Forms.model;

namespace nac.Forms;

public partial class Form
{
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

        var valueResult = getDataContextValue(parentContext: null,
            datacontext: bindingSource,
            modelFieldName: modelFieldName);

        WatchPathForModelChanges(modelFieldName, valueResult, codeToRunOnModelChange);
    }

    private void WatchPathForModelChanges(string modelFieldName, 
                                    DataContextValueResult valueResult,
                                    CodeToRunOnModelChange codeToRunOnModelChange)
    {
        if (valueResult.ParentContext == null)
        {
            log.Info($"WatchPath - {valueResult.CurrentFieldName} - new value: {valueResult.Value}");
            notifyOnModelChange(context: valueResult,
                codeToRunOnChange: codeToRunOnModelChange);
            return; // get out
        }

        // there was a parent context so it has to be watched for model changes
        
        /*
         We need to mnitor for changes on every part of the parent tree
         */
        var parentContext = valueResult.ParentContext;
        
        notifyOnModelChange(context: parentContext,
            codeToRunOnChange: (context, value) =>
            {
                log.Info(
                    $"WatchPath - Parent of root path [{modelFieldName}] with target path [{context.RelativeBindingPath}] and current field [{context.CurrentFieldName}] has value change to: [{value}]");

                var childContext = getDataContextValue(parentContext: null,
                    datacontext: value as INotifyPropertyChanged,
                    modelFieldName: context.RelativeBindingPath);
                WatchPathForModelChanges(context.RelativeBindingPath, childContext, codeToRunOnModelChange);
            });
    }

    private delegate void CodeToRunOnModelChange(model.DataContextValueResult bindingSource, object value);

    private void notifyOnModelChange(model.DataContextValueResult context, CodeToRunOnModelChange codeToRunOnChange)
    {
        log.Info($"AddBinding-Initial value [property: {context.RelativeBindingPath}; value: {context.Value}]");
        codeToRunOnChange(context, context.Value);

        context.DataContext.PropertyChanged += (_s, args) =>
        {
            if (!string.Equals(context.CurrentFieldName, args.PropertyName))
            {
                return; // ignore this
            }

            var changedValue = getDataContextValue(parentContext: null,
                datacontext: context.DataContext,
                modelFieldName: args.PropertyName);

            if (changedValue.invalid)
            {
                log.Warn($"PropertyChanged: Field: [{args.PropertyName}] is not valid for datacontext");
                return;
            }
            
            // save the path that we are trying to get to, this is for properties that change in a path we want to be able to keep monitoring for sub properties
            changedValue.RelativeBindingPath = context.RelativeBindingPath;

            log.Debug($"AddBinding-Model Value Change [Field: {context.RelativeBindingPath}; FieldName: {context.CurrentFieldName}; New Value: {changedValue.Value}]");
            codeToRunOnChange(changedValue, changedValue.Value);
        };
    }


    private model.DataContextValueResult getDataContextValue(model.DataContextValueResult parentContext,
        INotifyPropertyChanged datacontext, string modelFieldName)
    {
        DataContextValueResult result = new();
        result.ParentContext = parentContext;
        result.DataContext = datacontext;

        var fieldPath = new model.ModelFieldNamePathInfo(modelFieldName);
        result.CurrentFieldName = fieldPath.Current;
        result.RelativeBindingPath = fieldPath.ChildPath;

        if (datacontext == null)
        {
            result.invalid = true;
            log.Warn(
                $"Error accessing [field: {modelFieldName}]. DataContext is null, or is not INotifyPropertyChanged");
            return result;
        }

        if (datacontext is lib.BindableDynamicDictionary dynDict)
        {
            if (!dynDict.HasKey(fieldPath.Current))
            {
                result.invalid = true;
                log.Warn($"Field [{fieldPath.Current}] does not exist on DataContext object");
                return result;
            }

            result.Value = dynDict[fieldPath.Current];
            if (fieldPath.ChildPath.Length > 0)
            {
                return getDataContextValue(parentContext: result,
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
                log.Warn(
                    $"DataContext warning.  Child Path found [FullPath: {modelFieldName}] but current [Path: {fieldPath.Current}] is null.  Would not be able to find [child path: {fieldPath.ChildPath}] while parent is null.");
                return result;
            }

            return getDataContextValue(parentContext: result,
                datacontext: result.Value as INotifyPropertyChanged,
                modelFieldName: fieldPath.ChildPath);
        }

        return result;
    }

    private void setDataContextValue(INotifyPropertyChanged dataContext, string modelFieldName, object val)
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
            if (dataContext is lib.BindableDynamicDictionary dynDict)
            {
                dynDict[fieldPath.Current] = val;
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
        if (isTwoWayDataBinding)
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

                if (objVal == null)
                {
                    // there is never a situation where the UI would need to make a model property null right???
                    return;
                }

                log.Debug(
                    $"AddBinding-TwoWay-Control Value Change [Control Property: {property.Name}; Field: {modelFieldName}; New Value: {objVal}]");
                setModelValue(modelFieldName, objVal);
            });
        }


        // end of AddBinding
    }
}