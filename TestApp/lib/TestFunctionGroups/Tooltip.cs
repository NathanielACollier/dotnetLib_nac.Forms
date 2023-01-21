using System;
using nac.Forms;
using nac.Forms.model;

namespace TestApp.lib.TestFunctionGroups;

public class Tooltip
{

    public static void Text_Text(Form f)
    {
        f.Text("Hello World", style: new nac.Forms.model.Style
        {
            TooltipText = "This is a text display that says 'Hello World!'"
        });
    }


    public static void Text_ComplexForm(Form f)
    {
        f.Model["count"] = 0;
        f.Text("Hello World", style: new nac.Forms.model.Style
        {
            Tooltip = _tip => _tip.Text("Baloon", style: new nac.Forms.model.Style { foregroundColor = Avalonia.Media.Colors.Blue })
                                .Button(_b => _b.Text("Count=").TextFor("count"), async () =>
                                {
                                    f.Model["count"] = Convert.ToInt32(f.Model["count"]) + 1;
                                })
        });
    }


    public static void Button_Text(Form f)
    {
        f.Button("Click", async () =>
        {

        }, style: new Style { TooltipText = "This is a simple button that you can click, and does nothing" });
    }

    public static void ButtonComplexWithEmbdedCount_Text(Form f)
    {
        f.Model["count"] = 0;
        f.Button(_b => _b.Text("Count=").TextFor("count"), async () =>
        {
            f.Model["count"] = Convert.ToInt32(f.Model["count"]) + 1;
        }, style: new Style { TooltipText = "The text tooltip also works on a complex button" });
    }


}

