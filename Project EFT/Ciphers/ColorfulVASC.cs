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
    /// <summary>Implements the necessary components, as outlined in <see cref="Cipher"/>, to perform "Colorful Visual Alphabetic Substitution Cipher" encryption and decryption.
    /// Including, but not limited to, CVASC-specific form option generation, validation, and encryption and decryption execution.</summary>
    public class ColorfulVASC : Cipher
    {
        /// <summary>See <see cref="Cipher.Name"/>.</summary>
        public override string Name { get { return "Colorful VASC"; } }

        /// <summary>The encryption form index of the <see cref="ImageDisplayer"/> that displays the encrypted image.</summary>
        private const int ImageDisplayerIndex = 1;

        /// <summary>The decryption form index of the <see cref="ImageUploader"/> that contains the encrypted image to decrypt.</summary>
        private const int ImageUploaderIndex = 0;

        /// <summary>Creates an instance that returns one solution when decrypting.</summary>
        public ColorfulVASC() : base() { }

        /// <summary> Modifies the CVASC-specific encryption and decryption <see cref="Option"/>s list:<br/>
        /// Encryption: Assumes <see cref="Cipher.standardAlphabet"/> is used in encryption, so removes the alphabet option. Adds an <see cref="ImageDisplayer"/> to show the encrypted image.<br/>
        /// Decryption: Assumes <see cref="Cipher.standardAlphabet"/> is used in decryption, so removes the alphabet option. Also adds an <see cref="ImageUploader"/> for uploading an encrypted image,
        /// and removes the ciphertext input option.
        /// </summary>
        public override void GenerateForms()
        {
            EncryptionFormOptions.RemoveAt(1);
            DecryptionFormOptions.RemoveAt(0);
            DecryptionFormOptions.RemoveAt(0);
            EncryptionFormOptions.Add(new ImageDisplayer());
            DecryptionFormOptions.Add(new ImageUploader("encryptedImage", "Decrypt this image", null));
        }

        /// <summary>Performs CVASC encryption and stores the result in the encryption form <see cref="ImageDisplayer"/>. Encryption works as follows: <br/>
        /// The first four characters' indices in the standard alphabet are multiplied by 9 and stored as the R, G, B, and A values of the first pixel in the generated image, respectively.<br/>
        /// The second four characters are similarly stored in the second pixel in the generated image, and so on. <br/>
        /// The pixels are arranged in a square, starting in the top left corner and working to the right, then move back to the start and down a row before continuing in a similar manner. <br/>
        /// Characters not in the alphabet cause the max value to be stored for that RGBA component (255). <br/>
        /// If the plaintext input size is not a square number, the bottom row will fill with as many 'null' (0, 0, 0, 0) pixels that are necessary to fill the row.
        /// </summary>
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

        /// <summary>Performs CVASC decryption and stores the result in the encryption form <see cref="TextAreaOption"/>. Decryption works as follows: <br/>
        /// The first pixel's R value is divided by 9. If the obtained value is nonnegative and less than the size of <see cref="Cipher.standardAlphabet"/>, 
        /// the character whose index in <see cref="Cipher.standardAlphabet"/> is said value is added to the plaintext. Similarly for the G, B, and A values, in that order. <br/>
        /// The second pixel's values are similarly treated, and so on. <br/>
        /// Values outside of the predescribed range cause a space (' ') to be inserted in the plaintext.<br/>
        /// If the pixel is a 'null' (0, 0, 0, 0) pixel, nothing is added to the plaintext and the pixel is skipped.
        /// </summary>
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
                    else
                    {
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

        /// <summary>Converts the given bitmap to a Base64 string for displaying on the front end.</summary>
        /// <param name="bitmap">The bitmap to convert to a Base64 string.</param>
        /// <returns>A Base64 string representing the given bitmap.</returns>
        private string BitmapToBase64String(Bitmap bitmap) 
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            string result = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
            ms.Close();
            return result;
        }

        /// <summary>Performs validation of necessary encryption form data, including: <br/>
        /// - Input text size validation.<br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if CVASC encryption data is valid and encryption can occur, false otherwise.</returns>
        protected override bool EncryptionFormDataIsValid()
        {
            if (!IsValidTextInput((string)EncryptionFormOptions[InputIndex].GetValue()))
            {
                EncryptionFormOptions[InputIndex].ErrorMessage = InvalidInputMessage;
                return false;
            }
            return true;
        }

        /// <summary>Performs validation of necessary decryption form data, including: <br/>
        /// - Ensuring the uploaded image is properly formatted (pixel format wise - must be either 32bppARGB or 24bppRGB) and is not null. <br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if CVASC decryption data is valid and decryption can occur, false otherwise.</returns>
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
