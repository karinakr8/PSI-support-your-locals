using System;
using MapControl;
using SupportYourLocals.ExtensionMethods;

namespace SupportYourLocals.Data
{
    public class LocationData : GenericData
    {
        public Location Location { get; set; }

        public LocationData() { }

        public LocationData(Location location, string name, string id = null) : base(name, id)
        {
            Location = location;
        }
    }
}
