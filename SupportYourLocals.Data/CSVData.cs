using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using SupportYourLocals.ExtensionMethods;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;

namespace SupportYourLocals.Data
{
    public class CSVData : IUserStorage
    {
        private readonly string filePath = ConfigurationManager.AppSettings.Get("CSVDataFilePath");
        private readonly Dictionary<string, UserData> dictionaryUserData;

        public CSVData ()
        {
            dictionaryUserData = LoadData();
        }

        private Dictionary<string, UserData> LoadData()
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, UserData>();
            }
            var userDataList = new Dictionary<string, UserData>();
            using (TextFieldParser csvParser = new TextFieldParser(filePath))
            {
                csvParser.CommentTokens = new[] { "#" };
                csvParser.SetDelimiters(new[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();

                    var username = fields[0];
                    var passwordHash = fields[1];
                    var salt = fields[2];
                    var id = fields[3];

                    userDataList.Add(id, new UserData(username, passwordHash, salt, id));
                }
            }
            return userDataList; 
        }

        public Task SaveData()
        {
            StringBuilder csv = new StringBuilder();

            csv.AppendLine("Username, Hashed password, Salt, ID");

            foreach (var user in dictionaryUserData.Values)
            {
                var newLine = "{0},{1},{2},{3}".Format(user.Username, user.PasswordHash, user.Salt, user.ID);
                csv.AppendLine(newLine);
            }

            File.WriteAllText(filePath, csv.ToString());

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
