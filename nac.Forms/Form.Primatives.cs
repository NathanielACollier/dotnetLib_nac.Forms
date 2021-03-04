using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace nac.Forms
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


        public Form Menu(model.MenuItem[] items)
        {
            var menu = new global::Avalonia.Controls.Menu();
            menu.Items = items.Select(i => convertModelToAvaloniaMenuItem(i));
            
            AddRowToHost(menu);
            return this;
        }

        private global::Avalonia.Controls.MenuItem convertModelToAvaloniaMenuItem(model.MenuItem item)
        {
            var avaloniaItem = new global::Avalonia.Controls.MenuItem
            {
                Header = item.Header
            };

            if (item.Action != null)
            {
                avaloniaItem.Click += (_s, _args) =>
                {
                    item.Action();
                };
            }

            
            if (item.Items?.Any() == true)
            {
                var subMenuItems = new List<global::Avalonia.Controls.MenuItem>();
                foreach (var i in item.Items)
                {
                    subMenuItems.Add(
                        convertModelToAvaloniaMenuItem(i)    
                    );
                }

                avaloniaItem.Items = subMenuItems;
            }

            return avaloniaItem;
        }
        
        
    }
}
