using System;
namespace SleekChat.Data.Helpers
{
    public static class DataHelper
    {

        public static Guid GetGuid()
        {
            return Guid.NewGuid();
        }

        public static string Encrypt(string rawText)
        {
            string encryptedText = rawText;
            // Perform encryption here
            return encryptedText;
        }

        public static string Decrypt(string encryptedText)
        {
            string decryptedText = encryptedText;
            // Perform decryption here
            return decryptedText;
        }
    }
}
