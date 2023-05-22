using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using nac.Forms.model;

namespace nac.Forms
{
    public partial class Form
    {
        public Form List<T>(string itemSourcePropertyName, 
                        Action<Form> populateItemRow=null,
                        Style style=null,
                        Action<IEnumerable<T>> onSelectionChanged = null)
        {
            var itemsCtrl = new ListBox();
            lib.styleUtil.style(this, itemsCtrl, style);
            
            // allow multiple selections
            itemsCtrl.SelectionMode = SelectionMode.Multiple;
            

            // if T is string, or they just want to use ToString of T as the entry in the list, then they don't need an item template
            if (populateItemRow != null)
            {
                // this is documented here: https://avaloniaui.net/docs/templates/datatemplates-in-code
                itemsCtrl.ItemTemplate = new FuncDataTemplate<object>((itemModel, nameScope) =>
                {
                    var rowForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
                    // this has to have a unique model
                    rowForm.DataContext = itemModel;
                    populateItemRow(rowForm);

                    rowForm.Host.DataContext = itemModel;

                    return rowForm.Host;
                });
            }
            
            if( !(getModelValue(itemSourcePropertyName)?.Value is IEnumerable<T>))
            {
                throw new Exception($"Model items source property specified by name [{itemSourcePropertyName}] must be a IEnumerable<T>");
            }

            AddBinding<IEnumerable>(itemSourcePropertyName, itemsCtrl, ItemsControl.ItemsProperty,
                isTwoWayDataBinding: false);

            // handle selection changed
            itemsCtrl.SelectionChanged += (_s, _args) =>
            {
                if (itemsCtrl.SelectedItems.OfType<T>().Any())
                {
                    // only fire this if there is a change in what is selected.
                    onSelectionChanged?.Invoke(itemsCtrl.SelectedItems.Cast<T>());
                }
            };

            AddRowToHost(itemsCtrl, rowAutoHeight:false);

            var debugTest = itemsCtrl.Items;
            var debugTemplate = itemsCtrl.ItemTemplate;

            return this;
        }


        public Form DropDown<T>(string itemSourceModelName, 
                        string selectedItemModelName, 
                        Action<T> onSelectionChanged=null,
                        Action<Form> populateItemRow = null,
                        model.Style style = null)
        {
            var dp = new Avalonia.Controls.ComboBox();
            lib.styleUtil.style(this,dp,style);
            
            // Just as a safety check, make them init the model first
            if( !(getModelValue(itemSourceModelName)?.Value is IEnumerable<T>))
            {
                throw new Exception($"Model {nameof(itemSourceModelName)} source property specified by name [{itemSourceModelName}] must be a IEnumerable<T>");
            }

            
            // item source binding
            AddBinding<IEnumerable>(modelFieldName: itemSourceModelName,
                                        control: dp,
                                        property: Avalonia.Controls.ComboBox.ItemsProperty,
                                        isTwoWayDataBinding:false);
            
            // selected item binding
            AddBinding<object>(modelFieldName: selectedItemModelName,
                                        control: dp,
                                        property: Avalonia.Controls.ComboBox.SelectedItemProperty,
                                        isTwoWayDataBinding:true);
            
            if (populateItemRow != null)
            {
                dp.ItemTemplate = new FuncDataTemplate<object>((itemModel, nameScope) =>
                {
                    if (itemModel == null)
                    {
                        // how could itemModel be null?  Sometimes it is though, so strange
                        var tb = new Avalonia.Controls.TextBlock();
                        tb.Text = "(null)";
                        return tb;
                    }
                    
                    var rowForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
                    // this has to have a unique model
                    rowForm.DataContext = itemModel;
                    populateItemRow(rowForm);

                    rowForm.Host.DataContext = itemModel;

                    return rowForm.Host;
                });
            }

            
            
            dp.SelectionChanged += (_s, _args) =>
            {
                if (_args.AddedItems.OfType<T>().Any())
                {
                    var first = _args.AddedItems.Cast<T>().First();
                    onSelectionChanged?.Invoke(first);
                }
            };
            
            AddRowToHost(dp);
            return this;
        }



