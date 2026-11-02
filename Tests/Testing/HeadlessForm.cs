using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using nac.Forms;

namespace Tests.Testing
{
    /**
     * HeadlessForm — a thin, generic harness for rendering a REAL nac.Forms.Form
     * without a screen and asserting on its visual tree.  Design lives in
     * docs/AGENTS_nacFormsHeadlessTestSetup.md (§6b).  Lives in the Tests project
     * for now; the plan is to promote it to its own nac.Forms.Testing project.
     *
     * WHY THIS EXISTS
     *  DataGridRow / TableViewRow visuals only exist after a window is attached to
     *  a visual tree and runs a layout + render pass.  A headless session can do
     *  that without a real screen, so tests can assert "the table actually shows
     *  N rows" / "this label shows X" instead of just "grid.ItemsSource has N
     *  items".
     *
     * THE TWO RULES THAT SHAPE THE API
     *  1. The headless platform (IWindowingPlatform) is registered ONLY inside the
     *     AvaloniaLocator scope that HeadlessUnitTestSession opens during a
     *     Dispatch, and that scope is disposed when the Dispatch returns.  So
     *     EVERY UI call — build, Show, pump, and inspect — must run INSIDE a
     *     Dispatch.
     *  2. Avalonia controls are UI-thread-owned: reading a control's property
     *     (eg. TextBlock.Text) or walking its visual tree from ANY other thread
     *     throws "The calling thread cannot access this object".  So an
     *     inspection must also run inside a Dispatch.
      *
      * Because of (2), you do NOT pull a control out and read it on the test
      * thread.  Instead you run your search+read through On(...) / On<T>(...),
      * which opens a Dispatch, hands you a Visuals query rooted at the rendered
      * tree, and returns whatever value you compute on the UI thread.  A test
      * reads the world like this:
      *      string text = headless.On(v => v.LastChild<TextBlock>().Text);
      *      headless.Click("Set Parent");      // find the button, click, re-render
      *      int rows     = headless.On(v => v.OfType<DataGridRow>().Count());
      *  Note LastChild (TOP-LEVEL children of the host) not Last/Descendants for
      *  labels/buttons: a Button's content is itself a TextBlock deep in the tree,
      *  so Descendants().OfType<TextBlock>() would match it too.  DataGridRow lives
      *  deep, so use Descendants() (via OfType<T>) for those.  See §6 / §6b.
      *
      *  See §6 ROOT CAUSE for the "Unable to locate IWindowingPlatform" / "calling
      *  thread cannot access" errors that happen when a call leaks the scope.
      */
    public sealed class HeadlessForm : IDisposable
    {
        private readonly HeadlessUnitTestSession _session;
        private Window _window;
        private Form _form;

        private HeadlessForm(HeadlessUnitTestSession session) { _session = session; }

        public Form Form => _form;

