using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    /// <summary>Represents an HTML textarea element, the contents of which are stored as the <see cref="Option{T}.Value"/> of this class. 
    /// Also contains a field representing the textarea's placeholder text.</summary>
    public class TextAreaOption : Option<string>
    {
        /// <summary>The HTML placeholder text for the generated textarea.</summary>
        public string PlaceholderText;

        /// <summary>Creates a textarea with the given HTML id, name, contents text, and placeholder text.</summary>
        /// <param name="fieldName">The HTML id of this textarea.</param>
        /// <param name="displayName">The HTML name of this textarea.</param>
        /// <param name="value"> The contents text of this textarea.</param>
        /// <param name="placeholderText"> The placeholder text for this textarea.</param>
        public TextAreaOption(string fieldName, string displayName, string value, string placeholderText) : base(fieldName, displayName, value) 
        {
            PlaceholderText = placeholderText;
        }

        /// <summary>Creates a textarea with the given HTML id, name, and contents text.</summary>
        /// <param name="fieldName">The HTML id of this textarea.</param>
        /// <param name="displayName">The HTML name of this textarea.</param>
        /// <param name="value"> The contents text of this textarea.</param>
        public TextAreaOption(string fieldName, string displayName, string value) : base(fieldName, displayName, value) { }

        /// <summary>Creates a textarea with the given HTML id.</summary>
        /// <param name="fieldName">The HTML id of this textarea.</param>
        public TextAreaOption(string fieldName) : base(fieldName, "", "") { }

        /// <summary>Creates an empty textarea.</summary>
        public TextAreaOption() : base("", "", "") { }

        /// <summary>Generates HTML code to display a textarea element with the information contained by an instance of this class.</summary>
        /// <returns>An HTML snippet that renders a textarea element when interpreted as HTML code.</returns>
        public override string GetHTML()
        {
            return String.Format("<textarea name = '{0}' cols = '50' rows = '4' placeholder = '{1}' required>{2}</textarea>", FieldName, PlaceholderText, Value);
        }
    }
}
