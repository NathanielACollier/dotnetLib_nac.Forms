using nac.Forms;
using nac.Forms.model;

namespace TestApp.lib.TestFunctionGroups;

public class ContextMenu
{
    public static void Text(Form f)
    {
        f.Text("Right click me", style: new Style
        {
            contextMenu = (_c) =>
            {
                _c.Text("Hello World!");
            }
        });
    }
    
    
    public static void CustomUI(Form f)
    {
        f.Button("Right Click Me!", onClick:async()=>{}, style: new Style
        {
            contextMenu = (_c) =>
            {
                _c.HorizontalGroup(h =>
                {
                    h.Text("GO")
                        .Button("Click Me", async () => { });
                });
            }
        });
    }


    public static void MenuItems(Form f)
    {
        f.Model["counter"] = 0;
        
        f.HorizontalGroup(h =>
            {
                h.Text("Count is: ")
                    .TextFor("counter");
            })
            .Button("Right Click Me!", onClick: async () => { }, style: new Style
            {
                contextMenuItems = new[]
                {
                    new MenuItem
                    {
                        Header = "Testing...",
                        Action = () =>
                        {
                            f.Model["counter"] = (int)f.Model["counter"] + 1;
                        }
                    }
                }
            });
    }
}