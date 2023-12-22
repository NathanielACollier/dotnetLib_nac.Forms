namespace TestApp.model
{
    public class Contact : nac.ViewModelBase.ViewModelBase
    {
        public string DisplayName
        {
            get { return GetValue(() => DisplayName); }
            set { SetValue(() => DisplayName, value); }
        }

        public string Email
        {
            get { return GetValue(() => Email); }
            set { SetValue(() => Email, value);}
        }
        
    }
}