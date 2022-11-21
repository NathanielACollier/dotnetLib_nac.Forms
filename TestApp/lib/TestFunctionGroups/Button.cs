using System;
using nac.Forms;
using nac.Forms.model;

namespace TestApp.lib.TestFunctionGroups;

public class Button
{
    private static void ClickCount(Form child)
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
    
    
    private class Model_ClickCountInButtonWithTypedDataContext : nac.Forms.model.ViewModelBase
    {
        public int Count
        {
            get { return GetValue(() => Count); }
            set { SetValue(() => Count, value);}
        }

        public int Count2
        {
            get { return GetValue(() => Count2); }
            set { SetValue(() => Count2, value);}
        }
    }
    
    public static void ClickCountInButtonWithTypedDataContext(Form f)
    {
        var model = new Model_ClickCountInButtonWithTypedDataContext();
        f.DataContext = model;

        f.HorizontalGroup(buttonRow =>
            buttonRow.Button(_b => _b
                        .HorizontalGroup(hg =>
                        {
                            hg.Text("Click (")
                                .TextFor(nameof(model.Count),
                                    style: new nac.Forms.model.Style
                                        { foregroundColor = Avalonia.Media.Colors.Red })
                                .Text(")");
                        }),
                    async () =>
                    {
                        ++model.Count;
                    }
                )
                .Button(_b => _b.HorizontalGroup(hg =>
                {
                    hg.Text(" Click-2 (")
                        .TextFor(nameof(model.Count2),
                            style: new Style { foregroundColor = Avalonia.Media.Colors.Green })
                        .Text(")");
                }), async () =>
                {
                    ++model.Count2;
                })
        ); // end of horizontal group button row
    }
    
    
}