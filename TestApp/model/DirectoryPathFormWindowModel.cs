namespace TestApp.model;

public class DirectoryPathFormWindowModel: nac.ViewModelBase.ViewModelBase
{
    public string pathWithoutBeingInit
    {
        get { return GetValue(() => pathWithoutBeingInit); }
        set { SetValue(() => pathWithoutBeingInit, value); }
    }

    public string myPath
    {
        get { return GetValue(() => myPath); }
        set { SetValue(() => myPath, value);}
    }
    
}