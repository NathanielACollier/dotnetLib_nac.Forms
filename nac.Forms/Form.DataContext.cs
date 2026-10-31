using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Reactive;
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
        /*
         valueResult is the resolution of modelFieldName.  It may currently be
         invalid -- a parent somewhere in the path is null, eg. a nested bind like
         "SelectedBackup.BackupLocations" when SelectedBackup has not been set yet,
         or the DataContext is not INotifyPropertyChanged -- but that is NOT a
         permanent error: the parent can be set later, which heals the whole
         remaining path.  So always (re)watch this frame instead of giving up.
         notifyOnModelChange handles flat and nested paths uniformly by watching
         the frame's CurrentFieldName and re-resolving any remaining
         RelativeBindingPath under the new value.
         */
        notifyOnModelChange(context: valueResult,
            codeToRunOnChange: codeToRunOnModelChange);
    }

    private delegate void CodeToRunOnModelChange(model.DataContextValueResult bindingSource, object value);

    private void notifyOnModelChange(model.DataContextValueResult context, CodeToRunOnModelChange codeToRunOnChange)
    {
        /*
         A binding path like "SelectedBackup.BackupLocations" resolves, at build
         time, into a chain of single frames (context is the DEEPEST / terminal
         frame, reached via getDataContextValue which links each frame to the one
         above it with ParentContext):
           frame A: CurrentFieldName="SelectedBackup",  RelativeBindingPath="BackupLocations" (parentContext == null)
           frame B (terminal): CurrentFieldName="BackupLocations", RelativeBindingPath=""  (parentContext == A)
         When a parent is null at build time (SelectedBackup == null), frame B has
         DataContext == null -- nothing to watch -- and frame A is still
         un-subscribed, so the original code (which only watched the terminal
         frame and bailed on null, with a dead "RelativeBindingPath carry-over")
         never observed "SelectedBackup" changing.  That is why a binding whose
         parent starts null never healed.

         The fix: walk UP from the terminal frame.  For each frame whose
         DataContext is non-null, subscribe a PropertyChanged handler on that
         frame's object for its own CurrentFieldName.  When that property
         changes, re-resolve the whole remaining path to a fresh terminal frame,
         re-subscribe the newly built sub-chain, and push the new terminal value
         through codeToRunOnChange.  Walking up means an ancestor (e.g. the
         "SelectedBackup" frame, whose object is the binding source) is always
         subscribed, so a parent healing from null re-resolves the deeper
         (previously null) frames and keeps them watched.
         */

        log.Info($"AddBinding-Initial value [property: {context.RelativeBindingPath}; value: {context.Value}]");
        codeToRunOnChange(context, context.Value);

        /*
         Subscribe every frame of the chain that currently has an object, deepest
         first.  For "SelectedBackup.BackupLocations" with the parent null at
         build time the terminal frame has DataContext == null, so only
         "SelectedBackup" (whose object is the binding source) gets subscribed.
         When it later heals the handler below re-resolves the path and subscribes
         the newly built frames (including the new terminal one) so the control
         and every deeper property keep updating.
         */
        var seenForThisBinding = new System.Collections.Generic.HashSet<object>();
        subscribeChain(context, codeToRunOnChange, seenForThisBinding);
    }

    /*
     Subscribes a PropertyChanged handler on each frame of the chain starting at
     [frame] (deepest) and walking up via ParentContext, for every frame whose
     DataContext is non-null.  Each subscription, when that frame's own
     CurrentFieldName changes, (1) re-resolves the remaining path to a fresh
     terminal frame, (2) re-subscribes that whole sub-chain, and (3) pushes the
     new terminal value through [codeToRunOnChange].

     [seen] dedupes subscriptions per binding so an object that sits in the chain
     more than once (or is otherwise re-encountered during a heal) is only
     subscribed once -- this prevents duplicate update callbacks and runaway
     resubscription when nested properties fire.
     */
    private void subscribeChain(model.DataContextValueResult frame,
        CodeToRunOnModelChange codeToRunOnChange,
        System.Collections.Generic.HashSet<object> seen)
    {
        while (frame != null)
        {
            if (frame.DataContext != null)
            {
                // dedupe: only subscribe this object once for this binding
                if (seen.Add(frame.DataContext))
                {
                    var thisFrame = frame;

                    thisFrame.DataContext.PropertyChanged += (_s, args) =>
                    {
                        // a frame only reacts to its own property changing
                        if (!string.Equals(thisFrame.CurrentFieldName, args.PropertyName))
                        {
                            return;
                        }

                        /*
                         This frame's property changed.  Re-resolve the remaining
                         path -- the property that just changed plus whatever is
                         still left on this frame -- from this frame's object down
                         to a fresh terminal frame.  This rebuilds every deeper
                         frame (including ones that were null at build time), so
                         the chain "heals".
                         */
                        var remainingPath = string.IsNullOrEmpty(thisFrame.RelativeBindingPath)
                                ? args.PropertyName
                                : $"{args.PropertyName}.{thisFrame.RelativeBindingPath}";

                        var newChain = getDataContextValue(parentContext: null,
                            datacontext: thisFrame.DataContext,
                            modelFieldName: remainingPath);

                        // Re-subscribe the freshly built sub-chain, including its
                        // terminal frame, so a later change to the newly healed
                        // value also propagates.  Share [seen] so we never
                        // resubscribe objects we already have a handler for.
                        subscribeChain(newChain, codeToRunOnChange, seen);

                        log.Debug(
                            $"AddBinding-Model Value Change [Field: {remainingPath}; FieldName: {thisFrame.CurrentFieldName}; New Value: {newChain.Value}]");

                        // push the freshly resolved terminal value to the control.
                        codeToRunOnChange(newChain, newChain.Value);
                    };
                }
            }

            frame = frame.ParentContext; // move to the next frame up the chain
        }
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

        if (datacontext is nac.utilities.BindableDynamicDictionary dynDict)
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

            /*
             The parent of the remaining child path is null.  This is not a
             permanent error -- a parent such as "SelectedBackup" can be null for
             a while and then become non-null.  Link to the parent so that the
             watcher can re-resolve this path (and heal it) once the parent comes
             back, instead of treating the whole subtree as dead.
             */
            result.ParentContext = parentContext;

            // warn that we have a child path but a parent is null
            log.Warn(
                $"DataContext warning.  Child Path found [FullPath: {modelFieldName}] but current [Path: {fieldPath.Current}] is null.  Would not be able to find [child path: {fieldPath.ChildPath}] while parent is null.  Path will be re-resolved when the parent changes.");
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
            if (dataContext is nac.utilities.BindableDynamicDictionary dyncDict)
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
            if (dataContext is nac.utilities.BindableDynamicDictionary dynDict)
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
        var bindingSource = new nac.Forms.Reactive.Subject<T>();
        control.Bind<T>(property, bindingSource);

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

            controlValueChangesObservable.Subscribe(new AnonymousObserver<T>( newVal =>
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
            }));
        }


        // end of AddBinding
    }
}