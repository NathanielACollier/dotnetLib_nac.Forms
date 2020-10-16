using System;

namespace dotnetCoreAvaloniaNCForms.model
{
    public class MenuItem
    {
        public string Header { get; set; }
        public Action Action {get; set; }
        public MenuItem[] Items { get; set; }
    }

}