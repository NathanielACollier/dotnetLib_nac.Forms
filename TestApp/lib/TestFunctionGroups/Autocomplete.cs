using System.Collections.ObjectModel;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public class Autocomplete
{
    public static void StaticStringList(Form f)
    {
        var items = new ObservableCollection<string>(new[]
        {
            "New York",
            "Chicago",
            "Cupertino",
            "Sacramento"
        });

        f.Model["items"] = items;

        f.Autocomplete<string>(itemSourceModelName: "items",
                selectedItemModelName: "i")
            .HorizontalGroup(h =>
            {
                h.Text("Selected: ")
                    .TextFor("i");
            });
    }
    
    
    
    
    
}