        public static HeadlessForm Create()
            => new(HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApp)));

        /**
         * Build a form on the headless UI thread, attach its host grid to a window,
         * show the window, and pump render ticks so the first layout/render pass
         * happens.  [dataContext] is the object the form's bindings read/write
         * (the same thing a real app sets as the form's DataContext).
         */
        public Form Build(Action<Form> build,
            INotifyPropertyChanged dataContext = null,
            double width = 600,
            double height = 600,
            int pumpTicks = 3)
        {
            // One Dispatch.  We are ON the UI thread inside it, so the pump must
            // be INLINE here -- calling the public Pump() (which dispatches again)
            // would post a job to the UI thread that is busy running this one and
            // deadlock (see §6).
            Run(() =>
            {
                var app = Application.Current;
                var f = new Form(__app: app, _model: null)
                {
                    DataContext = dataContext,
                };
                build(f);

                Grid host = null;
                f._Extend_AccessHost(h => host = h);

                _window = new Window
                {
                    Content = host,
                    Width = width,
                    Height = height,
                };
                _window.Show();
                PumpInline(pumpTicks);

                _form = f;
                _host = host;
            });
            return _form;
        }

        /**
         * Run arbitrary UI work on the headless UI thread, inside the platform
         * scope.  Use this for model mutations that heal a binding
         * (eg. parent.Selected = child) so the binding's PropertyChanged handler
         * runs on the UI thread and the control updates thread-safely.
         */
        public void Run(Action uiWork)
        {
            _session.Dispatch(() =>
            {
                uiWork();
            }, CancellationToken.None).Wait();
        }

        /**
         * The generic inspection entry: open a Dispatch, hand the lambda a Visuals
         * query over the rendered tree, and run the lambda ON THE UI THREAD so the
         * search and any property reads are thread-safe.  Returns whatever the
         * lambda computed.  This is the single high-level primitive a test builds
         * its asserts on; the concrete helpers below are just special cases.
         */
        public T On<T>(Func<Visuals, T> inspect)
        {
            T result = default;
            Run(() =>
            {
                result = inspect(VisualsFor(_host));
            });
            return result;
        }

        /**
         * Void overload of On -- when the test only needs to perform an action on
         * the UI thread (eg. assert inside is not possible, so use this to mutate
         * or to call sub-helpers), call with no return.
         */
        public void On(Action<Visuals> inspect)
        {
            Run(() =>
            {
                inspect(VisualsFor(_host));
            });
        }

        /**
         * Force render timer ticks on the UI thread.  A DataGrid virtualizes its
         * rows into DataGridRowsPresenter.Children, which only fills during the
         * measure/arrange/render pass — so a render tick is what makes rows
         * materialize.  A few ticks are used because DataGrid can defer a render
         * pass.
         */
        public void Pump(int ticks = 3)
        {
            Run(() => PumpInline(ticks));
        }

        private void PumpInline(int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            }
        }

        /**
         * Find the first form Button (a top-level control the test built) whose
         * content is [contentText], raise its Click event (on the UI thread), then
         * pump so any model heal the click triggers is re-rendered.  The search is
         * over top-level children so a control's own template parts (which can
         * themselves be buttons) are not matched.  The raise and the pump are two
         * SEPARATE, FLAT Dispatches -- NOT nested -- because a Dispatch posted from
         * inside the raise's Dispatch would deadlock (§6).
         */
        public void Click(string contentText)
        {
            Avalonia.Controls.Button btn = null;
            Run(() =>
            {
                btn = _host?.GetVisualChildren()
                    .OfType<Avalonia.Controls.Button>()
                    .FirstOrDefault(b => string.Equals(b.Content as string, contentText,
                        StringComparison.Ordinal));
                btn?.RaiseEvent(new RoutedEventArgs(
                    Avalonia.Controls.Button.ClickEvent, btn));
            });
            Pump();
        }

        /**
         * Find the first top-level form Button of type [T] and "click" it, then
         * pump.  [T] is a Button (or subclass) since that is the thing that
         * carries a Click.  Same flat Dispatch/pump rule as Click(string).
         */
        public void Click<T>() where T : Avalonia.Controls.Button
        {
            T btn = null;
            Run(() =>
            {
                btn = _host?.GetVisualChildren().OfType<T>().FirstOrDefault();
                btn?.RaiseEvent(new RoutedEventArgs(
                    Avalonia.Controls.Button.ClickEvent, btn));
            });
            Pump();
        }

        // the form's host grid, set during Build; the root of the Visuals query
        // and of Click's tree search.
        private Avalonia.Controls.Grid _host;

        private static Visuals VisualsFor(Avalonia.Controls.Grid host)
            => new(host);

        public void Dispose()
        {
            _session.Dispose();
        }
    }

    /**
     * A generic, UI-thread query over the rendered visual tree.  Obtained only
     * from HeadlessForm.On(...) (which runs the body on the UI thread), so every
     * control it hands back is valid to walk and to read properties from --
     * because you are already on the owning thread.  It is intentionally a thin
     * wrapper over Avalonia's GetVisualDescendants / GetVisualChildren + OfType<T>:
     * a test searches the tree, grabs the control, and reads it -- eg.
     *      headless.On(v => v.LastChild<TextBlock>().Text)
     */
    public sealed class Visuals
    {
        /**
         * [host] is the form's host Grid (the root of the rendered controls).  It
         * may be null before Build has run.
         */
        internal Visuals(Avalonia.Controls.Grid host)
        {
            Root = host;
        }

        /// The form's host grid (the tree root).  Null before Build.
        public Avalonia.Controls.Grid Root { get; }

        /**
         * Every visual descendant of the tree root (the host grid), in tree order.
         * This is the DEEP search -- use it for things that live inside a control's
         * template, e.g. the materialized DataGridRow / TableViewRow of a table
         * (they are deep inside the DataGrid's DataGridRowsPresenter).
         */
        public IEnumerable<Avalonia.Visual> Descendants()
            => Root?.GetVisualDescendants() ?? Enumerable.Empty<Avalonia.Visual>();

        /**
         * The form's TOP-LEVEL controls -- the direct visual children of the host
         * grid, i.e. the controls the test actually built (a TextFor's TextBlock,
         * a Button, the table's DataGrid, ...).  Use this when you want "the
         * controls", avoiding the TextBlocks/controls that a control's own template
         * generates (eg. a Button's content is itself a TextBlock, which
         * Descendants() would also return but Children() does not).
         */
        public IEnumerable<Avalonia.Visual> Children()
            => Root?.GetVisualChildren() ?? Enumerable.Empty<Avalonia.Visual>();

        /**
         * Descendants filtered to [T].  Returns real controls, valid to read
         * because you are on the UI thread (On ran you there).
         */
        public IReadOnlyList<T> OfType<T>()
            where T : class
            => Descendants().OfType<T>().ToList();

        /**
         * The first descendant of type [T], or null.
         */
        public T Find<T>()
            where T : class
            => Descendants().OfType<T>().FirstOrDefault();

        /**
         * The last descendant of type [T], or null.
         */
        public T Last<T>()
            where T : class
            => Descendants().OfType<T>().LastOrDefault();

        /**
         * The first descendant of type [T] satisfying [predicate], or null.
         */
        public T Find<T>(Func<T, bool> predicate)
            where T : class
            => Descendants().OfType<T>().FirstOrDefault(predicate);

        // ---- top-level variants (the controls the test built, no template parts)

        /**
         * Top-level controls (direct children of the host) filtered to [T].  Like
         * OfType<T> but skips the deep template internals, so "the TextBlocks"
         * means the form's labels, not the TextBlocks inside a Button's content.
         */
        public IReadOnlyList<T> ChildrenOfType<T>()
            where T : class
            => Children().OfType<T>().ToList();

        /**
         * The last top-level control of type [T], or null.
         */
        public T LastChild<T>()
            where T : class
            => Children().OfType<T>().LastOrDefault();

        /**
         * The first top-level control of type [T] satisfying [predicate], or null.
         */
        public T FindChild<T>(Func<T, bool> predicate)
            where T : class
            => Children().OfType<T>().FirstOrDefault(predicate);
    }
}
