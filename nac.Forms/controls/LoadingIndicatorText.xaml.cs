using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace nac.Forms.controls
{
    /*
     Original marquee animation came from here: https://github.com/AvaloniaUI/Avalonia/issues/1263
     */
    public class LoadingIndicatorText : UserControl
    {
        public LoadingIndicatorText()
        {
            this.InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}