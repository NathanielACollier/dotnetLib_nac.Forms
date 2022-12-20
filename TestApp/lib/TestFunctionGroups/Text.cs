using System;
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
    
    
    public static void DisplayWhatIsTypedDataContext(Form f)
    {
        var model = new TestApp.model.DataContext_HelloWorld(); // this will be our model
        
        f.Model[nac.Forms.model.SpecialModelKeys.DataContext] = model; // this will enable our "DataContext" to have strongly types
        f.TextBoxFor(nameof(TestApp.model.DataContext_HelloWorld.Message))
            .HorizontalGroup(hg =>
            {
                hg.Text("You have typed: ")
                    .TextFor(nameof(TestApp.model.DataContext_HelloWorld.Message));
            });
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
    
    
    
    public static void TextBox_Password(Form f)
    {
        f.HorizontalGroup(h =>
            {
                h.Text("Enter a password?")
                    .TextBoxFor("myPassword", isPassword: true);
            }).HorizontalGroup(h =>
            {
                h.Text("Enter password2?")
                    .TextBoxFor("password2", isPassword: true,
                        watermarkText: "Enter a password");
            })
            .HorizontalGroup(h =>
            {
                h.Text("You password is: ")
                    .TextBoxFor("myPassword", isReadOnly: true);
            })
            .HorizontalGroup(h =>
            {
                h.Text("Your password2 is: ")
                    .TextBoxFor("password2", isReadOnly: true);
            });
    }
    
    
    
    public static void DatePicker_Simple(Form f)
    {
        f.Model["currentDate"] = DateTime.Now;
        f.HorizontalGroup(h =>
            {
                h.Text("Current Date")
                    .DateFor("currentDate");
            })
            .HorizontalGroup(h =>
            {
                h.Text("Empty Date")
                    .DateFor("emptyDate");
            })
            .HorizontalGroup(h =>
            {
                h.Text("Empty Date(Text)")
                    .TextBoxFor("emptyDate");
            });
    }
    
    
    
    
    public static void TextBox_NumberCounter(Form f)
    {
        var model = new model.DataContext_HelloWorld();
        f.DataContext = model;

        f.HorizontalGroup(hg =>
        {
            hg.Text("Counter: ")
                .TextBoxFor(nameof(model.myCounter),
                    convertFromUIToModel: (string text) =>
                    {
                        if (int.TryParse(text, out int myNumber))
                        {
                            return myNumber;
                        }

                        return 0;
                    });
        }).Button("Incriment", async () =>
        {
            ++model.myCounter;
        });
    }
    
    
    
}