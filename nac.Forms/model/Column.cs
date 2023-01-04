using System;

namespace nac.Forms.model;

public class Column
{
    public string Header { get; set; }
    public string modelBindingPropertyName { get; set; }
    public Action<Form> template { get; set; }
}