using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Media;
using nac.Forms;
using nac.Forms.model;
using TestApp.model;

using log = TestApp.model.LogEntry;

namespace TestApp.lib;

public static class TestFunctions
{
    public static List<model.TestEntry> PopulateFunctions()
    {
        var functions = new List<model.TestEntry>();
        
        var functionClasses = new Type[]
        {
            typeof(TestFunctionGroups.Button),
            typeof(TestFunctionGroups.TreeView),
            typeof(TestFunctionGroups.Image),
            typeof(TestFunctionGroups.List),
            typeof(TestFunctionGroups.Layout),
            typeof(TestFunctionGroups.Text),
            typeof(TestFunctionGroups.Events),
            typeof(TestFunctions)
        };
        
        functions.AddRange(
            functionClasses.SelectMany(c=> QuickGenerationTestEntries(c))
        );

        // sort the functions in alphabetical order
        return functions.OrderBy(f => f.Name).ToList();
    }
    
    private static Type GetDelegateType( MethodInfo methodInfo)
    {
        var parmTypes = methodInfo.GetParameters().Select(parm => parm.ParameterType);
        var parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
        var delegateType = Expression.GetDelegateType(parmAndReturnTypes);

        return delegateType;
    }

    private static List<model.TestEntry> QuickGenerationTestEntries(Type functionClass)
    {
        var methodList = functionClass.GetMethods(BindingFlags.Static | 
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Public
                                                  );
        
        var functions = from f in methodList
            let fDelegateType = GetDelegateType(f)
            where string.Equals("System.Action`1[nac.Forms.Form]", fDelegateType.ToString())
            select f.CreateDelegate<Action<Form>>();
        
        var entries = from f in functions
            select new model.TestEntry
            {
                Name = functionClass.Name + "_" + f.Method.Name,
                CodeToRun = f,
                SetupChildForm = true
            };

        return entries.ToList();
    }
    
    





    public static void Test1(Form child)
    {
        child.TextBoxFor("txt")
            .Button("Click Me!", async () =>
            {
                log.info("Button clicked");
            });

    }

    
    
    


