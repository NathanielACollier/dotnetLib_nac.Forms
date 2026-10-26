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

The easiest path is `Form.NewForm()`, which creates the Avalonia app and a main `Form` together:

```csharp
// Creates the Avalonia app + a main form for you:
var form = Form.NewForm();

// Or supply a ConfigureAppBuilder delegate to tweak the app builder before it starts:
var app = Form.NewForm(app =>
{
    app.LogToDebug(Avalonia.Logging.LogEventLevel.Verbose); // optional logging
});
```

> `SetupAvaloniaApp(...)` exists in `Form.cs` but is **internal** — external consumers should use
> `Form.NewForm(...)`. If you build the form yourself instead, the public constructor is
> `Form(Application app, BindableDynamicDictionary model = null)`.

#### Creating a form and model manually

```csharp
var model = new BindableDynamicDictionary();
model.Set("myValue", "initial value");

// The main form's window title is a string PROPERTY (assignment, not a fluent call):
form.Title = "My Form";

form
    .TextFor("myValue")
    .TextBoxFor("myEditField", isPassword: true)
    .Button("Click me", async () => { /* async handler required */ });

// For the main (top-level) form, Display() starts the app:
form.Display(width: 800, height: 600);
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
| `TextBoxFor(string modelFieldName, bool multiline=false, Style style=null, Action<string> onTextChanged=null, bool isPassword=false, bool isReadOnly=false, string watermarkText=null, Func<string,object> convertFromUIToModel=null, Func<KeyEventArgs,Task> onKeyPress=null)` | Text input | **Two-way binding by default**. UI→model conversion via `convertFromUIToModel` (e.g. parse `int`). `onKeyPress` fires on `KeyUp`. Multiline sets `AcceptsReturn/Tab` and wraps text. |
| `TextBoxFor(... onTextChanged: Action<string>)` | Change notifications | Uses Observable subscription (Avalonia issue workaround). |

> `TextBoxFor` exposes only `convertFromUIToModel` on its public signature (direction UI→model).
> The model→UI direction (`convertFromModelToUI`) exists only inside the internal `AddBinding<T>`
> mechanism, not as a `TextBoxFor` parameter.  For numeric fields, use `convertFromUIToModel`,
> e.g. `convertFromUIToModel: (text) => int.TryParse(text, out int n) ? n : 0`.

### Selection Controls

| Method | Purpose | Key Options |
|--------|---------|--------------|
| `DropDown<T>(string itemSourceModelName, string selectedItemModelName, Action<T> onSelectionChanged=null, Action<Form> populateItemRow=null, Style style=null)` | ComboBox dropdown | `itemSourceModelName` = list of T (model field); `selectedItemModelName` = selected item (two-way bind). `populateItemRow` supplies a `FuncDataTemplate` for custom row rendering. `onSelectionChanged` fires on selection. |
| `List<T>(string itemSourcePropertyName, Action<Form> populateItemRow=null, Style style=null, Action<IEnumerable<T>> onSelectionChanged=null, bool wrapContent=false)` | ListBox multi-select | Binds ItemsSource to `IEnumerable<T>`. Selection is `SelectionMode.Multiple`; `onSelectionChanged` gives `IEnumerable<T>`. `wrapContent` wraps items in a `WrapPanel`. |
| `SimpleDropDown<T>(List<T> items, Action<T> onItemSelected=null)` | Simple dropdown (no model) | Non-model binding version for quick lists. |
| `Autocomplete<T>(string selectedItemModelName, string itemSourceModelName=null, string selectedTextModelName=null, Action<T> onSelectionChanged=null, Action<Form> populateItemRow=null, Style style=null, Func<string,Task<IEnumerable<T>>> populateItemsOnTextChange=null)` | AutoCompleteBox | Exactly one of `itemSourceModelName` or `populateItemsOnTextChange` is required (not both). `selectedTextModelName` sets the `ItemSelector` used to render the chosen item's text. |

Autocomplete Box (exactly one of `itemSourceModelName` or `populateItemsOnTextChange`):
```csharp
// Bind to an existing list on the model:
form.Autocomplete<string>(
    itemSourceModelName: nameof(Model.Items),
    selectedItemModelName: nameof(Model.SelectedItem),
    onSelectionChanged: (item) => { /* ... */ }
);

