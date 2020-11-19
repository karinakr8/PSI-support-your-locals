using System;
using System.Collections.Generic;
using System.Linq;
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

    public class SellerData : LocationData, IEquatable<SellerData>
    {
        public int AddedByID { get; set; }
        public DateTime Time { get; set; }
        public Dictionary<ProductType, List<string> > Products { get; set; }

        public SellerData(Location location, string name, int addedByID, DateTime time, Dictionary<ProductType, List<string>> products, string id = null) : base (location, name, id)
        {
            AddedByID = addedByID;
            Time = time;
            Products = products;
        }

        public bool Equals (SellerData obj)
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
}