    public static void TestMenu_Simple(Form f)
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
                            Action = () =>
                            {
                                f.Model["Last Action"] = "Save";
                            }
                        },
                        new MenuItem
                        {
                            Header = "Open",
                            Action = () =>
                            {
                                f.Model["Last Action"] = "Open";
                            }
                        }
                    }
                }
            })
            .TextFor("Last Action");
    }















    public static void TestFilePickerFor_Basic(Form f)
    {
        f.FilePathFor("myPath", onFilePathChanged: async (newFileName) =>
            {
                model.LogEntry.debug($"New Filename is: {newFileName}");
            })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked file: ")
                    .TextFor("myPath");
            });
    }

    public static void TestFilePickerFor_TwoWithDifferentChangeEvents(Form f)
    {
        f.HorizontalGroup(h => {
            h.FilePathFor("myPath1", onFilePathChanged: async (newFileName1) =>
            {
                model.LogEntry.debug($"------New Filename for path1 is: {newFileName1}");
            })
            .HorizontalGroup(hg =>
            {
                hg.Text("myPath1 You picked file: ")
                    .TextFor("myPath1");
            });
        })
        .HorizontalGroup(h => {
            h.FilePathFor("myPath2", onFilePathChanged: async (newFileName2) =>
            {
                model.LogEntry.debug($"++++++New Filename for path2 is: {newFileName2}");
            })
            .HorizontalGroup(hg =>
            {
                hg.Text("myPath2 You picked file: ")
                    .TextFor("myPath2");
            });
        })
        .HorizontalGroup(h => {
            h.DirectoryPathFor("myDirPath1", onDirectoryPathChanged: async (newDirPath1) =>
            {
                model.LogEntry.debug($"++++++New DirectoryPath for path1 is: {newDirPath1}");
            })
            .HorizontalGroup(hg =>
            {
                hg.Text("myDirPath1 You picked file: ")
                    .TextFor("myDirPath1");
            });
        })
        .HorizontalGroup(h => {
            h.DirectoryPathFor("myDirPath2", onDirectoryPathChanged: async (newDirPath2) =>
            {
                model.LogEntry.debug($"------New DirectoryPath for path2 is: {newDirPath2}");
            })
            .HorizontalGroup(hg =>
            {
                hg.Text("myDirPath2 You picked file: ")
                    .TextFor("myDirPath2");
            });
        });
    }

    public static void TestFilePickerFor_NewFile(Form f)
    {
        f.FilePathFor("myPath", fileMustExist: false,
                onFilePathChanged: async (newFileName) =>
                {
                    model.LogEntry.debug($"New filename is: {newFileName}");
                })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked file: ")
                    .TextFor("myPath");
            });
    }
    
    
    
    
    public static void TestDirectoryPathFor_Simple(Form f)
    {
        f.Model["myPath"] = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        f.DirectoryPathFor("myPath",
                onDirectoryPathChanged: async (newFilePath) =>
                {
                    model.LogEntry.debug($"New Directory path is: {newFilePath}");
                })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked directory: ")
                    .TextFor("myPath");
            });
    }
    
    public static void TestDirectoryPathFor_ClassBinding(Form f)
    {
        var myModel = new model.DirectoryPathFormWindowModel();
        f.DataContext = myModel;

        myModel.myPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        f.DirectoryPathFor(nameof(model.DirectoryPathFormWindowModel.myPath),
                onDirectoryPathChanged: async (newFilePath) =>
                {
                    model.LogEntry.info($"New Directory path is: {newFilePath}");
                    model.LogEntry.info($"      Model myPath: {myModel.myPath}");
                })
            .HorizontalGroup(hg =>
            {
                hg.Text("You picked directory: ")
                    .TextFor(nameof(model.DirectoryPathFormWindowModel.myPath));
            })
            .Text("The below directory picker doesn't start with a path")
            .DirectoryPathFor(nameof(model.DirectoryPathFormWindowModel.pathWithoutBeingInit),
                onDirectoryPathChanged: async (newPath) =>
                {
                    model.LogEntry.info($"Old path: {myModel.pathWithoutBeingInit}");
                    model.LogEntry.info($"New path: {newPath}");
                });
    }
    
    
    public static void TestDirectAndFilePathFor_NoEvents(Form f)
    {
        f.HorizontalGroup(hg =>
            {
                hg.Text("FilePath ")
                    .FilePathFor("f1");
            })
            .HorizontalGroup(hg =>
            {
                hg.Text("DirectoryPath ")
                    .DirectoryPathFor("d1");
            });
    }


    public static void Test_Tabs_BasicTest(Form f)
    {
        f.Tabs(t =>
        {
            t.Header = "Tab 1";
            t.Populate = f =>
            {
                f.Text("Hello from tab 1");
            };
        }, t =>
        {
            t.Header = "Tab 2";
            t.Populate = f =>
            {
                f.Text("Hello from tab 2");
            };
        });
    }

    public static void Test_Tabs_HeaderFromTemplate(Form f)
    {
        f.Tabs( 
            new TabCreationInfo
            {
                PopulateHeader = (header) =>
                {
                    header.Text("My Tab 1")
                        .Button("Click Me!", async () =>
                        {
                            header.Model["tab1ClickCount"] =
                                Convert.ToInt32(header.Model["tab1ClickCount"] ?? 0) + 1;
                        });
                },
                Populate = (tab) =>
                {
                    tab.VerticalDock(vg =>
                    {
                        vg.Text("You have clicked the header this many: ")
                            .TextFor(modelFieldName: "tab1ClickCount");
                    });
                }
            }
        );
    }

    public static void Test_DataContext_HelloWorld(Form f)
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


    public static void Test_LoadingIndicator_TextDisplay(Form f)
    {
        f.Model["InProgress"] = true;

        f.VerticalGroup(vg =>
        {
            vg.HorizontalGroup(hg =>
                {
                    hg.Text("I'm visible when not loading");
                }, style: new nac.Forms.model.Style{isHiddenModelName = "InProgress"})
                .HorizontalStack(hg =>
                {
                    hg
                        .Text("Loading")
                        .LoadingTextAnimation(style: new Style()
                        {
                            width = 20
                        });
                }, style: new Style{isVisibleModelName = "InProgress"})
                .Button("Toggle Loading", async () =>
                {
                    f.Model["InProgress"] = !(bool) f.Model["InProgress"];
                });
        });
    }
    
    public static void Test_LoadingIndictator_DataContextTest(Form f)
    {
        var model = new model.DataContext_HelloWorld();
        f.DataContext = model;

        f.VerticalGroup(vg =>
        {
            vg.HorizontalGroup(hg =>
                {
                    hg.Text("I'm visible when not loading");
                }, style: new nac.Forms.model.Style{isHiddenModelName = nameof(model.Loading)})
                .HorizontalStack(hg =>
                {
                    hg.Text("Loading")
                        .LoadingTextAnimation(style: new Style()
                        {
                            width = 20
                        });
                }, style: new Style{isVisibleModelName = nameof(model.Loading)})
                .Button("Toggle Loading", async () =>
                {
                    model.Loading = !(bool) model.Loading;
                });
        });
    }

    public static void Test_DropDown_SimpleTextSelection(Form f)
    {
        var items = new ObservableCollection<string>();
        f.Model["items"] = items;
        items.Add("Bird Feeder");
            
        // test swapping out model with this second list
        var items2 = new ObservableCollection<string>();
        items2.Add("Canik TP9");
        items2.Add("Beretta M9");
        items2.Add("Remington 870");

        f.VerticalStack(vg =>
        {
            vg.HorizontalStack(h =>
                {
                    h.Text("New Item Text: ")
                        .TextBoxFor("newItemText", style: new Style(){width = 100})
                        .Button("Add", async () =>
                        {
                            var _curItems = f.Model["items"] as ObservableCollection<string>;
                            _curItems.Add(f.Model["newItemText"] as string);
                        })
                        .Button("Swap Lists", async () =>
                        {
                            if (f.Model["items"] == items)
                            {
                                f.Model["items"] = items2;
                            }
                            else
                            {
                                f.Model["items"] = items;
                            }
                        });
                }).DropDown<string>(itemSourceModelName: "items",
                    selectedItemModelName: "selected")
                .HorizontalStack(h =>
                {
                    h.Text("You have selected: ")
                        .TextFor("selected");
                });
        });
    }

    public static void TestList_JustStrings(Form f)
    {
        var items = new ObservableCollection<string>();
        new[] {"Walnut", "Peanut", "Cashew"}.ToList().ForEach(x=> items.Add(x));
        f.Model["myList"] = items;

        f.Text("This is a list of strings")
            .List<string>(itemSourcePropertyName: "myList",
                onSelectionChanged: (selectedItems) =>
                {
                    log.info("You selected: " + string.Join(";", selectedItems));
                });

        lib.UIElementsUtility.logViewer(f);
    }

    public static void Test_Threading_ModifyUIInThread(Form f)
    {
        var list = new ObservableCollection<model.TestList_ButtonCounterExample_ItemModel>();

        f.Model["entries"] = list;

        f.HorizontalGroup(h =>
        {
            h.Text("My List")
                .Button("Add", async () =>
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(200); // cause a delay
                        f.InvokeAsync(async () =>
                        {
                            list.Add(new model.TestList_ButtonCounterExample_ItemModel()
                            {
                                Label = Guid.NewGuid().ToString("N")
                            });
                        });
                            
                    });
                });
        }).List<model.TestList_ButtonCounterExample_ItemModel>(itemSourcePropertyName: "entries", populateItemRow: myRow =>
        {
            myRow.HorizontalStack(h =>
            {
                h.Text("Label: ")
                    .TextBoxFor("Label");
            });
        });
    }

    public static void TestStyle_Button_ChangeButtonBackground(Form f)
    {
        f.Model["counter"] = 0;
        var incrimentButtonFunctions = new nac.Forms.Form.ButtonFunctions();

        f.HorizontalGroup(h =>
        {
            h.Text("Counter: ")
                .TextBoxFor("counter", onTextChanged: (newVal) =>
                {
                    var counterText = f.Model["counter"] as string;

                    if (string.IsNullOrWhiteSpace(counterText) ||
                        !int.TryParse(counterText, out int counterInt))
                    {
                        return;
                    }
                        
                    if (counterInt % 2 == 0)
                    {
                        incrimentButtonFunctions.setStyle?.Invoke(new Style
                        {
                            backgroundColor = Colors.Purple
                        });
                    }
                    else
                    {
                        incrimentButtonFunctions.setStyle?.Invoke(new Style
                        {
                            backgroundColor = Colors.Azure
                        });
                    }
                })
                .Button("Incriment", async () =>
                {
                    int counterInt = Convert.ToInt32(f.Model["counter"] as string);
                    f.Model["counter"] = ++counterInt;
                }, functions: incrimentButtonFunctions);
        });
    }


    public static void Test_DataContext_ContactClassModel(Form f)
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
                h.Button("New", async () =>
                {
                    model.Contact = new model.Contact();
                }).Button("save", async () =>
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

    public static void Test_DropDown_DataContext_SelectedItemBinding(Form f)
    {
        // setup Model
        var m = new model.DropDown_DataContext_BindSelectedItem_MainWindowModel();
        f.DataContext = m;
            
        foreach (var __c in new model.Contact[]
                 {
                     new model.Contact { DisplayName = "Mike Brown", Email = "mike.brown@google.com" }, 
                     new model.Contact{ DisplayName = "Jamie Joe", Email = "jamie.joe@google.com"},
                     new model.Contact{ DisplayName = "Lisa Paige", Email = "lisa.paige@google.com"}
                 })
        {
            m.ContactList.Add(__c);
        }
            
        // setup UI
        f.DropDown<model.Contact>(
                itemSourceModelName: nameof(model.DropDown_DataContext_BindSelectedItem_MainWindowModel
                    .ContactList),
                selectedItemModelName: nameof(model.DropDown_DataContext_BindSelectedItem_MainWindowModel
                    .SelectedContact),
                onSelectionChanged: (_c) =>
                {
                    log.info($"You selected {_c?.DisplayName}");
                }, populateItemRow: r =>
                {
                    r.HorizontalGroup(h => h.Text("DisplayName: ").TextFor("DisplayName"));
                })
            .Panel<model.Contact>(modelFieldName: "SelectedContact", populatePanel: p =>
            {
                p.HorizontalGroup(h => h.Text("Selected Contact: ").TextFor("DisplayName"));
            });
    }

    public static void TestTextBox_Password(Form f)
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
    
    
    public static void TestDatePicker_Simple(Form f)
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

    
    public static void TestTextBox_NumberCounter(Form f)
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
    
    
    public static void Test_ChildForm_ShowAndShowDialog(Form f)
    {
        f.Button("Show", async () =>
        {
            await f.DisplayChildForm(child =>
            {
                child.Text("I'm show");
            }, isDialog: false);

            log.info("After show is displayed");
        }).Button("ShowDialog", async () =>
        {
            await f.DisplayChildForm(child =>
            {
                child.Text("I'm Show Dialog");
            }, isDialog: true);

            log.info("After show dialog is displayed");
        });
    }
    
    
}
