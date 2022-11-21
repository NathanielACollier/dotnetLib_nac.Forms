using System.Xml.Linq;
using nac.Forms;

namespace TestApp.lib.TestFunctionGroups;

public class TreeView
{
    
    private static async void Test_TreeView_ObjectViewer_Basic(Form f)
    { 
        f.Text("Basic View");

        await f.ObjectViewer(initialItemValue: new
        {
            A = "Dinosaur",
            B = "Penguin"
        });

    }
    
    
    
    private static async void ObjectViewer_XML_Basic(Form f)
    {
        f.Text("XML View");

        await f.ObjectViewer(initialItemValue: XElement.Parse(@"
            <cars>
                <car make='Chevrolet' model='Silverado'>
                    <passengers>
                       <passenger name='George' />
                    </passengers>
                </car>
                <car make='Ford' model='F150' />
                <car make='Ford' model='Bronco' />
            </cars>
        "));
    }
    
    
    
    private static async void ObjectViewer_UpdateFunction_Counter(Form f)
    {
        var objViewerOperations = new nac.Forms.Form.ObjectViewerFunctions<object>();
        var model = new
        {
            Message = "Initial Value",
            Counter = 0
        };

        f.Text("Button Counter on TreeView")
            .HorizontalGroup(hg =>
            {
                hg.Text("Model: ")
                    .Button("Incriment", async () =>
                    {
                        model = new
                        {
                            Message = "Incrimented",
                            Counter = model.Counter + 1
                        };
                        objViewerOperations.updateValue(model);
                    });
            });
        
        await f.ObjectViewer<object>(initialItemValue: model, functions: objViewerOperations);
    }
    
    
    
    
}