using System;
using System.Runtime.CompilerServices;

namespace nac.Forms.lib
{
    public class Log{
        
        public class LogMessage
        {
            public string Level { get; set; }
            public string Message { get; set; }
            public string CallingMemberName { get; set; }
            public DateTime EventDate { get; }

            public LogMessage()
            {
                this.EventDate = DateTime.Now;
            }
        }
        
        public static event EventHandler<LogMessage> OnNewMessage;

        public void Info(string message, [CallerMemberName] string callerMemberName = ""){
            OnNewMessage?.Invoke(this,new LogMessage
            {
                Level = "INFO",
                Message = message,
                CallingMemberName = callerMemberName
            });
        }

        public void Debug(string message, [CallerMemberName] string callerMemberName = ""){
            OnNewMessage?.Invoke(this,new LogMessage
            {
                Level = "DEBUG",
                Message = message,
                CallingMemberName = callerMemberName
            });
        }

        public void Warn(string message, [CallerMemberName] string callerMemberName = ""){
            OnNewMessage?.Invoke(this,new LogMessage
            {
                Level = "WARN",
                Message = message,
                CallingMemberName = callerMemberName
            });
        }

        public void Error(string message, [CallerMemberName] string callerMemberName = ""){
            OnNewMessage?.Invoke(this,new LogMessage
            {
                Level = "ERROR",
                Message = message,
                CallingMemberName = callerMemberName
            });
        }
    }
}