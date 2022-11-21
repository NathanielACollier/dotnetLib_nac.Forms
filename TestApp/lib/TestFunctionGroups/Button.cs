using System;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public static class Button
{
    private static void ButtonWithClickCount(Form child)
    {
        child.TextFor("txt1", "When you click button I'll change to count!")
            .Button("Click Me!", async () =>
            {
                var current = child.Model.GetOrDefault<int>("txt1", 0);
                ++current;
                child.Model["txt1"] = current;
            });
    }
    
    private static void ClickCountInButton(Form f)
    {
        f.Model["count"] = 0;
        f.Button(_b => _b.HorizontalGroup(hg =>
            {
                hg.Text("Click (")
                    .TextFor("count",
                        style: new nac.Forms.model.Style { foregroundColor = Avalonia.Media.Colors.Red })
                    .Text(")");
            }),
            async () =>
            {
                f.Model["count"] = Convert.ToInt32(f.Model["count"]) + 1;
            });
    }
}