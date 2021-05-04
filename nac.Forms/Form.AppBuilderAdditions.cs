using System.Collections.Generic;

namespace nac.Forms
{
    public partial class Form
    {
        public delegate void ConfigureAppBuilder<TAppBuilder>(TAppBuilder appBuilder)
            where TAppBuilder : Avalonia.Controls.AppBuilderBase<TAppBuilder>, new();
        /*
         The purpose of this file is to make it so you can add additions to the way the Avalonia App is setup
         */
        private static Dictionary<string, object> appBuilderCalls = new Dictionary<string, object>();

        public static void AddCall<TAppBuilder>(string key, ConfigureAppBuilder<TAppBuilder> configureAppBuilder)
            where TAppBuilder : Avalonia.Controls.AppBuilderBase<TAppBuilder>, new()
        {
            appBuilderCalls[key] = configureAppBuilder;
        }

        public static TAppBuilder configure<TAppBuilder>(TAppBuilder appBuilder)
            where TAppBuilder : Avalonia.Controls.AppBuilderBase<TAppBuilder>, new()
        {
            foreach (var call in appBuilderCalls)
            {
                if (call.Value is ConfigureAppBuilder<TAppBuilder> funcCall)
                {
                    funcCall.Invoke(appBuilder);
                }
            }

            return appBuilder;
        }
        
    }
}