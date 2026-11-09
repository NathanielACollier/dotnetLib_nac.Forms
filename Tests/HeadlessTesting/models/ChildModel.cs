using System.Collections.ObjectModel;
using nac.ViewModelBase;

namespace Tests.HeadlessTesting.models
{
    public class ChildModel : ViewModelBase
    {
        public ObservableCollection<Location> Locations
        {
            get => GetValue(() => Locations);
            set => SetValue(() => Locations, value);
        }
    }
}
