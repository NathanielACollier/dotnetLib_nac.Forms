using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using nac.Forms.lib.Extensions;
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
                        bool isReadOnly = false,
                        string watermarkText = null,
                        Func<string,object> convertFromUIToModel = null,
                        Action<Avalonia.Input.KeyEventArgs> onKeyPress = null
                        )
        {
            var tb = new TextBox();
            lib.styleUtil.style(this, tb, style);

            AddBinding<string>(modelFieldName, tb, TextBox.TextProperty,
				isTwoWayDataBinding: true,
                convertFromUIToModel: convertFromUIToModel);

            // for text changed you do observable because Avalonia hasn't implemented TextChanged for TextBox yet
            //  see: https://github.com/AvaloniaUI/Avalonia/issues/418
            tb.GetObservable(TextBox.TextProperty).Subscribe(new Avalonia.Reactive.AnonymousObserver<string>( newTextValue =>
            {
                onTextChanged?.Invoke(newTextValue);
            }));

            tb.IsReadOnly = isReadOnly;

            if (!string.IsNullOrWhiteSpace(watermarkText))
            {
                tb.Watermark = watermarkText;
                tb.UseFloatingWatermark = true;
            }

            if (isPassword)
            {
                tb.PasswordChar = '*';
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


            if (onKeyPress != null)
            {
                tb.KeyUp += (_s, _args) =>
                {
                    onKeyPress(_args);
                };
            }

            return this;
        }


        public Form Button(Action<Form> populateButtonContent,
            Func<Task> onClick,
            Style style = null,
            ButtonFunctions functions = null)
        {
            return Button(displayText: "",
                onClick: onClick,
                populateButtonContent: populateButtonContent,
                style: style,
                functions: functions);
        }
        

        public class ButtonFunctions
        {
            public Action<model.Style> setStyle;
        }

		public Form Button(string displayText, Func<Task> onClick, 
                Style style = null,
                Action<Form> populateButtonContent=null,
                ButtonFunctions functions = null)
        {
            var btn = new Button();
            lib.styleUtil.style(this, btn, style);

            if (populateButtonContent != null)
            {
                btn.Content = getBoundControlFromPopulateForm(populateButtonContent);
            }
            else
            {
                btn.Content = displayText;
            }

            btn.Click += async (_s, _args) =>
            {
                await onClick();
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

            dropdown.Items.Set( items);

            AddRowToHost(dropdown);
            return this;
        }


        public Form Menu(model.MenuItem[] items)
        {
            var menu = new global::Avalonia.Controls.Menu();
            menu.Items.Set(
                items.Select(i => lib.AvaloniaModelHelpers.convertModelToAvaloniaMenuItem(i))
                );
            
            AddRowToHost(menu);
            return this;
        }


        public Form LoadingTextAnimation(model.Style style=null)
        {
            var loadingDisplay = new controls.LoadingIndicatorText();
            lib.styleUtil.style(this,loadingDisplay, style);
            AddRowToHost(loadingDisplay);

            return this;
        }


        public Form Image(string modelFieldName,
            model.Style style = null)
        {
            var imgControl = new Avalonia.Controls.Image();
            lib.styleUtil.style(this, imgControl, style);

            imgControl.Stretch = Stretch.Uniform;

            AddBinding<Avalonia.Media.IImage>(modelFieldName: modelFieldName,
                                                    control: imgControl,
                                                    property: Avalonia.Controls.Image.SourceProperty,
                                                convertFromModelToUI: (val) =>
                                                {
                                                    byte[] imgData = val as byte[];

                                                    if (imgData == null)
                                                    {
                                                        imgData = lib.ImageHelpers.CreateEmptyBitmapImage();
                                                    }
                                                    
                                                    using (var ms = new System.IO.MemoryStream(imgData))
                                                    {
                                                        var image = new Avalonia.Media.Imaging.Bitmap(ms);
                                                        return image;
                                                    }
                                                });
            
            AddRowToHost(imgControl, rowAutoHeight: false);
            return this;
        }


        public Form DateFor(string modelFieldName,
            model.Style style = null)
        {
            var dateCtrl = new Avalonia.Controls.DatePicker();
            lib.styleUtil.style(this, dateCtrl, style);
            
            AddBinding<DateTimeOffset?>(modelFieldName: modelFieldName,
                control: dateCtrl,
                property: Avalonia.Controls.DatePicker.SelectedDateProperty,
                isTwoWayDataBinding: true,
                convertFromModelToUI: (val) =>
                {
                    if (val is string dateText &&
                        DateTime.TryParse(dateText, out DateTime dateVal)
                        )
                    {
                        return new DateTimeOffset(dateVal);
                    }else if (val is DateTime dateval)
                    {
                        return new DateTimeOffset(dateval);
                    }

                    return null;
                },
                convertFromUIToModel: (uiValue) =>
                {
                    if (uiValue.HasValue)
                    {
                        return uiValue.Value.DateTime;
                    }

                    return null;
                });
            
            AddRowToHost(dateCtrl);
            return this;
        }
        
        
    }
}
