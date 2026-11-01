using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Avalonia.Controls;
using nac.Forms;
using Tests.Testing;

namespace Tests
{
    /**
     * Smallest possible headless test using the GENERIC visual-tree inspection:
     * a label bound to a NESTED path "MyChildProperty.TextContent" whose parent
     * (MyChildProperty) is null at build time.  A button heals the parent; the
     * binding then renders the child's TextContent into the label.  Inspected by
     * searching the rendered visual tree for the TextBlock and reading its .Text,
     * and by finding the button in the tree and clicking it -- no per-type
     * helpers.
     */
    [TestClass]
    public class HeadlessTextHealTests
    {
        private class Child
        {
            public string TextContent { get; set; }
        }

        private class Parent : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private Child _myChildProperty;
            public Child MyChildProperty
            {
                get => _myChildProperty;
                set { _myChildProperty = value; OnPropertyChanged(nameof(MyChildProperty)); }
            }

            private void OnPropertyChanged(string name)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [TestMethod]
        public void NestedTextBinding_HealsWhenButtonSetsParent()
        {
            var parent = new Parent(); // MyChildProperty is null at build time

            using var headless = HeadlessForm.Create();

            headless.Build(f =>
            {
                f.TextFor("MyChildProperty.TextContent");
                f.Button("Set Parent", async () =>
                {
                    parent.MyChildProperty = new Child { TextContent = "Hello!" };
                });
            }, dataContext: parent);

            // parent is null, so the nested path can't resolve yet -- the label
            // has not rendered the child's content.  Inspect via the tree: grab
            // the form's TextBlock label (a top-level control, NOT the TextBlock
            // that is the button's content).
            string textBefore = headless.On(v => v.LastChild<TextBlock>()?.Text);
            Assert.IsFalse((textBefore ?? "").StartsWith("Hello!"),
                "before the parent is set, the label should not show the child content yet");

            // find the button in the top-level controls by its content and click
            // it; the click heals the parent and the binding re-renders.
            headless.Click("Set Parent");

            string textAfter = headless.On(v => v.LastChild<TextBlock>()?.Text);
            StringAssert.StartsWith("Hello!", textAfter ?? "",
                "after the button sets the parent, the nested path should render it");
        }
    }
}
