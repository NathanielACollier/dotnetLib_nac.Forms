using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using nac.Forms.model;

namespace nac.Forms
{
    public partial class Form
    {
        public Form List(string itemSourcePropertyName, Action<Form> populateItemRow, Style style=null)
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

            if( !(this.Model[itemSourcePropertyName] is ObservableCollection<object>))
            {
                throw new Exception($"Model items source property specified by name [{itemSourcePropertyName}] must be a ObservableCollection<object>");
            }

            AddBinding<ObservableCollection<object>>(itemSourcePropertyName, itemsCtrl, ItemsControl.ItemsProperty,
                isTwoWayDataBinding: true);

            // handle selection changed
            itemsCtrl.SelectionChanged += (_s, _args) =>
            {

            };

            AddRowToHost(itemsCtrl);

            var debugTest = itemsCtrl.Items;
            var debugTemplate = itemsCtrl.ItemTemplate;

            return this;
        }


        public Form DropDown(string itemSourceModelName, 
                        string selectedItemModelName, 
                        Action onSelectionChanged=null,
                        Action<Form> populateItemRow = null,
                        model.Style style = null)
        {
            var dp = new Avalonia.Controls.ComboBox();
            lib.styleUtil.style(this,dp,style);

            
            // item source binding
            AddBinding<ObservableCollection<object>>(modelFieldName: itemSourceModelName,
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
                onSelectionChanged();
            };
            
            AddRowToHost(dp);
            return this;
        }


	}
}
