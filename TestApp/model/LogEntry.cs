using System;

namespace TestApp.model
{
    public class LogEntry: nac.Forms.model.ViewModelBase
    {
        public DateTime date
        {
            get { return GetValue(() => date);}
            set { SetValue(() => date, value);}
        }
        public string message
        {
            get { return GetValue(() => message); }
            set { SetValue(() => message, value);}
        }
        public string level
        {
            get { return GetValue(() => level); }
            set { SetValue(() => level, value); }
        }


        public static event EventHandler<LogEntry> onNewMessage;


        public static void __translateNacFormsLogMessage(nac.Forms.lib.Log.LogMessage __formLogEntry)
        {
            onNewMessage?.Invoke(null, new LogEntry
            {
                date = __formLogEntry.EventDate,
                level = __formLogEntry.Level,
                message = __formLogEntry.Message
            });
        }

        public static void info(string message)
        {
            onNewMessage?.Invoke(null,new LogEntry()
            {
                date = DateTime.Now,
                message = message,
                level = "INFO"
            });
        }

        public static void debug(string message)
        {
            onNewMessage?.Invoke(null,new LogEntry()
            {
                date = DateTime.Now,
                message = message,
                level = "DEBUG"
            });
        }
        
        public static void warn(string message)
        {
            onNewMessage?.Invoke(null,new LogEntry()
            {
                date = DateTime.Now,
                message = message,
                level = "WARN"
            });
        }
        
        public static void error(string message)
        {
            onNewMessage?.Invoke(null,new LogEntry()
            {
                date = DateTime.Now,
                message = message,
                level = "ERROR"
            });
        }
    }
}