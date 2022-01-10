using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace nac.Forms.controls;

public class DirectoryPicker: UserControl
{
    
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
        
        // docs on Property change: https://avaloniaui.net/docs/binding/binding-from-code
        DirectoryPathProperty.Changed.AddClassHandler<DirectoryPicker>(x => DirectoryPath_Changed);
    }
    
    /*
    --------
    Methods
    --------
     */
    private void PickDirectoryButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
    
    private void DirectoryPath_Changed(AvaloniaPropertyChangedEventArgs e)
    {
        this.DirectoryPathChanged?.Invoke(this, e.NewValue as string);
    }
}