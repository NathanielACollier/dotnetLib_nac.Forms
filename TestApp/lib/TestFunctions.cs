using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using nac.Forms;
using nac.Forms.model;
using TestApp.model;

using log = TestApp.model.LogEntry;

namespace TestApp.lib
{
    public static class TestFunctions
    {
        public static void TestList_ButtonCounterExample(Form parentForm)
        {   
            var items = new System.Collections.ObjectModel.ObservableCollection<TestList_ButtonCounterExample_ItemModel>();

            // display 5 counters
            for( int i = 0; i < 10; ++i){
                items.Add(new TestList_ButtonCounterExample_ItemModel{
                    Counter = 0,
                    Label = $"Counter {i}"
                });
            }

            parentForm.DisplayChildForm(child=>{
                child.Model["items"] = items;
                child.List<TestList_ButtonCounterExample_ItemModel>("items", row=>{
                    
                    row.HorizontalGroup(hg=>{
                        hg.TextFor("Label")
                            .Button("Next", (arg)=>{
                                var model = row.Model[SpecialModelKeys.DataContext] as TestList_ButtonCounterExample_ItemModel;
                                ++model.Counter;
                            })
                            .Text("Counter is: ")
                            .TextFor("Counter");
                    });
                });
            });
        }

        public static void TestLayout_HorizontalSplit(Form parentForm)
        {
            parentForm.DisplayChildForm(child=>{
                child.HorizontalGroup(grp=> {
                    grp.Text("Text to the Left")
                        .Text("Text to the right");
                }, isSplit: true);
            });
        }

        public static void TestLayout_VerticalSplit(Form parentForm)
        {
            parentForm.DisplayChildForm(child=>{
                child.VerticalGroup(grp=> {
                    grp.Text("Text Above")
                        .Text("Text Below");
                }, isSplit: true);
            });

        }

        public static void TestLayout1_SimpleHorizontal(Form obj)
        {
            obj.DisplayChildForm(child =>
            {
                child.HorizontalGroup(hori =>
                {
                    hori.Text("Click Count: ", style: new Style(){ width = 100})
                        .TextBoxFor("clickCount")
                        .Button("Click Me!", arg =>
                        {
                            var current = child.Model.GetOrDefault<int>("clickCount", 0);
                            ++current;
                            hori.Model["clickCount"] = current;
                        }, style: new Style(){width = 60});
                });
            });
        }

        public static void Test3_DisplayWhatIsTyped(Form obj)
        {
            obj.DisplayChildForm(child =>
            {
                child.TextFor("txt2", "Type here")
                    .TextBoxFor("txt2");
            });
        }

        public static void Test2_ButtonWithClickCount(Form obj)
        {
            obj.DisplayChildForm(child =>
            {
                child.TextFor("txt1", "When you click button I'll change to count!")
                .Button("Click Me!", arg =>
                {
                    var current = child.Model.GetOrDefault<int>("txt1", 0);
                    ++current;
                    child.Model["txt1"] = current;
                });
            });
        }

        public static void Test1(Form parentForm)
        {
            parentForm.DisplayChildForm(child =>
            {
                child.TextBoxFor("txt")
                .Button("Click Me!", (_args) =>
                {

                });
            });

        }


        public static void TestCollections_SimpleItemsControl(Form parentForm)
        {
            parentForm.DisplayChildForm(child =>
            {
                var items = new System.Collections.ObjectModel.ObservableCollection<nac.Forms.lib.BindableDynamicDictionary>();
                child.Model["items"]  = items;
                var newItem = new nac.Forms.lib.BindableDynamicDictionary();
                newItem["Prop1"] = "fish";
                
                items.Add(newItem);
                newItem = new nac.Forms.lib.BindableDynamicDictionary();
                newItem["Prop1"] = "Blanket";
                items.Add(newItem);

                child.Text("Simple List")
                .List<nac.Forms.lib.BindableDynamicDictionary>("items", (itemForm) =>
                {
                    itemForm.TextFor("Prop1");
                }, style: new Style()
                {
                    height = 500,
                    width = 300,
                    backgroundColor = Avalonia.Media.Colors.Aquamarine
                }, onSelectionChanged: (_selectedEntries) =>
                {
                    log.info($"New items selected: {string.Join(",", _selectedEntries.Select(m=>m["Prop1"] as string))}");
                })
                .HorizontalGroup((hgChild) =>
                {
                    // default some stuff
                    child.Model["NewItem.Prop1"] = "Frog Prince";

                    hgChild.Text("Prop1: ")
                            .TextBoxFor("NewItem.Prop1")
                            .Button("Add Item", (_args) =>
                            {
                                newItem = new nac.Forms.lib.BindableDynamicDictionary();
                                newItem["Prop1"] = child.Model["NewItem.Prop1"] as string;
                                items.Add(newItem);
                            });
                });

                lib.UIElementsUtility.logViewer(child);
                log.info("App Ready to go");

            });
        }


