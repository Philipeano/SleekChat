using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SleekChat.Data.Helpers
{
    public class SecurityHelper
    {
        public string CreateHash(string plainText)
        {
            byte[] salt = GenerateSalt(16);
            byte[] encodedText = KeyDerivation
                .Pbkdf2(plainText, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(encodedText)}";
        }


        private byte[] GenerateSalt(int length)
        {
            byte[] salt = new byte[length];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }
            return salt;
        }


        public bool ValidatePassword(string existingHash, string newText)
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


        public string CreateToken(Guid userId, IOptions<AppSettings> config)
        {
            try
            {
                byte[] keyInBytes = Encoding.UTF8.GetBytes(config.Value.SecretKey);
                SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(3),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(keyInBytes),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken token = tokenHandler.CreateToken(descriptor);
                return tokenHandler.WriteToken(token);
            }
            catch
            {
                return null;
            }
        }
    }
}
