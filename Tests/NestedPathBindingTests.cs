using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.Forms;
using avaloniaDataGrid = Avalonia.Controls.DataGrid;

namespace Tests
{
    /**
     * <summary>
     * Characterization + regression tests for nested-path data binding in
     * nac.Forms  (e.g. form.Table&lt;T&gt;("SelectedBackup.BackupLocations")).
     *
     * The scenario these tests reproduce is the one nacBackup's Avalonia window
     * hits: a control binds to a *nested* model path "Parent.Child" where the
     * parent ("Parent" / SelectedBackup) is null when the form is built and only
     * gets a non-null value later, and the child (an ObservableCollection) is the
     * thing the control actually needs.
     *
     * The two failure modes we are characterizing:
     *   (1) the build-time guard in Table&lt;T&gt; / List&lt;T&gt; throws the
     *       "must be IEnumerable&lt;T&gt;" exception when the parent is null at
     *       build time, and
     *   (2) even when it builds, the binding does not "heal" -- when the parent
     *       later becomes non-null the remaining child path is not re-resolved,
     *       so the control's ItemsSource is never pushed its child collection.
     * </summary>
     */
    [TestClass]
    public class NestedPathBindingTests
    {
        // ---- minimal model mirroring nacBackup's MainViewModel / BackupType / BackupLocations ----

        private class ChildModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private ObservableCollection<Location> _locations = new();

            public ObservableCollection<Location> Locations
            {
                get => _locations;
                set { _locations = value; OnPropertyChanged(); }
            }

            public string Name
            {
                get => _name;
                set { _name = value; OnPropertyChanged(); }
            }
            private string _name;

            private ChildModel() { }

            public ChildModel(string name)
            {
                this._name = name;
            }

            private void OnPropertyChanged()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Locations)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
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
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private class Location
        {
            public string Label { get; set; }
        }

        // ---- helpers ----

        /**
         * <summary>
         * Form.NewForm() calls AppBuilder.SetupWithoutStarting() once per call, and
         * Avalonia throws "Setup was already called on one of AppBuilder instances"
         * after the first one in a process.  So build the FIRST form with NewForm()
         * (which sets up the app) and reuse that same Application for every
         * subsequent form via the public Form(Application, model) constructor,
         * which does NOT re-run setup.
         * </summary>
         */
        private static Avalonia.Application sharedApp;

        private static nac.Forms.Form BuildForm(INotifyPropertyChanged dataContext)
        {
            var f = sharedApp == null
                ? nac.Forms.Form.NewForm()
                : new nac.Forms.Form(__app: sharedApp, _model: null);

            if (sharedApp == null)
            {
                f._Extend_AccessApp(app => sharedApp = app);
            }

            if (dataContext != null)
            {
                f.DataContext = dataContext;
            }

            return f;
        }

        private static avaloniaDataGrid FindDataGrid(nac.Forms.Form f)
        {
            avaloniaDataGrid found = null;
            f._Extend_AccessHost(host =>
            {
                found = host.Children.OfType<avaloniaDataGrid>().FirstOrDefault();
            });
            return found;
        }

        // =====================================================================
        // 1. Baseline happy path: parent already non-null with a populated child
        //    list at build time.  This must work -- it proves the inspection
        //    approach (find the DataGrid, read ItemsSource) is sound.
        // =====================================================================

        [TestMethod]
        public void Table_NestedPath_ParentNonNullAtBuild_ItemsSourceIsChildList()
        {
            var child = new ChildModel("first");
            child.Locations.Add(new Location { Label = "A" });
            child.Locations.Add(new Location { Label = "B" });

            var parent = new ParentModel { Selected = child };

            var f = BuildForm(parent);

            f.Table<Location>(
                "Selected.Locations",
                columns: new[]
                {
                    new nac.Forms.model.Column { Header = "Label", modelBindingPropertyName = "Label" }
                });

            var grid = FindDataGrid(f);
            Assert.IsNotNull(grid, "a DataGrid should have been added to the form host");

            var items = grid.ItemsSource as IEnumerable<Location>;
            Assert.IsNotNull(items, "DataGrid.ItemsSource should be set to the child collection");
            Assert.AreEqual(2, items.Count(), "the two child locations should be the ItemsSource");
        }

        // =====================================================================
        // 2. The case the user actually wants: build with the PARENT NULL, then
        //    set the parent non-null with a populated child list, and assert the
        //    table picked up the rows.  This is the "heal" scenario.
        // =====================================================================