// Or supply items dynamically as the user types (AsyncPopulator):
form.Autocomplete<string>(
    selectedItemModelName: nameof(Model.SelectedItem),
    populateItemsOnTextChange: async (text) => FilteredItems,
    onSelectionChanged: (item) => { /* ... */ }
);
```

### Path Navigator (generic folder/tree path builder)

A generic hierarchical-path navigator (the Avalonia equivalent of the WPF `FolderPathControl`).
It is **not** tied to the disk — what a "path" means is up to your populator function.

`PathNavigatorFor(string pathFieldName, Func<string, string, Task<IEnumerable<string>>> populateSubFolders, string pathSeparator = "\\", string initialPath = null, Func<string,Task> onPathChanged = null, Style style = null)`

- **pathSeparator** — the string between segments. Default is `\`, but can be `/`, `::`, `:` … anything. This is what makes the control generic (file paths, namespaced/virtual paths, DB trees, etc).
- **populateSubFolders `Func<string,string,Task<IEnumerable<string>>>`** — your function. It receives
    1. the already-committed path, and
    2. the partial text the user has typed for the *next* segment name.
  It returns the list of candidate segment names for that level (you do the filtering).
- **pathFieldName** — the model field that stores the full committed path (two-way bound; the model is always the source of truth).
- **onPathChanged `Func<string,Task>`** — notified whenever the committed path changes (user picks a segment, goes up, or the model changed elsewhere).

The control layout is `[ committed path ] [ next-segment autocomplete ] [ Up ]`:
the user types/picks the next segment (append the separator + segment), or clicks **Up** to pop the last segment.

```csharp
f.PathNavigatorFor("myServerPath",
    populateSubFolders: async (committedPath, partialText) =>
    {
        var children = await myClient.GetChildren(committedPath);   // your source (folders, API, DB…)
        if (string.IsNullOrWhiteSpace(partialText)) return children;
        return children.Where(n => n.StartsWith(partialText, System.StringComparison.OrdinalIgnoreCase));
    },
    pathSeparator: "\\",
    initialPath: "Root\\Projects",
    onPathChanged: async newServerPath => { await DoSomething(newServerPath); })
    .Text("Committed path: ")
    .TextFor("myServerPath");
```

> Pure string logic is exposed + unit-tested headless: `PathNavigator.JoinPath(committed, segment, separator)` and
> `PathNavigator.PopTopSegment(committed, separator)`.
> (The PathNavigator TestApp group ends with `.ObjectViewer(tree)` to inspect the fake folder tree as you navigate.)

### ObjectViewer (object inspector tree)

A read-only inspector that renders any object as an expandable `TreeView`.
Useful in TestApp / dev screens for inspecting models, config objects, or intermediate data.

`async Task<Form> ObjectViewer<T>(T initialItemValue = null, ObjectViewerFunctions<T> functions = null)  where T : class`

- **initialItemValue** — the object to render on load. May be `null` if you plan to supply one later via the update function.
- **functions** — an `ObjectViewerFunctions<T>` with a single delegate, `updateValue`. The ObjectViewer **assigns** this delegate as it is created; call it later (with a new object) to fully rebuild the tree.

```csharp
public class ObjectViewerFunctions<T>
{
    public Func<T,Task> updateValue;   // assigned by ObjectViewer once the tree is built
}
```

#### What it renders

Rendering is done by `nac.Forms.lib.ObjectPropertiesView.TreeViewObjectPropertiesBuilder.BuildTree(...)`, which walks the object's graph. For each node it checks, in order:

1. **Primitives** (`int`, `bool`, `string`, other non-generic value types) — the value is rendered as plain node text.
2. **KeyValuePair entries** (what `Dictionary`s, `SortedDictionary`s, etc. enumerate over) — rendered as **one bold node showing the key**, with the value rendered underneath. There is **no `KeyValuePair\`2` type-name wrapper node**.
3. **Collections** (`List<T>`, arrays, dictionaries…) — a single "Collection" node. Each entry is then rendered using rule 2 for KeyValuePairs, rule 1 for primitives, a type-name node for other objects, or a red `Index[n]=NULL` node for null entries.
4. **XML LINQ objects** (`XDocument` / `XElement` / `XAttribute`) — an "XML" node; elements by tag name, attributes by name, element text under a "Value" node.
5. **Any other object** — every auto-implemented **property**, then every public **field**, becomes a bold node with the value's subtree attached (recurse into rules 1–4 for each value).

