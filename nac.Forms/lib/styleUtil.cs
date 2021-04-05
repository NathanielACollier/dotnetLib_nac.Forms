using System;
using Avalonia.Controls;
using Avalonia.Media;
using nac.Forms.model;

namespace nac.Forms.lib
{
    public static class styleUtil
    {

        public static void style(Control ctrl, Style style)
        {
            styleGeneric(ctrl, style);
            if (ctrl is Avalonia.Controls.Primitives.TemplatedControl tControl)
            {
                styleTemplated(tControl, style);
            }else if (ctrl is TextBlock textCtrl)
            {
                styleTextBlock(textCtrl, style);
            }
        }

        private static void styleTextBlock(TextBlock textCtrl, Style style)
        {
            if (style?.foregroundColor.IsSet == true)
            {
                textCtrl.Foreground = new SolidColorBrush(style.foregroundColor.Value);
            }

            if (style?.backgroundColor.IsSet == true)
            {
                textCtrl.Background = new SolidColorBrush(style.backgroundColor.Value);
            }
        }

        private static void styleTemplated(Avalonia.Controls.Primitives.TemplatedControl ctrl, Style style)
        {
            if (style?.foregroundColor.IsSet == true)
            {
                ctrl.Foreground = new SolidColorBrush(style.foregroundColor.Value);
            }

            if (style?.backgroundColor.IsSet == true)
            {
                ctrl.Background = new SolidColorBrush(style.backgroundColor.Value);
            }
        }

        private static void styleGeneric( Control ctrl, Style style)
        {
            if (style?.height.IsSet == true)
            {
                ctrl.Height = Convert.ToDouble(style.height.Value);
            }

            if (style?.width.IsSet == true)
            {
                ctrl.Width = Convert.ToDouble(style.width.Value);
            }
        }
        
        
        
        
    }
}