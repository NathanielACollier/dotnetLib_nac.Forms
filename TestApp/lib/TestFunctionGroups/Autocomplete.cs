using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using nac.Forms;
using nac.Forms.model;

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
    
    
    
    public static void AsyncPopulator_SimpleRandomText(Form f)
    {
        f.Autocomplete<string>(selectedItemModelName: "i",
                populateItemsOnTextChange: async (textboxValue) =>
                {
                    var n = new Random().Next(1000, 9999);
                    return new[]
                    {
                        textboxValue + n.ToString()
                    };
                })
            .HorizontalGroup(h =>
            {
                h.Text("Selected: ")
                    .TextFor("i");
            });
    }


    public static void AsyncPopulator_ModelWithTemplate(Form f)
    {
        var dataList = new[]
        {
            new model.Alphabet {A = "Spider Man", B = "New York"},
            new model.Alphabet {A = "Bat Man", B = "Gotham"},
            new model.Alphabet {A = "Super Man", B = "Smallville"},
            new model.Alphabet {A = "Clark Kent", B = "Metropollis"}
        };

        f.Model["i"] = new model.Alphabet();

        f.Autocomplete<model.Alphabet>(selectedItemModelName: "i",
                selectedTextModelName: "B",
                populateItemsOnTextChange: async (text) =>
                {
                    return dataList.Where(row => row.A.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                                                 row.B.Contains(text, StringComparison.OrdinalIgnoreCase)
                    );
                },
                populateItemRow: r =>
                {
                    r.HorizontalGroup(hg => hg.Text("Name: ", style: new Style { foregroundColor = Colors.Blue })
                        .TextFor("A")
                        .Text("  City: ", style: new Style { foregroundColor = Colors.Blue }).TextFor("B"));
                })
            .HorizontalGroup(h =>
            {
                h.Text("Selected: ")
                    .TextFor("i.A");
            })
            .HorizontalGroup(h =>
            {
                h.Text("i.B: ")
                    .TextBoxFor("i.B");
            });
    }
    
    
    
}