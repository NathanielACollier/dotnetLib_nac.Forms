using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Avalonia.Controls;
using nac.Forms;
using nac.ViewModelBase;
using Tests.Testing;

namespace Tests
{
    /**
     * A button click that increments a counter and a label that shows it.  The
     * test renders a real nac.Forms.Form headless, reads the label's displayed
     * value, clicks the button, and asserts the RENDERED value is one more than
     * the previous read -- repeated for 3 clicks.  This proves the click -> model
     * update -> binding re-render path actually drives the on-screen TextBlock.
     *
     * The counter model is a nac.ViewModelBase, so "increment" is just
     * model.Counter++ on the UI thread (the Click handler runs on the UI thread),
     * which fires PropertyChanged and heals the label's binding.
     */
    [TestClass]
    public class HeadlessButtonCounterTests
    {
        private class CounterModel : ViewModelBase
        {
            public int Counter
            {
                get => GetValue(() => Counter);
                set => SetValue(() => Counter, value);
            }
        }

        [TestMethod]
        public void Button_Click_IncrementsDisplayByOne_EachTime()
        {
            var model = new CounterModel();

            using var headless = HeadlessForm.Create();

            headless.Build(f =>
            {
                // a label bound to the counter; a top-level TextBlock, not the
                // button's content TextBlock, so LastChild<TextBlock> grabs it.
                f.TextFor("Counter");
                f.Button("Increment", () =>
                {
                    model.Counter++;
                    return Task.CompletedTask;
                });
            }, dataContext: model);

            string displayText() => headless.On(v => v.LastChild<TextBlock>()?.Text);

            // starts at 0
            int start = int.Parse(displayText() ?? "0");

            // 3 clicks; each must bump the rendered value by exactly 1
            for (int i = 0; i < 3; i++)
            {
                int before = int.Parse(displayText());
                headless.Click("Increment");
                int after = int.Parse(displayText());

                Assert.AreEqual(before + 1, after,
                    $"after click {i + 1}, the displayed counter should be one more than the previous value");
                Assert.AreEqual(start + i + 1, after,
                    $"after {i + 1} click(s), the displayed counter should be {start + i + 1}");
            }
        }
    }
}
