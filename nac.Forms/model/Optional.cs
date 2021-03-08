using System;

namespace nac.Forms.model
{
    public class Optional<T>
    {
        private T internalValue;
        private bool internalIsSet;

        public bool IsSet
        {
            get { return internalIsSet; }
        }

        public void Set(T _value)
        {
            this.internalValue = _value;
            this.internalIsSet = true;
        }

        public void Clear()
        {
            this.internalIsSet = false;
        }

        public T Value
        {
            get
            {
                if (internalIsSet)
                {
                    return this.internalValue;
                }

                throw new Exception("Value is not set");
            }
        }

        
        /*
         implicit casting of non optional to optional
         + see: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/user-defined-conversion-operators
         
         implicit makes it so that we can do like this, with no casting
         var t = new Optional<Avalonia.Media.Color>();
         t = Avalonia.Media.Colors.Red;
         
         if we had done an explicit operator then you would have to do
         t = (Optional<Avalonia.Media.Color>)Avalonia.Media.Colors.Red;
         
         The implicit lets us automate how to convert between those types.  It's key to makeing the Optional type work seemlessly
         */
        public static implicit operator Optional<T>(T val)
        {
            var opt = new Optional<T>();
            opt.Set(val);
            return opt;
        }


        public Optional()
        {
            this.internalIsSet = false;
        }



    }// end of class
}