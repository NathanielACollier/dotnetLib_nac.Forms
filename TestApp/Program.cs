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
            var methods = lib.TestFunctions.PopulateFunctions();

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
                                .Button("Run", async () =>
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