Safety behavior:

- **Cycle guard** — object identity is tracked per build; a class instance is rendered at most once, so self-referencing / recursive object graphs cannot infinite-loop.
- The whole tree is rebuilt from scratch on every `BuildTree` call (the old tree is cleared first).

Expansion of nodes is controlled by `NodeExpandSettings` (booleans: `ExpandRoot`, `ExpandCollections`, `ExpandPropertyName`, `ExpandPrimativeValues`, `ExpandXML`, `ExpandNullItem`). All `true` expands everything, all `false` leaves the tree collapsed. `ObjectViewer` itself builds with everything expanded.

#### Example

```csharp
// Simple inspector of an object
await f.ObjectViewer(initialItemValue: new
{
    A = "Dinosaur",
    B = "Penguin",
    Nested = new Dictionary<string, string> { ["One"] = "Uno" }
});

// Object viewer refreshed by a running counter
var ops = new nac.Forms.Form.ObjectViewerFunctions<object>();
var model = new { Message = "Initial", Counter = 0 };

f.Button("Increment", async () =>
{
    model = new { Message = "Incremented", Counter = model.Counter + 1 };
    await ops.updateValue(model);   // rebuilds the tree with the new object
});

await f.ObjectViewer<object>(initialItemValue: model, functions: ops);
```

### Buttons

```csharp
// Simple button; onClick MUST return a Task (Avalonia runs on the UI thread),
// so use `async () => ...`:
form.Button("Click me", async () => { await SomeWork(); });

// Custom button content via a populate delegate (this overload takes the delegate first):
form.Button(populateButtonContent: bf => bf.TextFor("BtnLabel"), onClick: async () =>
{
    /* ... */
});

// Runtime style mutation via ButtonFunctions:
var fns = new nac.Forms.Form.ButtonFunctions();
form.Button("Incriment", async () => { }, functions: fns);
// ...later:
fns.setStyle?.Invoke(new Style { backgroundColor = Colors.Purple });
```

### Tables (DataGrid) and TableView

`Table<T>` is the Avalonia `DataGrid`; `TableView<T>` is the Avalonia `TableView`.
Both are generic and require the model field to hold an `IEnumerable<T>`.

```csharp
form.Table<Model.Row>(
    itemsModelFieldName: nameof(Model.Items),     // must be IEnumerable<T>
    columns: new[] {
        new Column { Header="Name", modelBindingPropertyName="Name" },
        new Column { Header="Age", modelBindingPropertyName="Age" }
    },
    autoGenerateColumns: false,                   // only when you pass explicit columns
    onVisibleRowsChanged: rows => Console.WriteLine(rows.Count)  // rows is List<DataGridRow>
);

// Without explicit columns, let it auto-generate from the model (autoGenerateColumns = true by default).
form.Table<Model.Row>(nameof(Model.Rows));

// A column can also carry a template (custom cell UI) instead of a binding:
form.Table<Model.Alphabet>("list", columns: new[]
{
    new Column { Header="My Counter",
        template = row => row.Button("Incriment", async () => { /* ... */ }) }
});
```

#### TableView

```csharp
form.TableView<Model.Alphabet>(
    itemsModelFieldName: "list",
    columns: new[] { new Column { Header="Duplicate of X", modelBindingPropertyName="X" } },
    style: "width:1600px;"
);
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

`Panel<T>` binds a model field to a `ContentControl`. The field's current value must already be
of type `T` (it throws otherwise). The populate delegate builds the content from that item.

```csharp
// Binds a model field to the panel's content
form.Panel<Model.Page>(
    modelFieldName: nameof(Model.Page),
    populatePanel: pageForm =>
    {
        // This form displays whatever is bound to Page
        pageForm.TextFor(nameof(Model.Page.Title))
             .TextBoxFor(nameof(Model.Page.Content));
    });
