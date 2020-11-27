using System;
using System.Text;
using System.Security.Cryptography;

namespace SupportYourLocals.Data
{
    public class Password
    {
        public bool CompareHash(string attemptedPassword, string base64Hash, string salt)
        {
            string base64AttemptedHash = GenerateHash(attemptedPassword, salt);
            return base64Hash == base64AttemptedHash;
        }

        public string CreateSalt(int size)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);

            return Convert.ToBase64String(buff);
        }

        public string GenerateHash(string password, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            SHA256Managed sha256hashstring = new SHA256Managed();
            var hash = sha256hashstring.ComputeHash(bytes);

            return Convert.ToBase64String(hash).Replace("-", "").ToLower();
        }
    }
}
