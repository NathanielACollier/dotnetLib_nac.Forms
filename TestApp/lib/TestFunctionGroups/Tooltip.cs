using System;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public class Tooltip
{

    public static void Text(Form f)
    {
        f.Text("Hello World", style: new nac.Forms.model.Style
        {
            TooltipText = "This is a text display that says 'Hello World!'"
        });
    }


    public static void ComplexForm(Form f)
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


}

