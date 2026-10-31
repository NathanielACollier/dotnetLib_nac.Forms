using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.Forms;
using nac.Forms.model;
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

        private class ChildModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private ObservableCollection<Location> _locations = new();
            public ObservableCollection<Location> Locations
            {
                get => _locations;
                set { _locations = value; OnPropertyChanged(nameof(Locations)); }
            }

            private void OnPropertyChanged(string name)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private class ParentModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private ChildModel _selected;
            public ChildModel Selected
            {
                get => _selected;
                set { _selected = value; OnPropertyChanged(nameof(Selected)); }
            }

            private void OnPropertyChanged(string name)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [TestMethod]
        public void Table_ParentNonNullAtBuild_MaterializesRows()
        {
            var child = new ChildModel();
            child.Locations.Add(new Location { Label = "A" });
            child.Locations.Add(new Location { Label = "B" });
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

            Assert.AreEqual(2, headless.VisibleRowCount(),
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
            Assert.AreEqual(0, headless.VisibleRowCount(),
                "before the parent heals, the table should materialize 0 rows");

            // heal: parent now points at a child with 3 locations
            var child = new ChildModel();
            child.Locations.Add(new Location { Label = "X" });
            child.Locations.Add(new Location { Label = "Y" });
            child.Locations.Add(new Location { Label = "Z" });

            headless.Run(() =>
            {
                parent.Selected = child; // fires PropertyChanged("Selected")
            });
            headless.Pump();

            Assert.AreEqual(3, headless.VisibleRowCount(),
                "after the parent heals, the table should materialize the child's 3 rows");
        }
    }
}
