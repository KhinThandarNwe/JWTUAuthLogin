using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace JWTUAuthLogin.Shared.Utilities
{
    public class ConfigurationEncryption
    {
        private const int KeySize = 256;
        private const int IvSize = 128;
        private const int SaltSize = 32;
        private const int Iterations = 50000;

        public static string Encrypt(string plainText, string encryptionKey)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("Plain text cannot be null or empty.", nameof(plainText));
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentNullException("Encryption key cannot be null or empty.", nameof(encryptionKey));
            try
            {
                byte[] salt = GenerateSalt();
                using (var deriveBytes = new Rfc2898DeriveBytes(
                    encryptionKey,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256
                    ))
                {
                    byte[] key = deriveBytes.GetBytes(KeySize / 8);
                    byte[] iv = deriveBytes.GetBytes(IvSize / 8);
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = KeySize;
                        aes.BlockSize = IvSize;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.Key = key;
                        aes.IV = iv;
                        using (var encryptor = aes.CreateEncryptor())
                        using (var msEncrypt = new MemoryStream())
                        {
                            // Write salt first
                            msEncrypt.Write(salt, 0, salt.Length);

                            // Then encrypt the data
                            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            {
                                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                                csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                                csEncrypt.FlushFinalBlock();
                            }

                            // Return with ENCRYPTED: prefix so we know it's encrypted
                            return "ENCRYPTED:" + Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during encryption.", ex);
            }
        }
         /// <summary>
         /// Decrypts an encrypted string
         /// </summary>
         /// <param name="cipherText">Encrypted string (with or without ENCRYPTED: prefix)</param>
         /// <param name="encryptionKey">Decryption key</param>
         /// <returns>Decrypted plain text</returns>
        public static string Decrypt(string cipherText, string encryptionKey)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentNullException(nameof(encryptionKey));

            // If not encrypted, return as-is
            if (!IsEncrypted(cipherText))
                return cipherText;

            try
            {
                // Remove ENCRYPTED: prefix if present
                string base64Data = cipherText.Replace("ENCRYPTED:", "").Trim();
                byte[] cipherBytes = Convert.FromBase64String(base64Data);

                // Extract salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(cipherBytes, 0, salt, 0, SaltSize);

                // Derive key and IV
                using (var deriveBytes = new Rfc2898DeriveBytes(
                    encryptionKey,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256))
                {
                    byte[] key = deriveBytes.GetBytes(KeySize / 8);
                    byte[] iv = deriveBytes.GetBytes(IvSize / 8);

                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = KeySize;
                        aes.BlockSize = IvSize;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.Key = key;
                        aes.IV = iv;

                        using (var decryptor = aes.CreateDecryptor())
                        using (var msDecrypt = new MemoryStream(cipherBytes, SaltSize, cipherBytes.Length - SaltSize))
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        using (var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException(
                    $"Decryption failed. Ensure the encryption key is correct. Error: {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Checks if a value is encrypted (has ENCRYPTED: prefix)
        /// </summary>
        public static bool IsEncrypted(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith("ENCRYPTED:");
        }

        /// <summary>
        /// Generates a cryptographically secure random salt
        /// </summary>
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Generates a new secure encryption key
        /// Use this once to generate a key, then store it securely in AWS Parameter Store
        /// </summary>
        public static string GenerateEncryptionKey()
        {
            byte[] keyBytes = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            return Convert.ToBase64String(keyBytes);
        }
    }
}
