# AGENTS — Headless testing of nac.Forms UI (rendering windows in unit tests)

> Purpose: durable record of how to render nac.Forms windows **headless**
> (no real screen / window) so tests can assert on the **visual tree** —
> e.g. that a `DataGrid` actually materializes `DataGridRow`s when its bound
> list changes.  Written so a fresh context can continue.
>
> Scope goal: **usable for ALL of nac.Forms' functionality**, not just the
> table — text, buttons, dropdowns, lists, child forms, heal-on-parent-set,
> visible-row counts, etc.

Companion doc: `AGENTS_nacFormsChildBindingTodo.md` (the binding fix this
tests).

---

## 0. STATUS LINE (read this first — last updated in-session)

- The **binding fix is DONE + green** (7/7 `NestedPathBindingTests`). See
  `AGENTS_nacFormsChildBindingTodo.md` §6/§9.
- **HEADLESS RENDER NOW WORKS.** `Tests/Testing/HeadlessHealTests.cs` passes
  (run filtered — see §6 CONTAMINATION): a real `nac.Forms.Form` is built, shown
  in a headless window, and its `DataGrid` is asserted on the actual
  `DataGridRowsPresenter` visual tree.  The heal scenario asserts **0 rows → 3
  rows** when the nested-parent binding heals.  A generic harness
  (`Tests/Testing/HeadlessForm.cs`) wraps the session + pump + row count.
- **The single most important fact** (§6 "ROOT CAUSE"): the headless platform
  (`IWindowingPlatform`) is registered *only inside the `Dispatch` scope*. So
  `Win.Show()` + reading the visual tree **must happen inside**
  `session.Dispatch(() => …)`. Doing it outside → the "Unable to locate
  IWindowingPlatform" error.  The harness enforces this — callers never touch the
  session directly.
- **THREE gotchas that cost time this session (see §6):**
  1. The app the session starts needs a **theme AND the DataGrid control style**
     loaded, or the grid has no `DataGridRowsPresenter` → 0 rows.
  2. **Every read** (including `ItemsSource` and the visual tree) must run inside
     a `Dispatch` — Avalonia controls are UI-thread-owned, so a read from the test
     thread throws "The calling thread cannot access this object".
  3. **Never nest a `Dispatch`.** A `Dispatch` posted from *inside* a `Dispatch`
     (on the UI thread) deadlocks.  `Build` pumps *inline*, not via the public
     `Pump()`.
- **CONTAMINATION (§6, last bullet):** a real-app test (the `UsePlatformDetect()`
  `Form.NewForm()` path in `FormTests`/`NestedPathBindingTests`) poisons the
  one-shot global Avalonia state, so a headless render that runs *after* it in the
  same process materializes 0 rows.  MSTest 2.2.10 has **no** `[IsolatedProcess]`,
  so **run the headless tests filtered / in their own process** (command in §6b).
- **Deferred:** promoting the harness to its own `nac.Forms.Testing` project
  (§3.1).  For now it lives in `Tests/Testing/` so `Tests` is the only thing
  wired up.

---

## 1. Why the existing tests can't render

`DataGridRow` / `TableViewRow` visuals only exist after a window is
**attached to a visual tree and runs a layout/render pass**. The pieces that
depend on that:

- `DataGridVisibleRowsObserverRepo` / `TableViewVisibleRowsObserverRepo`
  (`nac.Forms/repos/*.cs`): they fire only after
  `DataGrid.AttachedToVisualTree` → `Dispatcher.UIThread.Post(Loaded)` →
  `LayoutUpdated`.  The "visible rows" count nac.Forms exposes needs all of it.
- `DataGrid` **virtualizes** rows into `DataGridRowsPresenter.Children`; rows
  materialize in a measure/arrange pass, and only when the window has a
  non-zero size.

Current `nac.Forms` entry points:
- `Form.Display(...)` → `this.app.Run(win)` (`Form.cs:262`) — runs the **blocking
  Avalonia message loop** on the calling thread.  Cannot be used in a test
  (blocks; also can't do a second `SetupWithoutStarting()` in the same process —
  Avalonia throws "Setup was already called on one of AppBuilder instances").
