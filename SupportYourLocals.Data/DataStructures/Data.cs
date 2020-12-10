using System;
using SupportYourLocals.ExtensionMethods;

namespace SupportYourLocals.Data
{
    public class GenericData : IComparable<GenericData>
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public GenericData() { }

        public GenericData(string name, string id = null)
        {
            Name = name;
            ID = id ?? GenerateId;
        }

        private static string GenerateId => Guid.NewGuid().ToString("N");

        public int CompareTo(GenericData obj)
        {
            return Name.Compare(obj.Name);
        }
    }
}
