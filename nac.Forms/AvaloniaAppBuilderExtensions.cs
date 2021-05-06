using Avalonia;
using Avalonia.Controls;

namespace nac.Forms
{
    public static class AvaloniaAppBuilderExtensions
    {
        public delegate void ConfigureAppBuilder<TAppBuilder>(TAppBuilder appBuilder)
            where TAppBuilder : Avalonia.Controls.AppBuilderBase<TAppBuilder>, new();
        
        public static Form NewForm<TAppBuilder>(this TAppBuilder appBuilder,
            ConfigureAppBuilder<TAppBuilder> beforeAppBuilderInit=null)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
        {
            beforeAppBuilderInit?.Invoke(appBuilder);
            var builder = appBuilder
                //.LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
                .UsePlatformDetect()
                .SetupWithoutStarting();

            var f = new Form(builder.Instance);
            return f;
        }
    }
}
