using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

using nac.utilities;


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
        private static nac.Logging.Logger log = new();
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
        }

        

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == FilePathProperty) {
                
                log.Debug($"OnPropertyChanged FilePath:[{FilePath}]");
                this.FilePathChanged?.Invoke(this, FilePath);
            }
        }
        
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        private async void openFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FileMustExist)
            {
                // OpenFileDIalog
                await promptForFileThatExists();
            }
            else
            {
                // SaveFileDialog
                await promptForFile();
            }
        }

        private Optional<string> getInitialDirectory()
        {
            var workingDirectory = new Optional<string>();

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                try
                {
                    string dirPath = System.IO.Path.GetDirectoryName(FilePath);

                    if (System.IO.Directory.Exists(dirPath))
                    {
                        workingDirectory = dirPath;
                    }
                }catch{}
            }
            
            return workingDirectory;
        }
        
        private async Task<IStorageFolder> GetInitialFolder(IStorageProvider storage)
        {
            Avalonia.Platform.Storage.IStorageFolder folder;
            
            var initialDirectory = getInitialDirectory();
            if (initialDirectory.IsSet)
            {
                folder = await storage.TryGetFolderFromPathAsync(new Uri(initialDirectory.Value));
                return folder;
            }
            
            folder = await storage.TryGetWellKnownFolderAsync(Avalonia.Platform.Storage.WellKnownFolder.Desktop);
            return folder;
        }
        
        private async Task promptForFileThatExists()
        {
            var storage = TopLevel.GetTopLevel(this).StorageProvider;

            var fileOpenOptions = new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                AllowMultiple = false,
            };

            fileOpenOptions.SuggestedStartLocation = await GetInitialFolder(storage);

            var files = await storage.OpenFilePickerAsync(fileOpenOptions);

            if (files.Any())
            {
                FilePath = files.First().Path.LocalPath;
            }
        }


        
        private async Task promptForFile()
        {
            var storage = TopLevel.GetTopLevel(this).StorageProvider;

            var fileSaveOptions = new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                
            };

            fileSaveOptions.SuggestedStartLocation = await GetInitialFolder(storage);
            
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                fileSaveOptions.SuggestedFileName = System.IO.Path.GetFileName(FilePath);
            }

            var file = await storage.SaveFilePickerAsync(fileSaveOptions);

            if (file != null)
            {
                FilePath = file.Path.LocalPath;
            }
        }
        
        
        
        
        
        
    }
}