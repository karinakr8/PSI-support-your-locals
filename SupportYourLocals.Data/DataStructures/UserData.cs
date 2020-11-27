using System;
using MapControl;
using SupportYourLocals.ExtensionMethods;

namespace SupportYourLocals.Data
{
    public class UserData
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }

        public UserData(string username, string passwordHash, string salt, int id)
        {
            ID = id;
            Username = username;
            PasswordHash = passwordHash;
            Salt = salt;
        }
    }
}
