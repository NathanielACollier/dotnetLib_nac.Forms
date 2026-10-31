using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using nac.Forms;

namespace Tests.Testing
{
    /**
     * HeadlessForm — a thin, generic harness for rendering a REAL nac.Forms.Form
     * without a screen and asserting on its visual tree.  This is the first half
     * of the design in docs/AGENTS_nacFormsHeadlessTestSetup.md (§6b).  It lives
     * in the Tests project for now; the doc's plan is to promote it to its own
     * nac.Forms.Testing project once the shape is settled.
     *
     * WHY THIS EXISTS
     *  DataGridRow / TableViewRow visuals only exist after a window is attached to
     *  a visual tree and runs a layout + render pass.  A headless session can do
     *  that without a real screen, so tests can assert "the table actually shows
     *  N rows" instead of just "grid.ItemsSource has N items".
     *
     * THE ONE RULE THAT MATTERS
     *  The headless platform (IWindowingPlatform) is registered ONLY inside the
     *  AvaloniaLocator scope that HeadlessUnitTestSession opens during a Dispatch,
     *  and that scope is disposed when the Dispatch returns.  So EVERY UI call —
     *  building the form, showing the window, pumping the render timer, and
     *  reading the visual tree — must run INSIDE a Dispatch.  This class hides
     *  that: every public method dispatches its own UI work, so callers never
     *  touch the platform scope directly.
     *
     *  See §6 ROOT CAUSE in the doc for the "Unable to locate
     *  IWindowingPlatform" error that happens when a UI call leaks outside a
     *  Dispatch.
     */
    public sealed class HeadlessForm : IDisposable
    {
        private readonly HeadlessUnitTestSession _session;
        private Window _window;
        private Form _form;

        private HeadlessForm(HeadlessUnitTestSession session) { _session = session; }

        public Form Form => _form;

        /**
         * Open a headless session.  Each created HeadlessForm runs its own
         * session, so one test can have several forms and they never fight over a
         * shared one-shot Application.
         */
        public static HeadlessForm Create()
            => new(HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApp)));

        /**
         * Build a form on the headless UI thread, attach its host grid to a window,
         * show the window, and pump render ticks so the first layout/render pass
         * happens.  [dataContext] is the object the form's bindings read/write
         * (the same thing a real app sets as the form's DataContext).
         *
         * Use this for the common "build once, then mutate + pump" pattern; a
         * subsequent Pump() drives later render passes after a model heal.
         */
        public Form Build(Action<Form> build,
            INotifyPropertyChanged dataContext = null,
            double width = 600,
            double height = 600,
            int pumpTicks = 3)
        {
            // One Dispatch.  We are already ON the UI thread inside it, so the
            // pump must be INLINE here -- calling the public Pump() (which
            // dispatches again) would post a job to the UI thread that is busy
            // running this one and deadlock.
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

        /**
         * The raw tick loop, run on the thread the caller is already on (the UI
         * thread, inside a Dispatch).  Do NOT wrap this in another Dispatch --
         * that nested Dispatch deadlocks.  Both Build and Pump funnel through
         * it so the tick logic lives in one place.
         */
        private void PumpInline(int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            }
        }

        /**
         * Count the materialized (visible) DataGridRows of the first DataGrid the
         * built form added.  This is the same observation nac.Forms itself makes
         * (repos.DataGridVisibleRowsObserverRepo), asserted here on the real
         * visual tree.  Returns 0 if there is no DataGrid yet.
         *
         * The read runs inside a Dispatch: Avalonia controls are owned by the
         * session's UI thread, and reading their properties / visual tree from any
         * other thread throws "The calling thread cannot access this object".
         */
        public int VisibleRowCount()
        {
            int count = 0;
            Run(() => { count = PresenterRowCount(FindDataGrid()); });
            return count;
        }

        /**
         * Count the materialized (visible) DataGridRows of a specific DataGrid.
         */
        public int VisibleRowCount(Avalonia.Controls.DataGrid dg)
        {
            int count = 0;
            Run(() => { count = PresenterRowCount(dg); });
            return count;
        }

        private Avalonia.Controls.DataGrid FindDataGrid()
        {
            Avalonia.Controls.DataGrid found = null;
            if (_form != null)
            {
                _form._Extend_AccessHost(host =>
                {
                    found = host.Children
                        .OfType<Avalonia.Controls.DataGrid>()
                        .FirstOrDefault();
                });
            }
            return found;
        }

        private static int PresenterRowCount(Avalonia.Controls.DataGrid dg)
        {
            if (dg == null)
            {
                return 0;
            }

            var presenter = dg.GetVisualDescendants()
                .OfType<DataGridRowsPresenter>()
                .FirstOrDefault();

            return presenter?.Children
                .OfType<Avalonia.Controls.DataGridRow>()
                .Count() ?? 0;
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}
