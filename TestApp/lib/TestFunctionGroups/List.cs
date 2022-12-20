using System.Linq;
using nac.Forms;
using nac.Forms.model;
using TestApp.model;

using log = TestApp.model.LogEntry;

namespace TestApp.lib.TestFunctionGroups;

public class List
{
    
    
    public static void ButtonCounter(Form child)
    {   
        var items = new System.Collections.ObjectModel.ObservableCollection<TestList_ButtonCounterExample_ItemModel>();

        // display 5 counters
        for( int i = 0; i < 10; ++i){
            items.Add(new TestList_ButtonCounterExample_ItemModel{
                Counter = 0,
                Label = $"Counter {i}"
            });
        }

        child.Model["items"] = items;
        child.List<TestList_ButtonCounterExample_ItemModel>("items", row=>{
                
            row.HorizontalGroup(hg=>{
                hg.TextFor("Label")
                    .Button("Next", async ()=>{
                        var model = row.Model[SpecialModelKeys.DataContext] as TestList_ButtonCounterExample_ItemModel;
                        ++model.Counter;
                    })
                    .Text("Counter is: ")
                    .TextFor("Counter");
            });
        });
    }
    
    
    
    public static void ItemsControlSimple(Form child)
    {
        var items = new System.Collections.ObjectModel.ObservableCollection<nac.Forms.lib.BindableDynamicDictionary>();
        child.Model["items"]  = items;
        var newItem = new nac.Forms.lib.BindableDynamicDictionary();
        newItem["Prop1"] = "fish";
            
        items.Add(newItem);
        newItem = new nac.Forms.lib.BindableDynamicDictionary();
        newItem["Prop1"] = "Blanket";
        items.Add(newItem);

        child.Text("Simple List")
            .List<nac.Forms.lib.BindableDynamicDictionary>("items", (itemForm) =>
            {
                itemForm.TextFor("Prop1");
            }, style: new Style()
            {
                height = 500,
                width = 300,
                backgroundColor = Avalonia.Media.Colors.Aquamarine
            }, onSelectionChanged: (_selectedEntries) =>
            {
                log.info($"New items selected: {string.Join(",", _selectedEntries.Select(m=>m["Prop1"] as string))}");
            })
            .HorizontalGroup((hgChild) =>
            {
                // default some stuff
                child.Model["NewItem.Prop1"] = "Frog Prince";

                hgChild.Text("Prop1: ")
                    .TextBoxFor("NewItem.Prop1")
                    .Button("Add Item", async () =>
                    {
                        newItem = new nac.Forms.lib.BindableDynamicDictionary();
                        newItem["Prop1"] = child.Model["NewItem.Prop1"] as string;
                        items.Add(newItem);
                    });
            });

        lib.UIElementsUtility.logViewer(child);
        log.info("App Ready to go");
    }
    
    
    
    
    
    
}