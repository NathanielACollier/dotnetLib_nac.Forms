using nac.Forms;
using nac.Forms.model;
using TestApp.model;

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
    
    
}