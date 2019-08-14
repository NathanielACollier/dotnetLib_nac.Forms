using System;
using Avalonia;
using Avalonia.Logging.Serilog;

namespace dotnetCoreAvaloniaNCForms
{
    public static class AvaloniaManager
    {
        static AppBuilder BuildAvaloniaApp()
            => AppBuilder
            .Configure<App>()
            .LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
            .UsePlatformDetect()
            .SetupWithoutStarting()
            ;

        public static Application startAvaloniaApp()
        {
            var builder = BuildAvaloniaApp();
            return builder.Instance;
        }
    }
}