        [TestMethod]
        public void Table_NestedPath_ParentNullAtBuild_HealsToChildListWhenParentSet()
        {
            var parent = new ParentModel(); // Selected is null

            var f = BuildForm(parent);

            // If the build-time guard throws, that itself documents failure mode (1).
            Exception buildException = null;
            try
            {
                f.Table<Location>(
                    "Selected.Locations",
                    columns: new[]
                    {
                        new nac.Forms.model.Column { Header = "Label", modelBindingPropertyName = "Label" }
                    });
            }
            catch (Exception ex)
            {
                buildException = ex;
            }

            if (buildException != null)
            {
                // The current nac.Forms build-time guard throws when the parent is
                // null.  Fail the test so the regression is visible until it heals.
                Assert.Fail(
                    $"Table<> threw at build time with a null parent: {buildException.GetType().Name}: {buildException.Message}");
            }

            var grid = FindDataGrid(f);
            Assert.IsNotNull(grid, "a DataGrid should have been added to the form host");

            // Before healing: no items (parent is still null).
            Assert.IsTrue(
                grid.ItemsSource == null || (grid.ItemsSource as System.Collections.ICollection)?.Count == 0,
                "before the parent is set, the table should have no rows");

            // Now heal: set the parent to a child that has a populated list.
            var child = new ChildModel("healed");
            child.Locations.Add(new Location { Label = "X" });
            child.Locations.Add(new Location { Label = "Y" });
            child.Locations.Add(new Location { Label = "Z" });
            parent.Selected = child; // fires PropertyChanged("Selected")

            // After healing the nested path "Selected.Locations" should resolve to the
            // child's Locations and the DataGrid should show those rows.
            var items = grid.ItemsSource;
            Assert.IsNotNull(items,
                "after the parent heals, DataGrid.ItemsSource should be the child's ObservableCollection");
            var asLoc = items as IEnumerable<Location>;
            Assert.IsNotNull(asLoc, "ItemsSource should be the child's location list");
            Assert.AreEqual(3, asLoc.Count(), "the three child locations should now be the ItemsSource");
        }

        // =====================================================================
        // 3. Change the parent to a DIFFERENT child after build and assert the
        //    table switches to the new child's list (the same heal path, just
        //    parent->parent instead of null->parent).
        // =====================================================================

        [TestMethod]
        public void Table_NestedPath_ParentChangesToAnotherChild_ItemsSourceSwitches()
        {
            var childA = new ChildModel("A");
            childA.Locations.Add(new Location { Label = "a1" });
            childA.Locations.Add(new Location { Label = "a2" });

            var childB = new ChildModel("B");
            childB.Locations.Add(new Location { Label = "b1" });
            childB.Locations.Add(new Location { Label = "b2" });
            childB.Locations.Add(new Location { Label = "b3" });
            childB.Locations.Add(new Location { Label = "b4" });

            var parent = new ParentModel { Selected = childA };

            var f = BuildForm(parent);

            f.Table<Location>(
                "Selected.Locations",
                columns: new[]
                {
                    new nac.Forms.model.Column { Header = "Label", modelBindingPropertyName = "Label" }
                });

            var grid = FindDataGrid(f);
            Assert.IsNotNull(grid);
            Assert.AreEqual(2, (grid.ItemsSource as IEnumerable<Location>).Count());

            parent.Selected = childB; // heal to a different child

            var items = grid.ItemsSource as IEnumerable<Location>;
            Assert.IsNotNull(items, "switching the parent should push the new child's list");
            Assert.AreSame(childB.Locations, grid.ItemsSource);
            Assert.AreEqual(4, items.Count());
        }

        // =====================================================================
        // 4. The parent heals, then a NEW item is added to the child's own
        //    ObservableCollection.  Because the child list is observable and the
        //    DataGrid binds to it by reference, a new item shows up.
        // =====================================================================

        [TestMethod]
        public void Table_NestedPath_AfterHeal_AddingToChildListGrowsRows()
        {
            var parent = new ParentModel(); // null at build

            var f = BuildForm(parent);

            f.Table<Location>(
                "Selected.Locations",
                columns: new[]
                {
                    new nac.Forms.model.Column { Header = "Label", modelBindingPropertyName = "Label" }
                });

            var grid = FindDataGrid(f);
            Assert.IsNotNull(grid);

            var child = new ChildModel("only");
            child.Locations.Add(new Location { Label = "one" });
            parent.Selected = child; // heal

            var col = (grid.ItemsSource as System.Collections.IList);
            Assert.IsNotNull(col, "ItemsSource should be the observable child collection");
            Assert.AreEqual(1, col.Count);

            child.Locations.Add(new Location { Label = "two" }); // observable add
            Assert.AreEqual(2, col.Count);
        }

