using nac.Forms;
using nac.Forms.model;

namespace TestApp.lib.TestFunctionGroups;

public class Image
{
    
    
    private static void Empty(Form f)
    {
        f.Model["img"] = null;

        f.Text("Display no image")
            .Image("img");
    }
    
    
    
    private static void EmbdedResource(Form f)
    {
        f.Model["playIcon"] = lib.Resources.GetImage("TestApp.resources.playIcon.png");

        f.Text("Embeded Resource Test")
            .Image("playIcon");
    }
    
    
    private static void FromWebURL(Form f)
    {
        // default the URL
        f.Model["url"] = "https://preview.redd.it/p9o46zrc21m81.jpg?auto=webp&s=f2f9c88a8e91c836f2b6377d5c900b416f02d62f";
        f.Text("Image from URL")
            .HorizontalGroup(hg =>
            {
                hg.Text("URL: ")
                    .TextBoxFor("url",
                        style: new Style
                        {
                            width = 350
                        })
                    .Button("Go", async () =>
                    {
                        string url = f.Model["url"] as string;
                        // download it and convert it to an avalonia Bitmap
                        using (var client = new System.Net.WebClient())
                        {
                            f.Model["img"] = client.DownloadData(url);
                        }
                    })
                    .Button("Clear", async () =>
                    {
                        f.Model["img"] = null;
                    });
            })
            .Image(modelFieldName: "img");
    }
    
    
}