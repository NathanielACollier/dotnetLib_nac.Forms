using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class TextBlockTests
{
    
    [TestMethod]
    public void Style_BackgroundAndForegroundFromHexColors()
    {
        var f = nac.Forms.Form
            .NewForm();

        f.Text("THis is a sample text.   Should be orange background and white foreground",
                style: "background-color: #FFBA00; color: #dde9e7; font-size:14pt; font-weight:bold;")
            .Text("Normal Text formatting", style: "font-size:14pt;")
            .Display();
    }
}