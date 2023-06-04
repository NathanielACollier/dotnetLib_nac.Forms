using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace nac.Forms.model;

public class Style
{
    public model.Optional<Avalonia.Media.Color> foregroundColor { get; set; } = new Optional<Color>();
    public model.Optional<Avalonia.Media.Color> backgroundColor { get; set; } = new Optional<Color>();
    public model.Optional<int> height { get; set; } = new Optional<int>();

    public model.Optional<int> width { get; set; } = new Optional<int>();

    public model.Optional<string> isVisibleModelName { get; set; } = new Optional<string>();
    public model.Optional<string> isHiddenModelName { get; set; } = new Optional<string>();


    public Action<Form> contextMenu { get; set; }

    public model.Optional<IEnumerable<model.MenuItem>> contextMenuItems { get; set; } = new Optional<IEnumerable<MenuItem>>();

    public Action<Form> Tooltip { get; set; }
    public model.Optional<string> TooltipText { get; set; } = new Optional<string>();


    public static implicit operator Style(string cssText)
    {
        return fromCSS(cssText);
    }

    public static Style fromCSS(string cssText)
    {
        
    }
}