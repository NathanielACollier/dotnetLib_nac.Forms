using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using nac.Forms;
using nac.Forms.lib;
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
                child.Model["NewItem"] = nac.Forms.lib.BindableDynamicDictionary.From(new
                {
                    Prop1 = "Frog Prince"
                });

                hgChild.Text("Prop1: ")
                    .TextBoxFor("NewItem.Prop1")
                    .Button("Add Item", async () =>
                    {
                        newItem = new nac.Forms.lib.BindableDynamicDictionary();
                        newItem["Prop1"] = child.Model.GetAsDict("NewItem")["Prop1"] as string;
                        items.Add(newItem);
                    });
            });

        lib.UIElementsUtility.logViewer(child);
        log.info("App Ready to go");
    }
    
    
    
    
    public static void JustStrings(Form f)
    {
        var items = new ObservableCollection<string>();
        new[] {"Walnut", "Peanut", "Cashew"}.ToList().ForEach(x=> items.Add(x));
        f.Model["myList"] = items;

        f.Text("This is a list of strings")
            .List<string>(itemSourcePropertyName: "myList",
                onSelectionChanged: (selectedItems) =>
                {
                    log.info("You selected: " + string.Join(";", selectedItems));
                });

        lib.UIElementsUtility.logViewer(f);
    }
    
    
    
    public static void ModifyUIInThread(Form f)
    {
        var list = new ObservableCollection<model.TestList_ButtonCounterExample_ItemModel>();

        f.Model["entries"] = list;

        f.HorizontalGroup(h =>
        {
            h.Text("My List")
                .Button("Add", async () =>
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(200); // cause a delay
                        f.InvokeAsync(async () =>
                        {
                            list.Add(new model.TestList_ButtonCounterExample_ItemModel()
                            {
                                Label = Guid.NewGuid().ToString("N")
                            });
                        });
                    });
                });
        }).List<model.TestList_ButtonCounterExample_ItemModel>(itemSourcePropertyName: "entries",
            populateItemRow: myRow =>
            {
                myRow.HorizontalStack(h =>
                {
                    h.Text("Label: ")
                        .TextBoxFor("Label");
                });
            });
    }



    private static void WrapPanel(Form f)
    {
        var items = new ObservableCollection<BindableDynamicDictionary>
        {
            BindableDynamicDictionary.From(new
            {
                Text = "Alpha"
            }),
            BindableDynamicDictionary.From(new
            {
                Text = "Bravo"
            }),
            BindableDynamicDictionary.From(new
            {
                Text = "Charlie"
            }),
            BindableDynamicDictionary.From(new
            {
                Text = "Delta"
            })
        };
        f.Model["items"] = items;

        f.HorizontalGroup(hg =>
        {
            hg.Text("You choose: ", style: "color:red;")
                .TextBoxFor("current");
        }).List<BindableDynamicDictionary>("items", row =>
        {
            row.Button(btn => btn.TextFor("Text"), onClick: async () =>
            {
                f.Model["current"] = (row.DataContext as BindableDynamicDictionary)["Text"];
            });
        }, wrapContent: true);
    }
    
    
}