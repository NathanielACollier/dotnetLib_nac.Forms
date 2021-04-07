using System;
using Avalonia.Controls;

namespace nac.Forms
{
    public partial class Form
    {

        public class AddRowToHostFunctions
        {
            public Action show;
            public Action hide;
        }

        private void SetupAddRowToHostFunctionsIfAny(Avalonia.Controls.Control row, AddRowToHostFunctions functions = null)
        {
            if (functions != null)
            {
                functions.show = () =>
                {
                    row.IsVisible = true;
                };
                functions.hide = () =>
                {
                    row.IsVisible = false;
                };
            }
        }

        private void handleAddingControlToIndexIfRequested(Control ctrl, string ctrlIndex = null)
        {
            if(!string.IsNullOrWhiteSpace(ctrlIndex))
            {
                if(this.controlsIndex.ContainsKey(ctrlIndex))
                {
                    throw new Exception($"Control index [{ctrlIndex}] already exists");
                }

                this.controlsIndex[ctrlIndex] = ctrl;
            }
        }


        private void AddRowToHost( Control ctrl,
            AddRowToHostFunctions functions = null, string ctrlIndex=null,
            bool rowAutoHeight = true)
        {
            handleAddingControlToIndexIfRequested(ctrl, ctrlIndex);

            AddHostChild(ctrl, rowAutoHeight);
            SetupAddRowToHostFunctionsIfAny(ctrl, functions);
        }


        private void AddRowToHost(Control ctrl, string rowLabel,
            AddRowToHostFunctions functions = null, string ctrlIndex = null,
            bool rowAutoHeight = true)
        {
            DockPanel row = new DockPanel();
            TextBlock label = new TextBlock();

            handleAddingControlToIndexIfRequested(ctrl, ctrlIndex);
            
            label.Text = rowLabel;
            
            row.Children.Add(label);
            row.Children.Add(ctrl);

            DockPanel.SetDock(label, Dock.Left);
            DockPanel.SetDock(ctrl, Dock.Right);

            AddHostChild(row, rowAutoHeight);

            SetupAddRowToHostFunctionsIfAny(row, functions);
        }


        private void AddHostChild(Control ctrl, bool rowAutoHeight)
        {
            var row = new RowDefinition();
            if(rowAutoHeight)
            {
                row.Height = GridLength.Auto;
            }
            else
            {
                // see if the control set a height, and if it did use that for the row height
                if (!double.IsNaN(ctrl.Height))
                {
                    row.Height = new Avalonia.Controls.GridLength(ctrl.Height);
                }
            }
            log.Info($"New Host Child of Type: [{ctrl.GetType().Name}].  It's Row height is: [{row.Height}]");
            
            this.Host.RowDefinitions.Add(row);
            int colIndex = 0;
            int rowIndex = this.Host.RowDefinitions.Count - 1;
            Grid.SetRow(ctrl, rowIndex);
            Grid.SetColumn(ctrl, colIndex);
            this.Host.Children.Add(ctrl);
        }



    }
}