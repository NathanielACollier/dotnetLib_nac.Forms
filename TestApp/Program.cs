using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using forms=NC.Forms.Avalonia;
using NC.Forms.Avalonia; // to bring in the extensions

namespace TestApp
{

    class TestEntry
    {
        public string Name { get; set; }
        public Action<forms.Form> CodeToRun { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            var f = Avalonia.AppBuilder.Configure<forms.App>()
                                .NewForm();

            mainUI(f);
        }

        static void mainUI(forms.Form f)
        {
            TestEntry selectedTestEntry = null;
            // setup test methods
            var methods = new List<TestEntry>
            {
                new TestEntry
                {
                    Name = "Test1",
                    CodeToRun = Test1
                },
                new TestEntry
                {
                    Name = "Test Button with click count",
                    CodeToRun = Test2_ButtonWithClickCount
                },
                new TestEntry
                {
                    Name = "Test Display what is typed",
                    CodeToRun = Test3_DisplayWhatIsTyped
                },
                new TestEntry
                {
                    Name = "Test Layout: Horizontal Group",
                    CodeToRun = TestLayout1_SimpleHorizontal
                },
                new TestEntry
                {
                    Name = "Test Layout: Vertical Group",
                    CodeToRun = TestVerticalGroup_Simple1
                },
                new TestEntry{
                    Name = "Test Layout: Vertical Group Split",
                    CodeToRun = TestLayout_VerticalSplit
                },
                new TestEntry{
                    Name = "Test Layout: Horizontal Group Split",
                    CodeToRun = TestLayout_HorizontalSplit
                },
                new TestEntry
                {
                    Name = "Test Layout: Visibility of Horizontal and Vertical Groups",
                    CodeToRun = TestControllingVisibilityOfControls
                },
                new TestEntry
                {
                    Name = "Test List: Simple Items Control",
                    CodeToRun = TestCollections_SimpleItemsControl
                },
                new TestEntry{
                    Name = "Test List: Button Counter via Model",
                    CodeToRun = TestList_ButtonCounterExample
                },
                new TestEntry
                {
                    Name = "Test Menu: Simple",
                    CodeToRun = TestMenu_Simple
                }



            };
            f.SimpleDropDown(methods, (i) => {
                try
                {
                    selectedTestEntry = i;
                    i.CodeToRun(f);
                }
                catch (Exception ex)
                {
                    writeLineError($"Error, trying [{i.Name}].  Exception: {ex}");
                }

            })
            .Button("Run", _args =>
            {
                try
                {
                    selectedTestEntry.CodeToRun(f);
                }catch(Exception ex)
                {
                    writeLineError($"Error, manually running {selectedTestEntry?.Name ?? "NULL"}.  Exception: {ex}");
                }
            })
            .Display();
        }

        public class TestList_ButtonCounterExample_ItemModel : forms.lib.model.ViewModelBase {
            public int Counter {
                get { return base.GetValue(()=> this.Counter);}
                set { base.SetValue(() => this.Counter, value);}
            }
            public string Label{
                get { return base.GetValue(() => this.Label);}
                set { base.SetValue(() => this.Label, value);}
            }

        }

        private static void TestList_ButtonCounterExample(Form parentForm)
        {   
            var items = new ObservableCollection<object>();

            // display 5 counters
            for( int i = 0; i < 10; ++i){
                items.Add(new TestList_ButtonCounterExample_ItemModel{
                    Counter = 0,
                    Label = $"Counter {i}"
                });
            }

            parentForm.DisplayChildForm(child=>{
                child.Model["items"] = items;
                child.List("items", row=>{
                    
                    row.HorizontalGroup(hg=>{
                        hg.TextFor("Label")
                            .Button("Next", (arg)=>{
                                var model = row.Model[forms.lib.model.SpecialModelKeys.DataContext] as TestList_ButtonCounterExample_ItemModel;
                                ++model.Counter;
                            })
                            .Text("Counter is: ")
                            .TextFor("Counter");
                    });
                });
            });
        }

