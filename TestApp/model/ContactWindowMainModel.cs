using System.Collections.ObjectModel;

namespace TestApp.model
{
    public class ContactWindowMainModel : nac.Forms.model.ViewModelBase
    {

        public model.Contact Contact
        {
            get { return GetValue(() => Contact); }
            set { SetValue(() => Contact, value);}
        }

        public string Results
        {
            get { return GetValue(() => Results);  }
            set { SetValue(() => Results, value);}
        }


        public ObservableCollection<model.Contact> savedContacts
        {
            get { return GetValue(() => savedContacts); }
            set { SetValue(() => savedContacts, value); }
        }

        public ContactWindowMainModel()
        {
            Contact = new model.Contact();
            Results = "";
            savedContacts = new ObservableCollection<Contact>();
        }
    }
}