using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    /// <summary>Represents an HTML number input option, whose value is the <see cref="Option{T}.Value"/> of this class.</summary>
    public class NumberFieldOption : Option<int>
    {
        /// <summary>Creates an HTML number input with the given HTML id, display text, and numerical value.</summary>
        /// <param name="fieldName">The HTML id of this number input.</param>
        /// <param name="displayName">The HTML name of this number input.</param>
        /// <param name="value"> The numerical value of this number input.</param>
        public NumberFieldOption(string fieldName, string displayName, int value) : base(fieldName, displayName, value){}
        
        /// <summary>Generates the HTML code that will render a number input tag.</summary>
        /// <returns>An HTML snippet that renders a number input tag when interpreted as HTML code.</returns>
        public override string GetHTML()
        {
            return String.Format("{0}: <input type='number' name='{1}' id = '{1}' value='{2}' required>", DisplayName, FieldName, Value);
        }
    }
}
