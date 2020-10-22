using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MapControl;
using Microsoft.VisualBasic.FileIO;


namespace SupportYourLocals.Data
{
    public enum ProductType
    {
        vegetables,
        fruits,
        berries,
        mushrooms,
        meat,
        tools,
        clothing,
        shoes,
        flowers,
        art,
        other
    }

    public class LocationData
    {
        public int id;
        public Location location;
        public string name;
        public int addedByID;
        public DateTime time;
        public Dictionary<ProductType, string> products;

        private static string filePath = @"./Data.csv";
        private static int personsID = 1000;

        public static void SaveData(String product, String time, Location position)
        {            
            var csv = new StringBuilder();

            personsID = GetPersonsID(personsID);

            var newLine = string.Format("{0},{1},{2},{3}", product, time, position, personsID);

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

    public interface IDataStorage
    {
        public LocationData GetData(int id);
        public List<LocationData> GetAllData();
        public void AddData(LocationData data);
        public void AddDataList(List<LocationData> dataList)
        {
            foreach (var data in dataList)
                AddData(data);
        }
        public void UpdateData(int id, LocationData data);
        public void RemoveData(int id);
    }
}
