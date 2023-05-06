using System.ComponentModel;

namespace nac.Forms.model;

public class DataContextValueResult
{
    public INotifyPropertyChanged DataContext;
    public object Value;
    public bool invalid;

    public DataContextValueResult()
    {
        this.invalid = false;
    }
}