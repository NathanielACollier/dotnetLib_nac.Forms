using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using nac.Forms.lib.Extensions;
using nac.Forms.model;

namespace nac.Forms.lib
{
    public static class styleUtil
    {

        public static void style(nac.Forms.Form form, Control ctrl, Style style)
        {
            styleGeneric(form, ctrl, style);
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

        private static void styleGeneric(nac.Forms.Form form, Control ctrl, Style style)
        {
            if (style?.height.IsSet == true)
            {
                ctrl.Height = Convert.ToDouble(style.height.Value);
            }

            if (style?.width.IsSet == true)
            {
                ctrl.Width = Convert.ToDouble(style.width.Value);
            }

            if (style?.isVisibleModelName.IsSet == true)
            {
                form.AddVisibilityTrigger(ctrl, isVisibleModelName: style.isVisibleModelName.Value, trueResultMeansVisible: true);
            }

            if (style?.isHiddenModelName.IsSet == true)
            {
                form.AddVisibilityTrigger(ctrl, isVisibleModelName: style.isHiddenModelName.Value, trueResultMeansVisible: false);
            }

            if (style?.contextMenu != null)
            {
                setupContextMenu(form: form, control: ctrl, contentOfPopup: style.contextMenu);
            }

            if (style?.contextMenuItems.IsSet == true)
            {
                setupContextMenu(form: form, control: ctrl, menuItems: style.contextMenuItems.Value);
            }

            if( style?.TooltipText.IsSet == true)
            {
                // It has a pretty strange API see this: https://github.com/AvaloniaUI/Avalonia/issues/5188
                Avalonia.Controls.ToolTip.SetTip(ctrl, style.TooltipText.Value);
            }

            if( style?.Tooltip != null)
            {
                var tooltipCtrl = form.getBoundControlFromPopulateForm(style.Tooltip);
                // again the same pretty strange API that is called an 'Attached Property'
                Avalonia.Controls.ToolTip.SetTip(ctrl, tooltipCtrl);
            }
        }

        private static void setupContextMenu(Form form, Control control, Action<Form> contentOfPopup)
        {
            // create a popup and populate it
            var contextMenu = new Avalonia.Controls.ContextMenu();
            contextMenu.Template = new FuncControlTemplate((templatedControl, scope) =>
            {
                var ctrl = form.getBoundControlFromPopulateForm(contentOfPopup);
                return ctrl;
            });

            contextMenu.PlacementMode = PlacementMode.Bottom;
            contextMenu.PlacementTarget = control;

            // add the popup to the form
            control.ContextMenu = contextMenu;
        }


        private static void setupContextMenu(Form form, Control control, IEnumerable<model.MenuItem> menuItems)
        {
            // create a popup and populate it
            var contextMenu = new Avalonia.Controls.ContextMenu();

            contextMenu.Items.Set(
                menuItems.Select(i => lib.AvaloniaModelHelpers.convertModelToAvaloniaMenuItem(i))
                );
            
            contextMenu.PlacementMode = PlacementMode.Bottom;
            contextMenu.PlacementTarget = control;

            // add the popup to the form
            control.ContextMenu = contextMenu;
        }
        
        
        public static Style fromCSS(string cssText)
        {
            var style = new Style();
            // process the css, and convert it into Style properties somewhere

            var cssStyle = nac.CSSParsing.StyleParsingHelper.ParseSingleCSSRule(cssText);

            // go through all the rules css supports
            if(cssStyle.fontColor.IsSet)
            {
                style.foregroundColor = Avalonia.Media.Color.Parse(cssStyle.fontColor.Value);
            }

            return style;
        }
        
    }
}