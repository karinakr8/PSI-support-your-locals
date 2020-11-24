using System;
using MapControl;
using SupportYourLocals.ExtensionMethods;

namespace SupportYourLocals.Data
{
    public class LocationData : IComparable<LocationData>
    {
        public string ID { get; set; }
        public Location Location { get; set; }
        public string Name { get; set; }

        public LocationData(Location location, string name, string id = null)
        {
            ID = id ?? GenerateId;
            Location = location;
            Name = name;
        }

        public static string GenerateId => Guid.NewGuid().ToString("N");

        public int CompareTo(LocationData obj)
        {
            return Name.Compare(obj.Name);
        }
    }
}
