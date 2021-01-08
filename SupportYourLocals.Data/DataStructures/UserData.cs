using System;
using System.Security.Cryptography;
using System.Text;

namespace SupportYourLocals.Data
{
    public class UserData
    {
        public const int saltSize = 10;
        public string ID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }

        public UserData(string username, string password, string salt = null, string id = null)
        {            
            Username = username;            
            Salt = salt ?? CreateSalt(saltSize);
            PasswordHash = GenerateHash(password, Salt);
            ID = id ?? GenerateId;
        }

        private static string GenerateId => Guid.NewGuid().ToString("N");

        public static string CreateSalt(int size)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);

            return Convert.ToBase64String(buff);
        }

        public static string GenerateHash(string password, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            SHA256Managed sha256hashstring = new SHA256Managed();
            var hash = sha256hashstring.ComputeHash(bytes);

            return Convert.ToBase64String(hash).Replace("-", "").ToLower();
        }
    }
}
