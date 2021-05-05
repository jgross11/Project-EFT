using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Project_EFT.Ciphers.Options
{
    /// <summary>Represents a list of HTML radio buttons, each of which are tied to a specific <see cref="Option"/>, 
    /// whose <see cref="Option{T}.Value"/> is the index of the selected option. The selected option will be used in computations; the other options are mostly ignored.</summary>
    public class RadioOptionsSet : Option<int>
    {
        /// <summary>The list of <see cref="Option"/>s to choose from in this set.</summary>
        public List<Option> Choices;

        /// <summary>Creates a radio button set with an empty list of options with the given HTML id, set name, and selected index.</summary>
        /// <param name="fieldName">The HTML id of this radio button set.</param>
        /// <param name="displayName">The HTML name of this radio button set.</param>
        /// <param name="value"> The selected index of this radio button set.</param>
        public RadioOptionsSet(string fieldName, string displayName, int value) : base(fieldName, displayName, value) 
        {
            Choices = new List<Option>();
        }

        /// <summary>Creates a radio button set with the given HTML id, set name, selected index, and options list.</summary>
        /// <param name="fieldName">The HTML id of this radio button set.</param>
        /// <param name="displayName">The HTML name of this radio button set.</param>
        /// <param name="value">The selected index of this radio button set.</param>
        /// <param name="choices">The list of <see cref="Option"/>s this radio button set includes.</param>
        public RadioOptionsSet(string fieldName, string displayName, int value, List<Option> choices) : base(fieldName, displayName, value)
        {
            Choices = choices;
        }

        /// <summary>Creates an empty radio button set with no id, selected index, or name, and an empty options list.</summary>
        public RadioOptionsSet() : base(null, null, 0)
        {
            Choices = new List<Option>();
        }

        /// <summary>Generates HTML code for each <see cref="Option"/> in this radio button set's list.</summary>
        /// <returns>An HTML snippet that renders a radio button set with the given options rendered as their own HTML snippets.</returns>
        public override string GetHTML()
        {
            string result = "";
            for(int i = 0; i < Choices.Count; i++) 
            {
                Option opt = Choices[i];
                result += String.Format(i == Value ?
                    "<input type='radio' id='{0}-choice' name='{1}' value='{2}' checked> " :
                    "<input type='radio' id='{0}' name='{1}' value='{2}'> ",
                    opt.FieldName, FieldName, i) + opt.GetHTML() + "<br>";
            }
            return result;
        }

        /// <summary>Obtains the index of the selected option in the set, and also sets the input value for each <see cref="Option"/> in the set.</summary>
        /// <param name="form">The HTML form object - which may or may not contain information for this option - to attempt to pull a value from.</param>
        public override void ObtainValueFromForm(IFormCollection form)
        {
            foreach (Option opt in Choices) 
            {
                opt.ObtainValueFromForm(form);
            }
            SetValue(form[FieldName].ToString());
        }
    }
}