using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace dotnetCoreAvaloniaNCForms
{
    public partial class Form
    {

		public Form TextFor(string modelFieldName, string defaultValue = null)
        {
            var label = new TextBlock();

            return this;
        }

		public Form Button(string displayText, Action<object> onClick)
        {
            var btn = new Button();

			
            return this;
        }
    }
}
