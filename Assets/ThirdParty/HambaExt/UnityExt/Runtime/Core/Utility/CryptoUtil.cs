using System;
using System.IO;
using System.Security.Cryptography;
using UnityExt.Debugging;

namespace UnityExt
{
    /// <summary>Utility helpers for AES encryption/decryption of save data using configuration-defined secrets and salts.</summary>
    public static class CryptoUtil
    {
        // ========================= PRIVATE METHODS =======================

        /// <summary>Validates configured encryption key and IV in development/editor builds.</summary>
        static void CheckSecurityData()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (!UConfig.encryptionKEYForOtherEncryption.IsStringValid())
            {
                GLog.Throw("Security key must be valid for encryption operation. Please set it in the config!");
            }
            if (!UConfig.encryptionIVForOtherEncryption.IsStringValid())
            {
                GLog.Throw("Security IV must be valid for encryption operation. Please set it in the config!");
            }
#endif
        }
        /// <summary>Converts a salt string into a byte array using project encoding utilities.</summary>
        /// <param name="saltString">Salt text.</param>
        /// <returns>Byte array representation of salt.</returns>
        static byte[] GetSalt(string saltString)
        {
            return EncodingUtil.GetBytes(saltString);
        }
        /// <summary>Reads a length-prefixed byte array from a stream.</summary>
        /// <param name="s">Stream containing [length(int)][data].</param>
        /// <returns>Byte array read from the stream.</returns>
        /// <exception cref="SystemException">Thrown if the stream does not contain a properly formatted array.</exception>
        static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        #region Encryption
        //Src: https://www.c-sharpcorner.com/UploadFile/a85b23/text-encrypt-and-decrypt-with-a-specified-key/
        /// <summary>Encrypts plain text save data (string) using configured shared secret and salt (AES).</summary>
        /// <param name="plainText">Plain text to encrypt.</param>
        /// <returns>Base64 encoded encrypted string; null if encryption fails.</returns>
        internal static string EncryptStringAESForSaveData(string plainText)
        {
            string sharedSecret = UConfig.encryptionSharedSecretForSaveData;
            string salt = UConfig.encryptionSaltForSaveData;
            return EncryptStringAESForSaveData(plainText, sharedSecret, salt);
        }

        /// <summary>Encrypts plain text save data using provided shared secret and salt (AES, Base64 output).</summary>
        /// <param name="plainText">Plain text to encrypt.</param>
        /// <param name="sharedSecret">Password used to derive key.</param>
        /// <param name="salt">Salt used for key derivation.</param>
        /// <returns>Base64 encoded encrypted string; null if encryption fails.</returns>
        internal static string EncryptStringAESForSaveData(string plainText, string sharedSecret, string salt)
        {
            var _salt = GetSalt(salt);
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr = null;                 // Encrypted string to return  
            RijndaelManaged aesAlg = null;        // RijndaelManaged object used to encrypt the data.  

            try
            {
                // generate the key from the shared secret and the salt  
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object  
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.  
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.  
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV  
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt =
                       new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.  
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            catch (Exception ex)
            {
                GLog.PrintError("could not encrypt player storage data save. Exception msg: " + ex.Message);
            }
            finally
            {
                // Clear the RijndaelManaged object.  
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.  
            return outStr;
        }

        //Src: https://www.c-sharpcorner.com/UploadFile/a85b23/text-encrypt-and-decrypt-with-a-specified-key/
        //I modified it to process byte array
        /// <summary>Encrypts raw byte data using configured shared secret and salt (AES).</summary>
        /// <param name="inputData">Raw bytes to encrypt.</param>
        /// <returns>Encrypted byte array (with prepended IV); null if encryption fails.</returns>
        internal static byte[] EncryptStringAESForSaveData(byte[] inputData)
        {
            string sharedSecret = UConfig.encryptionSharedSecretForSaveData;
            string salt = UConfig.encryptionSaltForSaveData;

            var _salt = GetSalt(salt);
            if (inputData == null)
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            byte[] outStr = null;                 // Encrypted string to return  
            RijndaelManaged aesAlg = null;        // RijndaelManaged object used to encrypt the data.  

            try
            {
                // generate the key from the shared secret and the salt  
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object  
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.  
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.  
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV  
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt =
                       new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.  
                            swEncrypt.Write(inputData);
                        }
                    }
                    outStr = msEncrypt.ToArray();
                }
            }
            catch (Exception ex)
            {
                GLog.PrintError("could not encrypt player storage data save. Exception msg: " + ex.Message);
            }
            finally
            {
                // Clear the RijndaelManaged object.  
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.  
            return outStr;
        }
        #endregion

        #region Decryption
        /// <summary>Decrypts Base64 encoded encrypted save data using configured shared secret and salt.</summary>
        /// <param name="cipherData">Base64 encrypted cipher text.</param>
        /// <returns>Decrypted plain text; null if decryption fails.</returns>
        internal static string DecryptStringAESForSaveData(string cipherData)
        {
            string sharedSecret = UConfig.encryptionSharedSecretForSaveData;
            string salt = UConfig.encryptionSaltForSaveData;

            var _salt = GetSalt(salt);
            if (string.IsNullOrEmpty(cipherData))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object  
            // used to decrypt the data.  
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold  
            // the decrypted text.  
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt  
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.  
                byte[] bytes = Convert.FromBase64String(cipherData);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object  
                    // with the specified key and IV.  
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream  
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.  
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt =
                        new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream  
                            // and place them in a string.  
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                GLog.PrintError("could not decrypt player storage data save. Exception msg: " + ex.Message);
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            return plaintext;
        }

        /// <summary>Decrypts raw encrypted save data bytes (with prepended IV) using configured shared secret and salt.</summary>
        /// <param name="cipherData">Encrypted byte array including IV length + IV prefix.</param>
        /// <returns>Decrypted plain text; null if decryption fails.</returns>
        internal static string DecryptStringAESForSaveData(byte[] cipherData)
        {
            string sharedSecret = UConfig.encryptionSharedSecretForSaveData;
            string salt = UConfig.encryptionSaltForSaveData;

            var _salt = GetSalt(salt);
            if (cipherData == null)
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object  
            // used to decrypt the data.  
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold  
            // the decrypted text.  
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt  
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.  
                byte[] bytes = cipherData;
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object  
                    // with the specified key and IV.  
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream  
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.  
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt =
                        new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream  
                            // and place them in a string.  
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                GLog.PrintError("could not decrypt player storage data save. Exception msg: " + ex.Message);
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            return plaintext;
        }
        #endregion
    }
}