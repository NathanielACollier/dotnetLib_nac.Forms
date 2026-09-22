# Adding Test Functions to TestApp

This document explains how to add a test function for a custom nac.forms component/functionality in the TestApp.

## Overview

The TestApp is located at `/TestApp/lib/TestFunctionGroups/*.cs`. Each C# file contains functions that are registered as test cases for different functionality groups in nac.Forms.

## Structure of a Test Function

Each test function follows this pattern:

```csharp
public static void YourFunctionName(Form child)
{
    // Form builder code here
}
```

The `Form` parameter is the main form onto which you build your UI using the nac.Forms fluent API.

## Step-by-Step Guide

### 1. Choose or Create a Group File

Test functions are organized in groups:

- **AGroup.cs** - For functionality that doesn't fit elsewhere (current location, use with caution)
- Component-specific folders (e.g., `Button.cs`, `Table.cs`) - For component-specific tests

To add a new function:
- If it's a general utility or edge case → Add to `AGroup.cs`
- If it's for a specific form component → Create/Update the corresponding group file

### 2. Define Your Test Function

```csharp
namespace TestApp.lib.TestFunctionGroups;

public class AGroup
{
    private static nac.Logging.Logger log = new();
    
    public static void YourNewFunction(Form child)
    {
        // Initialize any model state needed
        child.Model["yourKey"] = initialValue;
        
        // Build the form layout
        child.YourComponent()
            .Option().Option().Option()
            .Button("Click", async () => 
            { 
                log.Info("Action performed"); 
            });
    }
}
```

### 3. Common Patterns

#### Simple Button with Action
```csharp
public static void SimpleButtonTest(Form child)
{
    child.Text("Simple Button Test")
        .Button("Click Me", async () => { 
            log.Info($"Clicked at {DateTime.Now:HH:mm:ss}"); 
        });
}
```

#### Form with Model Binding
```csharp
public static void ModelBindingTest(Form child)
{
    var model = new model.TestModel();
    child.DataContext = model;
    
    child.TextBound("Enter Number:")
        .TextBox(modelFieldName: "Number", numeric: true)
        .Button("Calculate", async () => 
        { 
            log.Info($"Calculated: {model.Number}")  ; 
        });
}
```

#### Loading Indicator Pattern
```csharp
public static void LoadingIndicatorTest(Form child)
{
    child.Model["IsLoading"] = false;
    
    child.VerticalGroup(vg =>
    {
        vg.HorizontalGroup(hg => { hg.Text("Ready"); },
                style: new Style { isHiddenModelName = "IsLoading" })
            .HorizontalStack(hs =>
            {
                hs.LoadingTextAnimation(style: new Style { width = 50 });
            }, style: new Style { isVisibleModelName = "IsLoading" })
            .Button("Toggle Loading", async () => 
            { 
                child.Model["IsLoading"] = !(bool)child.Model["IsLoading"]; 
            });
    });
}
```

### 4. Common Properties and Methods Available on `Form`

- `.Text(string)` - Add text label
- `.Button(string, Func<Task>)` - Add button with action
- `.TextBox(modelFieldName, options...)` - Bind to model property
- `.DropDown<T>(itemSourceModelName, selectedItemModelName, populateItemRow)` - Populate dropdown
- `.Table()` - Create table component
- `.LoadingIndicator()` - Show loading animation
- `.VerticalGroup(delegate)` / `.HorizontalGroup(delegate)` - Layout containers
- `.Panel<>()` - Panel-based UI construction
- `.DataContext = model` - Set data context for Two-Way Binding

### 5. Using Data Context (Two-Way Binding)

```csharp
public static void DataContextTest(Form child)
{
    var model = new YourModel();  // Your INotifyPropertyChanged class
    
    child.DataContext = model;  // Enable automatic binding
    
    child.Text("Age:")
        .TextBox("MyModel_Age", numeric: true);
}
```

### 6. Using Model Property Path (Manual Binding)

```csharp
public static void ManualBindingTest(Form child)
{
    child.Model["Counter"] = 0;
    
    child.Text("Counter:")
        .TextBoxFor("Counter")
        .Button("Increment", async () => 
        {
            child.Model["Counter"] = 
                Convert.ToInt32(child.Model["Counter"]) + 1;
        });
}
```

### 7. Creating Child Forms (Dialogs vs Show)

```csharp
public static void DialogVsShowTest(Form child)
{
    child.Button("Show (blocking)", async () =>
    {
        await child.DisplayChildForm(subform =>
        {
            subform.Text("This is a show child form");
        }, isDialog: false);  // Non-blocking
    }).Button("ShowDialog (modal)", async () =>
    {
        await child.DisplayChildForm(subform =>
        {
            subform.Text("This is a dialog");
        }, isDialog: true);   // Modal, blocks parent
    });
}
```

### 8. Testing Async Actions with Progress

```csharp
public static async void LongOperationTest(Form child)
{
    child.Model["Progress"] = 0;
    
    child.HorizontalGroup(hg =>
    {
        hg.Button("Start", async () =>
        {
            child.Model["Progress"] = 0;
            
            // Simulate work
            for (int i = 0; i <= 100; i++)
            {
                await Task.Delay(50);
                child.Model["Progress"] = i;
            }
        })
        .Progress(modelFieldName: "Progress", min: 0, max: 100);
    });
}
```

### 9. Menu Example

```csharp
public static void MenuTest(Form child)
{
    child.Menu(new[] {
        new MenuItem
        {
            Header = "File",
            Items = new[]
            {
                new MenuItem
                {
                    Header = "Save",
                    Action = () => { 
                        child.Model["LastAction"] = "Save"; 
                    }
                },
                new MenuItem
                {
                    Header = "Open",
                    Action = () => { 
                        child.Model["LastAction"] = "Open"; 
                    }
                }
            }
        }
    }).TextFor("LastAction");
}
```

### 10. Testing Child Object Binding

```csharp
public static void ChildObjectBinding(Form child)
{
    var model = new model.ContactWindowMainModel();
    child.DataContext = model;
    
    child.Panel<model.Contact>(modelFieldName: "Contact", builder =>
    {
        builder.HorizontalGroup(h =>
        {
            h.Text("Name:")
                .TextBoxFor(h, "DisplayName");
            
            h.Text("Email:")
                .TextBoxFor(h, "Email");
        });
        
        return builder;
    }).TextBound("Saved Contacts: ")
       .DropDown<model.Contact>(...)
       .TextBound("Result:");
}
```

## Important Notes

1. **Static Methods Only**: Test functions must be `public static void` methods
2. **No return values**: These are void operations for UI construction
3. **Async Operations**: Use `async` when awaiting tasks (e.g., dialogs, progress)
4. **Logger**: Use `log.Info()`, `log.Fatal()` to report actions/events
5. **Model Initialization**: Initialize model properties before binding
6. **Namespaces**: Keep using `TestApp.lib.TestFunctionGroups` namespace

## File Organization

```
TestApp/lib/TestFunctionGroups/
├── AGroup.cs            # General utility tests, edge cases
├── Button.cs            # Button-specific tests
├── Table.cs             # Table component tests
├── List.cs              # ListView tests
└── ...
```

When adding functions:
- **New group**: Create `<Component>.cs` file for that component's specific tests
- **General utility**: Use `AGroup.cs` with appropriate comment/header
- **Review AGroup**: Periodically move specialized tests to appropriate component groups