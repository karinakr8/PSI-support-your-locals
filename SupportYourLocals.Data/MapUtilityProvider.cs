using System;
using System.Collections.Generic;
using System.Linq;
using SupportYourLocals.ExtensionMethods;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SupportYourLocals.Data
{
    public class MapUtilityProvider
    {
        private static readonly string apiAddress = "https://localhost:44311/api/"; // TODO: Move this to config
        private static JsonSerializerOptions serializerOptions;
        private static HttpClient client; // TODO: Inject this instead of creating.

        public MapUtilityProvider()
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

        public static async Task<List<SellerInfo>> GetSellersWithinRange(double latitude, double longitude, double range, string query = "")
        {
            List<SellerInfo> data = null;
            string fullPath = "{0}MapUtility/getSellersWithinRange?latitude={1}&longitude={2}&range={3}".Format(apiAddress, latitude, longitude, range);
            if (query.Trim() != "")
            {
                fullPath += "&query={0}".Format(query.Trim().Replace(" ", "%20"));
            }
            HttpResponseMessage response = await client.GetAsync(fullPath);

            if (response.IsSuccessStatusCode)
            {
                var dataString = await response.Content.ReadAsStringAsync();
                data = JsonSerializer.Deserialize<List<SellerInfo>> (dataString, serializerOptions);
            }

            return data;
        }

        public static async Task<List<SellerData>> GetSellersInMarketplace(string id)
        {
            List<SellerData> data = null;
            string fullPath = "{0}MapUtility/getSellersInMarketplace?id={1}".Format(apiAddress, id);

            HttpResponseMessage response = await client.GetAsync(fullPath);

            if (response.IsSuccessStatusCode)
            {
                var dataString = await response.Content.ReadAsStringAsync();
                data = JsonSerializer.Deserialize<List<SellerData>>(dataString, serializerOptions);
            }

            return data;
        }

        public static async Task<Tuple<string, string>> LocationToAddress(double latitude, double longitude)
        {
            Tuple<string, string> data = null;
            string fullPath = "{0}MapUtility/locationToAddress?latitude={1}&longitude={2}".Format(apiAddress, latitude, longitude);

            HttpResponseMessage response = await client.GetAsync(fullPath);

            if (response.IsSuccessStatusCode)
            {
                var dataString = await response.Content.ReadAsStringAsync();
                data = JsonSerializer.Deserialize<Tuple<string, string>>(dataString, serializerOptions);
            }

            return data;
        }

        public static async Task<Tuple<double, double>> AddressToLocation(string address)
        {
            Tuple<double, double> data = null;
            string fullPath = "{0}MapUtility/addressToLocation?address={1}".Format(apiAddress, address.Trim().Replace(" ", "%20"));

            HttpResponseMessage response = await client.GetAsync(fullPath);

            if (response.IsSuccessStatusCode)
            {
                var dataString = await response.Content.ReadAsStringAsync();
                data = JsonSerializer.Deserialize<Tuple<double, double>>(dataString, serializerOptions);
            }

            return data;
        }
    }
}
