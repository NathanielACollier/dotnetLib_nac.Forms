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
        private static nac.Logging.Logger log = new();

        static void Main(string[] args)
        {
            nac.Logging.Appenders.ColoredConsole.Setup();
            
            try
            {
                var f = nac.Forms.Form
                .NewForm(beforeAppBuilderInit: (appBuilder) =>
                {
                    appBuilder.LogToTrace(LogEventLevel.Debug);
                    Console.WriteLine("Logging Setup");
                })
                .DebugAvalonia();

                mainUI(f);
                nac.Forms.lib.AvaloniaAppManager.Shutdown(); // this isn't needed, and is a test to make sure it's safe to call it
            }catch(Exception ex)
            {
                log.Fatal($"App Exception occured: {ex}");
            }

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
            log.Info($"Test: [name={test.Name}] starting");
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
                        if( t.Exception != null)
                        {
                            logException(test, t.Exception);
                            return; // skip whatever else is going on
                        }
                        log.Info($"Test: [name={test.Name}] is complete");
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
                logException(test, ex);
            }
            

        }


        private static void logException(model.TestEntry test, Exception ex)
        {
            string errorMessage = $"Error, trying [{test.Name}].  Exception: {ex}";
            log.Error(errorMessage);
            writeLineError(errorMessage);
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
