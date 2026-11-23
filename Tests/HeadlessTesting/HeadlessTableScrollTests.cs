using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Avalonia.Controls;
using Avalonia.VisualTree;
using nac.Forms;
using nac.Forms.model;
using nac.ViewModelBase;
using Tests.HeadlessTesting;

namespace Tests
{
    /**
     * A table whose ROW holds a label BOUND TO A VARIABLE and a button
     * (a DataGridTemplateColumn whose cell is a nac.Forms.Form), 50 items, in a
     * window short enough that the DataGrid VIRTUALIZES them -- only the visible
     * rows exist in the visual tree.
     *
     * WHAT IT PROVES (exactly the user's point)
     *  1. Before scrolling, a far row (Item-49) is NOT materialized -- its bound
     *     label is not in the tree, so there is no on-screen text for it to read.
     *  2. Scrolling into the far row materializes it.
     *  3. THE MAIN POINT: read the text actually RENDERED ON that now-visible row
     *     (its bound label, not the button's own content) and assert it equals that
     *     same row's DataContext -- i.e. the on-screen text matches the variable the
     *     row is bound to.  A virtualized row view that got recycled without
     *     re-binding would still show a stale neighbor's value; this catches that.
     *  4. The button on that now-visible row is clicked and acts on that same row's
     *     DataContext.
     *  5. The top row was recycled away, proving the visible set genuinely changed.
     *
     * A row's cell has TWO TextBlocks: the bound label (TextFor "Label") AND the
     * button's own content.  To read the row's bound label we take, within that
     * row's subtree, the TextBlock that has NO Button ancestor (the button-content
     * TextBlock lives *inside* the Button).  The row's item is its DataContext
     * (Avalonia 12 has no DataGridRow.Item), compared by REFERENCE to the model
     * item so we know it is the right row's data, not a recycled view's.
     *
     * ISOLATION: like the other headless tests, run this class alone
     * (dotnet test --filter "FullyQualifiedName~HeadlessTableScrollTests") so the
     * desktop UsePlatformDetect() path hasn't run first in the process.  See
     * docs/AGENTS_nacFormsHeadlessTestSetup.md §6/§6b.
     */
    [TestClass]
    public class HeadlessTableScrollTests
    {
        // The "variable" each row binds its label to.  A nac.ViewModelBase so the
        // row's Form.TextFor("Label") has an INotifyPropertyChanged DataContext to
        // bind to (nac.Forms insists on it -- "Special DataContext used in model,
        // but DataContext is not INotifyPropertyChanged").
        class RowItem : ViewModelBase
        {
            public string Label
            {
                get => GetValue(() => Label);
                set => SetValue(() => Label, value);
            }
        }

        class TableScrollModel : ViewModelBase
        {
            public ObservableCollection<RowItem> Rows
            {
                get => GetValue(() => Rows);
                set => SetValue(() => Rows, value);
            }

            public string LastClickedLabel
            {
                get => GetValue(() => LastClickedLabel);
                set => SetValue(() => LastClickedLabel, value);
            }
        }

        /**
         * The text rendered ON the now-visible row bound to [item] -- the row's
         * bound label (a non-button TextBlock), NOT the button's own content.  Null
         * if that row is not materialized.  Runs on the UI thread (inside On);
         * [item] is compared by reference so a recycled view can't be mistaken for
         * the real row.
         */
        static string RowVisibleText(Tests.HeadlessTesting.Visuals v, object item)
        {
            var row = v.OfType<Avalonia.Controls.DataGridRow>()
                .FirstOrDefault(r => ReferenceEquals(r.DataContext, item));
            if (row == null)
            {
                return null;
            }
            var label = row.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(tb => (!tb.GetVisualAncestors().OfType<Button>().Any()));
            return label?.Text;
        }

        /**
         * Whether a now-visible row's DataContext IS [item] (by reference).  Used to
         * confirm the row scrolled onto screen is the one bound to the model item.
         */
        static bool RowVisibleBound(Tests.HeadlessTesting.Visuals v, object item)
            => v.OfType<Avalonia.Controls.DataGridRow>()
                .Any(r => ReferenceEquals(r.DataContext, item));

