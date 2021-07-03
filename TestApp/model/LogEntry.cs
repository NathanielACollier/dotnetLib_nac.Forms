using System;

namespace TestApp.model
{
    public class LogEntry
    {
        public DateTime date { get; set; }
        public string message { get; set; }
        public string level { get; set; }


        public static event EventHandler<LogEntry> onNewMessage;

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