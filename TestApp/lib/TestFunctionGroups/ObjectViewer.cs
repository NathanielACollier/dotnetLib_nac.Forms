using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestApp.lib.TestFunctionGroups;

public class ObjectViewer
{
    private static async Task DictionaryWithSubListTree(nac.Forms.Form f)
    {
        var dict = nac.utilities.List.CreateDictionaryFromEnumerable(new[]
        {
            new
            {
                Prop1 = 3,
                Prop2 = 1
            },
            new
            {
                Prop1 = 3,
                Prop2 = 6
            }
        });

        f = await f.ObjectViewer(dict);

        f.Button("Increment", async () =>
        {
            int counter = (int)dict[0]["Prop1"];
            dict[0]["Prop1"] = counter + 1;
        });
    }
    
    
    
    
    
    
    
    
}