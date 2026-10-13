namespace TestApp.model;

public class BindableString : nac.ViewModelBase.ViewModelBase
{
    public string Value
    {
        get => GetValue(() => Value);
        set => SetValue(() => Value, value);
    }

    public BindableString(string val)
    {
        this.Value = val;
    }
    
}