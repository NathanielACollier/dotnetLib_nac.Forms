namespace nac.Forms.lib.ObjectPropertiesView;

public class NodeExpandSettings
{
    public bool ExpandCollections { get; set; }
    public bool ExpandPropertyName { get; set; }

    public bool ExpandPrimativeValues { get; set; }
    public bool ExpandNonPrimativeValues { get; set; }

    public bool ExpandXML { get; set; }

    public bool ExpandNullItem { get; set; }

    public bool ExpandRoot { get; set; }


    public NodeExpandSettings() : this(true)
    {
    }

    public NodeExpandSettings(bool expandAll)
    {
        this.ExpandCollections = expandAll;
        this.ExpandPropertyName = expandAll;
        this.ExpandPrimativeValues = expandAll;
        this.ExpandNonPrimativeValues = expandAll;
        this.ExpandXML = expandAll;
        this.ExpandNullItem = expandAll;
        this.ExpandRoot = expandAll;
    }
}