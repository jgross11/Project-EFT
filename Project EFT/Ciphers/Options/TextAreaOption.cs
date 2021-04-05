using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    public class TextAreaOption : Option<string>
    {
        public TextAreaOption(string fieldName, string displayName, string value) : base(fieldName, displayName, value) { }

        public TextAreaOption(string fieldName) : base(fieldName, "", "") { }

        public TextAreaOption() : base("", "", "") { }

        public override string GetHTML()
        {
            return String.Format("<textarea name = '{0}' cols = '50' rows = '4' placeholder = 'Enter plaintext here...' required>{1}</textarea>", FieldName, Value);
        }
    }
}
