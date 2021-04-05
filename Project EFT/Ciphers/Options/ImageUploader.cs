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
    public class ImageUploader : Option<Bitmap>
    {
        public ImageUploader(string fieldName, string displayName, Bitmap value) : base(fieldName, displayName, value) { }

        public override string GetHTML()
        {
            return String.Format("{0}: <input type='file' name='{1}' id = '{1}' accept ='image/*' required>", DisplayName, FieldName);
        }

        public override void ObtainValueFromForm(IFormCollection form)
        {
            if (form.Files.Count == 1)
            {
                IFormFile file = form.Files[0];
                if (Path.GetExtension(file.FileName) == ".png")
                {
                    Stream fileStream = file.OpenReadStream();
                    byte[] bytes = new byte[file.Length];
                    fileStream.Read(bytes, 0, (int)file.Length);
                    fileStream.Close();
                    MemoryStream ms = new MemoryStream(bytes);
                    Bitmap bitmap = (Bitmap)Bitmap.FromStream(ms);
                    ms.Close();
                    Value = bitmap;
                }
                else 
                {
                    Value = null;
                }
            }
            else
            {
                Value = null;
            }
        }

    }
}
