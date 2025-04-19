using System.Collections.ObjectModel;

namespace nac.Forms.UITesterApp.model;

public class MainWindowModel : nac.ViewModelBase.ViewModelBase
{


    public ObservableCollection<model.TestEntry> MethodsList
    {
        get => GetValue(() => MethodsList);
    }


    public model.TestEntry SelectedMethod
    {
        get => GetValue(() => SelectedMethod);
        set => SetValue(() => SelectedMethod, value);
    }


    public ObservableCollection<model.LogViewerMessage> LogMessageList
    {
        get => GetValue(() => LogMessageList);
    }
    
}