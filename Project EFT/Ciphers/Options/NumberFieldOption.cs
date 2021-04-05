using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    public class NumberFieldOption : Option<int>
    {
        public NumberFieldOption(string fieldName, string displayName, int value) : base(fieldName, displayName, value){}
        
        public override string GetHTML()
        {
            return String.Format("{0}: <input type='number' name='{1}' id = '{1}' value='{2}' required>", DisplayName, FieldName, Value);
        }
    }
}
