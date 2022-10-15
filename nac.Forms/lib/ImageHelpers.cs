namespace nac.Forms.lib;

public class ImageHelpers
{
    public static byte[] CreateEmptyBitmapImage()
    {
        /*
         Created an empty bitmap with mspaint, then extracted out the bytes from it by reading the file like this
            var data = System.IO.File.ReadAllBytes(@"C:\Users\nathaniel\Desktop\Untitled.bmp");
            string.Join(',', data.Select(b=> b.ToString())).Dump();
         */
                                                        
        var emptyBitmapData = new byte[]
        {
            66, 77, 66, 0, 0, 0, 0, 0, 0, 0, 62, 0, 0, 0, 40, 0, 0, 0,
            1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 4, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            255, 255, 255, 0, 128, 0, 0, 0
        };

        return emptyBitmapData;
    }
    
    
    
}