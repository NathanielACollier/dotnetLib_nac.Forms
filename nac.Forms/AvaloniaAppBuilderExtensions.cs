using Avalonia;
using Avalonia.Controls;

namespace nac.Forms
{
    public static class AvaloniaAppBuilderExtensions
    {
        public static Form NewForm<TAppBuilder>(this TAppBuilder appBuilder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
        {
            appBuilder = Form.configure(appBuilder);
            var builder = appBuilder
                //.LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
                .UsePlatformDetect()
                .SetupWithoutStarting();

            var f = new Form(builder.Instance);
            return f;
        }
    }
}
