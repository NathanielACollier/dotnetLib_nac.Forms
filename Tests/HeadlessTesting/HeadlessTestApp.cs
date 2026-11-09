using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Simple;

namespace Tests.HeadlessTesting
{
    // Minimal Application the headless test session starts.
    //
    // A DataGrid only gets its visual template (and therefore its
    // DataGridRowsPresenter) when BOTH a theme AND the DataGrid control style
    // are loaded.  SimpleTheme alone is not enough -- DataGrid ships its own
    // template in Avalonia.Controls.DataGrid/Themes/Simple.xaml.  This mirrors
    // what nac.Forms does (Form.Table.cs addDataGridStyleToApp + App.xaml
    // SimpleTheme).
    public class HeadlessTestApp : Application
    {
        public override void Initialize()
        {
            Styles.Add(new SimpleTheme());

            var datagridStyleUri = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Simple.xaml");
            Styles.Add(new StyleInclude(datagridStyleUri) { Source = datagridStyleUri });
        }
    }
}
