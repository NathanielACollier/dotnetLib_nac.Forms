using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using nac.Forms.model;

namespace nac.Forms.UITesterApp.repos;

public static class UIElementsUtility
{
    public static nac.Forms.Form logViewer(nac.Forms.Form f, string logEntriesListModelName)
    {
        var entries = EnsureLogEntriesListIsOnModel(f, logEntriesListModelName);
        
        nac.Logging.Appenders.Notification.Setup( ( _e) =>
        {
            if(_e.CallingClassType.FullName.StartsWith(typeof(nac.Forms.Form).Namespace))
            {
                return; // don't log anything from nac.Forms incase that causes problems with rendering the log messages
            }

            // Modifying the entries list has to be thread safe so use Invoke
            f.InvokeAsync(async () =>
            {
                /*
                 REMEMBER!! - You have to use LogViewerMessage because you have to bind to INotifyPropertyChanged
                */
                entries.Insert(0, new model.LogViewerMessage
                {
                    Date = _e.EventDate,
                    Level = _e.Level.ToString().ToUpper(),
                    Message = _e.Message
                });
            });

        });

        f.List<model.LogViewerMessage>(itemSourcePropertyName: logEntriesListModelName,
            populateItemRow: (_rF) =>
        {
            _rF.HorizontalStack(h =>
            {
                var model = h.Model[nac.Forms.model.SpecialModelKeys.DataContext] as model.LogViewerMessage;

                if (model == null)
                {
                    return; // somehow null model is getting sent in here sometimes
                }

                var levelStyle = new nac.Forms.model.Style();
                if (string.Equals(model.Level, "warn", StringComparison.InvariantCulture))
                {
                    levelStyle.foregroundColor = Avalonia.Media.Colors.Yellow;
                }else if (string.Equals(model.Level, "info", StringComparison.OrdinalIgnoreCase))
                {
                    levelStyle.foregroundColor = Avalonia.Media.Colors.Green;
                }else if (string.Equals(model.Level, "debug", StringComparison.OrdinalIgnoreCase))
                {
                    levelStyle.foregroundColor = Avalonia.Media.Colors.Cyan;
                }else if (string.Equals(model.Level, "error", StringComparison.OrdinalIgnoreCase))
                {
                    levelStyle.foregroundColor = Avalonia.Media.Colors.Red;
                }
                
                h.TextFor(nameof(model.Date))
                    .Text(" - [")
                    .TextFor(nameof(model.Level), style: levelStyle)
                    .Text("] - ")
                    .TextFor(nameof(model.Message));
            });
        });

        return f;
    }

    private static ObservableCollection<model.LogViewerMessage> EnsureLogEntriesListIsOnModel(Form form, string logEntriesListModelName)
    {
        var modelValueInfoResult = form.getModelValue(logEntriesListModelName);

        var emptyEntries = new ObservableCollection<model.LogViewerMessage>();
        
        if (modelValueInfoResult.Value == null)
        {
            form.setModelValue(logEntriesListModelName, emptyEntries);
            return emptyEntries;
        }
        
        // is it the right type?
        var existingEntries = modelValueInfoResult.Value as ObservableCollection<model.LogViewerMessage>;
        if (existingEntries == null)
        {
            throw new Exception($"Existing LogEntriesList model value is of Type={modelValueInfoResult.Value.GetType().FullName}.   It must be Type={emptyEntries.GetType().FullName}");
        }
        
        return existingEntries;
    }
    
    
    
}