        public Form Autocomplete<T>(
            string selectedItemModelName,
            string itemSourceModelName = null,
            string selectedTextModelName = null,
            Action<T> onSelectionChanged = null,
            Action<Form> populateItemRow = null,
            model.Style style = null,
            Func<string, Task<IEnumerable<T>>> populateItemsOnTextChange = null)
        {
            var tb = new Avalonia.Controls.AutoCompleteBox();
            lib.styleUtil.style(this, tb, style);
                        
            // check to make sure they aren't trying to item source bind, and use a populator function
            if (populateItemsOnTextChange != null &&
                !string.IsNullOrWhiteSpace(itemSourceModelName))
            {
                throw new Exception("You cannot use an Item Source, and a populatoItems function");
            }

            if (populateItemsOnTextChange == null &&
                string.IsNullOrWhiteSpace(itemSourceModelName))
            {
                throw new Exception(
                    "You must either use an Item Source or use a populateItems function.  Neither where set");
            }


            /*
             Make sure the ItemSelector function gets set near the very begining, and at least before setting the SelectedItem binding
              + This is so that the initial value of the selecteditem can be translated to text
             */
            if (!string.IsNullOrWhiteSpace(selectedTextModelName))
            {
                /*
                 See the pull request where this was implemented: https://github.com/AvaloniaUI/Avalonia/pull/4685
                 */
                tb.ItemSelector = (text, item) =>
                {
                    var currentValue = getDataContextValue(null, item as INotifyPropertyChanged, selectedTextModelName);

                    return currentValue.Value as string;
                };

            }

            if (populateItemsOnTextChange != null)
            {
                tb.FilterMode = AutoCompleteFilterMode.None; // this is important! it will make it show all options
                // setup the populate items
                tb.AsyncPopulator = new Func<string, CancellationToken, Task<IEnumerable<object>>>(
                    async (textboxValue, cancelToken) =>
                    {
                        IEnumerable<object> results = null;
                        try
                        {
                            var items = await populateItemsOnTextChange(textboxValue);
                            results = items.Select(i => (object) i);
                        }
                        catch (Exception ex)
                        {
                            log.Error($"[Autocomplete] Failure in populating async items.  Exception: {ex}");
                        }

                        return results;
                    });
            }
            else if (!string.IsNullOrWhiteSpace(itemSourceModelName))
            {
                // Just as a safety check, make them init the model first
                if (!(getModelValue(itemSourceModelName)?.Value is IEnumerable<T>))
                {
                    throw new Exception($"Model {nameof(itemSourceModelName)} source property specified by name [{itemSourceModelName}] must be a IEnumerable<T>");
                }

                // item source binding
                AddBinding<IEnumerable>(modelFieldName: itemSourceModelName,
                    control: tb,
                    property: Avalonia.Controls.AutoCompleteBox.ItemsProperty,
                    isTwoWayDataBinding:false);
            }

            // selected item binding
            AddBinding<object>(modelFieldName: selectedItemModelName,
                control: tb,
                property: Avalonia.Controls.AutoCompleteBox.SelectedItemProperty,
                isTwoWayDataBinding:true);

            if (populateItemRow != null)
            {
                tb.ItemTemplate = new FuncDataTemplate<object>((itemModel, nameScope) =>
                {
                    if (itemModel == null)
                    {
                        // how could itemModel be null?  Sometimes it is though, so strange
                        var tb = new Avalonia.Controls.TextBlock();
                        tb.Text = "(null)";
                        return tb;
                    }
                    
                    var rowForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
                    // this has to have a unique model
                    rowForm.DataContext = itemModel;
                    populateItemRow(rowForm);

                    rowForm.Host.DataContext = itemModel;

                    return rowForm.Host;
                });
            }



            tb.SelectionChanged += (_s, _args) =>
            {
                if (_args.AddedItems.OfType<T>().Any())
                {
                    var first = _args.AddedItems.Cast<T>().First();
                    onSelectionChanged?.Invoke(first);
                }
            };
            
            AddRowToHost(tb);
            return this;
            
        }


	}
}
