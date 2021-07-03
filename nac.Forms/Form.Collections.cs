using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using nac.Forms.model;

namespace nac.Forms
{
    public partial class Form
    {
        public Form List<T>(string itemSourcePropertyName, Action<Form> populateItemRow, Style style=null,
                        Action<IEnumerable<T>> onSelectionChanged = null)
        {
            var itemsCtrl = new ListBox();
            lib.styleUtil.style(this, itemsCtrl, style);

            // this is documented here: https://avaloniaui.net/docs/templates/datatemplates-in-code
            itemsCtrl.ItemTemplate = new FuncDataTemplate<object>((itemModel, nameScope) =>
            {
                var rowForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
                // this has to have a unique model
                rowForm.Model[SpecialModelKeys.DataContext] = itemModel;
                populateItemRow(rowForm);

                rowForm.Host.DataContext = itemModel;

                return rowForm.Host;
            });

            if( !(this.Model[itemSourcePropertyName] is IEnumerable<T>))
            {
                throw new Exception($"Model items source property specified by name [{itemSourcePropertyName}] must be a IEnumerable<T>");
            }

            AddBinding<IEnumerable<T>>(itemSourcePropertyName, itemsCtrl, ItemsControl.ItemsProperty,
                isTwoWayDataBinding: true);

            // handle selection changed
            itemsCtrl.SelectionChanged += (_s, _args) =>
            {
                if (_args.AddedItems.OfType<T>().Any())
                {
                    // only fire this if new stuff was selected
                    onSelectionChanged?.Invoke(_args.AddedItems.Cast<T>());
                }
            };

            AddRowToHost(itemsCtrl);

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
            if( !(this.Model[itemSourceModelName] is IEnumerable<T>))
            {
                throw new Exception($"Model {nameof(itemSourceModelName)} source property specified by name [{itemSourceModelName}] must be a IEnumerable<T>");
            }

            
            // item source binding
            AddBinding<IEnumerable<T>>(modelFieldName: itemSourceModelName,
                                        control: dp,
                                        property: Avalonia.Controls.ComboBox.ItemsProperty,
                                        isTwoWayDataBinding:true);
            
            // selected item binding
            AddBinding<object>(modelFieldName: selectedItemModelName,
                                        control: dp,
                                        property: Avalonia.Controls.ComboBox.SelectedItemProperty,
                                        isTwoWayDataBinding:true);
            
            if (populateItemRow != null)
            {
                dp.ItemTemplate = new FuncDataTemplate<object>((itemModel, nameScope) =>
                {
                    var rowForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
                    // this has to have a unique model
                    rowForm.Model[SpecialModelKeys.DataContext] = itemModel;
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


	}
}
