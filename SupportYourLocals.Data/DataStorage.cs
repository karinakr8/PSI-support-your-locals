using System;
using System.Collections.Generic;
using System.Linq;
using MapControl;
using SupportYourLocals.ExtensionMethods;

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

    public class LocationData : IComparable<LocationData>, IEquatable<LocationData>
    {
        public string ID { get; set; }
        public Location Location { get; set; }
        public string Name { get; set; }
        public int AddedByID { get; set; }
        public DateTime Time { get; set; }
        public Dictionary<ProductType, List<string> > Products { get; set; }

        public LocationData(Location location, string name, int addedByID, DateTime time, Dictionary<ProductType, List<string>> products, string id = null)
        {
            ID = id ?? GenerateId;
            Location = location;
            Name = name;
            AddedByID = addedByID;
            Time = time;
            Products = products;
        }

        public static string GenerateId => Guid.NewGuid().ToString("N");

        public int CompareTo(LocationData obj)
        {
            return Name.Compare(obj.Name);
        }

        public bool Equals (LocationData obj)
        {
            bool productsEqual = true;
            // See if the products dictionaries are equal
            foreach (var key in Products.Keys)
            {
                if (!obj.Products.ContainsKey(key))
                {
                    productsEqual = false;
                    break;
                }

                if (Products[key].Except(obj.Products[key]).Count() > 0)
                {
                    productsEqual = false;
                    break;
                }
            }

            return ID == obj.ID &&
                   Location == obj.Location &&
                   Name == obj.Name &&
                   AddedByID == obj.AddedByID &&
                   Time == obj.Time &&
                   productsEqual;
        }
    }

    public interface IDataStorage
    {
        public LocationData GetData(string id);
        public List<LocationData> GetAllData();
        public int GetDataCount();
        public void AddData(LocationData data);
        public void AddDataList(List<LocationData> dataList)
        {
            foreach (var data in dataList)
                AddData(data);
        }
        public void UpdateData(LocationData data);
        public void RemoveData(string id);
        public void SaveData();
    }
}
