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
using TestApp.model;

using log = TestApp.model.LogEntry;

namespace TestApp.lib;

public static class TestFunctions
{
    public static List<model.TestEntry> PopulateFunctions()
    {
        var functions = new List<model.TestEntry>();
        
        var functionClasses = new Type[]
        {
            typeof(TestFunctionGroups.AGroup),
            typeof(TestFunctionGroups.Button),
            typeof(TestFunctionGroups.TreeView),
            typeof(TestFunctionGroups.Image),
            typeof(TestFunctionGroups.List),
            typeof(TestFunctionGroups.Table),
            typeof(TestFunctionGroups.Layout),
            typeof(TestFunctionGroups.Text),
            typeof(TestFunctionGroups.Events),
            typeof(TestFunctionGroups.FileSystem),
            typeof(TestFunctionGroups.DropDown),
            typeof(TestFunctionGroups.ContextMenu),
            typeof(TestFunctions)
        };
        
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
        var methodList = functionClass.GetMethods(BindingFlags.Static | 
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Public
                                                  );
        
        var functions = from f in methodList
            let fDelegateType = GetDelegateType(f)
            where string.Equals("System.Action`1[nac.Forms.Form]", fDelegateType.ToString())
            select f.CreateDelegate<Action<Form>>();
        
        var entries = from f in functions
            select new model.TestEntry
            {
                Name = functionClass.Name + "_" + f.Method.Name,
                CodeToRun = f,
                SetupChildForm = true
            };

        return entries.ToList();
    }


    
    
}
