using System;
using Avalonia;
using Avalonia.Controls;

namespace NC.Forms.Avalonia
{
    public static class AvaloniaAppBuilderExtensions
    {
        public static Form NewForm<TAppBuilder>(this TAppBuilder appBuilder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
        {
            var builder = appBuilder
                //.LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
                .UsePlatformDetect()
                .SetupWithoutStarting();

            var f = new Form(builder.Instance);
            return f;
        }
    }
}
