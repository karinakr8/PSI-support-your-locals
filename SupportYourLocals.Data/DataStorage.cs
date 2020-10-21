using System;
using System.Collections.Generic;
using MapControl;

namespace SupportYourLocals.Data
{
    public class LocationData
    {
        public int id;
        public Location location;
        public string name;
        public int addedByID;
        public DateTime time;
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
    }
}
