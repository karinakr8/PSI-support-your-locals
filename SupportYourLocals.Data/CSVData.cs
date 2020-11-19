﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MapControl;
using Microsoft.VisualBasic.FileIO;
using SupportYourLocals.ExtensionMethods;
using System.Security.Cryptography;

namespace SupportYourLocals.Data
{
    public class CSVData
    {
        private static string filePath = @"./Data.csv";
        private static int personsID = 1000;
        private static int saltSize = 10;
        public bool CheckLoginData(String password, String username)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            else
            {
                using (TextFieldParser csvParser = new TextFieldParser(filePath))
                {
                    csvParser.CommentTokens = new[] { "#" };
                    csvParser.SetDelimiters(new[] { "," });
                    csvParser.HasFieldsEnclosedInQuotes = true;

                    // Skip the row with the column names
                    csvParser.ReadLine();

                    while (!csvParser.EndOfData)
                    {
                        // Read current line fields, pointer moves to the next line.
                        string[] fields = csvParser.ReadFields();

                        if (username == fields[0])
                        {
                            String salt = fields[2];
                            if (CompareHash(password, Encoding.ASCII.GetBytes(fields[1]), salt))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
        }

        public bool CheckRegisterData(String password, String username)
        {
            return false;
        }
        public void SaveRegisterData(String password, String username)
        {
            StringBuilder csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                csv.AppendLine("Username, Password, ID");
            }

            String salt = CreateSalt(saltSize);

            //String hashedPassword = BitConverter.ToString(GenerateHash(password, salt)).Replace("-", "");

            var newLine = "{0},{1},{2},{3}".Format(username, GenerateHash(password, salt), salt, personsID);

            csv.AppendLine(newLine);

            File.AppendAllText(filePath, csv.ToString());
        }
        public bool CompareHash(string attemptedPassword, byte[] hash, string salt)
        {
            string base64Hash = Convert.ToBase64String(hash);
            string base64AttemptedHash = Convert.ToBase64String(GenerateHash(attemptedPassword, salt));

            return base64Hash == base64AttemptedHash;
        }

        public String CreateSalt(int size)
        {
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        public byte[] GenerateHash(String password, String salt)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
            System.Security.Cryptography.SHA256Managed sha256hashstring = new System.Security.Cryptography.SHA256Managed();
            byte[] hash = sha256hashstring.ComputeHash(bytes);

            return hash;
        }
    }
}
