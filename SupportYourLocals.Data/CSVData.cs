using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MapControl;
using Microsoft.VisualBasic.FileIO;
using SupportYourLocals.ExtensionMethods;

namespace SupportYourLocals.Data
{
    public class CSVData : IDataStorage
    {
        private string filePath = @"./Data.csv";

        public LocationData GetData(int id)
        {
            throw new NotImplementedException();
        }

        public List<LocationData> GetAllData()
        {
            List<LocationData> listLocationData = new List<LocationData>();            

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
                        LocationData locationData = new LocationData();

                        string[] fields = csvParser.ReadFields();

                        // Saving one marker data
                        locationData.Location = new Location(double.Parse(fields[1]), double.Parse(fields[2]));
                        locationData.ID = int.Parse(fields[3]);
                        //locationData.Name = ;
                        //locationData.AddedByID = ;
                        locationData.Time = fields[4];
                        //locationData.Products.Add();

                        // Saving data to a list
                        listLocationData.Add(locationData);
                    }
                }
            }
            
            return listLocationData;
        }

        public int GetDataCount()
        {
            throw new NotImplementedException();
        }

        public void AddData(LocationData data)
        {
            throw new NotImplementedException();
        }

        public void UpdateData(int id, LocationData data)
        {
            throw new NotImplementedException();
        }

        public void RemoveData(int id)
        {
            throw new NotImplementedException();
        }

        public void SaveData(String product, Location position)
        {
            int locationID = 1000;

            List<int> listLocationID = new List<int>();
            
            StringBuilder csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                csv.AppendLine("Something,xCoord,yCoord,ID,Time");
            }
            // Checking location's ID -----------------------------------------------
            else
            {
                LocationData locationData = new LocationData();
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
                        listLocationID.Add(Int32.Parse(fields[3]));
                    }
                }
            }
            //----------------------------------------------------------------------

            // Setting person's ID
            locationID += listLocationID.Count;

            string currentTime = DateTime.Now.ToString("H:mm:ss");

            var newLine = "{0},{1},{2},{3}".Format(product, position, locationID, currentTime);
            csv.AppendLine(newLine);
            File.AppendAllText(filePath, csv.ToString());
        }
    }
}
