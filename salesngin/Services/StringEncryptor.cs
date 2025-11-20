namespace salesngin.Services;

public class StringEncryptor
{
    private readonly byte[] encryptionKey;
    public StringEncryptor(string encryptionPhrase)
    {
        using (var sha256 = SHA256.Create())
        {
            encryptionKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionPhrase));
        }
    }

    public string EncryptText(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey;
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
    }

    public string DecryptText(string encryptedText, string ivString)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey;
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
    }
}


