using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Project_EFT.Ciphers.Options
{
    public class RadioOptionsSet : Option<int>
    {
        public List<Option> Choices;

        public RadioOptionsSet(string fieldName, string displayName, int value) : base(fieldName, displayName, value) 
        {
            Choices = new List<Option>();
        }

        public RadioOptionsSet(string fieldName, string displayName, int value, List<Option> choices) : base(fieldName, displayName, value)
        {
            Choices = choices;
        }

        public RadioOptionsSet() : base(null, null, 0){ }

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