using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace dotnetCoreAvaloniaNCForms
{
    public partial class Form
    {
        public Form List(string itemSourcePropertyName, Action<Form> populateItemRow)
        {
            var horizontalGroupForm = new Form(_parentForm: this);

            horizontalGroupForm.HorizontalGroup(f => populateItemRow(f));

            var grid = (horizontalGroupForm.Host.Children[0] as DockPanel).Children[0] as Grid;

            var itemsCtrl = new ItemsControl();

            itemsCtrl.ItemContainerGenerator.Materialized += (_s, _args) =>
            {
                
            };

            AddBinding<ObservableCollection<object>>(itemSourcePropertyName, itemsCtrl, ItemsControl.ItemsProperty,
                isTwoWayDataBinding: true);


            // list should be scrollable and ItemsControl doesn't have built in scrollviewer
            var listScroller = new ScrollViewer();
            listScroller.Content = itemsCtrl;

            AddRowToHost(listScroller);

            return this;
        }


	}
}
