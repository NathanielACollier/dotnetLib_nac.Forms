using System;
using System.Collections.Generic;
using Avalonia;

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
            // setup test methods
            var methods = new List<TestEntry>
            {
                new TestEntry
                {
                    Name = "Test1",
                    CodeToRun = Test1
                }
            };
            f.SimpleDropDown(methods, (i) => {
                try
                {
                    i.CodeToRun(f);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error, trying [{i.Name}].  Exception: {ex}");
                }

            });
            f.Display();
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
    }
}
