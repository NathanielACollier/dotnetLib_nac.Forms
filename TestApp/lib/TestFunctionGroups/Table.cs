using System.Collections.ObjectModel;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public class Table
{
    public static void DisplayObservableCollection(Form f)
    {
        var people = new ObservableCollection<model.Person>
        {
            new model.Person
            {
                First = "George",
                Last = "Washington"
            },
            new model.Person
            {
                First = "John",
                Last = "Adams"
            }
        };
        f.Model["persons"] = people;
                
        f.Table<model.Person>("persons");

    }
}