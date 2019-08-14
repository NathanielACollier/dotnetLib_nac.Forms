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
            AddBinding<string>(modelFieldName, label, TextBlock.TextProperty);

			if( defaultValue != null)
            {
                this.Model[modelFieldName] = defaultValue;
            }

            AddRowToHost(label);
            return this;
        }

		public Form TextBoxFor(string modelFieldName)
        {
            var tb = new TextBox();
            AddBinding<string>(modelFieldName, tb, TextBox.TextProperty,
				isTwoWayDataBinding: true);

            AddRowToHost(tb);
            return this;
        }

		public Form Button(string displayText, Action<object> onClick)
        {
            var btn = new Button();

            btn.Content = displayText;

            btn.Click += (_s, _args) =>
            {
                onClick(null);
            };
            AddRowToHost(btn);
            return this;
        }

        public Form SimpleDropDown<T>(List<T> items, Action<T> onItemSelected = null)
            where T: class
        {
            var dropdown = new ComboBox();

            dropdown.SelectionChanged += (_s, _args) =>
            {
                if( onItemSelected != null)
                {
                    onItemSelected(_args.AddedItems[0] as T);
                }
            };

            dropdown.Items = items;

            AddRowToHost(dropdown);
            return this;
        }
    }
}
