using System;

namespace nac.Forms.lib
{
    public class Log{
        private void writeMessage(string level, string message){
            string line = $"[{DateTime.Now:hh_mm_tt}] - {level} - {message}";

            if( string.Equals(level, "Info", StringComparison.OrdinalIgnoreCase)){
                System.Console.ForegroundColor = ConsoleColor.White;
            }else if( string.Equals(level, "Debug", StringComparison.OrdinalIgnoreCase)){
                System.Console.ForegroundColor = ConsoleColor.Cyan;
            }else if( string.Equals(level, "Warn", StringComparison.OrdinalIgnoreCase)){
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            }else if( string.Equals(level, "Error", StringComparison.OrdinalIgnoreCase)){
                System.Console.ForegroundColor = ConsoleColor.Red;
            }
            System.Console.WriteLine(line);

            System.Console.ResetColor();
        }

        public void Info(string message){
            writeMessage(level: "INFO", message: message);
        }

        public void Debug(string message){
            writeMessage(level: "DEBUG", message: message);
        }

        public void Warn(string message){
            writeMessage(level: "WARN", message: message);
        }

        public void Error(string message){
            writeMessage(level: "ERROR", message: message);
        }
    }
}