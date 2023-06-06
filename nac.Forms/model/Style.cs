using System;
using System.Collections.Generic;
using Avalonia.Media;
using nac.utilities;

namespace nac.Forms.model;

public class Style
{
    public Optional<Avalonia.Media.Color> foregroundColor { get; set; } = new Optional<Color>();
    public Optional<Avalonia.Media.Color> backgroundColor { get; set; } = new Optional<Color>();
    public Optional<int> height { get; set; } = new Optional<int>();

    public Optional<int> width { get; set; } = new Optional<int>();

    public Optional<string> isVisibleModelName { get; set; } = new Optional<string>();
    public Optional<string> isHiddenModelName { get; set; } = new Optional<string>();


    public Action<Form> contextMenu { get; set; }

    public Optional<IEnumerable<model.MenuItem>> contextMenuItems { get; set; } = new Optional<IEnumerable<MenuItem>>();

    public Action<Form> Tooltip { get; set; }
    public Optional<string> TooltipText { get; set; } = new Optional<string>();


    public static implicit operator Style(string cssText)
    {
        return lib.styleUtil.fromCSS(cssText);
    }
}