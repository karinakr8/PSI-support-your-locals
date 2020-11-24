using System;
using System.Collections.Generic;

namespace SupportYourLocals.Data
{
    public interface IDataStorage <T>
    {
        public T GetData(string id);
        public List<T> GetAllData();
        public int GetDataCount();
        public void AddData(T data);
        public void AddData(List<T> dataList) => dataList.ForEach(p => AddData(p));
        public void UpdateData(T data);
        public void RemoveData(string id);
        public void SaveData();
    }

    public interface ISellerStorage : IDataStorage<SellerData> { }

    public interface IMarketStorage : IDataStorage<MarketplaceData> { }
}
