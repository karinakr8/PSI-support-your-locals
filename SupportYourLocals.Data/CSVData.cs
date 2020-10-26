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
            StringBuilder csv = new StringBuilder();

            if(!File.Exists(filePath))
            {                
                csv.AppendLine("Something,xCoord,yCoord,ID,Time");                                          
            }      
            
            personsID = GetPersonsID(personsID);

            string currentTime = DateTime.Now.ToString("H:mm:ss");

            var newLine = "{0},{1},{2},{3}".Format(product, position, personsID, currentTime);
            csv.AppendLine(newLine);
            File.AppendAllText(filePath, csv.ToString());                   
        }

        public static int GetPersonsID(int personsID)
        {
            // Checking person's ID
            List<double> personID = new List<double>();

            if(File.Exists(filePath))
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
                        personID.Add(double.Parse(fields[3]));
                    }
                }
            }            

            // Setting person's ID
            personsID += personID.Count;
            return personsID;
        }

        public static void SetMarkers(List<double> listXCoord, List<double> listYCoord, List<int> listPersonsID)
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
                    // Saving data to a list
                    string[] fields = csvParser.ReadFields();
                    listXCoord.Add(double.Parse(fields[1]));
                    listYCoord.Add(double.Parse(fields[2]));
                    listPersonsID.Add(int.Parse(fields[3]));
                }
            }
        }
    }
}
