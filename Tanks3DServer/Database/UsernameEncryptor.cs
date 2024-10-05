using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.Database
{
    internal class UsernameEncryptor
    {
        // Обрезаем ключ до 32 байт
        private static readonly byte[] key = Encoding.UTF8.GetBytes("7A9C4D123F8B1231A9BC2D4E7C9F8E12").Take(32).ToArray(); // 32 символа для ключа

        // Обрезаем IV до 16 байт
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("4A3D9E2C8F1B4E3A").Take(16).ToArray();

        public static string Encrypt(string username)
        {
            Console.WriteLine("Encrypt 1 " + username);
            using (Aes aesAlg = Aes.Create())
            {
                Console.WriteLine("Encrypt 2");
                Console.WriteLine("Key Length: " + key.Length);
                Console.WriteLine("IV Length: " + iv.Length);
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                Console.WriteLine("Encrypt 3");

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    Console.WriteLine("Encrypt 4");

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(username);
                        }
                        Console.WriteLine("Encrypt 5");

                        byte[] encrypted = msEncrypt.ToArray();
                        return Convert.ToBase64String(encrypted).Substring(0, 24); // Обрезаем до 24 символов
                    }
                }
            }
        }

        public static string Decrypt(string encryptedUsername)
        {
            string base64Encrypted = encryptedUsername.PadRight(32, '='); // Восстанавливаем строку до base64
            byte[] cipherText = Convert.FromBase64String(base64Encrypted);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
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
}
