using System.Collections.Generic;
using System.Linq;
using System.Data;
using MySqlConnector;
using System.Configuration;
using System.Threading.Tasks;

namespace SupportYourLocals.Data
{
    public class LoginData : IUserStorage
    {
        private readonly Dictionary<string, UserData> dictionaryUserData;
        private string  connectionString = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        public LoginData ()
        {
            dictionaryUserData = LoadData();
        }

        private Dictionary<string, UserData> LoadData()
        {
            var userDataList = new Dictionary<string, UserData>();

            using (var con = new MySqlConnection())
            {
                con.ConnectionString = connectionString;

                con.Open();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    
                    cmd.CommandText = $"SELECT username, hashed_psw, salt, id FROM logindata;";

                    using MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var username = reader.GetString(0);
                        var passwordHash = reader.GetString(1);
                        var salt = reader.GetString(2);
                        var id = reader.GetString(3);

                        userDataList.Add(id, new UserData(username, passwordHash, salt, id));
                    }
                }
                con.Close();
            }

            return userDataList;
        }

        public Task SaveData()
        {
            using (var con = new MySqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = "TRUNCATE logindata";
                    cmd.ExecuteNonQuery();
                                        
                    foreach (var user in dictionaryUserData.Values)
                    {    
                        cmd.CommandText = "INSERT INTO logindata(username, hashed_psw, salt, id) VALUES(\"" + user.Username + "\", \"" + user.PasswordHash + "\", \"" + user.Salt + "\", \"" + user.ID + "\");";

                        cmd.ExecuteNonQuery();
                    }
                }
                con.Close();
            }

            return Task.CompletedTask;
        }

        public Task<UserData> GetData(string id) => Task.FromResult(dictionaryUserData[id]);

        public Task<List<UserData>> GetAllData() => Task.FromResult(dictionaryUserData.Values.ToList());

        public Task<int> GetDataCount() => Task.FromResult(dictionaryUserData.Count);

        public Task AddData(UserData data)
        {
            dictionaryUserData.Add(data.ID, data);
            return Task.CompletedTask;
        }

        public Task UpdateData(UserData data)
        {
            dictionaryUserData[data.ID] = data;
            return Task.CompletedTask;
        }

        public Task RemoveData(string id)
        {
            dictionaryUserData.Remove(id);
            return Task.CompletedTask;
        }
    }
}
