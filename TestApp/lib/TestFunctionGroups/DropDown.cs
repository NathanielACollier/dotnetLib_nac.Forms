using System.Collections.ObjectModel;
using nac.Forms;
using nac.Forms.model;

using log = TestApp.model.LogEntry;

namespace TestApp.lib.TestFunctionGroups;

public class DropDown
{
    public static void SimpleTextSelection(Form f)
    {
        var items = new ObservableCollection<string>();
        f.Model["items"] = items;
        items.Add("Bird Feeder");
            
        // test swapping out model with this second list
        var items2 = new ObservableCollection<string>();
        items2.Add("Canik TP9");
        items2.Add("Beretta M9");
        items2.Add("Remington 870");

        f.VerticalStack(vg =>
        {
            vg.HorizontalStack(h =>
                {
                    h.Text("New Item Text: ")
                        .TextBoxFor("newItemText", style: new Style(){width = 100})
                        .Button("Add", async () =>
                        {
                            var _curItems = f.Model["items"] as ObservableCollection<string>;
                            _curItems.Add(f.Model["newItemText"] as string);
                        })
                        .Button("Swap Lists", async () =>
                        {
                            if (f.Model["items"] == items)
                            {
                                f.Model["items"] = items2;
                            }
                            else
                            {
                                f.Model["items"] = items;
                            }
                        });
                }).DropDown<string>(itemSourceModelName: "items",
                    selectedItemModelName: "selected")
                .HorizontalStack(h =>
                {
                    h.Text("You have selected: ")
                        .TextFor("selected");
                });
        });
    }
    
    
    
    
    public static void DataContext_SelectedItemBinding(Form f)
    {
        // setup Model
        var m = new model.DropDown_DataContext_BindSelectedItem_MainWindowModel();
        f.DataContext = m;
            
        foreach (var __c in new model.Contact[]
                 {
                     new model.Contact { DisplayName = "Mike Brown", Email = "mike.brown@google.com" }, 
                     new model.Contact{ DisplayName = "Jamie Joe", Email = "jamie.joe@google.com"},
                     new model.Contact{ DisplayName = "Lisa Paige", Email = "lisa.paige@google.com"}
                 })
        {
            m.ContactList.Add(__c);
        }
            
        // setup UI
        f.DropDown<model.Contact>(
                itemSourceModelName: nameof(model.DropDown_DataContext_BindSelectedItem_MainWindowModel
                    .ContactList),
                selectedItemModelName: nameof(model.DropDown_DataContext_BindSelectedItem_MainWindowModel
                    .SelectedContact),
                onSelectionChanged: (_c) =>
                {
                    log.info($"You selected {_c?.DisplayName}");
                }, populateItemRow: r =>
                {
                    r.HorizontalGroup(h => h.Text("DisplayName: ").TextFor("DisplayName"));
                })
            .Panel<model.Contact>(modelFieldName: "SelectedContact", populatePanel: p =>
            {
                p.HorizontalGroup(h => h.Text("Selected Contact: ").TextFor("DisplayName"));
            });
    }
    
    
    
    
    
}