namespace TestApp.model
{
    public class DataContext_HelloWorld: nac.Forms.model.ViewModelBase
    {
        public string Message
        {
            get { return GetValue(() => Message); }
            set { SetValue(() => Message, value);}
        }


        public bool Loading
        {
            get { return GetValue(() => Loading); }
            set { SetValue(() => Loading, value);}
        }


        public int myCounter
        {
            get { return GetValue(() => myCounter); }
            set { SetValue(() => myCounter, value);}
        }
    }
}