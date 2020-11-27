using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MapControl;
using Microsoft.VisualBasic.FileIO;
using SupportYourLocals.ExtensionMethods;
using System.Security.Cryptography;

namespace SupportYourLocals.Data
{
    public class CSVData : IUserStorage
    {
        private static string filePath = @"./UserData.csv";

        readonly List<UserData> listUserData = new List<UserData>();

        public CSVData ()
        {
            listUserData = LoadData();
        }

        private List<UserData> LoadData()
        {
            StringBuilder csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                return new List<UserData>();                
            }
            else
            {
                var userDataList = new List<UserData>();
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
                        var id = int.Parse(fields[3]);

                        userDataList.Add(new UserData(username, passwordHash, salt, id));
                    }
                }
                return userDataList;
            }            
        }

        public void SaveData()
        {
            StringBuilder csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                csv.AppendLine("Username, Hashed password, Salt, ID");
            }

            foreach (var user in listUserData)
            {
                var newLine = "{0},{1},{2},{3}".Format(user.Username, user.PasswordHash, user.Salt, user.ID);
                csv.AppendLine(newLine);
            }

            File.AppendAllText(filePath, csv.ToString());
        }        

        public UserData GetData(string username)
        {
            foreach(var userData in listUserData)
            {
                if(username.Equals(userData.Username))
                {
                    return userData;
                }    
            }
            return new UserData("","","",0);
        }

        public int GetDataCount() => listUserData.Count;

        public void AddData(UserData data) => listUserData.Add(data);

        public void UpdateData(UserData data) => throw new NotImplementedException();

        public void RemoveData(string id) => throw new NotImplementedException();

        public List<UserData> GetAllData() => listUserData;
    }
}
