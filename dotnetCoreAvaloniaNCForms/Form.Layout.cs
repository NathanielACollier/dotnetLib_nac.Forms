using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace dotnetCoreAvaloniaNCForms
{
    public partial class Form
    {

        public Form HorizontalGroup(Action<Form> populateHorizontalGroup)
        {
            var horizontalGroupForm = new Form(_parentForm: this);

            populateHorizontalGroup(horizontalGroupForm);

            // take all the child items of host and put them in a grid with equal space between?
            Grid horiontalGroup = new Grid();
            var gridRow = new RowDefinition();
            horiontalGroup.RowDefinitions.Add(gridRow);
            int rowIndex = 0;
            int columnIndex = 0;
            foreach (var child in horizontalGroupForm.Host.Children
                                            .ToList()
                                            )
            {
                // called ToList above so we can remove this guy from the host children without breaking the foreach
                horizontalGroupForm.Host.Children.Remove(child); // we have to remove it from the host to be able to add it to the Grid
                var col = new ColumnDefinition();
                horiontalGroup.ColumnDefinitions.Add(col);

                Grid.SetRow((AvaloniaObject)child, rowIndex);
                Grid.SetColumn((AvaloniaObject)child, columnIndex);
                horiontalGroup.Children.Add(child);

                ++columnIndex; // last statement
            }


            AddRowToHost(horiontalGroup);
            return this;
        }
    }
}
