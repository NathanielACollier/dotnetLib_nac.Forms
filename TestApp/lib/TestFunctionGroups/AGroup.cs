using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using nac.Forms;
using nac.Forms.model;

namespace TestApp.lib.TestFunctionGroups;

public class AGroup
{
    private static nac.Logging.Logger log = new();

    /*
     This is for stuff that doesn't fit anywhere else
     + TODO: Should be reviewed periodically to see if we need new groups
     */


    public static void ATest1(Form child)
    {
        child.TextBoxFor("txt")
            .Button("Click Me!", async () => { log.Info("Button clicked"); });
    }


    public static void Menu_Simple(Form f)
    {
        f.Menu(new[]
            {
                new MenuItem
                {
                    Header = "File",
                    Items = new[]
                    {
                        new MenuItem
                        {
                            Header = "Save",
                            Action = () => { f.Model["Last Action"] = "Save"; }
                        },
                        new MenuItem
                        {
                            Header = "Open",
                            Action = () => { f.Model["Last Action"] = "Open"; }
                        }
                    }
                }
            })
            .TextFor("Last Action");
    }


    public static void LoadingIndicator_TextDisplay(Form f)
    {
        f.Model["InProgress"] = true;

        f.VerticalGroup(vg =>
        {
            vg.HorizontalGroup(hg => { hg.Text("I'm visible when not loading"); },
                    style: new nac.Forms.model.Style { isHiddenModelName = "InProgress" })
                .HorizontalStack(hg =>
                {
                    hg
                        .Text("Loading")
                        .LoadingTextAnimation(style: new Style()
                        {
                            width = 20
                        });
                }, style: new Style { isVisibleModelName = "InProgress" })
                .Button("Toggle Loading", async () => { f.Model["InProgress"] = !(bool)f.Model["InProgress"]; });
        });
    }


    public static void LoadingIndictator_DataContextTest(Form f)
    {
        var model = new model.DataContext_HelloWorld();
        f.DataContext = model;

        f.VerticalGroup(vg =>
        {
            vg.HorizontalGroup(hg => { hg.Text("I'm visible when not loading"); },
                    style: new nac.Forms.model.Style { isHiddenModelName = nameof(model.Loading) })
                .HorizontalStack(hg =>
                {
                    hg.Text("Loading")
                        .LoadingTextAnimation(style: new Style()
                        {
                            width = 20
                        });
                }, style: new Style { isVisibleModelName = nameof(model.Loading) })
                .Button("Toggle Loading", async () => { model.Loading = !(bool)model.Loading; });
        });
    }
    


    public static void DataContext_ContactClassModel(Form f)
    {
        var model = new model.ContactWindowMainModel();

        f.DataContext = model;

        f.Text("Contact Editor")
            .Panel<model.Contact>(modelFieldName: "Contact", _f =>
            {
                _f.HorizontalGroup(h =>
                    {
                        h.Text("Name: ")
                            .TextBoxFor("DisplayName");
                    })
                    .HorizontalGroup(h =>
                    {
                        h.Text("Email: ")
                            .TextBoxFor("Email");
                    });
            })
            .HorizontalGroup(h =>
            {
                h.Button("New", async () => { model.Contact = new model.Contact(); }).Button("save", async () =>
                {
                    model.Results = $@"
                    Display Name: {model.Contact.DisplayName}
                    Email: {model.Contact.Email}
                    ";
                    model.savedContacts.Add(model.Contact);
                    model.Contact = new model.Contact
                    {
                        Email = model.Contact.Email,
                        DisplayName = model.Contact.DisplayName
                    };
                });
            })
            .HorizontalGroup(h =>
            {
                h.Text("Saved Contacts: ")
                    .DropDown<model.Contact>(itemSourceModelName: "savedContacts",
                        selectedItemModelName: "Contact",
                        populateItemRow: (r) => r.HorizontalGroup(h =>
                        {
                            h.Text("Email: ")
                                .TextFor("Email");
                        })
                    );
            })
            .HorizontalGroup(h =>
            {
                h.Text("Results: ")
                    .TextBoxFor("Results", multiline: true, style: new nac.Forms.model.Style
                    {
                        height = 100
                    });
            })
            .DebugAvalonia(); // enables press F12 to get the avalonia debug window
    }
    
    
    
    
    public static void ChildForm_ShowAndShowDialog(Form f)
    {
        f.Button("Show", async () =>
        {
            await f.DisplayChildForm(child =>
            {
                child.Text("I'm show");
            }, isDialog: false);

            log.Info("After show is displayed");
        }).Button("ShowDialog", async () =>
        {
            await f.DisplayChildForm(child =>
            {
                child.Text("I'm Show Dialog");
            }, isDialog: true);

            log.Info("After show dialog is displayed");
        });
    }
    
    
    

    public static async void AppManager_CreateWindowWithExistingAvaloniaApp(Form f)
    {
        await nac.Forms.lib.AvaloniaAppManager.DisplayForm(outsideF =>
        {
            outsideF.Text("I'm reusing the existing Avalonia App to create a new window");
        });
    }
    
    
    
}