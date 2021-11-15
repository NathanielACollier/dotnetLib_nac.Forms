using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;

namespace nac.Forms
{
    public partial class Form
    {
        
        public Form VerticalDock(Action<Form> populateVerticalGroup,
            model.Style style = null)
        {
            var vertGroupForm = new Form(_parentForm: this);

            populateVerticalGroup(vertGroupForm);

            var vertGroup = new DockPanel();
            
            lib.styleUtil.style(this,vertGroup, style);
            
            var childControls = vertGroupForm.Host.Children.ToList();
            foreach (var child in childControls)
            {
                vertGroupForm.Host.Children.Remove(child); // get the child out of the form so we can move it to the grid
                DockPanel.SetDock((global::Avalonia.Controls.Control)child, Dock.Top);
                vertGroup.Children.Add(child);
            }

            AddRowToHost(vertGroup, rowAutoHeight: false);
            return this;
        }
        
        
        public Form VerticalStack(Action<Form> populateVerticalGroup,
            model.Style style = null)
        {
            var vertGroupForm = new Form(_parentForm: this);

            populateVerticalGroup(vertGroupForm);

            var vertGroup = new StackPanel();
            
            lib.styleUtil.style(this,vertGroup, style);
            
            var childControls = vertGroupForm.Host.Children.ToList();
            foreach (var child in childControls)
            {
                vertGroupForm.Host.Children.Remove(child); // get the child out of the form so we can move it to the grid
                vertGroup.Children.Add(child);
            }

            AddRowToHost(vertGroup, rowAutoHeight: false);
            return this;
        }



        /*
         For grid info see these two links
         https://www.c-sharpcorner.com/Resources/676/how-to-create-a-grid-in-wpf-dynamically.aspx
         https://www.wpf-tutorial.com/panels/gridsplitter/
        */
        public Form VerticalGroup(Action<Form> populateVerticalGroup,
                    bool isSplit = false,
                    model.Style style = null)
        {
            var vertGroupForm = new Form(_parentForm: this);

            populateVerticalGroup(vertGroupForm);

            Grid vertGroup = new Grid();
            lib.styleUtil.style(this,vertGroup, style);

            var gridCol = new ColumnDefinition();
            vertGroup.ColumnDefinitions.Add(gridCol);
            int rowIndex = 0;
            int columnIndex = 0;
            var childControls = vertGroupForm.Host.Children.OfType<Control>().ToList();
            foreach( var child in childControls)
            {
                vertGroupForm.Host.Children.Remove(child); // get the child out of the form so we can move it to the grid

                // 1 row for the child control
                var row = new RowDefinition();
                vertGroup.RowDefinitions.Add(row);

                // set child into the grid
                Grid.SetRow(child, rowIndex);
                Grid.SetColumn(child, columnIndex);

                // if child height is set, then set the row to start that height (If isSplit=true, then you'll be able to resize that row)
                if (!double.IsNaN(child.Height))
                {
                    // size the row to child height
                    row.Height = new Avalonia.Controls.GridLength(child.Height);
                }
                
                vertGroup.Children.Add(child);

                // 1 row for grid splitter (If not last child)
                if( isSplit && child != childControls.Last())
                {
                    ++rowIndex; // we are on the next row because we are going to add a row definition
                    int splitterHeight = 5;
                    var gridSplitRow = new RowDefinition();
                    gridSplitRow.Height = new GridLength(splitterHeight);
                    vertGroup.RowDefinitions.Add(gridSplitRow);

                    var gridSplitter = new GridSplitter();
                    gridSplitter.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
                    gridSplitter.Height = splitterHeight;
                    Grid.SetRow(gridSplitter, rowIndex);
                    Grid.SetColumn(gridSplitter, columnIndex);
                    vertGroup.Children.Add(gridSplitter);
                }

                ++rowIndex; // make sure this is last statement
            }

            AddRowToHost(vertGroup, rowAutoHeight: false);
            return this;
        }


