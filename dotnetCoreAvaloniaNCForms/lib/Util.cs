using System;
using System.Collections.Generic;
using System.Text;

namespace dotnetCoreAvaloniaNCForms.lib
{
    public class Util
    {
        public static bool CanChangeType<T>(object val, out T valConvertedToT)
        {
            valConvertedToT = default(T);
            try
            {
                var result = Convert.ChangeType(val, typeof(T));
                if( result is T newVal)
                {
                    valConvertedToT = newVal;
                    return true;
                }
            }catch(Exception ex)
            {

            }
            return false;
        }
    }
}
