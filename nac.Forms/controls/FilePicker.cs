using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;


/*
 Followed user control guide documentation here:
 https://avaloniaui.net/docs/tutorial/creating-a-view
 */

namespace nac.Forms.controls
{
    public class FilePicker : UserControl
    {
        public FilePicker()
        {
            this.InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        
    }
}