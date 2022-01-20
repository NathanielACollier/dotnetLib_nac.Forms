namespace TestApp.model;

public class DirectoryPathFormWindowModel: nac.Forms.model.ViewModelBase
{
    public string myPath
    {
        get { return GetValue(() => myPath); }
        set { SetValue(() => myPath, value);}
    }
    
}