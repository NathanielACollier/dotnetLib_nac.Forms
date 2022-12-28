using System;
using Avalonia.Controls;
using Avalonia.Media;
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

            if (style?.popUp != null)
            {
                setupPopUp(form: form, control: ctrl, contentOfPopup: style.popUp);
            }
        }

        private static void setupPopUp(Form form, Control control, Action<Form> contentOfPopup)
        {
            // create a popup and populate it
            var popupControl = new Avalonia.Controls.Primitives.Popup();

            popupControl.Child = form.getBoundControlFromPopulateForm(contentOfPopup);

            popupControl.PlacementMode = PlacementMode.Bottom;
            popupControl.PlacementTarget = control;
            popupControl.IsLightDismissEnabled = true;
            
            // add the popup to the form
            form.AddRowToHost(popupControl);
        }
    }
}