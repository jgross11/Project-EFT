using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    public abstract class Option
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }

        public Option(string fieldName, string displayName) 
        {
            FieldName = fieldName;
            DisplayName = displayName;
        }

        public abstract string GetHTML();

        // https://bit.ly/3u2ymVn
        // lines 63 and 64
        public abstract void SetValue(string newVal);
        public abstract object GetValue();
    }

    public abstract class Option<T> : Option
    {
        public T Value;

        public Option(string fieldName, string displayName, T value) : base(fieldName, displayName)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(string newVal)
        {
            // https://bit.ly/3u2ymVn
            // lines 63 and 64

            // string
            try
            {
                Value = (T)((object)newVal);
            }
            catch 
            {
                // int
                try
                {
                    Value = (T)(object)int.Parse(newVal);
                }
                catch 
                {
                    Value = default;
                }
            }
        }
    }
}