        private static void TestLayout_HorizontalSplit(Form parentForm)
        {
            parentForm.DisplayChildForm(child=>{
                child.HorizontalGroupSplit(grp=> {
                    grp.Text("Text to the Left")
                        .Text("Text to the right");
                });
            });
        }

        private static void TestLayout_VerticalSplit(Form parentForm)
        {
            parentForm.DisplayChildForm(child=>{
                child.VerticalGroupSplit(grp=> {
                    grp.Text("Text Above")
                        .Text("Text Below");
                });
            });

        }

        private static void writeLineError(string message)
        {
            var originalForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = originalForeground;
        }

        private static void TestLayout1_SimpleHorizontal(Form obj)
        {
            obj.DisplayChildForm(child =>
            {
                child.HorizontalGroup(hori =>
                {
                    hori.Text("Click Count: ")
                        .TextBoxFor("clickCount")
                        .Button("Click Me!", arg =>
                        {
                            var current = child.Model.GetOrDefault<int>("clickCount", 0);
                            ++current;
                            hori.Model["clickCount"] = current;
                        });
                });
            });
        }

        private static void Test3_DisplayWhatIsTyped(Form obj)
        {
            obj.DisplayChildForm(child =>
            {
                child.TextFor("txt2", "Type here")
                    .TextBoxFor("txt2");
            });
        }

        static void Test2_ButtonWithClickCount(Form obj)
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

        static void Test1(forms.Form parentForm)
        {
            parentForm.DisplayChildForm(child =>
            {
                child.TextBoxFor("txt")
                .Button("Click Me!", (_args) =>
                {

                });
            });

        }




        static void TestCollections_SimpleItemsControl(forms.Form parentForm)
        {
            parentForm.DisplayChildForm(child =>
            {
                var items = new ObservableCollection<object>();
                child.Model["items"]  = items;
                var newItem = new forms.lib.BindableDynamicDictionary();
                newItem["Prop1"] = "fish";
                
                items.Add(newItem);
                newItem = new forms.lib.BindableDynamicDictionary();
                newItem["Prop1"] = "Blanket";
                items.Add(newItem);

                child.Text("Simple List")
                .List("items", (itemForm) =>
                {
                    itemForm.TextFor("Prop1");
                })
                .HorizontalGroup((hgChild) =>
                {
                    // default some stuff
                    child.Model["NewItem.Prop1"] = "Frog Prince";

                    hgChild.Text("Prop1: ")
                            .TextBoxFor("NewItem.Prop1")
                            .Button("Add Item", (_args) =>
                            {
                                newItem = new forms.lib.BindableDynamicDictionary();
                                newItem["Prop1"] = child.Model["NewItem.Prop1"] as string;
                                items.Add(newItem);
                            });
                });
            });
        }


        static void TestVerticalGroup_Simple1(forms.Form parentForm)
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



        static void TestControllingVisibilityOfControls(forms.Form parentForm)
        {
            parentForm.DisplayChildForm(mainForm =>
            {
                mainForm.Model["isTextVisible"] = false;

                mainForm.HorizontalGroup(hg =>
                {
                    hg.Text("This text is visible");
                }, isVisiblePropertyName: "isTextVisible")
                .Button("Show or Hide Text", (_args) =>
                {
                    mainForm.Model["isTextVisible"] = !(mainForm.Model["isTextVisible"] as bool? ?? false);
                });
            });
        }
        
        
        private static void TestMenu_Simple(Form parentForm)
        {
            parentForm.DisplayChildForm(f =>
            {
                f.Menu(new[]
                {
                    new forms.model.MenuItem
                    {
                        Header = "File",
                        Items = new[]
                        {
                            new forms.model.MenuItem
                            {
                                Header = "Save",
                                Action = () =>
                                {
                                    f.Model["Last Action"] = "Save";
                                }
                            },
                            new forms.model.MenuItem
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

    }
}
