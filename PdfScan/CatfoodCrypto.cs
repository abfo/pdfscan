using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Catfood.Utils
{
    /// <summary>
    /// Cryptography helper methods
    /// </summary>
    public static class CatfoodCrypto
    {
        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="toEncrypt">The string to encrypt</param>
        /// <param name="productName">The name of the current product</param>
        /// <returns>Encrypted version of the string</returns>
        public static string EncryptString(string toEncrypt, string productName)
        {
            if (toEncrypt == null) { return null; }

            byte[] toEncryptBytes = Encoding.Unicode.GetBytes(toEncrypt);

            byte[] encryptedBytes = ProtectedData.Protect(toEncryptBytes,
                string.IsNullOrEmpty(productName) ? null : Encoding.Unicode.GetBytes(productName), 
                DataProtectionScope.CurrentUser);
            
            return Convert.ToBase64String(encryptedBytes, Base64FormattingOptions.InsertLineBreaks);
        }

        /// <summary>
        /// Decrypts a string
        /// </summary>
        /// <param name="toDecrypt">The string to decrypt</param>
        /// <param name="productName">The name of the current product</param>
        /// <returns>Decrypted version of the string</returns>
        public static string DecryptString(string toDecrypt, string productName)
        {
            if (toDecrypt == null) { return null; }

            byte[] toDecryptBytes = Convert.FromBase64String(toDecrypt);

            byte[] decryptedBytes = ProtectedData.Unprotect(toDecryptBytes, 
                string.IsNullOrEmpty(productName) ? null : Encoding.Unicode.GetBytes(productName), 
                DataProtectionScope.CurrentUser);
            
            return Encoding.Unicode.GetString(decryptedBytes);
        }
    }
}
