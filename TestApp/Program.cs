using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using dotnetCoreAvaloniaNCForms;

namespace TestApp
{

    class TestEntry
    {
        public string Name { get; set; }
        public Action<dotnetCoreAvaloniaNCForms.Form> CodeToRun { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            var f = Avalonia.AppBuilder.Configure<dotnetCoreAvaloniaNCForms.App>()
                                .NewForm();

            mainUI(f);
        }

        static void mainUI(dotnetCoreAvaloniaNCForms.Form f)
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

        private static void TestList_ButtonCounterExample(Form parentForm)
        {   
            var items = new ObservableCollection<object>();

            // display 5 counters
            for( int i = 0; i < 10; ++i){
                items.Add(new {
                    Counter = 0,
                    Label = $"Counter {i}"
                });
            }

            parentForm.DisplayChildForm(child=>{
                child.Model["items"] = items;
                child.List("items", row=>{
                    row.TextFor("Label")
                        .Button("Next", (arg)=>{
                            
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

        static void Test1(dotnetCoreAvaloniaNCForms.Form parentForm)
        {
            parentForm.DisplayChildForm(child =>
            {
                child.TextBoxFor("txt")
                .Button("Click Me!", (_args) =>
                {

                });
            });

        }




        static void TestCollections_SimpleItemsControl(dotnetCoreAvaloniaNCForms.Form parentForm)
        {
            parentForm.DisplayChildForm(child =>
            {
                child.Model["items"] = new ObservableCollection<object>
                {
                    new
                    {
                        Prop1 = "Fish"
                    },
                    new
                    {
                        Prop1 = "Blanket"
                    }
                };

                child.Text("Simple List")
                .List("items", (itemForm) =>
                {
                    itemForm.TextFor("Prop1");
                })
                .HorizontalGroup((hgChild) =>
                {
                    hgChild.Text("Click this button to add to list")
                            .Button("add", (_args) =>
                            {
                                var items = child.Model["items"] as ObservableCollection<object>;
                                items.Add(new
                                {
                                    Prop1 = "Frog Prince"
                                });
                            });
                });
            });
        }


        static void TestVerticalGroup_Simple1(dotnetCoreAvaloniaNCForms.Form parentForm)
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



        static void TestControllingVisibilityOfControls(dotnetCoreAvaloniaNCForms.Form parentForm)
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

    }
}
