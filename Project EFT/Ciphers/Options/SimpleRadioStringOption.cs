using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    /// <summary>Represents a simple, text-only radio button option, the value of which is contained in the <see cref="Option.FieldName"/> of this class.</summary>
    public class SimpleRadioStringOption : Option
    {
        /// <summary>Creates a text-only option that displays the given value.</summary>
        /// <param name="value">The text snippet to display.</param>
        public SimpleRadioStringOption(string value) : base(value, null){}

        /// <summary>Returns the value to display as plain text in an HTML file.</summary>
        /// <returns>The string to display as HTML plain text.</returns>
        public override string GetHTML()
        {
            return FieldName;
        }

        public override object GetValue()
        {
            return FieldName;
        }

        public override void SetValue(string newVal)
        {
            FieldName = newVal;
        }
    }
}
