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
            var f = new dotnetCoreAvaloniaNCForms.Form();
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
                    Name = "Test Layout: Simple Horizontal",
                    CodeToRun = TestLayout1_SimpleHorizontal
                },
                new TestEntry
                {
                    Name = "Test List: Simple Items Control",
                    CodeToRun = TestCollections_SimpleItemsControl
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
                .List<string>("items", (itemForm) =>
                {
                    itemForm.Text("Here is an item");
                });
            });
        }



    }
}