- `Form.Display_Internal(...)` (`Form.cs:181`) — set size/content, `win.Show()`,
  **returns** without running the loop.  This is the "attach + show without
  Run" hook we need; it's `internal`.
- `SetupAvaloniaApp()` (`Form.cs:97`, `internal static`) — `AppBuilder
  .Configure<App>().UsePlatformDetect().SetupWithoutStarting()`.  **Real screen**,
  runs once per process, no teardown.

So: to render headless we need (a) a headless app (no `UsePlatformDetect`
screen), (b) a controlled lifecycle that doesn't block, and (c) a way to **pump
the dispatcher** so attach + load + layout happen.

---

## 2. What's available

- `Avalonia.Headless` **12.1.2 is already in the nuget cache**
  (`%USERPROFILE%\.nuget\packages\avalonia.headless\12.1.2`).  No network
  restore required.
- `nac.Forms` is a class library; `Tests` is MSTest (`net10.0`) referencing it.
- The only existing "run a UI on its own thread" code is
  `nac.Forms.lib.AvaloniaAppManager` (spins a Windows STA thread, sets
  `Application.Current`, builds forms, calls `app.Run`).  Its `DisplayForm`/
  `InvokeAsync`/`Shutdown` pattern is the closest precedent, but it still ends in
  the blocking `app.Run` — not usable headless as-is.

---

## 3. Design decisions (record as they firm up)

### 3.1 Where the harness lives
DECISION: create a small **`nac.Forms.Testing`** project in the
`dotnetCoreAvaloniaNCForms` repo, referenced by `Tests`.  Rationale:
- Keeps test-only concerns out of the published `nac.Forms` nuget package.
- `Tests` gets a clean surface; `nac.Forms` only changes where the *product*
  genuinely needs it (a headless-capable app-setup path, see 3.2).
- "Works for all functionality": the harness is generic (`build the form, show
  headless, pump, inspect, teardown`), so any `Form` builder call is testable.

STATUS (this session): the harness **exists but lives in `Tests/Testing/`**
(`HeadlessForm.cs`, `HeadlessTestApp.cs`) rather than its own project, so
`Tests` is the only thing to compile/wire.  Promoting it to `nac.Forms.Testing`
is deferred (§0) — the shape is now proven by the passing heal test.  The reason
it can sit in `Tests` is that it only references `nac.Forms`'s *public* surface
(`Form`, `Form._Extend_AccessHost`, `model.Column`) plus `Avalonia.Headless`,
which `Tests` already pulls in transitively via `nac.Forms`.

### 3.2 Product changes to `nac.Forms` — NONE NEEDED (corrected this session)
The original plan was to give `nac.Forms` a headless app-setup path
(`UseHeadless()` instead of `UsePlatformDetect()`, a `NewHeadlessForm`, and to
add `Avalonia.Headless` to the product).  **This session proved that is not
necessary**, and `nac.Forms` was left **untouched**:
- The harness builds a **real** `Form` against a bare `Application` subclass that
  lives in the *test* project (`Tests/Testing/HeadlessTestApp.cs`, a `SimpleTheme`
  + the DataGrid style).  It uses only `nac.Forms`'s public surface:
  `new Form(__app: app, _model: …)`, `Form.DataContext`, `Form.Table<T>`,
  `Form._Extend_AccessHost`, and `model.Column`.
- `HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApp))` auto-forces
  `.UseHeadless()` when the builder's `WindowingSubsystemName != "Headless"`, so
  we never call `UseHeadless()` ourselves.
- The DataGrid control style is added to `Application.Current` by
  `Form.Table<T>` itself (`Form.Table.cs` `addDataGridStyleToApp`), and
  `HeadlessTestApp` also loads it — either way the presenter exists.
- `Avalonia.Headless` is referenced by `Tests` (transitively available via the
  `nac.Forms` reference + a direct `Avalonia.Headless` package ref in
  `Tests.csproj`), **not** added to the published `nac.Forms` package.
  ⇒ headless stays a testing-only concern, as §3.1 intended.