```

### File System Controls

| Method | Purpose |
|--------|---------|
| `FilePathFor(string fieldName, string fileFilter=null, string initialFileName=null, bool fileMustExist=true, Func<string,Task> onFilePathChanged=null, FilePathFor_Functions functions=null)` | File open/save picker; two-way binds the path string to a model field. `fileMustExist: false` allows typing/creating a new path. `fileFilter` is an Avalonia-style filter like `"Word (*.docx)\|*.docx\|\|All (*.*)\|*.*"`. |
| `DirectoryPathFor(string fieldName, Func<string,Task> onDirectoryPathChanged=null)` | Folder picker; two-way binds the directory path to a model field. |
| `ShowOpenFolderDialog()` | Returns `Task<string>` via the Avalonia storage API (no UI control added). Returns `""` if the user cancels. Use it from a button: `f.Model["path"] = await f.ShowOpenFolderDialog();` |

```csharp
f.FilePathFor("myPath",
        fileFilter: "Word (*.docx)|*.docx||All (*.*)|*.*",
        initialFileName: "test.docx",
        fileMustExist: false,
        onFilePathChanged: async (newFileName) => { /* ... */ })
    .TextFor("myPath");

f.DirectoryPathFor("myDir",
        onDirectoryPathChanged: async (newDir) => { /* ... */ });
```

### Menu

```csharp
f.Menu(new[]
    {
        new MenuItem
        {
            Header = "File",
            Items = new[]
            {
                new MenuItem { Header = "Save", Action = () => { f.Model["last"] = "Save"; } },
                new MenuItem { Header = "Open", Action = () => { f.Model["last"] = "Open"; } }
            }
        }
    })
    .TextFor(nameof(Model.Last));
