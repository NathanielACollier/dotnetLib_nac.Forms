using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;


/*
 Followed user control guide documentation here:
 https://avaloniaui.net/docs/tutorial/creating-a-view
 
 There is good information on an example of the OpenFileDialog here:
 https://stackoverflow.com/questions/56604439/filedialog-opens-on-loop-using-openfiledialog-on-avalonia
 
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