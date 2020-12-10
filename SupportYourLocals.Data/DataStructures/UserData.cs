using System;
using System.Security.Cryptography;
using System.Text;

namespace SupportYourLocals.Data
{
    public class UserData : GenericData
    {
        private const int saltSize = 10;
        public string Username => Name;
        public string PasswordHash { get; private set; }
        public string Salt { get; set; }
        public string Password
        {
            set
            {
                Salt ??= CreateSalt(saltSize);
                PasswordHash = GenerateHash(value, Salt);
            }
        }

        public UserData() { }

        public UserData(string name, string password, string salt = null, string id = null) : base(name, id)
        {                
            Salt = salt ?? CreateSalt(saltSize);
            Password = password;
        }

        private static string CreateSalt(int size)
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