        // =====================================================================
        // 5. Isolate DEFECT B (the "heal does not propagate" bug) on a scalar
        //    binding that has NO build-time guard: TextFor.  Build the
        //    "Selected.Name" binding while the parent is null, then heal the
        //    parent.  The bound control's value should become the child's Name.
        //    With the current binding code it does NOT -- the heal is lost.
        // =====================================================================

        [TestMethod]
        public void TextFor_NestedPath_HealsWhenParentBecomesNonNull()
        {
            var parent = new ParentModel(); // Selected is null at build

            var f = BuildForm(parent);

            // TextFor has no "must be IEnumerable<T>" guard, so this builds fine
            // even with a null parent.
            f.TextFor("Selected.Name", "no selection");

            // Grab the bound TextBlock nac.Forms created (it is the TextFor label).
            Avalonia.Controls.TextBlock textBlock = null;
            f._Extend_AccessHost(host =>
            {
                textBlock = host.Children.OfType<Avalonia.Controls.TextBlock>().Last();
            });

            Assert.IsNotNull(textBlock, "the bound TextBlock should exist");

            // heal: parent now points at a child with a Name.  With the current
            // binding code the nested binding does NOT heal, so this assertion
            // documents the bug.
            var child = new ChildModel("The Answer");
            child.Name = "The Answer";
            parent.Selected = child;

            Assert.AreEqual("The Answer", textBlock.Text,
                "after the parent heals, the nested 'Selected.Name' binding should update the control");
        }

        // =====================================================================
        // 6. CONTROL / HEALTHY BASELINE: parent is NON-NULL at build time, so
        //    binding setup subscribes to the child.  Changing the child's own
        //    property then propagates to the control.  This PASSES and shows the
        //    child->control path is otherwise fine -- which isolates the bug to
        //    "parent null at build time, then healed later" (tests 2, 4, 5).
        // =====================================================================

        [TestMethod]
        public void TextFor_NestedPath_ParentNonNullAtBuild_ChildChangePropagates()
        {
            var child = new ChildModel("initial");
            var parent = new ParentModel { Selected = child }; // non-null at build

            var f = BuildForm(parent);
            f.TextFor("Selected.Name");

            Avalonia.Controls.TextBlock textBlock = null;
            f._Extend_AccessHost(host =>
            {
                textBlock = host.Children.OfType<Avalonia.Controls.TextBlock>().Last();
            });
            Assert.IsNotNull(textBlock);
            Assert.AreEqual("initial", textBlock.Text);

            // Change the child's own property; the binding must follow it.
            child.Name = "changed";
            Assert.AreEqual("changed", textBlock.Text,
                "with a non-null parent at build, the child's PropertyChanged must reach the control");
        }

        // =====================================================================
        // 7. DIAGNOSTIC PROBE: after the parent heals, is the child subscription
        //    ever set up?  Build "Selected.Name" with a null parent, heal it,
        //    THEN change the child's Name.  If a real child subscription had been
        //    created on heal, the control would update.  With the current code it
        //    does not -- confirming the subscription is the thing the heal loses.
        //    (Expected to FAIL until the heal wires a child subscription.)
        // =====================================================================

        [TestMethod]
        public void TextFor_NestedPath_AfterHeal_ChildChangePropagates()
        {
            var parent = new ParentModel(); // null at build

            var f = BuildForm(parent);
            f.TextFor("Selected.Name");

            Avalonia.Controls.TextBlock textBlock = null;
            f._Extend_AccessHost(host =>
            {
                textBlock = host.Children.OfType<Avalonia.Controls.TextBlock>().Last();
            });
            Assert.IsNotNull(textBlock);

            var child = new ChildModel("seeded");
            parent.Selected = child;          // heal to a non-null child
            child.Name = "after heal change";  // change the child's own property

            Assert.AreEqual("after heal change", textBlock.Text,
                "after healing, a child PropertyChanged must reach the control " +
                "-- this fails today because no child subscription is created on heal");
        }
    }
}
