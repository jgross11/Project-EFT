using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    public class ImageDisplayer : Option<string>
    {
        public ImageDisplayer() : base(null, null, null) { }

        public override string GetHTML()
        {
            return Value != null ? String.Format("<img src='{0}' width = 100px height = 100px alt='Encrypted image'>", Value) : null;
        }
    }
}
