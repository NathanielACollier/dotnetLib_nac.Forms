using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace nac.Forms;

public partial class Form
{
    public class ObjectViewerFunctions<T>
    {
        public Func<T,Task> updateValue;
    }


    private async Task ObjectViewerUpdateTreeView<T>(TreeView tv, T newItem)
    {
        try
        {
            await this.InvokeAsync(async () =>
            {
                lib.ObjectPropertiesView.TreeViewObjectPropertiesBuilder.BuildTree(tv, "ObjectView", newItem,
                    expandSettings: new lib.ObjectPropertiesView.NodeExpandSettings(expandAll: true)
                );
            });
        }
        catch (Exception ex)
        {
            log.Error($"Failed to update treeview with newItem argument.  Exception: {ex}");
        }
    }


    public async Task<Form> ObjectViewer<T>(T initialItemValue = null,
        ObjectViewerFunctions<T> functions = null)
        where T : class
    {
        var tv = new TreeView();

        if (initialItemValue != null)
        {
            await ObjectViewerUpdateTreeView(tv, initialItemValue);
        }

        if (functions != null)
        {
            functions.updateValue = async (newItem) =>
            {
                await ObjectViewerUpdateTreeView(tv, newItem);
            };
        }

        AddRowToHost(tv, rowAutoHeight: false);
        return this;
    }
}