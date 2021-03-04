using Avalonia;
using Avalonia.Markup.Xaml;

namespace nac.Forms
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
