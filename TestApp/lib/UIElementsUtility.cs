using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using nac.Forms.model;

namespace TestApp.lib
{
    public static class UIElementsUtility
    {
        public static nac.Forms.Form logViewer(nac.Forms.Form f)
        {
            var entries = new ObservableCollection<model.LogEntry>();
            model.LogEntry.onNewMessage += (_s, _e) =>
            {
                // Modifying the entries list has to be thread safe so use Invoke
                f.InvokeAsync(async () =>
                {
                    entries.Insert(0, _e);
                });

            };

            f.Model["logEntriesList"] = entries;

            f.List<model.LogEntry>("logEntriesList", populateItemRow: (_rF) =>
            {
                _rF.HorizontalStack(h =>
                {
                    var model = h.Model[nac.Forms.model.SpecialModelKeys.DataContext] as model.LogEntry;

                    if (model == null)
                    {
                        return; // somehow null model is getting sent in here sometimes
                    }

                    var levelStyle = new nac.Forms.model.Style();
                    if (string.Equals(model.level, "warn", StringComparison.InvariantCulture))
                    {
                        levelStyle.foregroundColor = Avalonia.Media.Colors.Yellow;
                    }else if (string.Equals(model.level, "info", StringComparison.OrdinalIgnoreCase))
                    {
                        levelStyle.foregroundColor = Avalonia.Media.Colors.Green;
                    }else if (string.Equals(model.level, "debug", StringComparison.OrdinalIgnoreCase))
                    {
                        levelStyle.foregroundColor = Avalonia.Media.Colors.Cyan;
                    }else if (string.Equals(model.level, "error", StringComparison.OrdinalIgnoreCase))
                    {
                        levelStyle.foregroundColor = Avalonia.Media.Colors.Red;
                    }
                    
                    h.TextFor("date")
                        .Text(" - [")
                        .TextFor("level", style: levelStyle)
                        .Text("] - ")
                        .TextFor("message");
                });
            });

            return f;
        }
    }
}