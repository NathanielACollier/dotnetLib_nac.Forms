using System.Collections.ObjectModel;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public class TableView
{
    
    public static void SpecifiedColumnBinding(Form f)
    {
        var list = new ObservableCollection<model.Alphabet>();
        
        f.Model["list"] = list;

        f.HorizontalGroup(hg =>
            {
                hg.Text("X")
                    .TextBoxFor("X");
            })
            .Button("Add", async () =>
            {
                var newItem = new model.Alphabet();
                newItem.X = f.Model["X"] as string;
                list.Add(newItem);
            })
            .TableView<model.Alphabet>("list",
                columns: new[]
                {
                    new nac.Forms.model.Column
                    {
                        Header = "Duplicate of X",
                        modelBindingPropertyName = "X"
                    }
                }, style: "width:1600px;");
    }
    
    
}