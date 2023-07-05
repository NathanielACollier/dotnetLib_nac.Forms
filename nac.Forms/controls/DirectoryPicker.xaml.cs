using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace nac.Forms.controls;

public class DirectoryPicker: UserControl
{
    private static lib.Log log = new lib.Log();
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
    private void PickDirectoryButton_OnClick(object sender, RoutedEventArgs e)
    {
        promptForDirectoryPath();
    }

    private async Task promptForDirectoryPath()
    {
        var dialog = new Avalonia.Controls.OpenFolderDialog();
        
        // if there is allready a folder picked, then use it if it exists.  If something isn't right use Desktop
        if (!string.IsNullOrEmpty(this.DirectoryPath) && System.IO.Directory.Exists(this.DirectoryPath))
        {
            dialog.Directory = this.DirectoryPath;
        }
        else
        {
            dialog.Directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        }

        var win = (Window)this.GetVisualRoot();
        string result = await dialog.ShowAsync(win);

        if (!string.IsNullOrWhiteSpace(result))
        {
            DirectoryPath = result;
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