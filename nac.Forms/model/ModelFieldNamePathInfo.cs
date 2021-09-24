using System;
using System.Collections.Generic;
using System.Linq;

namespace nac.Forms.model
{
    public class ModelFieldNamePathInfo
    {
        private static lib.Log log = new lib.Log();
        
        public string Current { get; set; }
        public string ChildPath { get; set; }

        public ModelFieldNamePathInfo(string modelFieldName)
        {
            var fields = processModelFieldNameForPathInfo(modelFieldName);
            this.Current = fields[0];  // there will allways be a first field, because we don't call this if it's empty
            this.ChildPath = "";
            if (fields.Count > 1)
            {
                this.ChildPath = string.Join(".", fields.Skip(1)); // join whatever is left together
                log.Info($"Processing [modelFieldName: {modelFieldName}] found field path.  [Current Path Val: {this.Current}] [Next Path: {this.ChildPath}]");
            }
        }
        
        private static List<string> processModelFieldNameForPathInfo(string modelFieldName)
        {
            if (string.IsNullOrWhiteSpace(modelFieldName))
            {
                throw new Exception("Model field name was null or whitespace");
            }
            
            // could specify a path
            var fields = modelFieldName.Split('.')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .ToList();

            return fields;
        }
    }
}