### 3.3 The controlled lifecycle — SUPERSEDED, see §6 ROOT CAUSE
(SUPERSEDED.) The original idea was `AppBuilder.UseHeadless()` +
`ControlledAppLifetime`.  That does NOT work: `SetupWithoutStarting()` leaves
`ApplicationLifetime == null`, and a manually pumped never-`Run` app materializes
0 rows.  The working route is Avalonia's `HeadlessUnitTestSession` + `Dispatch`
+ `ForceRenderTimerTick`, all inside the `Dispatch` scope (see §6).  Do NOT go
back to the `ControlledAppLifetime`/manual-`Post` approach.

### 3.4 Thread / dispatcher model
- **NOT** a custom `[TestMethod]` attribute for the first version.  An attribute
  is just sugar over the harness; start with a plain helper.  Optionally bump to
  MSTest v3 to get `[IsolatedProcess]` (MSTest 2.2.10 has none), or filter the
  probe so nothing else touches Avalonia in the process (the
  "Setup was already called" trap).
- Pattern (WORKING, per §6): `HeadlessUnitTestSession.StartNew(appType)` runs the
  app on a managed UI thread; do ALL UI work — build the form / `Show()` /
  `RunJobs()` / `ForceRenderTimerTick()` / read the visual tree — inside
  `session.Dispatch(() => …, CancellationToken.None).Wait()`.  `HeadlessUnitTestSession`
  is `IDisposable`.  This replaces the old "dedicated STA thread + manual
  `InvokeAsync` + `ControlledAppLifetime` + `Post`" idea, which doesn't materialize
  rows.
- **THREE things that break this if you aren't careful (all hit this session):**
  1. **Theme + DataGrid style required.** The `Application` the session starts
     must load a theme *and* the DataGrid control style, or the grid has no
     `DataGridRowsPresenter` and renders 0 rows.  `nac.Forms`'s own
     `Form.Table<T>` already adds the DataGrid style to `Application.Current`
     (`Form.Table.cs` `addDataGridStyleToApp`), and `nac.Forms.App.xaml` loads
     `SimpleTheme` — so when the harness builds a *real* `Form`, both are present.
     The harness's own `HeadlessTestApp` loads both too (see that file).
  2. **Every read is UI-thread-owned.** Reading `dg.ItemsSource` *or* walking the
     visual tree from the test (non-UI) thread throws
     `InvalidOperationException: The calling thread cannot access this object`.
     So `VisibleRowCount()` *dispatches its read*; it is not a plain getter.
  3. **No nested Dispatch.** A `Dispatch` posted from *inside* a `Dispatch` runs
     on the UI thread that is already busy in the outer `Dispatch`.
     `.Wait()` on the outer one then deadlocks.  So `Build` (which is already
     inside a `Dispatch`) pumps *inline*; only the public `Pump()`/`Run()`/
     `VisibleRowCount()` — called from the *test* thread — do the outer
     `Dispatch`.  Keep the tick loop in one private `PumpInline` used by both.

### 3.5 What a test asserts
Same observation nac.Forms itself uses:
`dataGrid.GetVisualDescendants().OfType<DataGridRowsPresenter>().Children
.OfType<DataGridRow>()` → count.  `HeadlessForm.VisibleRowCount()` does exactly
this inline (it does **not** wrap `DataGridVisibleRowsObserverRepo` — that
observer needs the window to reach the Loaded stage via a posted job, which is
harder to drive deterministically headless; counting the presenter directly is
simpler and is what the test asserts).  The product observer remains the
in-app mechanism; the harness is the test-side equivalent.

---

## 4. File map

- `nac.Forms/Form.cs` — `SetupAvaloniaApp`, `NewForm`, `Display`,
  `Display_Internal`, `Close` (the show/pump surface).  **Untouched by the
  harness** (see §3.2).
- `nac.Forms/Form.Table.cs` — `Table<T>` + `addDataGridStyleToApp`: builds the
  `DataGrid` and adds its control style to `Application.Current`.
- `nac.Forms/repos/DataGridVisibleRowsObserverRepo.cs` (+ `TableView…`) —
  the attach/load/layout observation mechanism the harness mirrors (the harness
  counts the same `DataGridRowsPresenter.Children` the observer watches).
- `nac.Forms/lib/AvaloniaAppManager.cs` — the existing "UI on its own thread"
  precedent (the real-app `UsePlatformDetect()` path; this is what contaminates
  the process, see §6 CONTAMINATION).
