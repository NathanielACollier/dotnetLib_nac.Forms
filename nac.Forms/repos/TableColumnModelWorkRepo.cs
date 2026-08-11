using System;
using System.Collections.Generic;
using System.Linq;
using nac.Forms.model;

namespace nac.Forms.repos;

internal static class TableColumnModelWorkRepo
{
    internal static List<Column> populateColumnsListWithPropertyColumns<T>(IEnumerable<Column> columns)
    {
        var resultList = new List<Column>();
        if (columns != null)
        {
            resultList.AddRange(columns);
        }

        var targetProperties = typeof(T).GetProperties()
            .Where(prop => prop.CanRead &&
                           prop.GetIndexParameters().Length == 0
            )
            .ToList();

        foreach (var prop in targetProperties)
        {
            resultList.Add(new Column
            {
                Header = prop.Name,
                modelBindingPropertyName = prop.Name
            });
        }

        return resultList;
    }


    internal static IEnumerable<model.Column> generateColumnsForBindableDynamicDictionary(IEnumerable<Column> columns,
        IEnumerable<nac.utilities.BindableDynamicDictionary> dictList)
    {
        var dictColumns = new List<model.Column>();

        if (columns != null)
        {
            dictColumns.AddRange(columns);
        }

        var firstDict = dictList.FirstOrDefault();

        if (firstDict == null)
        {
            throw new Exception(
                "If using BindableDynamicDictionary for the type of the list items, you start with 1 item in the list, because there is no other way to figure out the type");
        }

        foreach (var key in firstDict.GetDynamicMemberNames())
        {
            /*
             4/30/2024 - Changed this to be a Template column because the DataGridTextColumn code creates a TextBlock but doesn't seem to bind to it in a way that this would work
                       + This makes things alot simpler by just controlling the binding
                       + See the DataGridTextColumn code here: https://github.com/AvaloniaUI/Avalonia/blob/334a8f7d0c947eb535f2ad7accc914e36727f334/src/Avalonia.Controls.DataGrid/DataGridTextColumn.cs#L187
                       +
             */
            dictColumns.Add(new model.Column
            {
                Header = key,
                template = f => f.TextFor(modelFieldName: key)
            });
        }

        return dictColumns;
    }
}