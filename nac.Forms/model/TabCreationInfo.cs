using System;

namespace nac.Forms.model
{
    public class TabCreationInfo
    {
        public string Header { get; set; }
        public Action<Form> Populate { get; set; }
        
        public Action<Form> PopulateHeader { get; set; }
    }
}