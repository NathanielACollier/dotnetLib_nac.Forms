using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace dotnetCoreAvaloniaNCForms
{
    public partial class Form
    {

        public Form Text( string textToDisplay)
        {
            var label = new TextBlock();
            label.Text = textToDisplay;

            AddRowToHost(label);
            return this;
        }

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
                if( onItemSelected != null && 
                    _args.AddedItems.Count > 0 &&
                    _args.AddedItems[0] is T itemSelected &&
                    itemSelected != null
                )
                {
                    onItemSelected(itemSelected);
                }
            };

            dropdown.Items = items;

            AddRowToHost(dropdown);
            return this;
        }
    }
}
