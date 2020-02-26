using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

namespace SleekChat.Data.Helpers
{
    public class SecurityHelper
    {
        //readonly AppSettings settings = new AppSettings();
        //private readonly IConfiguration config;

        //public SecurityHelper(IOptions<AppSettings> appSettings)
        //{
        //    settings = appSettings.Value;
        //}

        //private readonly IConfiguration settings;

        //public SecurityHelper(IConfiguration configuration)
        //{
        //    settings = configuration;
        //}

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
            //try
            //{
                //byte[] keyInBytes = Encoding.UTF8.GetBytes(settings.GetValue<string>("SecretKey"));
                byte[] keyInBytes = Encoding.UTF8.GetBytes(config.Value.SecretKey);
                SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, userId.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(keyInBytes),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken token = tokenHandler.CreateToken(descriptor);
                return tokenHandler.WriteToken(token);
            //}
            //catch
            //{
            //    return null;
            //}
        }


        public bool ValidateToken(string token)
        {
            try
            {
                // Attempt decoding token here
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
