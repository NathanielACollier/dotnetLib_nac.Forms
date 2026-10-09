namespace nac.Forms.UITesterApp.repos;

public static class MainWindowUI
{
    private static nac.Logging.Logger log = new();
    
    public static void Run(nac.Forms.Form f, Type targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList)
    {
        var context = new model.MainWindowModel();
        f.DataContext = context;

        string testAppName = targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList.Assembly.FullName;
        
        f.Title = $"Test App - ({testAppName})";
            
        f.Tabs(new[]
            {
                new nac.Forms.model.TabCreationInfo
                {
                    Header = "Methods",
                    Populate = (t) =>
                    {
                        t.DropDown<model.TestEntry>(itemSourceModelName: nameof(context.MethodsList),
                                selectedItemModelName: nameof(context.SelectedMethod),
                                onSelectionChanged: (i) =>
                            {
                                invokeTest(f, context.SelectedMethod);
                            }, populateItemRow: row =>
                                {
                                    row.TextFor(nameof(model.TestEntry.Name));
                                })
                            .Button("Run", async () =>
                            {
                                invokeTest(f,context.SelectedMethod);
                            });
                    }
                }, new nac.Forms.model.TabCreationInfo
                {
                    Header = "Log",
                    Populate = (t) =>
                    {
                        repos.UIElementsUtility.logViewer(t, nameof(context.LogMessageList));
                    }
                }
            })
            .Display(onDisplay: async (_f) =>
            {
                var methods = repos.TestFunctionsRepo.PopulateFunctions(targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList);

                foreach (var m in methods)
                {
                    context.MethodsList.Add(m);
                }
            });
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
                        if (test.CodeToRunAsync != null)
                        {
                            // DisplayChildForm takes a synchronous setup delegate (Action<Form>),
                            //    so we cannot await the async test function inside it.
                            //    Start it, and if it throws, log it out
                            test.CodeToRunAsync(childForm).ContinueWith(t =>
                            {
                                if (t.Exception != null)
                                {
                                    logException(test, t.Exception);
                                }
                            }, TaskContinuationOptions.OnlyOnFaulted);
                        }
                        else
                        {
                            test.CodeToRun(childForm);
                        }
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
                if (test.CodeToRunAsync != null)
                {
                    test.CodeToRunAsync(parentForm).ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            logException(test, t.Exception);
                        }
                        else
                        {
                            log.Info($"Test: [name={test.Name}] is complete");
                        }
                    });
                }
                else
                {
                    test.CodeToRun(parentForm);
                }
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