using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    /// <summary>Represents an HTML textbox element, the contents of which are stored as the <see cref="Option{T}.Value"/> of this class.</summary>
    public class TextBoxOption : Option<string>
    {
        /// <summary>Creates a text box with the given HTML id, name, and contents text.</summary>
        /// <param name="fieldName">The HTML id of this text box.</param>
        /// <param name="displayName">The HTML name of this text box.</param>
        /// <param name="value"> The contents text of this text box.</param>
        public TextBoxOption(string fieldName, string displayName, string value) : base(fieldName, displayName, value) { }

        /// <summary>Generates HTML code to display a textbox element with the information contained by an instance of this class.</summary>
        /// <returns>An HTML snippet that renders a textbox element when interpreted as HTML code.</returns>
        public override string GetHTML()
        {
            return String.Format("{0}: <input type='text' name='{1}' value='{2}' />", DisplayName, FieldName, Value);
        }
    }
}
