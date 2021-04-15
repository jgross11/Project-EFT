using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    public class ColorfulVASC : Cipher
    {
        public override string Name { get { return "Colorful VASC"; } }
        private const int ImageDisplayerIndex = 1;
        private const int ImageUploaderIndex = 0;

        public ColorfulVASC(int numSols) : base(numSols)
        {

        }

        public ColorfulVASC() : base() { }

        public override void GenerateForms()
        {
            EncryptionFormOptions.RemoveAt(1);
            DecryptionFormOptions.RemoveAt(0);
            DecryptionFormOptions.RemoveAt(0);
            EncryptionFormOptions.Add(new ImageDisplayer());
            DecryptionFormOptions.Add(new ImageUploader("encryptedImage", "Decrypt this image", null));
        }

        protected override void Encrypt()
        {
            string plaintext = ((string)EncryptionFormOptions[InputIndex].GetValue()).ToLower();
            int dimension = (int)Math.Ceiling(Math.Sqrt(plaintext.Length/4));
            Bitmap encryptedImage = new Bitmap(dimension, dimension);
            int count = 0;
            int rValue, gValue, bValue, aValue;

            for (int y = 0; y < dimension; y++) 
            {
                for (int x = 0; x < dimension; x++)
                {
                    if (count < plaintext.Length)
                    {
                        rValue = standardAlphabet.IndexOf(plaintext[count++]) * 9;
                        gValue = count < plaintext.Length ? standardAlphabet.IndexOf(plaintext[count++]) * 9 : 255;
                        bValue = count < plaintext.Length ? standardAlphabet.IndexOf(plaintext[count++]) * 9 : 255;
                        aValue = count < plaintext.Length ? standardAlphabet.IndexOf(plaintext[count++]) * 9 : 255;
                        if (rValue < 0) rValue = 255;
                        if (gValue < 0) gValue = 255;
                        if (bValue < 0) bValue = 255;
                        if (aValue < 0) aValue = 255;
                        encryptedImage.SetPixel(x, y, Color.FromArgb(aValue, rValue, gValue, bValue));
                    }
                    else break;
                }
            }
            EncryptionFormOptions[ImageDisplayerIndex].SetValue(BitmapToBase64String(encryptedImage));
        }

        protected override void Decrypt()
        {
            string plaintext = "";
            Bitmap encryptedImage = ((ImageUploader)DecryptionFormOptions[ImageUploaderIndex]).Value;

            // this method is slightly faster than using GetPixel() - 3 min 31 s to decrypt Wigan as opposed to GetPixel() taking 3 min 49 s, 
            // but https://www.codeproject.com/Articles/617613/Fast-pixel-operations-in-NET-with-and-without-unsa
            // suggests that it should be running ~300 times faster, so something is definitely amiss.. 

            // Debug.WriteLine(encryptedImage.PixelFormat);
            // Format32bppArgb

            BitmapData data = encryptedImage.LockBits(new Rectangle(0, 0, encryptedImage.Width, encryptedImage.Height), ImageLockMode.ReadOnly, encryptedImage.PixelFormat);
                
            // cheap fix for different pixel formats amongst png's
            if (encryptedImage.PixelFormat == PixelFormat.Format32bppArgb)
            {
                byte[] imageBytes = new byte[encryptedImage.Width * encryptedImage.Height * 4];
                IntPtr scanner = data.Scan0;

                Marshal.Copy(scanner, imageBytes, 0, imageBytes.Length);

                for (int i = 0; i < imageBytes.Length; i += 4)
                {
                    // stored as BGRA instead of RGBA???
                    byte pixR = imageBytes[i + 2];
                    byte pixB = imageBytes[i];
                    byte pixG = imageBytes[i + 1];
                    byte pixA = imageBytes[i + 3];
                    if (pixB == 0 && pixR == pixB && pixB == pixG && pixB == pixA) continue;
                    else {
                        plaintext += pixR < 226 ? standardAlphabet[pixR / 9] : ' ';
                        plaintext += pixG < 226 ? standardAlphabet[pixG / 9] : ' ';
                        plaintext += pixB < 226 ? standardAlphabet[pixB / 9] : ' ';
                        plaintext += pixA < 226 ? standardAlphabet[pixA / 9] : ' ';
                    }
                }

                encryptedImage.UnlockBits(data);

                EncryptionFormOptions[ImageDisplayerIndex].SetValue(BitmapToBase64String(encryptedImage));
                EncryptionFormOptions[InputIndex].SetValue(plaintext);
            }
            else if (encryptedImage.PixelFormat == PixelFormat.Format24bppRgb)
            {
                byte[] imageBytes = new byte[encryptedImage.Width * encryptedImage.Height * 3];
                IntPtr scanner = data.Scan0;

                Marshal.Copy(scanner, imageBytes, 0, imageBytes.Length);

                for (int i = 0; i < imageBytes.Length; i += 3)
                {
                    // stored as BGRA instead of RGBA???
                    byte pixR = imageBytes[i + 2];
                    byte pixB = imageBytes[i];
                    byte pixG = imageBytes[i + 1];
                    if (pixB == 0 && pixR == pixB && pixB == pixG) continue;
                    else
                    {
                        plaintext += pixR != 255 ? standardAlphabet[pixR / 9] : ' ';
                        plaintext += pixG != 255 ? standardAlphabet[pixG / 9] : ' ';
                        plaintext += pixB != 255 ? standardAlphabet[pixB / 9] : ' ';
                    }
                }

                encryptedImage.UnlockBits(data);

                EncryptionFormOptions[ImageDisplayerIndex].SetValue(BitmapToBase64String(encryptedImage));
                EncryptionFormOptions[InputIndex].SetValue(plaintext);
            }
            DecryptionFormOptions[ImageUploaderIndex].SetValue(null);
        }

        private string BitmapToBase64String(Bitmap bitmap) 
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            string result = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
            ms.Close();
            return result;
        }

        protected override bool EncryptionFormDataIsValid()
        {
            if (!IsValidTextInput((string)EncryptionFormOptions[InputIndex].GetValue()))
            {
                EncryptionFormOptions[InputIndex].ErrorMessage = InvalidInputMessage;
                return false;
            }
            return true;
        }

        protected override bool DecryptionFormDataIsValid()
        {
            Bitmap info = ((ImageUploader)DecryptionFormOptions[ImageUploaderIndex]).Value;
            if (info == null || (info.PixelFormat != PixelFormat.Format32bppArgb && info.PixelFormat != PixelFormat.Format24bppRgb))
            {
                DecryptionFormOptions[ImageUploaderIndex].ErrorMessage = "Please ensure only one .png file was uploaded.";
                EncryptionFormOptions[ImageDisplayerIndex].SetValue(null);
                EncryptionFormOptions[InputIndex].SetValue(null);
                return false;
            }
            return true;
        }
    }
}
