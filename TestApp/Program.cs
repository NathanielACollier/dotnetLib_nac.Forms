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
            setupNacFormsLogging();
            var f = Avalonia.AppBuilder.Configure<nac.Forms.App>()
                .NewForm(beforeAppBuilderInit: (appBuilder) =>
                {
                    appBuilder.LogToTrace(LogEventLevel.Debug);
                    Console.WriteLine("Logging Setup");
                })
                .DebugAvalonia();

            mainUI(f);
        }

        private static void setupNacFormsLogging()
        {
            nac.Forms.lib.Log.OnNewMessage += (_s, _logEntry) =>
            {
                string line = $"[{_logEntry.EventDate:hh_mm_tt}] - {_logEntry.Level} - {_logEntry.CallingMemberName} - {_logEntry.Message}";

                if( string.Equals(_logEntry.Level, "Info", StringComparison.OrdinalIgnoreCase)){
                    System.Console.ForegroundColor = ConsoleColor.White;
                }else if( string.Equals(_logEntry.Level, "Debug", StringComparison.OrdinalIgnoreCase)){
                    System.Console.ForegroundColor = ConsoleColor.Cyan;
                }else if( string.Equals(_logEntry.Level, "Warn", StringComparison.OrdinalIgnoreCase)){
                    System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                }else if( string.Equals(_logEntry.Level, "Error", StringComparison.OrdinalIgnoreCase)){
                    System.Console.ForegroundColor = ConsoleColor.Red;
                }
                System.Console.WriteLine(line);

                System.Console.ResetColor();
            };
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
                    Name  = "Test List: Just Strings",
                    CodeToRun = lib.TestFunctions.TestList_JustStrings
                },
                new model.TestEntry
                {
                    Name = "Test Menu: Simple",
                    CodeToRun = lib.TestFunctions.TestMenu_Simple
                },
                new model.TestEntry
                {
                    Name = "Test Button: Close on click",
                    CodeToRun = lib.TestFunctions.TestButton_CloseForm,
                    SetupChildForm = false
                },
                new model.TestEntry
                {
                    Name = "Test Event: OnDisplay",
                    CodeToRun = lib.TestFunctions.TestEvent_OnDisplay,
                    SetupChildForm = false
                },
                new model.TestEntry
                {
                    Name = "Test Textbox: Multiline",
                    CodeToRun = lib.TestFunctions.TestTextBox_Multiline
                },
                new model.TestEntry
                {
                    Name  = "Test Textbox: Password",
                    CodeToRun = lib.TestFunctions.TestTextBox_Password
                },
                new model.TestEntry
                {
                    Name = "Test Textbox: Number Counter",
                    CodeToRun = lib.TestFunctions.TestTextBox_NumberCounter
                },
                new model.TestEntry
                {
                    Name  = "Test Image: From Web URL",
                    CodeToRun = lib.TestFunctions.TestImage_FromWebURL
                },
                new TestEntry
                {
                    Name = "Test Style: TextBlock: Basic font changes",
                    CodeToRun = lib.TestFunctions.TestStyle_TextBlock_BasicFontChanges
                },
                new TestEntry
                {
                    Name  = "Test Style: Change button color",
                    CodeToRun = lib.TestFunctions.TestStyle_Button_ChangeButtonBackground
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
                    Name = "Test FilePickerFor: Test change on two different file pickers.",
                    CodeToRun = lib.TestFunctions.TestFilePickerFor_TwoWithDifferentChangeEvents
                },
                new TestEntry()
                {
                    Name  = "Test DirectoryPathFor: Simple example",
                    CodeToRun = lib.TestFunctions.TestDirectoryPathFor_Simple
                },
                new TestEntry()
                {
                    Name  = "Test DirectoryPathFor: Class Model binding Directory Path",
                    CodeToRun = lib.TestFunctions.TestDirectoryPathFor_ClassBinding
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
                    Name = "Test DataContext: Hello World!",
                    CodeToRun = lib.TestFunctions.Test_DataContext_HelloWorld
                },
                new TestEntry
                {
                    Name = "Test DataContext: Contact Class Model",
                    CodeToRun = lib.TestFunctions.Test_DataContext_ContactClassModel
                },
                new TestEntry()
                {
                    Name = "Test Loading Indicator: Text Display",
                    CodeToRun = lib.TestFunctions.Test_LoadingIndicator_TextDisplay
                },
                new TestEntry
                {
                    Name = "Test Loading Indicator: DataContext Based",
                    CodeToRun = lib.TestFunctions.Test_LoadingIndictator_DataContextTest
                },
                new TestEntry()
                {
                    Name = "Test Dropdown: Simple Text Selection",
                    CodeToRun = lib.TestFunctions.Test_DropDown_SimpleTextSelection
                },
                new TestEntry
                {
                    Name = "Test Dropdown: DataContext SelectedItem Binding",
                    CodeToRun = lib.TestFunctions.Test_DropDown_DataContext_SelectedItemBinding
                },
                new TestEntry()
                {
                    Name = "Threading: Modify UI in another thread",
                    CodeToRun = lib.TestFunctions.Test_Threading_ModifyUIInThread
                },
                new TestEntry
                {
                    Name = "Child Form: Show and ShowDialog",
                    CodeToRun = lib.TestFunctions.Test_ChildForm_ShowAndShowDialog
                }



            };

            f.Title = "Test App (nac.forms Tests)";
            
            f.Tabs(new[]
                {
                    new nac.Forms.model.TabCreationInfo
                    {
                        Header = "Methods",
                        Populate = (t) =>
                        {
                            t.SimpleDropDown(methods, (i) =>
                                {
                                    selectedTestEntry = i;
                                    invokeTest(f, selectedTestEntry);
                                })
                                .Button("Run", _args =>
                                {
                                    invokeTest(f,selectedTestEntry);
                                });
                        }
                    }, new nac.Forms.model.TabCreationInfo
                    {
                        Header = "Log",
                        Populate = (t) =>
                        {
                            lib.UIElementsUtility.logViewer(t);
                        }
                    }
                })
            .Display();
        }


        private static void invokeTest(nac.Forms.Form parentForm, model.TestEntry test)
        {
            model.LogEntry.info($"Test: [name={test.Name}] starting");
            try
            {
                if (test.SetupChildForm)
                {
                    parentForm.DisplayChildForm(childForm =>
                    {
                        test.CodeToRun(childForm);
                    }, useIsolatedModelForThisChildForm: true)
                    .ContinueWith(t =>
                    {
                        model.LogEntry.info($"Test: [name={test.Name}] is complete");
                    });
                }
                else
                {
                    // just run it directly
                    test.CodeToRun(parentForm);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error, trying [{test.Name}].  Exception: {ex}";
                model.LogEntry.error(errorMessage);
                writeLineError(errorMessage);
            }
            

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
