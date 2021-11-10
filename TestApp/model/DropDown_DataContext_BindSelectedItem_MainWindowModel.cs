using System.Collections.ObjectModel;

namespace TestApp.model
{
    public class DropDown_DataContext_BindSelectedItem_MainWindowModel : nac.Forms.model.ViewModelBase
    {
        public ObservableCollection<model.Contact> ContactList
        {
            get { return GetValue(() => ContactList); }
            set { SetValue(() => ContactList, value);}
        }

        public model.Contact SelectedContact
        {
            get { return GetValue(() => SelectedContact); }
            set { SetValue(() => SelectedContact, value);}
        }

        public DropDown_DataContext_BindSelectedItem_MainWindowModel()
        {
            this.ContactList = new ObservableCollection<Contact>();
        }
    }
}