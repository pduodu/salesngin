namespace salesngin.Extensions;

/// <summary>
/// Provides extension methods for Encrypting and Decrypting strings.
/// </summary>
public static class StringEncryptionExtensions
{
    #region Private Field(s)

    /// <summary>
    /// Encryption Key used as salt value for AES Encryption.
    /// </summary>
    #endregion

    public static string EncryptText(this string plainText, string encryptionPhrase)
    {
        using (StringEncryptor encryptor = new(encryptionPhrase))
        {
            return encryptor.EncryptText(plainText);
        }
    }

    public static string DecryptText(this string encryptedText, string encryptionPhrase, string ivString)
    {
        using (StringEncryptor encryptor = new(encryptionPhrase))
        {
            return encryptor.DecryptText(encryptedText, ivString);
        }
    }

    private class StringEncryptor : IDisposable
    {
        private readonly byte[] encryptionKey;
        private readonly Aes aesAlg;

        public StringEncryptor(string encryptionPhrase)
        {
            using (var sha256 = SHA256.Create())
            {
                encryptionKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionPhrase));
            }

            aesAlg = Aes.Create();
            aesAlg.Key = encryptionKey;
        }

        public string EncryptText(string plainText)
        {
            aesAlg.GenerateIV(); // Generate a random IV
            byte[] iv = aesAlg.IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }

        public string DecryptText(string encryptedText, string ivString)
        {
            byte[] iv = Convert.FromBase64String(ivString);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        public void Dispose()
        {
            aesAlg.Dispose();
        }
    }


}

