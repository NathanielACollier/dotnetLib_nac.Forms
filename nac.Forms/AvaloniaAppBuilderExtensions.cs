using Avalonia;
using Avalonia.Controls;

namespace nac.Forms
{
    public static class AvaloniaAppBuilderExtensions
    {
        public delegate void ConfigureAppBuilder(Avalonia.AppBuilder appBuilder);
        
        public static Form NewForm(this Avalonia.AppBuilder appBuilder,
            ConfigureAppBuilder beforeAppBuilderInit=null)
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