        public Form HorizontalStack(Action<Form> populateHorizontalGroup,
            model.Style style = null)
        {
            var horizontalForm = new Form(_parentForm: this);

            populateHorizontalGroup(horizontalForm);

            var horizontalPanel = new StackPanel();
            horizontalPanel.Orientation = Orientation.Horizontal;
            
            lib.styleUtil.style(this,horizontalPanel, style);
            
            var childControls = horizontalForm.Host.Children.ToList();
            foreach (var child in childControls)
            {
                horizontalForm.Host.Children.Remove(child); // get the child out of the form so we can move it to the grid
                horizontalPanel.Children.Add(child);
            }

            AddRowToHost(horizontalPanel, rowAutoHeight: false);
            return this;
        }
        
        
        public Form HorizontalGroup(Action<Form> populateHorizontalGroup,
            bool isSplit = false,
            model.Style style = null)
        {
            var horizontalGroupForm = new Form(_parentForm: this);

            populateHorizontalGroup(horizontalGroupForm);

            // take all the child items of host and put them in a grid with equal space between?
            Grid horiontalGroup = new Grid();
            lib.styleUtil.style(this,horiontalGroup, style);
            
            var gridRow = new RowDefinition();
            horiontalGroup.RowDefinitions.Add(gridRow);
            int rowIndex = 0;
            int columnIndex = 0;
            var childControls = horizontalGroupForm.Host.Children
                                            .OfType<Control>().ToList();
            foreach (var child in childControls)
            {
                // called ToList above so we can remove this guy from the host children without breaking the foreach
                horizontalGroupForm.Host.Children.Remove(child); // we have to remove it from the host to be able to add it to the Grid
                var col = new ColumnDefinition();
                horiontalGroup.ColumnDefinitions.Add(col);

                Grid.SetRow(child, rowIndex);
                Grid.SetColumn(child, columnIndex);
                
                // if child width is set, then start the column at that width
                if (!double.IsNaN(child.Width))
                {
                    col.Width = new Avalonia.Controls.GridLength(child.Width);
                }
                
                horiontalGroup.Children.Add(child);

                // 1 column for grid splitter (If not last child)
                if (isSplit && child != childControls.Last())
                {
                    ++columnIndex; // incriment because we are going to add a new column below
                    int splitterWidth = 5;
                    var gridSplitColumn = new ColumnDefinition();
                    gridSplitColumn.Width = new GridLength(splitterWidth);
                    horiontalGroup.ColumnDefinitions.Add(gridSplitColumn);

                    var gridSplitter = new GridSplitter();
                    gridSplitter.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
                    gridSplitter.Width = splitterWidth;
                    Grid.SetRow(gridSplitter, rowIndex);
                    Grid.SetColumn(gridSplitter, columnIndex);
                    horiontalGroup.Children.Add(gridSplitter);
                }

                ++columnIndex; // last statement
            }

            AddRowToHost(horiontalGroup);
            return this;
        }




        public Form Panel<T>(string modelFieldName, Action<Form> populatePanel)
        {
            /*
             Here is documentation on how ContentControl works:
             https://docs.avaloniaui.net/docs/controls/contentcontrol
             */
            var panelControl = new Avalonia.Controls.ContentControl();

            if (!(getModelValue(modelFieldName) is T))
            {
                throw new Exception(
                    $"Model {nameof(modelFieldName)} source property specified by name [{modelFieldName}] is not of type T: {typeof(T).Name}");
            }
            
            /*
             One Way binding, because the ContentControl cannot set the model back, it just displays the model
             */
            AddBinding<object>(modelFieldName: modelFieldName,
                        control: panelControl,
                        property: Avalonia.Controls.ContentControl.ContentProperty,
                        isTwoWayDataBinding: false);

            panelControl.ContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<T>((itemModel, nameScope) =>
            {
                // if the model is null then just display nothing
                if (itemModel == null)
                {
                    // how could itemModel be null?  Sometimes it is though, so strange
                    var tb = new Avalonia.Controls.TextBlock();
                    tb.Text = "(null)";
                    return tb;
                }

                var contentForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
                contentForm.DataContext = itemModel;
                populatePanel(contentForm);

                contentForm.Host.DataContext = itemModel;
                return contentForm.Host;
            });
            
            AddRowToHost(panelControl);
            return this;
        }

    }
}
