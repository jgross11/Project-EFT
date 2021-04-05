using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    public class VisualMASC : Cipher
    {
        public override string Name { get { return "Visual Monoalphabetic Substitution Cipher (VMASC)"; } }
        private const int ImageDisplayerIndex = 1;
        private const int ImageUploaderIndex = 0;

        public VisualMASC(int numSols) : base(numSols)
        {

        }

        public VisualMASC() : base() { }

        public override void GenerateForms()
        {
            EncryptionFormOptions.RemoveAt(1);
            DecryptionFormOptions.RemoveAt(0);
            DecryptionFormOptions.RemoveAt(0);
            EncryptionFormOptions.Add(new ImageDisplayer());
            DecryptionFormOptions.Add(new ImageUploader("encryptedImage", "Decrypt this image", null));
        }

        public override void Encrypt()
        {
            string plaintext = (string)EncryptionFormOptions[InputIndex].GetValue();
            int dimension = (int)Math.Ceiling(Math.Sqrt(plaintext.Length));
            Bitmap encryptedImage = new Bitmap(dimension, dimension);
            int count = 0;
            int colorValue;

            for (int x = 0; x < dimension; x++) 
            {
                for (int y = 0; y < dimension; y++)
                {
                    if (count < plaintext.Length)
                    {
                        colorValue = standardAlphabet.IndexOf(plaintext[count++]) * 9;
                        encryptedImage.SetPixel(x, y, colorValue > -1 ? Color.FromArgb(colorValue, 0, 0) : Color.FromArgb(0, 0, 255));
                    }
                }
            }
            EncryptionFormOptions[ImageDisplayerIndex].SetValue(BitmapToBase64String(encryptedImage));
        }

        public override void Decrypt()
        {
            string plaintext = "";
            Bitmap encryptedImage = ((ImageUploader)DecryptionFormOptions[ImageUploaderIndex]).Value;
            if (encryptedImage != null)
            {
                int dimension = encryptedImage.Width;
                Color currentColor;

                // the runtime on this scales attrociously - encrypting the entire Road to Wigan Pier text took <2 seconds, but decrypting took > 1 minute
                // an optimization probably lies in directly manipulating the image's data in bit array form, rather than continuously getting the color of the current pixel
                // but for our purposes, it works..
                for (int x = 0; x < dimension; x++)
                {
                    for (int y = 0; y < dimension; y++)
                    {
                        currentColor = encryptedImage.GetPixel(x, y);
                        plaintext += currentColor.B == 255 ? ' ' : standardAlphabet[currentColor.R / 9];
                    }
                }
                EncryptionFormOptions[ImageDisplayerIndex].SetValue(BitmapToBase64String(encryptedImage));
                EncryptionFormOptions[InputIndex].SetValue(plaintext);
            }
            else 
            {
                DecryptionFormOptions[ImageUploaderIndex].ErrorMessage = "Please ensure only one .png file was uploaded.";
                EncryptionFormOptions[ImageDisplayerIndex].SetValue(null);
                EncryptionFormOptions[InputIndex].SetValue(null);
            }
            DecryptionFormOptions[ImageUploaderIndex].SetValue(null);
        }

        private string BitmapToBase64String(Bitmap bitmap) 
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            string result = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
            ms.Close();
            return result;
        }
    }
}
