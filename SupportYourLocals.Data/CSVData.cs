using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MapControl;
using Microsoft.VisualBasic.FileIO;
using SupportYourLocals.ExtensionMethods;

namespace SupportYourLocals.Data
{
    public class CSVData
    {
        private static string filePath = @"./Data.csv";
        private static int personsID = 1000;

        public static void SaveData(String product, Location position)
        {
            var csv = new StringBuilder();

            personsID = GetPersonsID(personsID);

            var newLine = "{0},{1},{2}".Format(product, position, personsID);

            csv.AppendLine(newLine);

            File.AppendAllText(filePath, csv.ToString());
        }

        public static int GetPersonsID(int personsID)
        {
            // Checking person's ID
            List<double> personID = new List<double>();
            using (TextFieldParser csvParser = new TextFieldParser(filePath))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    personID.Add(Double.Parse(fields[4]));
                }
            }

            // Setting person's ID
            personsID = personsID + personID.Count;
            return personsID;
        }

        public static void SetMarkers(List<double> listXCoord, List<double> listYCoord, List<int> listPersonsID)
        {
            using (TextFieldParser csvParser = new TextFieldParser(filePath))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Saving data to a list
                    string[] fields = csvParser.ReadFields();
                    listXCoord.Add(Double.Parse(fields[2]));
                    listYCoord.Add(Double.Parse(fields[3]));
                    listPersonsID.Add(Int32.Parse(fields[4]));
                }
            }
        }
    }
}
