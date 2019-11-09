using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace dotnetCoreAvaloniaNCForms
{
    public partial class Form
    {
        public Form List(string itemSourcePropertyName, Action<Form> populateItemRow)
        {
            var horizontalGroupForm = new Form(_parentForm: this);

            horizontalGroupForm.HorizontalGroup(f => populateItemRow(f));

            var grid = (horizontalGroupForm.Host.Children[0] as DockPanel).Children[0] as Grid;

            var itemsCtrl = new ListBox();

            // this is documented here: https://avaloniaui.net/docs/templates/datatemplates-in-code
            itemsCtrl.ItemTemplate = new FuncDataTemplate<object>((itemModel) =>
            {
                //return grid;
                return new TextBlock
                {
                    Text = "Hello World!"
                };
            });

            if( !(this.Model[itemSourcePropertyName] is ObservableCollection<object>))
            {
                throw new Exception($"Model items source property specified by name [{itemSourcePropertyName}] must be a ObservableCollection<object>");
            }

            AddBinding<ObservableCollection<object>>(itemSourcePropertyName, itemsCtrl, ItemsControl.ItemsProperty,
                isTwoWayDataBinding: true);

            AddRowToHost(itemsCtrl);

            var debugTest = itemsCtrl.Items;
            var debugTemplate = itemsCtrl.ItemTemplate;

            itemsCtrl.Height = 500;
            itemsCtrl.Width = 300;

            itemsCtrl.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Aquamarine);

            return this;
        }


	}
}
