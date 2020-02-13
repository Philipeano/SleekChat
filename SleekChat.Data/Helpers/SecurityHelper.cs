using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace SleekChat.Data.Helpers
{
    public class SecurityHelper
    {
        public static string CreateHash(string plainText)
        {
            byte[] salt = GenerateSalt(16);
            byte[] encodedText = KeyDerivation
                .Pbkdf2(plainText, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(encodedText)}";
        }

        private static byte[] GenerateSalt(int length)
        {
            byte[] salt = new byte[length];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }
            return salt;
        }

        public static bool Validate(string existingHash, string newText)
        {
            try
            {
                string[] parts = existingHash.Split(':');
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] newHash = KeyDerivation
                    .Pbkdf2(newText, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);
                return parts[1].Equals(Convert.ToBase64String(newHash));
            }
            catch
            {
                return false;
            }
        }
    }
}
