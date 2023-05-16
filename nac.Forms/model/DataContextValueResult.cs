using System.ComponentModel;

namespace nac.Forms.model;

public class DataContextValueResult
{
    public INotifyPropertyChanged DataContext;
    public object Value;
    public string TargetBindingPath;
    public bool invalid;
    public DataContextValueResult ParentContext { get; set; }
    public string CurrentFieldName { get; set; }
    public string RelativeBindingPath { get; set; }

    public DataContextValueResult()
    {
        this.invalid = false;
    }

    
}