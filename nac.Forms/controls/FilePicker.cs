using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
        /*
         -----
         Events
         -----
         */
        public event EventHandler<string> FilePathChanged;
        
        /*
         ------
         Properties
         ------
         */
        public static readonly StyledProperty<string> FilePathProperty =
            AvaloniaProperty.Register<FilePicker, string>(nameof(FilePath));

        public string FilePath
        {
            get { return GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }
        
        
        /*
         -----
         Constructor
         ------
         */
        public FilePicker()
        {
            this.InitializeComponent();

            // docs on Property change: https://avaloniaui.net/docs/binding/binding-from-code
            FilePathProperty.Changed.AddClassHandler<FilePicker>(x => FilePath_Changed);
        }


        private void FilePath_Changed(AvaloniaPropertyChangedEventArgs e)
        {
            this.FilePathChanged?.Invoke(this, e.NewValue as string);
        }
        
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        private void openFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.FilePath = DateTime.Now.ToString();
        }
    }
}