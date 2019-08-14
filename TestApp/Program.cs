using System;
using System.Collections.Generic;

namespace TestApp
{

    class TestEntry
    {
        public string Name { get; set; }
        public Action CodeToRun { get; set; }

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
                    i.CodeToRun();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error, trying [{i.Name}].  Exception: {ex}");
                }
                
            });
            f.Display();
        }


        static void Test1()
        {
            var f = new dotnetCoreAvaloniaNCForms.Form();
            f.TextBoxFor("txt")
                .Button("Click Me!", (_args) =>
                {

                })
                .Display();
        }
    }
}
