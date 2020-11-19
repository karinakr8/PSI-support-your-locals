using System;
using System.Collections.Generic;

namespace SupportYourLocals.Data
{
    public interface IDataStorage
    {
        public SellerData GetSellerData(string id);
        public List<SellerData> GetAllSellerData();
        public int GetSellerDataCount();
        public void AddSellerData(SellerData data);
        public void AddSellerData(List<SellerData> dataList) => dataList.ForEach(p => AddSellerData(p));
        public void UpdateSellerData(SellerData data);
        public void RemoveSellerData(string id);

        // Remove these NotImplementedException calls once XMLData is updated to use these methods
        public MarketplaceData GetMarketData(string id) => throw new NotImplementedException();
        public List<MarketplaceData> GetAllMarketData() => throw new NotImplementedException();
        public int GetMarketDataCount() => throw new NotImplementedException();
        public void AddMarketData(MarketplaceData data) => throw new NotImplementedException();
        public void AddMarketData(List<MarketplaceData> dataList) => dataList.ForEach(p => AddMarketData(p));
        public void UpdateMarketData(MarketplaceData data) => throw new NotImplementedException();
        public void RemoveMarketData(string id) => throw new NotImplementedException();

        public void SaveData();
    }
}
