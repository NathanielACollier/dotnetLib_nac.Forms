using System;
using System.Collections.Generic;
using System.Text;

namespace nac.Forms.lib.Extensions;

public static class GeneralExtensions
{

    public static void Set<T>(this Avalonia.Controls.ItemCollection items, IEnumerable<T> listItems)
    {
        items.Clear();
        foreach (var item in listItems)
        {
            items.Add(item);
        }
    }

}
