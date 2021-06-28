using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Logging;
using nac.Forms;
using nac.Forms.lib;
using nac.Forms.model;
using TestApp.model;

// to bring in the extensions

namespace TestApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var f = Avalonia.AppBuilder.Configure<nac.Forms.App>()
                                .NewForm(beforeAppBuilderInit: (appBuilder) =>
                                {
                                    appBuilder.LogToTrace(LogEventLevel.Debug);
                                    Console.WriteLine("Logging Setup");
                                });

            mainUI(f);
        }

        static void mainUI(Form f)
        {
            model.TestEntry selectedTestEntry = null;
            // setup test methods
            var methods = new List<model.TestEntry>
            {
                new model.TestEntry
                {
                    Name = "Test1",
                    CodeToRun = lib.TestFunctions.Test1
                },
                new model.TestEntry
                {
                    Name = "Test Button with click count",
                    CodeToRun = lib.TestFunctions.Test2_ButtonWithClickCount
                },
                new model.TestEntry
                {
                    Name = "Test Display what is typed",
                    CodeToRun = lib.TestFunctions.Test3_DisplayWhatIsTyped
                },
                new model.TestEntry
                {
                    Name = "Test Layout: Horizontal Group",
                    CodeToRun = lib.TestFunctions.TestLayout1_SimpleHorizontal
                },
                new model.TestEntry
                {
                    Name = "Test Layout: Vertical Group",
                    CodeToRun = lib.TestFunctions.TestVerticalGroup_Simple1
                },
                new model.TestEntry()
                {
                    Name = "Test Layout: Vertical Dock",
                    CodeToRun = lib.TestFunctions.TestVerticalDock_Simple1
                },
                new model.TestEntry{
                    Name = "Test Layout: Vertical Group Split",
                    CodeToRun = lib.TestFunctions.TestLayout_VerticalSplit
                },
                new model.TestEntry{
                    Name = "Test Layout: Horizontal Group Split",
                    CodeToRun = lib.TestFunctions.TestLayout_HorizontalSplit
                },
                new model.TestEntry
                {
                    Name = "Test Layout: Visibility of Horizontal",
                    CodeToRun = lib.TestFunctions.TestControllingVisibilityOfControls_HorizontalGroup
                },
                new model.TestEntry()
                {
                    Name = "Test Layout: Visibility of Vertical",
                    CodeToRun = lib.TestFunctions.TestControlVisibilityOfControls_VerticalGroup
                },
                new model.TestEntry
                {
                    Name = "Test List: Simple Items Control",
                    CodeToRun = lib.TestFunctions.TestCollections_SimpleItemsControl
                },
                new model.TestEntry{
                    Name = "Test List: Button Counter via Model",
                    CodeToRun = lib.TestFunctions.TestList_ButtonCounterExample
                },
                new model.TestEntry
                {
                    Name = "Test Menu: Simple",
                    CodeToRun = lib.TestFunctions.TestMenu_Simple
                },
                new model.TestEntry
                {
                    Name = "Test Button: Close on click",
                    CodeToRun = lib.TestFunctions.TestButton_CloseForm
                },
                new model.TestEntry
                {
                    Name = "Test Event: OnDisplay",
                    CodeToRun = lib.TestFunctions.TestEvent_OnDisplay
                },
                new model.TestEntry
                {
                    Name = "Test Textbox: Multiline",
                    CodeToRun = lib.TestFunctions.TestTextBox_Multiline
                },
                new TestEntry
                {
                    Name = "Test Style: TextBlock: Basic font changes",
                    CodeToRun = lib.TestFunctions.TestStyle_TextBlock_BasicFontChanges
                },
                new TestEntry
                {
                    Name = "Test FilePickerFor: Test file picker basics",
                    CodeToRun = lib.TestFunctions.TestFilePickerFor_Basic
                },
                new TestEntry()
                {
                    Name = "Test FilePickerFor: Test save file dialog (New File)",
                    CodeToRun = lib.TestFunctions.TestFilePickerFor_NewFile
                },
                new TestEntry()
                {
                    Name = "Tabs: Basic Test",
                    CodeToRun = lib.TestFunctions.Test_Tabs_BasicTest
                },
                new TestEntry()
                {
                    Name = "Tabs: Header from Template",
                    CodeToRun = lib.TestFunctions.Test_Tabs_HeaderFromTemplate
                },
                new TestEntry()
                {
                    Name = "DataContext: Hello World!",
                    CodeToRun = lib.TestFunctions.Test_DataContext_HelloWorld
                },
                new TestEntry()
                {
                    Name = "Test Loading Indicator: Text Display",
                    CodeToRun = lib.TestFunctions.Test_LoadingIndicator_TextDisplay
                },
                new TestEntry()
                {
                    Name = "Test Dropdown: Simple Text Selection",
                    CodeToRun = lib.TestFunctions.Test_DropDown_SimpleTextSelection
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

    }
}
