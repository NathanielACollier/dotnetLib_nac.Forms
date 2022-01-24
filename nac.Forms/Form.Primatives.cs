using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using nac.Forms.model;

namespace nac.Forms
{
    public partial class Form
    {

        public Form Text( string textToDisplay,
            model.Style style=null)
        {
            var label = new TextBlock();
            lib.styleUtil.style(this,label, style);
            label.Text = textToDisplay;

            AddRowToHost(label);
            return this;
        }

		public Form TextFor(string modelFieldName, 
                string defaultValue = null,
                model.Style style = null)
        {
            var label = new TextBlock();
            lib.styleUtil.style(this,label,style);
            AddBinding<string>(modelFieldName, label, TextBlock.TextProperty);

			if( defaultValue != null)
            {
                setModelIfNull(modelFieldName, defaultValue);
            }

            AddRowToHost(label);
            return this;
        }

		public Form TextBoxFor(string modelFieldName,
                        bool multiline = false,
                        model.Style style = null,
                        Action<string> onTextChanged=null,
                        bool isPassword = false,
                        bool isReadOnly = false)
        {
            var tb = new TextBox();
            lib.styleUtil.style(this, tb, style);

            AddBinding<string>(modelFieldName, tb, TextBox.TextProperty,
				isTwoWayDataBinding: true);

            // for text changed you do observable because Avalonia hasn't implemented TextChanged for TextBox yet
            //  see: https://github.com/AvaloniaUI/Avalonia/issues/418
            tb.GetObservable(TextBox.TextProperty).Subscribe(newTextValue =>
            {
                onTextChanged?.Invoke(newTextValue);
            });

            tb.IsReadOnly = isReadOnly;

            if (isPassword)
            {
                tb.PasswordChar = '*';
                tb.Watermark = "Type in password";
                tb.UseFloatingWatermark = true;
            }
            
            if (multiline)
            {
                tb.AcceptsReturn = true;
                tb.AcceptsTab = true;
                tb.TextWrapping = TextWrapping.Wrap;
                
                AddRowToHost(tb, rowAutoHeight: false);
            }
            else
            {
                AddRowToHost(tb);
            }

            return this;
        }

        public class ButtonFunctions
        {
            public Action<model.Style> setStyle;
        }

		public Form Button(string displayText, Action<object> onClick, 
                Style style = null,
                ButtonFunctions functions = null)
        {
            var btn = new Button();
            lib.styleUtil.style(this, btn, style);
            btn.Content = displayText;

            btn.Click += (_s, _args) =>
            {
                onClick(null);
            };

            if (functions != null)
            {
                functions.setStyle = (_newStyle) =>
                {
                    lib.styleUtil.style(this, btn, _newStyle);
                };
            }
            
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


        public Form LoadingTextAnimation(model.Style style=null)
        {
            var loadingDisplay = new controls.LoadingIndicatorText();
            lib.styleUtil.style(this,loadingDisplay, style);
            AddRowToHost(loadingDisplay);

            return this;
        }
        
        
        
    }
}
