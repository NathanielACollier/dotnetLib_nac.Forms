using nac.ViewModelBase;

namespace Tests.HeadlessTesting.models
{
    public class ParentModel : ViewModelBase
    {
        public ChildModel Selected
        {
            get => GetValue(() => Selected);
            set => SetValue(() => Selected, value);
        }
    }
}
