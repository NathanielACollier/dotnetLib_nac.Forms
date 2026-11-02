using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Avalonia.Controls;
using nac.Forms;
using nac.Forms.model;
using nac.ViewModelBase;
using Tests.Testing;

namespace Tests
{
    /**
     * Renders a real nac.Forms.Form headless and asserts on the actual DataGridRow
     * visual tree.  The scenario is nacBackup's: a table bound to a nested path
     * "Selected.Locations" whose parent (Selected) is null at build time, then
     * heals.  We assert the table shows 0 rows before the heal and the child's
     * rows after, on the rendered visuals rather than on grid.ItemsSource.
     *
     * The models are nac.ViewModelBase, so healing a parent is a one-line
     * SetValue(() => Selected, ...) -- no INotifyPropertyChanged boilerplate.
     *
     * ISOLATION: the headless session and the real UsePlatformDetect() path
     * (Form.NewForm in FormTests / NestedPathBindingTests) both set Avalonia's
     * one-shot, process-global Application/platform state.  If a real-app test
     * runs first in the same process, the later headless render cannot
     * materialize rows.  MSTest 2.2.10 (the version here) has NO
     * [IsolatedProcess] (that is MSTest v3), so isolate by running this class in
     * its own `dotnet test --filter` invocation.  See docs/AGENTS_nacFormsHead-
     * lessTestSetup.md §6/§6b.
     */
    [TestClass]
    public class HeadlessHealTests
    {
        private class Location { public string Label { get; set; } }

        private class ChildModel : ViewModelBase
        {
            public ObservableCollection<Location> Locations
            {
                get => GetValue(() => Locations);
                set => SetValue(() => Locations, value);
            }
        }

        private class ParentModel : ViewModelBase
        {
            public ChildModel Selected
            {
                get => GetValue(() => Selected);
                set => SetValue(() => Selected, value);
            }
        }

        [TestMethod]
        public void Table_ParentNonNullAtBuild_MaterializesRows()
        {
            var child = new ChildModel
            {
                Locations = new ObservableCollection<Location>
                {
                    new Location { Label = "A" },
                    new Location { Label = "B" },
                },
            };
            var parent = new ParentModel { Selected = child };

            using var headless = HeadlessForm.Create();

            headless.Build(f =>
            {
                f.Table<Location>(
                    "Selected.Locations",
                    columns: new Column[]
                    {
                        new Column { Header = "Label", modelBindingPropertyName = "Label" }
                    });
            }, dataContext: parent);

            // Inspect via the generic tree query: search the rendered visuals for
            // the materialized DataGridRows (the same thing the product's
            // DataGridVisibleRowsObserverRepo observes, but on the visual tree).
            int rows = headless.On(v => v.OfType<Avalonia.Controls.DataGridRow>().Count());
            Assert.AreEqual(2, rows,
                "a table whose parent is non-null at build should materialize its 2 rows");
        }

        [TestMethod]
        public void Table_ParentNullAtBuild_MaterializesRowsAfterHeal()
        {
            var parent = new ParentModel(); // Selected is null at build time

            using var headless = HeadlessForm.Create();

            headless.Build(f =>
            {
                f.Table<Location>(
                    "Selected.Locations",
                    columns: new Column[]
                    {
                        new Column { Header = "Label", modelBindingPropertyName = "Label" }
                    });
            }, dataContext: parent);

            // parent is still null -> no rows yet
            int rowsBefore = headless.On(v => v.OfType<Avalonia.Controls.DataGridRow>().Count());
            Assert.AreEqual(0, rowsBefore,
                "before the parent heals, the table should materialize 0 rows");

            // heal: parent now points at a child with 3 locations
            var child = new ChildModel
            {
                Locations = new ObservableCollection<Location>
                {
                    new Location { Label = "X" },
                    new Location { Label = "Y" },
                    new Location { Label = "Z" },
                },
            };

            headless.Run(() =>
            {
                parent.Selected = child; // SetValue fires PropertyChanged("Selected")
            });
            headless.Pump();

            int rowsAfter = headless.On(v => v.OfType<Avalonia.Controls.DataGridRow>().Count());
            Assert.AreEqual(3, rowsAfter,
                "after the parent heals, the table should materialize the child's 3 rows");
        }
    }
}
