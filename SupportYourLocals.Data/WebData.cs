using System.Collections.Generic;
using System.Text;
using SupportYourLocals.ExtensionMethods;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text.Json;

namespace SupportYourLocals.Data
{
    public class WebData : IUserStorage, ISellerStorage, IMarketStorage
    {
        private static readonly string apiAddress = "https://localhost:44311/api/"; // TODO: Move this to config
        private static JsonSerializerOptions serializerOptions;
        private static HttpClient client; // TODO: Inject this instead of creating.

        public WebData()
        {
            if (client == null)
            {
                client = new HttpClient();
            }

            if (serializerOptions == null)
            {
                serializerOptions = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
            }    
        }

        public Task SaveData()
        {
            // Data is sent and retrieved immediately, no use for this function
            return Task.CompletedTask;
        }

        static async Task<T> GetDataAsync<T>(string path, string id) where T : GenericData
        {
            T data = null;
            string fullPath = "{0}{1}/{2}".Format(apiAddress, path, id);
            HttpResponseMessage response = await client.GetAsync(fullPath).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var dataString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                data = JsonSerializer.Deserialize<T>(dataString, serializerOptions);
            }

            return data;
        }

        static async Task<List<T>> GetAllDataAsync<T>(string path) where T : GenericData
        {
            List<T> data = null;
            string fullPath = "{0}{1}".Format(apiAddress, path);
            HttpResponseMessage response = await client.GetAsync(fullPath).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var dataString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                data = JsonSerializer.Deserialize<List <T> >(dataString, serializerOptions);
            }

            return data;
        }

        static async Task<int> GetDataCountAsync(string path)
        {
            int data = 0;
            string fullPath = "{0}{1}/{2}".Format(apiAddress, path, 0);
            HttpResponseMessage response = await client.GetAsync(fullPath).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var dataString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                data = JsonSerializer.Deserialize<int>(dataString, serializerOptions);
            }

            return data;
        }

        static async Task<Uri> SetDataAsync<T>(string path, T data) where T : GenericData
        {
            var json = JsonSerializer.Serialize(data);
            var dataHttp = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiAddress + path, dataHttp).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        static async Task<Uri> UpdateDataAsync<T>(string path, T data) where T : GenericData
        {
            var json = JsonSerializer.Serialize(data);
            var dataHttp = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(apiAddress + path, dataHttp).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        static async Task<HttpStatusCode> RemoveDataAsync(string path, string id)
        {
            HttpResponseMessage response = await client.DeleteAsync($"{apiAddress}{path}/{id}").ConfigureAwait(false);
            return response.StatusCode;
        }


        async Task<UserData> IDataStorage<UserData>.GetData(string id) => await GetDataAsync<UserData>("UserData", id);

        async Task<List<UserData>> IDataStorage<UserData>.GetAllData() => await GetAllDataAsync<UserData>("UserData");

        async Task<int> IDataStorage<UserData>.GetDataCount() => await GetDataCountAsync("UserData");

        async Task IDataStorage<UserData>.AddData(UserData data) => await SetDataAsync("UserData", data);

        async Task IDataStorage<UserData>.UpdateData(UserData data) => await UpdateDataAsync("UserData", data);

        async Task IDataStorage<UserData>.RemoveData(string id) => await RemoveDataAsync("UserData", id);


        async Task<SellerData> IDataStorage<SellerData>.GetData(string id) => await GetDataAsync<SellerData>("SellerData", id);

        async Task<List<SellerData>> IDataStorage<SellerData>.GetAllData() => await GetAllDataAsync<SellerData>("SellerData");

        async Task<int> IDataStorage<SellerData>.GetDataCount() => await GetDataCountAsync("SellerData");

        async Task IDataStorage<SellerData>.AddData(SellerData data) => await SetDataAsync("SellerData", data);

        async Task IDataStorage<SellerData>.UpdateData(SellerData data) => await UpdateDataAsync("SellerData", data);

        async Task IDataStorage<SellerData>.RemoveData(string id) => await RemoveDataAsync("SellerData", id);


        async Task<MarketplaceData> IDataStorage<MarketplaceData>.GetData(string id) => await GetDataAsync<MarketplaceData>("MarketplaceData", id);

        async Task<List<MarketplaceData>> IDataStorage<MarketplaceData>.GetAllData() => await GetAllDataAsync<MarketplaceData>("MarketplaceData");

        async Task<int> IDataStorage<MarketplaceData>.GetDataCount() => await GetDataCountAsync("MarketplaceData");

        async Task IDataStorage<MarketplaceData>.AddData(MarketplaceData data) => await SetDataAsync("MarketplaceData", data);

        async Task IDataStorage<MarketplaceData>.UpdateData(MarketplaceData data) => await UpdateDataAsync("MarketplaceData", data);

        async Task IDataStorage<MarketplaceData>.RemoveData(string id) => await RemoveDataAsync("MarketplaceData", id);
    }
}
