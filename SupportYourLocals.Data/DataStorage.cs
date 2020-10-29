using System;
using System.Collections.Generic;
using MapControl;

namespace SupportYourLocals.Data
{
    public enum ProductType
    {
        Vegetables,
        Fruits,
        Berries,
        Mushrooms,
        Meat,
        Tools,
        Clothing,
        Shoes,
        Flowers,
        Art,
        Other
    }

    public class LocationData
    {
        public int ID { get; set; }
        public Location Location { get; set; }
        public string Name { get; set; }
        public int AddedByID { get; set; }
        public DateTime Time { get; set; }
        public Dictionary<ProductType, List<string> > Products { get; set; }

        public LocationData(int id, Location location, string name, int addedByID, DateTime time, Dictionary<ProductType, List<string>> products)
        {
            ID = id;
            Location = location;
            Name = name;
            AddedByID = addedByID;
            Time = time;
            Products = products;
        }
    }

    public interface IDataStorage
    {
        public LocationData GetData(int id);
        public List<LocationData> GetAllData();
        public int GetDataCount();
        public void AddData(LocationData data);
        public void AddDataList(List<LocationData> dataList)
        {
            foreach (var data in dataList)
                AddData(data);
        }
        public void UpdateData(int id, LocationData data);
        public void RemoveData(int id);
        public void SaveData();
    }
}