- `Tests/NestedPathBindingTests.cs` — the binding regression guard
  (logic-only, must stay green).
- NEW: `Tests/Testing/HeadlessForm.cs` — the generic harness
  (`Create` / `Build` / `Pump` / `Run` / `VisibleRowCount`).
- NEW: `Tests/Testing/HeadlessTestApp.cs` — the bare `Application` the session
  starts (loads `SimpleTheme` + the DataGrid style).
- NEW: `Tests/Testing/HeadlessHealTests.cs` — the heal-scenario row-count test
  (the nacBackup scenario, asserted on the visual tree).
- `docs/AGENTS_nacFormsHeadlessTestSetup.md` — this file.

NOTE: the old `Tests/Probe/` spike files (`HeadlessSpikeTests`, `HeadlessProbeTests`,
`HeadlessTestApp`, `MinimalHeadlessApp`) were removed this session — `HeadlessProbeTests`
was the documented bad path (Probe A, `ControlledAppLifetime`/manual `Post` → 0 rows)
and its `Show()` outside `Dispatch` form was the source of the "IWindowingPlatform" error.

---

## 5. Current status / next step (UPDATE AS I GO)

- [x] Confirmed `Avalonia.Headless` 12.1.2 is cached (no network).
- [x] Confirmed the show-without-Run hook is `Display_Internal` and the
      pump-dependent piece is the rows observer (attach+load+layout).
- [x] Confirmed the manual `ControlledAppLifetime.Start()` + dispatcher `Post`
      pump path is a **bad** path (Probe A).
- [x] Found Avalonia's **official** headless test harness that avoids the
      blocking loop and materializes rows: `HeadlessUnitTestSession` +
      `ForceRenderTimerTick` + `Dispatcher.UIThread.RunJobs()`, all inside
      `Dispatch`.  See §6 ROOT CAUSE + THE WORKING PATTERN.
- [x] Rewrote the probe to the working pattern and verified a bare `DataGrid`
      materializes rows headless (3 rows for a 3-item source, explicit column).
- [x] Built the generic harness `Tests/Testing/HeadlessForm.cs`
      (`Create`/`Build`/`Pump`/`Run`/`VisibleRowCount`) + `HeadlessTestApp`.
- [x] Wrote the heal-scenario row-count test `HeadlessHealTests.cs`
      (parent null → 0 rows → heal → 3 rows).  **PASSES when run filtered.**
- [x] Established that no product change to `nac.Forms` is needed (§3.2).
- [x] Discovered + documented the 3 gotchas: theme+style required, reads are
      UI-thread-owned, no nested Dispatch (§3.4 / §6).
- [x] Discovered + documented the cross-contamination trap and that
      `[IsolatedProcess]` is unavailable in cached MSTest 2.2.10 (§6
      CONTAMINATION).
- [ ] Deferred: promote the harness to its own `nac.Forms.Testing` project
      (§0/§3.1).  Shape is proven; it can move as-is.
- [ ] Keep the real-app `Display()` path unchanged; keep
      `NestedPathBindingTests` green (verified — they pass; just don't run them
      in the *same process* before a headless test, §6 CONTAMINATION).

---

## 6. Findings / gotchas log (append as discovered)
- "Setup was already called on one of AppBuilder instances" = can't call
  `SetupWithoutStarting()` / start a 2nd `Application` in the same process after
  the first.  → isolate headless tests with a fresh process, or share one app
  via a harness.  (MSTest 2.2.10 has NO `[IsolatedProcess]`; that is MSTest v3.
  For the probe we just filter to `HeadlessProbeTests` so nothing else sets up
  Avalonia in the process.  Alternatively bump to MSTest v3 for `[IsolatedProcess]`.)
- `DataGridVisibleRowsObserverRepo` posts the "scrollviewer available" step at
  `DispatcherPriority.Loaded`, so it needs the window to actually reach the
  Loaded/render stage.
