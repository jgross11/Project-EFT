using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Project_EFT.Ciphers.Options
{
    public abstract class Option
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public string ErrorMessage { get; set; }

        public Option(string fieldName, string displayName) 
        {
            FieldName = fieldName;
            DisplayName = displayName;
            ErrorMessage = null;
        }

        public Option(string fieldName, string displayName, string errorMessage)
        {
            FieldName = fieldName;
            DisplayName = displayName;
            ErrorMessage = errorMessage;
        }

        public abstract string GetHTML();
        public virtual string GetErrorHTML()
        {
            return String.Format("<div class = 'error' id='{0}-error'>{1}</div>", FieldName, ErrorMessage);
        }

        // https://bit.ly/3u2ymVn
        // lines 63 and 64
        public abstract void SetValue(string newVal);
        public abstract object GetValue();
        public virtual void ObtainValueFromForm(IFormCollection form)
        {
            if (FieldName != null)
            {
                SetValue(form[FieldName].ToString());
            }
        }
    }

    public abstract class Option<T> : Option
    {
        public T Value;

        public Option(string fieldName, string displayName, T value) : base(fieldName, displayName)
        {
            Value = value;
        }

        public Option(string fieldName, string displayName, string errorMessage, T value) : base(fieldName, displayName, errorMessage)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(string newVal)
        {
            // https://bit.ly/3u2ymVn
            // lines 63 and 64

            // string
            try
            {
                Value = (T)((object)newVal);
            }
            catch 
            {
                // int
                try
                {
                    Value = (T)(object)int.Parse(newVal);
                }
                catch 
                {
                    Value = default;
                }
            }
        }
    }
}
