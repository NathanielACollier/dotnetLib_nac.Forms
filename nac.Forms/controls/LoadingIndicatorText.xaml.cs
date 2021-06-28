using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace nac.Forms.controls
{
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