        [TestMethod]
        public void Table_ScrollToRow_VisibleRowTextMatchesThatRowDataContext_ClickActsOnIt()
        {
            const int itemCount = 50;
            const string farLabel = "Item-49";

            var model = new TableScrollModel
            {
                // seed by assignment (a settable ViewModelBase prop's getter is null
                // until set, per the doc's caveat -- don't .Add on an unset getter).
                Rows = new ObservableCollection<RowItem>(
                    Enumerable.Range(0, itemCount)
                        .Select(i => new RowItem { Label = $"Item-{i}" })),
            };

            // the "variable" each row's label binds to; the far row's value.
            var farModelItem = model.Rows[itemCount - 1];

            using var headless = HeadlessForm.Create();

            // Two columns, like a real grid: the action Button FIRST, and a label
            // BOUND TO A VARIABLE ("Label") in its OWN text column -- not stuffed
            // into the button's cell.  autoGenerateColumns false so only these
            // columns are present.  The button's CellTemplate runs per materialized
            // row with rowForm.DataContext == that row's item, so it captures that
            // row's item; the text column binds row.Label by reference.
            headless.Build(
                f =>
                {
                    f.Table<RowItem>(
                        "Rows",
                        columns: new Column[]
                        {
                            new Column
                            {
                                Header = "Go",
                                template = (rowForm) =>
                                {
                                    var item = rowForm.DataContext as RowItem;
                                    rowForm.Button(item.Label, () =>
                                    {
                                        model.LastClickedLabel = item.Label;
                                        return Task.CompletedTask;
                                    });
                                },
                            },
                            new Column
                            {
                                Header = "Name",
                                modelBindingPropertyName = "Label",
                            },
                        },
                        autoGenerateColumns: false
                    );
                },
                dataContext: model,
                width: 400,
                height: 120
            );

            // ---- before scrolling: the far row is NOT materialized -------------
            // No on-screen text for it, and its top neighbor (Item-0) IS shown.
            Assert.IsNull(
                headless.On(v => RowVisibleText(v, farModelItem)),
                "before scrolling, the far row is not on screen, so it has no " +
                "rendered text to read");
            Assert.IsTrue(
                headless.On(v => RowVisibleText(v, model.Rows[0]) != null),
                "before scrolling, a top row (Item-0) should be rendered on screen");

            // ---- scroll into the far row; it materializes and renders ---------
            headless.ScrollTable(farModelItem);

            // ---- the row that is now on screen is the one bound to the far item
            Assert.IsTrue(
                headless.On(v => RowVisibleBound(v, farModelItem)),
                "after scrolling, the on-screen far row's DataContext must be the " +
                "far model item, not a recycled view");

            // ---- THE MAIN CHECK: the text visible ON that row equals the row's
            //      DataContext variable ----------------------------------------
            string farRowVisibleText = headless.On(v => RowVisibleText(v, farModelItem));
            Assert.AreEqual(farModelItem.Label, farRowVisibleText,
                "the text rendered on the on-screen far row must match that row's " +
                "DataContext -- the bound variable, not a recycled neighbor's value");
            Assert.AreEqual(farLabel, farRowVisibleText,
                "the on-screen far row's rendered text should be the far item's " +
                "value, not a recycled one like Item-48");

            // ---- the same now-visible row's button is clicked; it acts on that
            //      row's DataContext ------------------------------------------
            Button farButton = headless.On(v =>
                v.OfType<Avalonia.Controls.DataGridRow>()
                    .FirstOrDefault(r => ReferenceEquals(r.DataContext, farModelItem))
                    ?.GetVisualDescendants()
                    .OfType<Button>().FirstOrDefault());
            Assert.IsNotNull(farButton,
                "the on-screen far row must carry a clickable button");

            headless.ClickButton(farButton);

            Assert.AreEqual(farLabel, model.LastClickedLabel,
                "clicking the on-screen row's button must act on THAT row's " +
                "DataContext (the row visible on screen)");

            // ---- and the top row was recycled out of view by the scroll -------
            Assert.IsNull(
                headless.On(v => RowVisibleText(v, model.Rows[0])),
                "after scrolling to the far row, the top row should be recycled " +
                "away, proving the visible set genuinely changed");
        }
    }
}
