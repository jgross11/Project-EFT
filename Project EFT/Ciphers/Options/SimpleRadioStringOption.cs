using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    public class SimpleRadioStringOption : Option
    {
        public SimpleRadioStringOption(string fieldName) : base(fieldName, null){}

        public override string GetHTML()
        {
            return FieldName;
        }

        public override object GetValue()
        {
            return null;
        }

        public override void SetValue(string newVal)
        {
            
        }
    }
}
