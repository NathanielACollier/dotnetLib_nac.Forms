using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace nac.Forms.controls;

public class DirectoryPicker: UserControl
{
    private static nac.Logging.Logger log = new();
    /*
     -----
     Events
     -----
     */
    public event EventHandler<string> DirectoryPathChanged;
        
    /*
     ------
     Properties
     ------
     */
    public static readonly StyledProperty<string> DirectoryPathProperty =
        AvaloniaProperty.Register<DirectoryPicker, string>(nameof(DirectoryPath));

    public string DirectoryPath
    {
        get { return GetValue(DirectoryPathProperty); }
        set { SetValue(DirectoryPathProperty, value); }
    }
    

    /*
     -----------
     Constructor
     -----------
     */
    public DirectoryPicker()
    {
        this.InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    /*
    --------
    Methods
    --------
     */
    private async void PickDirectoryButton_OnClick(object sender, RoutedEventArgs e)
    {
        await promptForDirectoryPath();
    }

    private async Task promptForDirectoryPath()
    {
        var storage = TopLevel.GetTopLevel(this).StorageProvider;

        var folderOpenOptions = new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            AllowMultiple = false
        };
        
        // if there is allready a folder picked, then use it if it exists.  If something isn't right use Desktop
        if (!string.IsNullOrEmpty(this.DirectoryPath) && System.IO.Directory.Exists(this.DirectoryPath))
        {
            folderOpenOptions.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(this.DirectoryPath));
        }
        else
        {
            folderOpenOptions.SuggestedStartLocation = await storage.TryGetWellKnownFolderAsync(Avalonia.Platform.Storage.WellKnownFolder.Desktop);
        }

        var folders = await storage.OpenFolderPickerAsync(folderOpenOptions);

        if (folders.Any())
        {
            DirectoryPath = folders.First().Path.LocalPath;
        }
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DirectoryPathProperty) {
            log.Debug($"OnPropertyChanged FilePath:[{DirectoryPath}]");
            this.DirectoryPathChanged?.Invoke(this, DirectoryPath);
        }
    }
}