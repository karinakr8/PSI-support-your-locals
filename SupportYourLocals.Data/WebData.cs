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
        private static readonly string apiAddress = "https://localhost:44311/api/";
        private static JsonSerializerOptions serializerOptions;
        private static HttpClient client;

        public WebData()
        {
            if (client == null)
            {
                client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
            }

            if (serializerOptions == null)
            {
                serializerOptions = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
            }    
        }

        public void SaveData()
        {
            // Data is sent and retrieved immediately, no use for this function
        }

        async Task<T> GetDataAsync<T>(string path, string id) where T : GenericData
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

        async Task<List<T>> GetAllDataAsync<T>(string path) where T : GenericData
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

        async Task<int> GetDataCountAsync(string path)
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

        async Task<Uri> SetDataAsync<T>(string path, T data) where T : GenericData
        {
            var json = JsonSerializer.Serialize(data);
            var dataHttp = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiAddress + path, dataHttp).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        async Task<Uri> UpdateDataAsync<T>(string path, T data) where T : GenericData
        {
            var json = JsonSerializer.Serialize(data);
            var dataHttp = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(apiAddress + path, dataHttp).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        async Task<HttpStatusCode> RemoveDataAsync(string path, string id)
        {
            HttpResponseMessage response = await client.DeleteAsync($"{apiAddress}{path}/{id}").ConfigureAwait(false);
            return response.StatusCode;
        }


        UserData IDataStorage<UserData>.GetData(string id) => GetDataAsync<UserData>("UserData", id).Result;

        List<UserData> IDataStorage<UserData>.GetAllData() => GetAllDataAsync<UserData>("UserData").Result;

        int IDataStorage<UserData>.GetDataCount() => GetDataCountAsync("UserData").Result;

        void IDataStorage<UserData>.AddData(UserData data) => SetDataAsync("UserData", data);

        void IDataStorage<UserData>.UpdateData(UserData data) => UpdateDataAsync("UserData", data);

        void IDataStorage<UserData>.RemoveData(string id) => RemoveDataAsync("UserData", id);


        SellerData IDataStorage<SellerData>.GetData(string id) => GetDataAsync<SellerData>("SellerData", id).Result;

        List<SellerData> IDataStorage<SellerData>.GetAllData() => GetAllDataAsync<SellerData>("SellerData").Result;

        int IDataStorage<SellerData>.GetDataCount() => GetDataCountAsync("SellerData").Result;

        void IDataStorage<SellerData>.AddData(SellerData data) => SetDataAsync("SellerData", data);

        void IDataStorage<SellerData>.UpdateData(SellerData data) => UpdateDataAsync("SellerData", data);

        void IDataStorage<SellerData>.RemoveData(string id) => RemoveDataAsync("SellerData", id);


        MarketplaceData IDataStorage<MarketplaceData>.GetData(string id) => GetDataAsync<MarketplaceData>("MarketplaceData", id).Result;

        List<MarketplaceData> IDataStorage<MarketplaceData>.GetAllData() => GetAllDataAsync<MarketplaceData>("MarketplaceData").Result;

        int IDataStorage<MarketplaceData>.GetDataCount() => GetDataCountAsync("MarketplaceData").Result;

        void IDataStorage<MarketplaceData>.AddData(MarketplaceData data) => SetDataAsync("MarketplaceData", data);

        void IDataStorage<MarketplaceData>.UpdateData(MarketplaceData data) => UpdateDataAsync("MarketplaceData", data);

        void IDataStorage<MarketplaceData>.RemoveData(string id) => RemoveDataAsync("MarketplaceData", id);
    }
}
