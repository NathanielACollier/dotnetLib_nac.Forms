using nac.ViewModelBase;

namespace Tests.HeadlessTesting.models
{
    public class Parent : ViewModelBase
    {
        public Child MyChildProperty
        {
            get => GetValue(() => MyChildProperty);
            set => SetValue(() => MyChildProperty, value);
        }
    }
}
