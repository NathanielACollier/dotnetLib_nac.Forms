using System;
using Avalonia.Controls;
using Avalonia.Media;
using nac.Forms.model;

namespace nac.Forms.lib
{
    public static class styleUtil
    {

        public static void style(TextBlock tb, model.Style style=null)
        {
            styleGeneric(tb, style);
            if (style?.foregroundColor.IsSet == true)
            {
                tb.Foreground = new SolidColorBrush(style.foregroundColor.Value);
            }
        }


        public static void style(Button btn, Style style=null)
        {
            if (style?.foregroundColor.IsSet == true)
            {
                btn.Foreground = new SolidColorBrush(style.foregroundColor.Value);
            }

            if (style?.backgroundColor.IsSet == true)
            {
                btn.Background = new SolidColorBrush(style.backgroundColor.Value);
            }
        }

        private static void styleGeneric( Control ctrl, Style style = null)
        {
            if (style?.height.IsSet == true)
            {

            }
        }
    }
}