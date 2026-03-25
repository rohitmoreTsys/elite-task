using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.SecretVault;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Elite.Common.Utilities.Encription
{
    public class AesCryption
    {
        private static readonly ISecretVault _secretVault = Elite.Common.Utilities.SecretVault.SecretVault.Instance;
        public static string EncryptString(string plainText, string EncryptionKey)
        {
            string key = _secretVault.GetValuesFromVault("AesCryptionKey");
            EncryptionKey = string.IsNullOrEmpty(EncryptionKey) ? key : EncryptionKey.Replace("-", "");
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                var keyvalue = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.Key = keyvalue;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText, string DecryptionKey)
        {
            string key = _secretVault.GetValuesFromVault("AesCryptionKey");
            DecryptionKey = string.IsNullOrEmpty(DecryptionKey) ? key : DecryptionKey.Replace("-", "");
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {


                aes.Key = Encoding.UTF8.GetBytes(DecryptionKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string DecryptUID(string cipherText, string DecryptionKey)
        {
            string key = _secretVault.GetValuesFromVault("AesCryptionKey");
            byte[] cipherBytes;
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                {
                    return null;
                }
                cipherBytes = Convert.FromBase64String(cipherText);
           
                var cipherTextArray = key.Split("|");
                string cipherPhrase = cipherTextArray[0];
                string iv = cipherTextArray[1];


                DecryptionKey = string.IsNullOrEmpty(DecryptionKey) ? key : DecryptionKey.Replace("-", "");
                using (Aes encryptor = Aes.Create())
                {
                    encryptor.Key = Encoding.UTF8.GetBytes(cipherPhrase);
                    encryptor.Padding = PaddingMode.PKCS7;
                    encryptor.Mode = CipherMode.CBC;
                    encryptor.FeedbackSize = 128;
                    encryptor.IV = Encoding.UTF8.GetBytes(iv);
                    using (MemoryStream ms = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cs, Encoding.UTF8))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
               return cipherText;
            }
        }
    }
}
