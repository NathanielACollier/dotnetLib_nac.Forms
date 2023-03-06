﻿using System;
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
            
            if( !(getModelValue(itemSourcePropertyName) is IEnumerable<T>))
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
            if( !(getModelValue(itemSourceModelName) is IEnumerable<T>))
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



        public Form Autocomplete<T>(string itemSourceModelName,
            string selectedItemModelName,
            Action<T> onSelectionChanged = null,
            Action<Form> populateItemRow = null,
            model.Style style = null)
        {
            var tb = new Avalonia.Controls.AutoCompleteBox();
            lib.styleUtil.style(this, tb, style);
            
            // Just as a safety check, make them init the model first
            if( !(getModelValue(itemSourceModelName) is IEnumerable<T>))
            {
                throw new Exception($"Model {nameof(itemSourceModelName)} source property specified by name [{itemSourceModelName}] must be a IEnumerable<T>");
            }
            
            // item source binding
            AddBinding<IEnumerable>(modelFieldName: itemSourceModelName,
                control: tb,
                property: Avalonia.Controls.AutoCompleteBox.ItemsProperty,
                isTwoWayDataBinding:false);
            
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
