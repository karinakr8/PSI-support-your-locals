using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportYourLocals.Data
{
    public interface IDataStorage <T>
    {
        public Task<T> GetData(string id);
        public Task<List<T>> GetAllData();
        public Task<int> GetDataCount();
        public Task AddData(T data);
        public Task UpdateData(T data);
        public Task RemoveData(string id);
        public Task SaveData();

        public async Task AddData(List<T> dataList)
        {
            var tasks = dataList.Select(item => AddData(item));
            await Task.WhenAll(tasks);
        }
    }

    public interface ISellerStorage : IDataStorage<SellerData> { }

    public interface IMarketStorage : IDataStorage<MarketplaceData> { }

    public interface IUserStorage : IDataStorage<UserData> { }
}
