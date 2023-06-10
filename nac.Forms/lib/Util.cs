using System;

namespace nac.Forms.lib
{
    public class Util
    {
        public static bool CanChangeType<T>(object val, out T valConvertedToT)
        {
            valConvertedToT = default(T);
            try
            {
                var result = Convert.ChangeType(val, typeof(T));
                if (result is T newVal)
                {
                    valConvertedToT = newVal;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        internal static string writeXAMLNS()
        {
            return @"
                xmlns=""https://github.com/avaloniaui""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            ";
        }
    }
}
