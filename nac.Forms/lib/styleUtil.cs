using Avalonia.Controls;
using Avalonia.Media;

namespace nac.Forms.lib
{
    public static class styleUtil
    {

        public static void style(TextBlock tb, model.Style style=null)
        {
            if (style?.foregroundColor.IsSet == true)
            {
                tb.Foreground = new SolidColorBrush(style.foregroundColor.Value);
            }
        }
        
        
    }
}