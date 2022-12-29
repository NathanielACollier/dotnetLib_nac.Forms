using System.Collections.Generic;
using System.Linq;

namespace nac.Forms.lib;

public static class AvaloniaModelHelpers
{
    
    
    public static global::Avalonia.Controls.MenuItem convertModelToAvaloniaMenuItem(model.MenuItem item)
    {
        var avaloniaItem = new global::Avalonia.Controls.MenuItem
        {
            Header = item.Header
        };

        if (item.Action != null)
        {
            avaloniaItem.Click += (_s, _args) =>
            {
                item.Action();
            };
        }

            
        if (item.Items?.Any() == true)
        {
            var subMenuItems = new List<global::Avalonia.Controls.MenuItem>();
            foreach (var i in item.Items)
            {
                subMenuItems.Add(
                    convertModelToAvaloniaMenuItem(i)    
                );
            }

            avaloniaItem.Items = subMenuItems;
        }

        return avaloniaItem;
    }
    
    
    
}