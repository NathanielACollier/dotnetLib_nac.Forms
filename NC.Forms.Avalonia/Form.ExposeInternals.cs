using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;

namespace NC.Forms.Avalonia
{
    public partial class Form
    {
        /*
         * This file provides a place where methods can be put to Expose Internals, so that other libraries can extend the Formw ith new controls
         *    + Prefix the function names with _Extend_ to ensure that they are used as intended
         */
        
                
        // this is the public version of AddRowToHost
        public void _Extend_AddRowToHost(Control ctrl, AddRowToHostFunctions functions = null,
            bool rowAutoHeight = true)
        {
            AddRowToHost(ctrl: ctrl,
                functions: functions,
                ctrlIndex: null,
                rowAutoHeight : rowAutoHeight);
        }
        
        
        // public version of AddBinding
        public void _Extend_AddBinding<T>(string modelFieldName,
            AvaloniaObject control,
            AvaloniaProperty property,
            bool isTwoWayDataBinding = false)
        {
            AddBinding<T>(modelFieldName: modelFieldName,
                control: control,
                property: property,
                isTwoWayDataBinding: isTwoWayDataBinding);
        }
        
        
        
        
    }
}