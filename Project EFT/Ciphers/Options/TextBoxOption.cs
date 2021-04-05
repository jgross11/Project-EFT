using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    public class TextBoxOption : Option<string>
    {
        public TextBoxOption(string fieldName, string displayName, string value) : base(fieldName, displayName, value) { }

        public override string GetHTML()
        {
            // <input type="text" name="alphabet" value="@activeSystem.Alphabet" />
            return String.Format("{0}: <input type='text' name='{1}' value='{2}' required />", DisplayName, FieldName, Value);
        }
    }
}
