using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Ciphers.Options
{
    /// <summary> Represents an HTML img tag, whose src is the image as a Base64 string, which is saved as the <see cref="Option{T}.Value"/> of this class. </summary>
    public class ImageDisplayer : Option<string>
    {
        /// <summary>Creates an img tag with no src.</summary>
        public ImageDisplayer() : base(null, null, null) { }

        /// <summary>Generates the HTML code that will render this <see cref="Option{T}.Value"/> as an image.</summary>
        /// <returns>An HTML snippet that will render an image when interpreted as HTML code.</returns>
        public override string GetHTML()
        {
            return Value != null ? String.Format("<img src='{0}' width = 100px height = 100px alt='Encrypted image'>", Value) : null;
        }
    }
}
