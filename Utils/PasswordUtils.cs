using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InstaminiWebService.Utils
{
    public static class PasswordUtils
    {
        public static string GenerateSalt(int length = 64)
        {
            var salt = new byte[length];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return Encoding.UTF8.GetString(salt);
        }

        public static string HashPasswordWithSalt(string originalPass, string salt)
        {
            string combinedPass = originalPass + salt;
            // Create a SHA256   
            using SHA512 sha512Hash = SHA512.Create();
            // ComputeHash - returns byte array  
            byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedPass));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static bool ValidatePasswordWithSalt(string password, string salt, string hashed)
        {
            return HashPasswordWithSalt(password, salt).Equals(hashed);
        }
    }
}
