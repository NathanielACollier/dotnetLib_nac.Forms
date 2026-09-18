# nac.Forms - Documentation for opencode

## Overview

**nac.Forms** is a .NET library that provides a fluent API for creating interactive GUI applications using **Avalonia UI**. Instead of writing raw XAML or building controls manually, it offers high-level builder methods (like `TextBoxFor()`, `Button()`, etc.) that automatically wire up model binding.

### Core Capabilities:
- Creates Windows Forms with Avalonia cross-platform rendering (Windows, Linux, macOS)
- Auto-wires UI controls to INotifyPropertyChanged models
- Supports both `BindableDynamicDictionary` (dynamic) and strongly-typed DataContext
- Provides pre-built controls for lists, tables, dropdowns, autocomplete, etc.

### NuGet Package Info:
```xml
<PackageId>nac.Forms</PackageId>
<Version>12.0.3</Version>
<ProjectUrl>https://github.com/NathanielACollier/dotnetCoreAvaloniaNCForms</ProjectUrl>
```

---

## Setup & Usage

### 1. Installing the Package

```bash
dotnet add package nac.Forms --prerelease
# or for local project, reference directly to nuget source
```

### 2. Basic App Structure

```csharp
// Step 1: Create and configure Avalonia app (optional if using NewForm)
var app = Form.SetupAvaloniaApp()
    // .LogToDebug(LogEventLevel.Verbose) // optional logging
    ;

// Step 2: Create a form and model
var model = new BindableDynamicDictionary();
model.Set(nameof(model.MyProperty), "initial value");

var form = new Form(app, model);

// Step 3: Wire up controls
form
    .Title("My Form")
    .TextFor(nameof(MyValue))
    .TextBoxFor(nameof(MyEditField), isPassword: true)
    .Button("Click me", _ => LogInfo());

// Step 4: Display the form
// For main (top-level) forms:
form.Display(width: 800, height: 600);

// OR using NewForm helper (creates app + form automatically):
var newForm = Form.NewForm(); // returns ready-to-use Form with app
newForm
    .TextFor(nameof(MyName))
    .Display();
```

### 3. The Model Pattern

Forms work with `INotifyPropertyChanged` models. Two approaches:

#### Approach A: BindableDynamicDictionary (Recommended for simple cases)
```csharp
using nac.utilities;

var model = new BindableDynamicDictionary();
model.Set(nameof(Name), "Alice");
model.Set(nameof(Age), 30);
// Any property set becomes an INotifyPropertyChanged member
```

#### Approach B: Strongly-typed ViewModel
```csharp
public class MyViewModel : INotifyPropertyChanged
{
    public string Name { get; set; } = "Bob";
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    public void Notify() => PropertyChanged?.Invoke(this, EventArgs.Empty);
}

var form = new Form(app);
form.DataContext = new MyViewModel(); // binds typed model to form

form.TextFor("Name").TextBoxFor(nameof(viewModel.MyValue));
```

---

## Control Builders (Facts I Need to Remember)

### Text / Display Controls

| Method | Purpose | Binding Details |
|--------|---------|-----------------|
| `Text(string value, style)` | Static text label | Manual set of Text property |
| `TextFor(string modelProp, defaultValue=null, style)` | Bound label | Binds to string; auto-notifies on property change |
| `Progress(string prop, min, max, style)` | Progress bar | Double binding (progress value) |

### Input Controls

| Method | Purpose | Key Options |
|--------|---------|--------------|
| `TextBoxFor(string prop, multiline=false, style, onTextChanged=null, isPassword=false, isReadOnly=false, watermarkText=null, onKeyPress=null)` | Text input | **Two-way binding by default**. Converters via `convertFromModelToUI` / `convertFromUIToModel`. Key press events supported. Multiline sets `AcceptsReturn/Tab`, wraps text. |
| `TextBoxFor(... onTextChanged: Action<string>)` | Change notifications | Uses Observable subscription (Avalonia issue workaround) |

### Selection Controls

| Method | Purpose | Key Options |
|--------|---------|--------------|
| `DropDown<T>(string itemsSourceProp, string selectedItemProp, style)` | ComboBox dropdown | `itemsSourceProp` = list of T; `selectedItemProp` = selected item (two-way bind). Supports FuncDataTemplate for custom row rendering. OnSelectionChanged fires on selection. |
| `List<T>(string itemsSourceProp, populateItemRow=null, style, onSelectionChanged, wrapContent=false)` | ListBox multi-select | Binds ItemsSource to IEnumerable<T>. Selection supports multiple; wraps content if enabled. |
| `SimpleDropDown<List<T>>(items, onItemSelected)` | Simple dropdown (no model) | Non-model binding version for quick lists |

Autocomplete Box:
```csharp
form.Autocomplete(
    selectedItemProp: nameof(Model.SelectedItem),
    itemSourceModelName: nameof(Model.Items), // optional - use AsyncPopulator instead
    selectedTextModelName: nameof(Model.SelectedText), // maps to ItemSelector text extraction
    populateItemsOnTextChange: async (text) => FilteredItems, // dynamic autocomplete
    onSelectionChanged: ...
);
```

### Buttons

```csharp
form.Button("Click me", async () => { await SomeWork(); });

// With custom content:
form.Button(displayText: "", 
            populateButtonContent: bf => form.TextFor("BtnLabel"), // adds bound control as button content
            onClick: async () => ...);

// Async click handler is required (Avalonia runs on UI thread)
```

### Tables (DataGrid)

