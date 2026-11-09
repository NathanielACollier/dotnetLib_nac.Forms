using nac.ViewModelBase;

namespace Tests.HeadlessTesting.models
{
    public class CounterModel : ViewModelBase
    {
        public int Counter
        {
            get => GetValue(() => Counter);
            set => SetValue(() => Counter, value);
        }
    }
}