- **API facts (Avalonia 12.1.2 / Avalonia.Headless 12.1.2):**
  - `UseHeadless` requires opts: `UseHeadless(new AvaloniaHeadlessPlatformOptions())`.
  - `SetupWithoutStarting()` returns the `AppBuilder`; the app is `.Instance`.
  - `ApplicationLifetime` is a property of `Application`, not of `AppBuilder`.
  - `DataGridRowsPresenter` lives in `Avalonia.Controls.Primitives`
    (the `Avalonia.Controls.DataGrid` assembly), NOT `Avalonia.Controls`.
  - `HeadlessUnitTestSession.StartNew(Type[, AvaloniaTestIsolationLevel])`
    auto-forces `UseHeadless` if the builder's `WindowingSubsystemName !=
    "Headless"`.  The `Type` arg must be an `Application` subclass OR a type
    with a `static AppBuilder BuildAvaloniaApp()`.  A plain non-Application type
    is a common failure.
  - `Dispatch` overloads REQUIRE a `CancellationToken`
    (no 1-arg overload): `Dispatch(Action, CancellationToken)` and
    `Dispatch<T>(Func<T>, CancellationToken)`.  Pass `CancellationToken.None`.
  - `AvaloniaTestIsolationLevel` enum currently exposes `PerTest` / `PerAssembly`
    (only `PerTest` observed in the 12.1.2 cache).  Use `PerAssembly` to share
    one app across the assembly's tests.
  - `HeadlessWindowExtensions`: `CaptureRenderedFrame(TopLevel)`,
    `GetLastRenderedFrame(TopLevel)`, plus `KeyPress*` / `MouseDown*` /
    `KeyTextInput` — the input/inspect surface.
  - `AvaloniaHeadlessPlatform.ForceRenderTimerTick(int count = 1)`: forces the
    (headless) render timer ticks — this is what materializes `DataGrid` rows.
    With `ShouldRenderOnUIThread=true` the tick runs on the UI thread; call a
    few times (e.g. 3) for DataGrid which can defer a render pass.
- **PROBE A result (FAILS this way; the WRONG path):** `UseHeadless() +
  SetupWithoutStarting()` gives an app whose `ApplicationLifetime` is **NULL** —
  `SetupWithoutStarting` does *not* create the headless lifetime.  Manual
  `win.Show()` + `Measure`/`Arrange` + `Dispatcher.UIThread.Post(Render)`
  pumping on a never-started app yields `presenter=null` → **0 rows**, and
  `Show()`/tree-reads outside the proper scope throw `IWindowingPlatform`.
  ⇒ **Do NOT use the `ControlledAppLifetime` + manual-`Post` route.**
- **ROOT CAUSE of "Unable to locate 'Avalonia.Platform.IWindowingPlatform'":**
  the headless platform (which registers `IWindowingPlatform` via
  `AvaloniaHeadlessPlatform.Initialize` → `UseWindowingSubsystem(..., "Headless")`)
  is registered **only inside the AvaloniaLocator scope that
  `HeadlessUnitTestSession` opens *during* `Dispatch`**, and that scope is
  **disposed when the `Dispatch` returns**.  ⇒ Every UI call — `Win.Show()`,
  `Dispatcher.UIThread.RunJobs()`, `ForceRenderTimerTick()`, AND reading the
  visual tree / row count — **must happen INSIDE the `Dispatch` lambda**.  Doing
  any of it outside the lambda → the IWindowingPlatform error.
- **THE WORKING PATTERN (from Avalonia's own headless unit tests, 12.1.2):**

  Manual (full control — this is what nac.Forms should use; nac.Forms builds
  forms by hand rather than XAML, so the `[AvaloniaFact]` auto-start is less
  applicable, but equivalent):
  ```csharp
  using var session = HeadlessUnitTestSession.StartNew(typeof(MyAppBuilder));
  int rowCount = 0;
  session.Dispatch(() =>
  {
      // ALL UI work here — platform scope is open only within this lambda
      var grid = new DataGrid { Width = 400, Height = 300,
                                 ItemsSource = new[] {1,2,3,4,5} };
      var win = new Window { Width = 400, Height = 300, Content = grid };
      win.Show();
      for (int i = 0; i < 3; i++)
      {
          Dispatcher.UIThread.RunJobs();                 // layout pass
          AvaloniaHeadlessPlatform.ForceRenderTimerTick(); // render pass
      }
      // read the visual tree NOW (still inside the lambda)
      var presenter = grid.GetVisualDescendants()
          .OfType<DataGridRowsPresenter>().First();
      rowCount = presenter.Children.Count;
  }, CancellationToken.None).Wait();
  ```
  Auto-start (the `[AvaloniaFact]`/`[AvaloniaTest]` path starts the session,
  registers the platform, and runs `RunJobs()` after the body — good for
  Avalonia's own XAML tests, less so for nac.Forms hand-built forms).
- **`DataGrid` virtualization caveat:** `DataGridRowsPresenter.Children` holds
  only the *visible/materialized* rows, which require a non-zero size AND a
  layout pass.  `HeadlessWindowImpl` gives a real client size (default 1024x768),
  so set an explicit `Window`/`DataGrid` size and run ≥2 (use 3) ticks.  To
  force *all* rows to materialize, either give the grid enough height or set a
  non-virtualizing `ItemsPanel`; otherwise assert on the presenter's row count,
  not that *every* model item has a row.
- **BARE DataGrid needs explicit columns (nuance, not a real problem):** the
  earliest spike used a hand-built `DataGrid` with `AutoGenerateColumns=true` and
  it materialized `presenter=ok / rows=0` because auto-column-generation did not
  produce a `DataGridRowsPresenter` child *on that bare grid*.  Two ways out:
  (a) add an explicit `DataGridTextColumn`, or (b) — what the harness actually
  uses — build a real `Form` via `f.Table<T>(...)`, where `autoGenerateColumns:
  true` **does** materialize the rows (verified: a 3-item `ObservableCollection`
  source → 3 rows).  ⇒ the "0 rows" you may see is a *bare-grid* artifact, not a
  nac.Forms problem.
- **Probe A / Probe B: both superseded.** The `Tests/Probe/` spike files
  (`HeadlessSpikeTests`, `HeadlessProbeTests`, `HeadlessTestApp`,
  `MinimalHeadlessApp`) were deleted this session.  `HeadlessProbeTests` was the
  documented bad path (Probe A: `ControlledAppLifetime`/manual `Post` → 0 rows,
  and `Show()`/tree-read *outside* `Dispatch` → "IWindowingPlatform").  The
  equivalent of "Probe B green" is now the passing `HeadlessHealTests` (see
  §0/§5).  Do not reintroduce the probe files.
- **Theme + DataGrid control style are REQUIRED on the Application** the session
  starts, or the grid has no `DataGridRowsPresenter` and renders 0 rows.
  *Verified this session:* `SimpleTheme` alone is NOT enough for `DataGrid` —
  `DataGrid` ships its own template in
  `avares://Avalonia.Controls.DataGrid/Themes/Simple.xaml` and must be loaded via
  a `StyleInclude` (as `Form.Table.cs` `addDataGridStyleToApp` does).  When the
  harness builds a *real* `Form`, `Table<T>` adds that style to
  `Application.Current` and `HeadlessTestApp` also declares it, so the presenter
  exists.  This is why the original bare-`Application` spike got `presenter=ok /
  rows=0` — the style/theme were missing.
