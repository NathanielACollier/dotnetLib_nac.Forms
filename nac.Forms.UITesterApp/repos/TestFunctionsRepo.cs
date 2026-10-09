using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Media;
using nac.Forms;
using nac.Forms.model;


namespace nac.Forms.UITesterApp.repos;

public static class TestFunctionsRepo
{
    public static List<model.TestEntry> PopulateFunctions(Type targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList)
    {
        var functions = new List<model.TestEntry>();

        var functionClasses = from t in targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList.Assembly.GetTypes()
            where t.IsClass && t.Namespace == targetClassToUseToDetermineAssemblyAndNamespaceForTestClassList.Namespace
            select t;
        
        functions.AddRange(
            functionClasses.SelectMany(c=> QuickGenerationTestEntries(c))
        );

        // sort the functions in alphabetical order
        return functions.OrderBy(f => f.Name).ToList();
    }
    
    private static Type GetDelegateType( MethodInfo methodInfo)
    {
        var parmTypes = methodInfo.GetParameters().Select(parm => parm.ParameterType);
        var parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
        var delegateType = Expression.GetDelegateType(parmAndReturnTypes);

        return delegateType;
    }

    private static List<model.TestEntry> QuickGenerationTestEntries(Type functionClass)
    {
        string formClassFullName = typeof(Form).FullName;
        // both delegate shapes are discovered:
        //   sync  => void   TestName(Form f)   => System.Action`1[nac.Forms.Form]
        //   async => Task   TestName(Form f)   => System.Func`2[nac.Forms.Form,System.Threading.Tasks.Task]
        string syncDelegateTypeString = $"System.Action`1[{formClassFullName}]";
        string asyncDelegateTypeString = $"System.Func`2[{formClassFullName},System.Threading.Tasks.Task]";
        var methodList = functionClass.GetMethods(BindingFlags.Static | 
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Public
                                                  );
        
        var entries = new List<model.TestEntry>();

        foreach (MethodInfo f in methodList)
        {
            var fDelegateType = GetDelegateType(f).ToString();

            if (string.Equals(syncDelegateTypeString, fDelegateType))
            {
                entries.Add(
                    new model.TestEntry
                    {
                        Name = functionClass.Name + "_" + f.Name,
                        CodeToRun = f.CreateDelegate<Action<Form>>(),
                        SetupChildForm = true
                    });
            }
            else if (string.Equals(asyncDelegateTypeString, fDelegateType))
            {
                // async test function - we can await it and capture exceptions
                entries.Add(
                    new model.TestEntry
                    {
                        Name = functionClass.Name + "_" + f.Name,
                        CodeToRunAsync = f.CreateDelegate<Func<Form, Task>>(),
                        SetupChildForm = true
                    });
            }
            // anything else (other param/return combos) is ignored
        }

        return entries;
    }


    
    
}
