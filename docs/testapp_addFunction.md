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
    
    child.TextFor("Enter Number:")
        .TextBoxFor(modelFieldName: "Number",
            convertFromUIToModel: (text) => int.TryParse(text, out int n) ? n : 0)
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

- `.Text(string)` - Static text label
- `.TextFor(modelFieldName)` - Display a model property (bound label)
- `.TextBoxFor(modelFieldName, ...)` - Two-way bind a model property to a text box. For numeric
  fields pass `convertFromUIToModel: (text) => int.TryParse(text, out int n) ? n : 0`
  (there is no `numeric:` argument).
- `.Button(string, Func<Task>)` - Add button with action (the handler MUST return `Task`).
  There is also an overload `Button(Action<Form> populateButtonContent, Func<Task> onClick, ...)`.
- `.DropDown<T>(itemSourceModelName, selectedItemModelName, ...)` - Populate dropdown
- `.List<T>(itemSourcePropertyName, ...)` - Multi-select list
- `.Table<T>(...)` / `.TableView<T>(...)` - Table / DataGrid components
- `.LoadingTextAnimation(Style)` / `.SpinningElipse(Style)` - Loading animations
- `.VerticalGroup(delegate)` / `.HorizontalGroup(delegate)` / `.VerticalDock` / `.HorizontalStack` /
  `.VerticalStack` - Layout containers (each takes an `Action<Form>`)
- `.Panel<T>(modelFieldName, Action<Form>)` - Panel-based UI construction
- `f.Title = "..."` - Window title (a string PROPERTY, set by assignment, not a fluent call)
- `child.DataContext = model` - Set data context for Two-Way Binding

### 5. Using Data Context (Two-Way Binding)

```csharp
public static void DataContextTest(Form child)
{
    var model = new YourModel();  // Your INotifyPropertyChanged class
    
    child.DataContext = model;  // Enable automatic binding
    
    child.Text("Age:")
        .TextBoxFor("Age",
            convertFromUIToModel: (text) => int.TryParse(text, out int n) ? n : 0);
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

    // Panel<T>: the model field's value must already be of type T.
    // The populate delegate is an Action<Form> (no return value); its DataContext is the item.
    child.Text("Contact Editor")
        .Panel<model.Contact>(modelFieldName: "Contact", _f =>
        {
            _f.HorizontalGroup(h =>
            {
                h.Text("Name: ")
                    .TextBoxFor("DisplayName");   // row DataContext is the item, so no "h," prefix
            })
            .HorizontalGroup(h =>
            {
                h.Text("Email: ")
                    .TextBoxFor("Email");
            });
        })
        .TextFor("Results");
}
```

> Note: when inside a `Panel<T>` / `List<T>` / `DropDown populateItemRow` delegate, the row's
> `DataContext` is the item itself, so you bind by property name directly: `h.TextBoxFor("Email")`.
> There is no overload that takes a `Form` as the first argument of `TextBoxFor`.

## Important Notes

1. **Static Methods Only**: Test functions must be `static`. Discovered via
   `BindingFlags.Static | NonPublic | Public`, so `public`, `private`, or `internal` static methods
   all work (the examples below use `public static`).
2. **Two discovered signatures** (both take `Form` as their only parameter):
   - `void TestName(Form f)` — synchronous (legacy)
   - `Task TestName(Form f)` — **async is now supported** (use `static async Task TestName(Form f)`
     and `await` what you need, e.g. `await f.ObjectViewer(...)`). Exceptions in async tests are
     captured and written to the Log tab.
   - Prefer `async Task` over `async void` — `async void` does not match either delegate, so it is
     NOT discovered as a test, and its exceptions escape to the Avalonia dispatcher.
3. **No other return values**: Only `void TestName(Form)` or `Task TestName(Form)` are discovered.
   Every discovered test (both kinds) runs **as a child form** (`SetupChildForm = true`).
4. **Async Operations**: Use `async` when awaiting tasks (e.g., dialogs, progress)
5. **Logger**: Use `log.Info()`, `log.Fatal()` to report actions/events
6. **Model Initialization**: Initialize model properties before binding
7. **Namespaces**: Keep using `TestApp.lib.TestFunctionGroups` namespace

## File Organization

Each test function group lives in its own file under `TestApp/lib/TestFunctionGroups/`:

```
TestApp/lib/TestFunctionGroups/
├── AGroup.cs            # General/uncategorized tests, edge cases
├── Button.cs            # Button + ButtonFunctions / runtime style
├── Text.cs              # Text/TextFor/TextBoxFor/DateFor/keypress
├── List.cs              # List<T> (multi-select)
├── DropDown.cs          # DropDown<T>
├── Autocomplete.cs      # Autocomplete<T> (item source + async populator)
├── Table.cs             # Table<T> (DataGrid)
├── TableView.cs         # TableView<T>
├── TreeView.cs          # (tree-related tests)
├── ObjectViewer.cs      # ObjectViewer<T>
├── PathNavigator.cs     # PathNavigatorFor
├── FileSystem.cs        # FilePathFor / DirectoryPathFor / ShowOpenFolderDialog
├── Image.cs             # Image
├── Layout.cs            # Horizontal/Vertical Group/Stack/Dock + Tabs
├── ContextMenu.cs       # Style.contextMenu / contextMenuItems
├── Tooltip.cs           # Style.Tooltip / TooltipText
├── Events.cs            # onDisplay / onClosing child-form events
└── ...
```

When adding functions:
- **New group**: Create `<Component>.cs` file for that component's specific tests
- **General utility**: Use `AGroup.cs` with appropriate comment/header
- **Review AGroup**: Periodically move specialized tests to appropriate component groups