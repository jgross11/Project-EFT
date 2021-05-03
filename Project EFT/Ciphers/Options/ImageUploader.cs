using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Project_EFT.Ciphers.Options
{
    /// <summary>Represents an HTML file uploader that only accepts image uploads, the contents of which are stored as the <see cref="Option{T}.Value"/> of this class.</summary>
    public class ImageUploader : Option<Bitmap>
    {
        /// <summary>Creates a file uploader with the given HTML id, display text, and image file value.</summary>
        /// <param name="fieldName">The HTML id of this image uploader.</param>
        /// <param name="displayName">The HTML name of this image uploader.</param>
        /// <param name="value"> The image file of this image uploader.</param>
        public ImageUploader(string fieldName, string displayName, Bitmap value) : base(fieldName, displayName, value) { }

        /// <summary>Generates the HTML code that will render an image uploader with these field values.</summary>
        /// <returns>An HTML snippet that will render an image uploader when interpreted as HTML code.</returns>
        public override string GetHTML()
        {
            return String.Format("{0}: <input type='file' name='{1}' id = '{1}' accept ='image/*' required>", DisplayName, FieldName);
        }

        /// <summary>Facilitates the type conversion of the <paramref name="form"/> image 
        /// from <see cref="IFormFile"/> to <see cref="Bitmap"/>, assuming only one image exists, and it is a valid png.</summary>
        /// <param name="form">The HTML form object - which may or may not contain information for this option - to attempt to pull a value from.</param>
        public override void ObtainValueFromForm(IFormCollection form)
        {
            if (form.Files.Count == 1)
            {
                IFormFile file = form.Files[0];

                // if an I/O error occurs, inform user there was an error
                // TODO does this still work?????
                try
                {
                    if (Path.GetExtension(file.FileName) == ".png")
                    {
                        // read file contents into byte array
                        Stream fileStream = file.OpenReadStream();
                        byte[] bytes = new byte[file.Length];
                        fileStream.Read(bytes, 0, (int)file.Length);
                        fileStream.Close();

                        // convert byte array to bitmap
                        MemoryStream ms = new MemoryStream(bytes);
                        Bitmap bitmap = (Bitmap)Image.FromStream(ms);
                        ms.Close();

                        // set value
                        Value = bitmap;
                    }
                    else
                    {
                        Value = null;
                    }
                }
                catch 
                {
                    Value = null;
                    ErrorMessage = "An error occurred when handling your image. Please try again.";
                }
            }
            else
            {
                Value = null;
            }
        }

    }
}