```csharp
form.Table(
    itemsModelFieldName: nameof(Model.Items),         // must be IEnumerable<T>
    columns: new[] {
        new Column { Header="Name", modelBindingPropertyName="Name" },
        new Column { Header="Age", modelBindingPropertyName="Age" }
    },
    autoGenerateColumns: false,                        // if you provide column definitions
    onVisibleRowsChanged: rows => Console.WriteLine(rows.Count)
);

// Without explicit columns, specify a list first and let it generate:
var colDefs = new List<Column> 
{ 
    new() { Header="Name", modelBindingPropertyName="FullName" } 
};
form.Table(nameof(Model.Rows), columns: colDefs);
```

### Layout Containers

These work by passing an `Action<Form>` lambda to populate a secondary form.

| Method | Purpose |
|--------|---------|
| `VerticalDock(Action<Form> populateVert, style)` | DockPanel (top-to-bottom) |
| `HorizontalStack(Action<Form> popHoriz, style)` | StackPanel horizontal |
| `VerticalStack(Action<Form> popVert, style)` | StackPanel vertical |
| `VerticalGroup(popVert, isSplit=false)` | Grid with optional splitters between rows |
| `HorizontalGroup(popHoriz, isSplit=false)` | Grid with optional splitters between columns |

Example:
```csharp
form.VerticalDock(vertForm => 
{
    vertForm.TextFor("HeaderTitle")
         .TextBoxFor(nameof(InputData))
         .Button("Submit", _ => HandleSubmit());
});
```

### Panels (Dynamic Content)

```csharp
// Binds a model field to the panel's content
form.Panel<T>(modelFieldName: nameof(Model.Page), pageForm => 
{
    // This form displays whatever is bound to Page
    pageForm.TextFor("PageTitle")
          .TextBoxFor(nameof(Content));
}, style);
```

---

## Styling

Controls support a `Style` parameter (type `nac.Forms.model.Style`). Available properties:

| Property | Avalonia Property | Example Usage |
|----------|------------------|---------------|
| `width`, `height` | Control.Width/Height | `.Style(w: 200, h: 50)` |
| `marginTop`, etc. | Margin properties | `.Style(mTop: 10)` |
| `fontSize`, `fontColor`, etc. | TextBlock.FontSize/FontColor | Style for labels |
| `isDisabled` | IsEnabled=false | `.Style(isDisabled: true)` |

---

## Special Keys & Model Handling

### Reserved property names on DataContext:

| Key | Purpose |
|-----|---------|
| `model.SpecialModelKeys.DataContext` | The ViewModel object itself (INotifyPropertyChanged source) |
| `model.SpecialModelKeys.ModelContext` | Internal - avoid using |

Example to set typed context:
```csharp
model.Set(nameof(SpecialModelKeys.DataContext), new MyViewModel());
form.DataContext = model[SpecialModelKeys.DataContext]; 
// or simply: form.DataContext = viewModel;
```

---

## Helper Methods on Form

| Method | Purpose |
|--------|---------|
| `getBindingSource()` | Returns the INotifyPropertyChanged source (returns model, or DataContext if set) |
| `getModelValue(string prop)` | Retrieves value from model for a property (respects DataContext paths) |
| `setModelValue(string prop, val)` | Sets property on model |
| `setModelIfNull(string prop, val)` | Only sets if the current value is null |
| `InvokeAsync(Func<Task>)` | Executes action on Avalonia GUI thread |

---

## Key Facts About Binding Mechanism

### How two-way works:
1. **Observable Subject** created per binding (via `nac.Forms.Reactive.Subject<T>`)
2. UI control binds to this Subject
3. When model changes, we fire Subject, which notifies the control
4. For two-way: Control observable is subscribed; value changes write back to model property
5. Converters (`convertFromModelToUI`, `convertFromUIToModel`) are called before/after transformation

### Pathing for nested properties:
```csharp
// Nested property binding example
form.TextBoxFor(nameof(Outer.InnerProp)); // splits on "."
```

---

## Error Handling & Known Gotchas

- **Cannot call Display() twice** on same form (isDisplayed flag enforces)
- **Child forms must use DisplayChildForm**, not Display() — main app form manages lifecycle
- **DataGrid ItemsSource must be IEnumerable<T>** — check model type before binding
- Use **`BindableDynamicDictionary` for list items if using dynamic typing** — otherwise throw on 0-item lists

---

## Quick Start Example (Complete)

```csharp
using nac.Forms;
using nac.utilities;

// Create app and form together, or configure your own app first
var main = Form.NewForm(ConfigureAppBuilder: app =>
{
    // app.LogToDebug(...)// enable if needed
});

main.Title("Simple App")
   .TextFor(nameof(UserName))
   .TextBoxFor(nameof(UserEmail))
   .Button("Register", async () => { return await Register(user); })
   .Display();


model = new BindableDynamicDictionary();
model.Set(nameof(UserName), "alice@example.com");

var form = Form.NewForm();
form.TextFor(nameof(Subject))
     .TextBoxFor(nameof(MyNotes));
form.Title("My Notes App");
```

---

## Related Projects / Dependencies

From the `.csproj`:
| Package | Purpose |
|---------|---------|
| `Avalonia.Desktop` 12.1 | UI rendering framework |
| `nac.Logging` 1.0.10 | Logging infrastructure |
| `nac.utilities` 1.0.5 | BindableDynamicDictionary, etc. |
| `nac.CSSParsing` 1.0.2 | CSS-like styling parser (Style support) |

---

## Testing / Debugging

```csharp
form.DebugAvalonia(); // attaches Avalonia dev tools (F12 in older ver; Avalonia 12 removed)
```

Check `Form.Host.Content` for grid layout being built before display.

---

*Generated by opencode analyzing nac.Forms source code on 2024.*
