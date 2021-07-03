using System.Collections.ObjectModel;

namespace TestApp.lib
{
    public static class UIElementsUtility
    {
        public static nac.Forms.Form logViewer(nac.Forms.Form f)
        {
            var entries = new ObservableCollection<model.LogEntry>();
            model.LogEntry.onNewMessage += (_s, _e) =>
            {
                entries.Insert(0, _e);
            };

            f.Model["logEntriesList"] = entries;

            f.List<model.LogEntry>("logEntriesList", populateItemRow: (_rF) =>
            {
                _rF.HorizontalStack(h =>
                {
                    h.TextFor("date")
                        .Text(" - [")
                        .TextFor("level")
                        .Text("] - ")
                        .TextFor("message");
                });
            });

            return f;
        }
    }
}