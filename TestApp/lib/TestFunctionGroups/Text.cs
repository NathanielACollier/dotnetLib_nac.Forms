using Avalonia.Media;
using nac.Forms;
using nac.Forms.model;

using log = TestApp.model.LogEntry;

namespace TestApp.lib.TestFunctionGroups;

public class Text
{
    
    
    public static void DisplayWhatIsTyped(Form child)
    {
        child.TextFor("txt2", "Type here")
            .TextBoxFor("txt2");
    }
    
    
    public static void TextBoxMultiline(Form f)
    {
        f.VerticalGroup(vg =>
        {
            vg.Text("Text above the Textbox", new Style(){height=20})
                .TextBoxFor("message", multiline: true)
                .Text("Text below the textbox", new Style(){height = 20});
        }, isSplit: true);
    }
    
    
    public static void TextBlock_BasicFontChanges(Form f)
    {
        f.Text("Hello World!", style: new Style
            {
                foregroundColor = Colors.Green,
                backgroundColor = Colors.Black
            })
            .HorizontalGroup(hg =>
            {
                hg.Button("Red", async () =>
                {
                    log.info("Red button click");
                }, style: new nac.Forms.model.Style
                {
                    backgroundColor = Avalonia.Media.Colors.Red,
                    foregroundColor = Avalonia.Media.Colors.White
                });

            });
    }
    
}