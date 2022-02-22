using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;


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


        public static readonly StyledProperty<bool> FileMustExistProperty =
            AvaloniaProperty.Register<FilePicker, bool>(nameof(FileMustExist), defaultValue: false);

        public bool FileMustExist
        {
            get { return GetValue(FileMustExistProperty); }
            set { SetValue(FileMustExistProperty, value); }
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
            if (FileMustExist)
            {
                // OpenFileDIalog
                promptForFileThatExists();
            }
            else
            {
                // SaveFileDialog
                promptForFile();
            }
        }

        private model.Optional<string> getInitialDirectory()
        {
            var workingDirectory = new model.Optional<string>();

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                try
                {
                    string dirPath = System.IO.Path.GetDirectoryName(FilePath);

                    if (System.IO.Directory.Exists(dirPath))
                    {
                        workingDirectory = dirPath;
                    }
                }catch(Exception ex){}
            }
            
            return workingDirectory;
        }
        
        
        private async Task promptForFileThatExists()
        {
            var dialog = new Avalonia.Controls.OpenFileDialog();
            //dialog.Filters.Add(new FileDialogFilter(){Name = "All", Extensions = {"*"}});

            var initialDirectory = getInitialDirectory();
            if (initialDirectory.IsSet)
            {
                dialog.Directory = initialDirectory.Value;
            }

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                dialog.InitialFileName = System.IO.Path.GetFileName(FilePath);
            }

            // how to get the window from a control: https://stackoverflow.com/questions/56566570/openfiledialog-in-avalonia-error-with-showasync
            var win = (Window) this.GetVisualRoot();
            string[] result = await dialog.ShowAsync(win);

            if (result != null && result.Any())
            {
                FilePath = result.First();
            }
        }
        
        
        private async Task promptForFile()
        {
            var dialog = new Avalonia.Controls.SaveFileDialog();
            //dialog.Filters.Add(new FileDialogFilter(){Name = "All", Extensions = {"*"}});

            var initialDirectory = getInitialDirectory();
            if (initialDirectory.IsSet)
            {
                dialog.Directory = initialDirectory.Value;
            }

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                dialog.InitialFileName = System.IO.Path.GetFileName(FilePath);
            }

            var win = (Window) this.GetVisualRoot();
            string result = await dialog.ShowAsync(win);

            if (!string.IsNullOrWhiteSpace(result))
            {
                FilePath = result;
            }
        }
        
        
        
        
        
        
    }
}