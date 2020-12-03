using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using SupportYourLocals.ExtensionMethods;
using System.Linq;
using System.Configuration;

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

        public void SaveData()
        {
            StringBuilder csv = new StringBuilder();

            csv.AppendLine("Username, Hashed password, Salt, ID");

            foreach (var user in dictionaryUserData.Values)
            {
                var newLine = "{0},{1},{2},{3}".Format(user.Username, user.PasswordHash, user.Salt, user.ID);
                csv.AppendLine(newLine);
            }

            File.WriteAllText(filePath, csv.ToString());
        }

        UserData IDataStorage<UserData>.GetData(string id) => dictionaryUserData[id];

        int IDataStorage<UserData>.GetDataCount() => dictionaryUserData.Count;

        void IDataStorage<UserData>.AddData(UserData data) => dictionaryUserData.Add(data.ID, data);

        void IDataStorage<UserData>.UpdateData(UserData data) => dictionaryUserData[data.ID] = data;

        void IDataStorage<UserData>.RemoveData(string id) => dictionaryUserData.Remove(id);

        List<UserData> IDataStorage<UserData>.GetAllData() => dictionaryUserData.Select(d => d.Value).ToList();
    }
}
