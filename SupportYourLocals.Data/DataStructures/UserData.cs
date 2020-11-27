using System;
using System.Security.Cryptography;
using System.Text;
using MapControl;
using SupportYourLocals.ExtensionMethods;

namespace SupportYourLocals.Data
{
    public class UserData
    {
        private static int saltSize = 10;
        public string ID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }

        public UserData(string username, string passwordHash = null, string salt = null, string id = null)
        {            
            Username = username;            
            Salt = salt ?? CreateSalt(saltSize);
            PasswordHash = passwordHash ?? GenerateHash(passwordHash, Salt);
            ID = id ?? GenerateUserID().ToString();
        }

        private int GenerateUserID()
        {
            IUserStorage userData = new CSVData();
            var users = userData.GetAllData();

            Random rnd = new Random();
            int randomID = rnd.Next(1000, 9999);

            foreach(var id in users)
            {
                if(int.Parse(id.ID) == randomID)
                {
                    return GenerateUserID();
                }                
            }
            return randomID;
        }

        private string CreateSalt(int size)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);

            return Convert.ToBase64String(buff);
        }

        private string GenerateHash(string password, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            SHA256Managed sha256hashstring = new SHA256Managed();
            var hash = sha256hashstring.ComputeHash(bytes);

            return Convert.ToBase64String(hash).Replace("-", "").ToLower();
        }
    }
}