```

> `MenuItem` is `nac.Forms.model.MenuItem` with `Header`, `Action`, and a nested `Items`
> collection.  A context menu can also be attached to any control through
> `Style.contextMenu` / `Style.contextMenuItems`.

### Loading Indicators

| Method | Purpose |
|--------|---------|
| `LoadingTextAnimation(Style style=null)` | Animated "loading…" text control. Usually paired with `Style.isVisibleModelName` / `Style.isHiddenModelName`. |
| `SpinningElipse(Style style=null)` | Spinning-ellipse spinner (note the spelling). |

```csharp
f.HorizontalStack(h =>
{
    h.Text("Loading").LoadingTextAnimation(style: new Style { width = 20 });
}, style: new Style { isVisibleModelName = "InProgress" });
```

### Image

`Image(string modelFieldName, Style style=null)` binds a model field holding a `byte[]` to an
Avalonia `Image` (the bytes are decoded to a `Bitmap`, with an empty bitmap placeholder if null).
You can assign image bytes to the model field from a download, a file, or an embedded resource.

```csharp
f.Model["img"] = someBytes;     // byte[]
f.Image("img", style: new Style { width = 30 });
```

### Date

`DateFor(string modelFieldName, Style style=null)` renders a `DatePicker`. The model field may be a
`DateTime`, `DateTimeOffset?`, or a parseable `string` (two-way bound).

```csharp
f.Model["currentDate"] = DateTime.Now;
f.DateFor("currentDate");
```

### Closing the Window / Child Forms

| Method | Purpose |
|--------|---------|
| `Close()` | Closes the current form's window. |
| `DisplayChildForm(Action<Form> setup, int height=600, int width=800, Func<Form,Task<bool?>> onClosing=null, Func<Form,Task> onDisplay=null, bool useIsolatedModelForThisChildForm=false, bool isDialog=false)` | Opens a child form. `isDialog: true` makes it modal (`ShowDialog`); `false` shows a separate window. `onClosing` returning `true` cancels the close. |
### Closing the Window / Child Forms

| Method | Purpose |
|--------|---------|
| `Close()` | Closes the current form's window. |
| `DisplayChildForm(Action<Form> setup, int height=600, int width=800, Func<Form,Task<bool?>> onClosing=null, Func<Form,Task> onDisplay=null, bool useIsolatedModelForThisChildForm=false, bool isDialog=false)` | Opens a child form. `isDialog: true` makes it modal (`ShowDialog`); `false` shows a separate window. `onClosing` returning `true` cancels the close. |
| `DebugAvalonia()` | No-op in Avalonia 12 (dev tools were removed), kept for API compatibility. |

```csharp
f.Button("Show", async () =>
{
    await f.DisplayChildForm(child => child.Text("I'm show"), isDialog: false);
});
```

---

## Styling

Every builder control accepts a `Style` parameter (type `nac.Forms.model.Style`).

### Two ways to create a Style

1. **Implicit CSS string** — `Style` has an implicit operator from `string`, so a CSS-like text
   (`nac.CSSParsing`) is accepted anywhere a `Style` is expected:
   ```csharp
   f.Text("Colorful", style: "background-color: #FFBA00; color: #dde9e7");
   ```
2. **Object initializer** — set the individual properties:
   ```csharp
   f.Text("Hi", style: new Style
   {
       foregroundColor = Colors.Green,
       backgroundColor = Colors.Black,
       width = 100,
       height = 50,
       fontSize = 16
   });
   ```

### Style properties (from `model/Style.cs`)

| Property | Type | Purpose |
|----------|------|---------|
| `width` | `Optional<int>` | Control width in pixels |
| `height` | `Optional<int>` | Control height in pixels |
| `foregroundColor` | `Optional<Color>` | Text / foreground color |
| `backgroundColor` | `Optional<Color>` | Background color |
| `fontSize` | `Optional<int>` | Text font size |
| `fontWeight` | `Optional<FontWeight>` | Text font weight |
| `isVisibleModelName` | `Optional<string>` | Model field; when true the control is shown |
| `isHiddenModelName` | `Optional<string>` | Model field; when true the control is hidden |
| `contextMenu` | `Action<Form>` | Build a right-click context menu via a Form |
| `contextMenuItems` | `Optional<IEnumerable<MenuItem>>` | Declarative context menu items |
| `Tooltip` | `Action<Form>` | Build a complex tooltip via a Form |
| `TooltipText` | `Optional<string>` | Simple text tooltip |

> Note: each `Optional<>` tracks whether it was set (`.IsSet`), so a property that is not
> assigned does not force the underlying Avalonia property to a default value.

---

## Special Keys & Model Handling

### Reserved property names on DataContext:

| Key | Purpose |
|-----|---------|
| `model.SpecialModelKeys.DataContext` | The ViewModel object itself (INotifyPropertyChanged source) |

> `SpecialModelKeys` only defines `DataContext` (a string constant `"datacontext"`).
> There is no `ModelContext` key.

Example to set typed context:
```csharp
model.Set(SpecialModelKeys.DataContext, new MyViewModel());
// or simply assign the DataContext property directly:
form.DataContext = viewModel;
```

---

## Helper Methods on Form

| Method | Purpose |
|--------|---------|
| `getModelValue(string prop)` | Retrieves value from model for a property (respects DataContext paths). Returns `DataContextValueResult` (has `.Value`, `.invalid`, `.DataContext`). |
| `setModelValue(string prop, val)` | Sets property on model |
| `setModelIfNull(string prop, val)` | Only sets if the current value is null |
| `InvokeAsync(Func<Task>)` | Executes action on Avalonia GUI thread |

> `getBindingSource()` is **private** (it exists in `Form.DataContext.cs` but is not part of the
> public API). It resolves to the `INotifyPropertyChanged` source: the `DataContext` if one is
> set on the model, otherwise the `BindableDynamicDictionary` model itself.

### Extension Hooks (`_Extend_` prefix)

`Form.ExposeInternals.cs` exposes the public building blocks that extension libraries use to add
their own controls to a `Form`. By convention these are prefixed `_Extend_`:

| Method | Purpose |
|--------|---------|
| `_Extend_AddRowToHost(Control ctrl, AddRowToHostFunctions functions=null, bool rowAutoHeight=true)` | Adds a raw Avalonia `Control` to the form. `functions.show` / `functions.hide` toggle its `IsVisible`. |
| `_Extend_AddBinding<T>(string modelFieldName, AvaloniaObject control, AvaloniaProperty<T> property, bool isTwoWayDataBinding=false)` | Wires a model field to an Avalonia control's property (the public entry point to the binding engine). |
| `_Extend_AccessApp(Action<Application>)` | Access the underlying `Avalonia.Application`. |
| `_Extend_AccessHost(Action<Grid>)` | Access the form's root `Grid` host. |

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

main.Title = "Simple App";
   main.TextFor(nameof(UserName))
   .TextBoxFor(nameof(UserEmail))
   .Button("Register", async () => { return await Register(user); })
   .Display();


model = new BindableDynamicDictionary();
model.Set(nameof(UserName), "alice@example.com");

var form = Form.NewForm();
form.TextFor(nameof(Subject))
     .TextBoxFor(nameof(MyNotes));
form.Title = "My Notes App";
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
