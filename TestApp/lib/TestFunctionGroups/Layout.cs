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
    
    
    
    
    public static void VerticalSimple(Form mainForm)
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
    
    
    
    public static void VerticalDockSimple(Form mainForm)
    {
        mainForm.HorizontalGroup((hgForm) =>
        {
            hgForm.VerticalDock((vg1) =>
                {
                    vg1.Text("Here is a column of controls in a vertical group")
                        .Button("Click Me!", async ()=>
                        {
                            log.info("vg1 button click");
                        });
                })
                .VerticalDock((vg2) =>
                {
                    vg2.Text("Here is a second column of controls")
                        .Button("Click me 2!!", async () =>
                        {
                            log.info("vg2 button click");
                        });
                });
        });
    }
    
    
    
    public static void orizontalGroupVisibility(Form mainForm)
    {
        mainForm.Model["isTextVisible"] = false;

        mainForm.HorizontalGroup(hg =>
            {
                hg.HorizontalGroup(hideableHG =>
                    {
                        hideableHG.Text("This text is visible");
                    }, style: new Style()
                    {
                        isVisibleModelName = "isHoriVis"
                    })
                    .Button("Hide/show ME!", async () =>
                    {
                        mainForm.Model["isHoriVis"] = !(mainForm.Model["isHoriVis"] as bool? ?? true);
                    }, style: new Style(){width = 120});
            }, style: new Style()
            {
                isVisibleModelName = "isTextVisible"
            } )
            .Button("Show or Hide Text", async () =>
            {
                mainForm.Model["isTextVisible"] = !(mainForm.Model["isTextVisible"] as bool? ?? true);
            });
    }
    
    
    public static void VerticalGroupVisibility(Form f)
    {
        f.VerticalGroup(vg =>
            {
                vg.Text("I'm Visible");
            }, style:new Style()
            {
                height = 50,
                isVisibleModelName = "isDisplay"
            })
            .Button("Hide or Show", async () =>
            {
                f.Model["isDisplay"] = !(f.Model["isDisplay"] as bool? ?? true);
            }, style: new Style(){width = 100});
    }
    
    
    
    
    
}