using nac.Forms;
using nac.Forms.model;

using log = TestApp.model.LogEntry;

namespace TestApp.lib.TestFunctionGroups;

public class Layout
{
    public static void HorizontalSplit(Form child)
    {
        child.HorizontalGroup(grp=> {
            grp.Text("Text to the Left")
                .Text("Text to the right");
        }, isSplit: true);
    }
    
    
    
    public static void VerticalSplit(Form child)
    {
        child.VerticalGroup(grp=> {
            grp.Text("Text Above")
                .Text("Text Below");
        }, isSplit: true);
    }
    
    
    public static void HorizontalSimple(Form child)
    {
        child.HorizontalGroup(hori =>
        {
            hori.Text("Click Count: ", style: new Style(){ width = 100})
                .TextBoxFor("clickCount")
                .Button("Click Me!", async () =>
                {
                    var current = child.Model.GetOrDefault<int>("clickCount", 0);
                    ++current;
                    hori.Model["clickCount"] = current;
                }, style: new Style(){width = 60});
        });
    }
    
    
    
    
    public static void VerticalSimple1(Form mainForm)
    {
        mainForm.HorizontalGroup((hgForm) =>
        {
            hgForm.VerticalGroup((vg1) =>
                {
                    vg1.Text("Here is a column of controls in a vertical group")
                        .Button("Click Me!", async ()=>
                        {
                            log.info("vg1 button click");
                        });
                })
                .VerticalGroup((vg2) =>
                {
                    vg2.Text("Here is a second column of controls")
                        .Button("Click me 2!!", async () =>
                        {
                            log.info("vg2 button click");
                        });
                });
        });
    }
    
    
    
    
    
}