        public static void TestVerticalGroup_Simple1(Form parentForm)
        {
            parentForm.DisplayChildForm(mainForm =>
            {
                mainForm.HorizontalGroup((hgForm) =>
                {
                    hgForm.VerticalGroup((vg1) =>
                    {
                        vg1.Text("Here is a column of controls in a vertical group")
                            .Button("Click Me!", (_args)=>
                            {

                            });
                    })
                    .VerticalGroup((vg2) =>
                    {
                        vg2.Text("Here is a second column of controls")
                            .Button("Click me 2!!", (_args) =>
                            {

                            });
                    });
                });
            });
        }
        
        public static void TestVerticalDock_Simple1(Form parentForm)
        {
            parentForm.DisplayChildForm(mainForm =>
            {
                mainForm.HorizontalGroup((hgForm) =>
                {
                    hgForm.VerticalDock((vg1) =>
                        {
                            vg1.Text("Here is a column of controls in a vertical group")
                                .Button("Click Me!", (_args)=>
                                {

                                });
                        })
                        .VerticalDock((vg2) =>
                        {
                            vg2.Text("Here is a second column of controls")
                                .Button("Click me 2!!", (_args) =>
                                {

                                });
                        });
                });
            });
        }


        public static void TestControllingVisibilityOfControls_HorizontalGroup(Form parentForm)
        {
            parentForm.DisplayChildForm(mainForm =>
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
                        .Button("Hide/show ME!", (_args) =>
                        {
                            mainForm.Model["isHoriVis"] = !(mainForm.Model["isHoriVis"] as bool? ?? true);
                        }, style: new Style(){width = 120});
                    }, style: new Style()
                    {
                        isVisibleModelName = "isTextVisible"
                    } )
                .Button("Show or Hide Text", (_args) =>
                {
                    mainForm.Model["isTextVisible"] = !(mainForm.Model["isTextVisible"] as bool? ?? true);
                });
            });
        }
        
        
        public static void TestControlVisibilityOfControls_VerticalGroup(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.VerticalGroup(vg =>
                {
                    vg.Text("I'm Visible");
                }, style:new Style()
                {
                    height = 50,
                    isVisibleModelName = "isDisplay"
                })
                .Button("Hide or Show", (_args) =>
                {
                    f.Model["isDisplay"] = !(f.Model["isDisplay"] as bool? ?? true);
                }, style: new Style(){width = 100});
            });
        }

        public static void TestMenu_Simple(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
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
            });
        }


        public static void TestButton_CloseForm(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.Model["closeCount"] = 0;
                f.Model["isQuit"] = false;
                f.Text("Clicking ok will close this form")
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("Close count: ")
                            .TextFor("closeCount");
                    })
                    .HorizontalGroup(hg =>
                    {
                        hg.Button("Quit", (_args) =>
                        {
                            f.Close();
                        }).Button("Force Quit", (_args) =>
                        {
                            f.Model["isQuit"] = true;
                            f.Close();
                        });
                    });
            }, onClosing: (f) =>
            {
                dynamic closeCount = f.Model["closeCount"];
                f.Model["closeCount"] = ++closeCount;

                if (f.Model["isQuit"] as bool? == true)
                {
                    return false; // don't cancel
                }
                else
                {
                    return true; // prevent closing the window (return if cancel or not)
                }
                
            });
        }


        public static void TestEvent_OnDisplay(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.TextFor("message");
            }, onDisplay: (f) =>
            {
                f.Model["message"] = "Form is displayed";
            });
        }

        public static void TestTextBox_Multiline(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.VerticalGroup(vg =>
                {
                    vg.Text("Text above the Textbox", new Style(){height=20})
                        .TextBoxFor("message", multiline: true)
                        .Text("Text below the textbox", new Style(){height = 20});
                }, isSplit: true);
            });
        }


        public static void TestStyle_TextBlock_BasicFontChanges(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.Text("Hello World!", style: new Style
                    {
                        foregroundColor = Colors.Green,
                        backgroundColor = Colors.Black
                    })
                    .HorizontalGroup(hg =>
                    {
                        hg.Button("Red", (_args) =>
                        {

                        }, style: new nac.Forms.model.Style
                        {
                            backgroundColor = Avalonia.Media.Colors.Red,
                            foregroundColor = Avalonia.Media.Colors.White
                        });

                    });
            });
        }


        public static void TestFilePickerFor_Basic(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.FilePathFor("myPath", onFilePathChanged: (newFileName) =>
                    {
                        Console.WriteLine($"New Filename is: {newFileName}");
                    })
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("You picked file: ")
                            .TextFor("myPath");
                    });
            });
        }

        public static void TestFilePickerFor_NewFile(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.FilePathFor("myPath", fileMustExist: false,
                    onFilePathChanged: (newFileName) =>
                    {
                        Console.WriteLine($"New filename is: {newFileName}");
                    })
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("You picked file: ")
                            .TextFor("myPath");
                    });
            });
        }


        public static void Test_Tabs_BasicTest(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
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
            });
        }

        public static void Test_Tabs_HeaderFromTemplate(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.Tabs( 
                    new TabCreationInfo
                    {
                        PopulateHeader = (header) =>
                        {
                            header.Text("My Tab 1")
                                .Button("Click Me!", (args) =>
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
            });
        }

        public static void Test_DataContext_HelloWorld(Form parentForm)
        {
            var model = new TestApp.model.DataContext_HelloWorld(); // this will be our model
            parentForm.Model[nac.Forms.model.SpecialModelKeys.DataContext] = model; // this will enable our "DataContext" to have strongly types
            parentForm.DisplayChildForm(f =>
            {
                f.TextBoxFor(nameof(TestApp.model.DataContext_HelloWorld.Message))
                    .HorizontalGroup(hg =>
                    {
                        hg.Text("You have typed: ")
                            .TextFor(nameof(TestApp.model.DataContext_HelloWorld.Message));
                    });
            });
        }


        public static void Test_LoadingIndicator_TextDisplay(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.Model["InProgress"] = true;

                f.VerticalGroup(vg =>
                {
                    vg.HorizontalStack(hg =>
                    {
                        hg.Text("Loading")
                            .LoadingTextAnimation(style: new Style()
                            {
                                width = 20,
                                isVisibleModelName = "InProgress"
                            });
                    })
                    .Button("Toggle Loading", (_a) =>
                    {
                        f.Model["InProgress"] = !(bool) f.Model["InProgress"];
                    });
                });
            });
        }

        public static void Test_DropDown_SimpleTextSelection(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
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
                                .Button("Add", (_a) =>
                                {
                                    var _curItems = f.Model["items"] as ObservableCollection<string>;
                                    _curItems.Add(f.Model["newItemText"] as string);
                                })
                                .Button("Swap Lists", (_a) =>
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
            });
        }

        public static void TestList_JustStrings(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
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
            });
        }

        public static void Test_Threading_ModifyUIInThread(Form parentForm)
        {
            var list = new ObservableCollection<model.TestList_ButtonCounterExample_ItemModel>();

            parentForm.DisplayChildForm(f =>
            {
                f.Model["entries"] = list;

                f.HorizontalGroup(h =>
                {
                    h.Text("My List")
                        .Button("Add", (args) =>
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
            });
        }
    }
}