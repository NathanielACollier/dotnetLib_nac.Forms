using System.IO;
using System.Reflection;

namespace TestApp.lib;

public static class Resources
{
    public static byte[] GetImage(string resourcePath)
    {
        Assembly _assembly = Assembly.GetExecutingAssembly();

        Stream _imageStream = _assembly.GetManifestResourceStream(resourcePath);
        
        using (MemoryStream ms = new MemoryStream())
        {
            _imageStream.CopyTo(ms);
            return ms.ToArray();
        }
    }
    
    
    
}