- **Every control read is UI-thread-owned — dispatch it.** Reading
  `dg.ItemsSource` *or* walking `GetVisualDescendants()` from the test thread
  (outside a `Dispatch`) throws
  `InvalidOperationException: The calling thread cannot access this object`.
  ⇒ `HeadlessForm.VisibleRowCount()` runs its read *inside* a `Dispatch`; it is
  not a plain getter.  (This bug passed in isolation by luck and only failed when
  another test class ran first — see CONTAMINATION.)
- **No nested Dispatch or the session deadlocks.** A `Dispatch` posted from
  *inside* a `Dispatch` runs on the UI thread that is already busy in the outer
  `Dispatch`; `.Wait()`ing on the outer one then hangs forever (the test process
  burns 100% CPU and times out).  ⇒ `HeadlessForm.Build` (already inside a
  `Dispatch`) must pump *inline* — it calls a private `PumpInline`, NOT the
  public `Pump()` (which does its own outer `Dispatch`).  Only the public
  `Pump()`/`Run()`/`VisibleRowCount()`, called from the *test* thread, do the
  outer `Dispatch`.
- **CONTAMINATION — the one remaining trap (why we run filtered):** the
  real-app path (`Form.NewForm()` → `SetupAvaloniaApp` → `UsePlatformDetect()` +
  `SetupWithoutStarting()`, used in `FormTests` and `NestedPathBindingTests`)
  registers the *desktop* windowing subsystem as the process-global Avalonia
  platform.  A headless `HeadlessUnitTestSession` that runs *after* it in the
  same process then cannot materialize rows: the binding heals fine
  (`grid.ItemsSource` becomes the child's 3-item list) but the presenter stays at
  0 rows — the desktop platform "won" the global slot, or the two platform inits
  fight.  ⇒ run the headless tests in a process with no prior real-app
  `SetupWithoutStarting()`.  MSTest 2.2.10 (the version pinned here) has **NO**
  `[IsolatedProcess]` (that's MSTest v3, and the cached v3 line — up to 3.11.1 —
  does NOT define the attribute either, so a version bump is not a clean offline
  fix).  Pragmatic mitigations, in order of cleanliness: (1) filter the run so
  headless tests are alone — see §6b; (2) put the headless tests in their own
  `nac.Forms.Testing` test project so `dotnet test` spins a separate runner;
  (3) bump to a future MSTest that actually ships `[IsolatedProcess]` and put it
  on `HeadlessHealTests`.  Each `HeadlessForm` already makes its **own** session,
  so the contamination is purely about the *desktop* `UsePlatformDetect()` path
  running first, not about the headless sessions fighting each other.

---

## 6b. RECIPE (DONE this session — how a headless test is written today)

STATUS: items 1–4 below are DONE.  The harness lives in `Tests/Testing/`
(promoting it to its own `nac.Forms.Testing` project is the only thing deferred,
§0/§3.1).

The working, repeatable shape (all discovered this session — see §6):

1. One `HeadlessUnitTestSession` per test, started from a bare `Application`
   subclass that loads a theme **and** the DataGrid style:
   ```csharp
   using var headless = HeadlessForm.Create();   // StartNew(typeof(HeadlessTestApp))
   ```
   (`HeadlessTestApp` = `SimpleTheme` + `StyleInclude(avares://Avalonia.Controls.
   DataGrid/Themes/Simple.xaml)`.)
2. Build a **real** `Form` and show it, all on the UI thread via `Build`:
   ```csharp
   headless.Build(f => f.Table<Location>("Selected.Locations",
       columns: new Column[] { new Column { Header = "Label",
                          modelBindingPropertyName = "Label" } }),
       dataContext: parent);
   ```
   `Build` does the window/`Show()`/pump **inline** (NOT via the public `Pump`,
   which would be a nested `Dispatch` → deadlock).
3. Read the rows via `VisibleRowCount()` (which dispatches its own read — a read
   from the test thread throws the "different thread owns it" error):
   ```csharp
   Assert.AreEqual(0, headless.VisibleRowCount());        // parent null yet
   headless.Run(() => parent.Selected = childWith3Rows);  // heal on the UI thread
   headless.Pump();                                       // re-layout
   Assert.AreEqual(3, headless.VisibleRowCount());        // rows materialized
   ```
4. Run the headless tests **filtered / in their own process** so the desktop
   `UsePlatformDetect()` path has not run first (CONTAMINATION, §6):
   ```
   dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~HeadlessHealTests"
   ```
   (or any `...~Headless` filter that excludes `FormTests`/`NestedPathBindingTests`.)

The `HeadlessForm` API that emerged (differs from the original §6b sketch — no
`Table(string)` wrapper needed, because you build the form directly so any
`Form` builder call works):
```csharp
sealed class HeadlessForm : IDisposable
{
    static HeadlessForm Create();                    // StartNew(HeadlessTestApp)
    Form Build(Action<Form> build, INotifyPropertyChanged dataContext = null,
               double width = 600, double height = 600, int pumpTicks = 3);
    void Run(Action uiWork);                         // model heal / mutation
    void Pump(int ticks = 3);                        // render passes (UI-thread only)
    int VisibleRowCount();                           // first DataGrid's presenter rows
    int VisibleRowCount(Avalonia.Controls.DataGrid dg);
    Form Form { get; }
    void Dispose();                                  // session.Dispose()
}
```

---
