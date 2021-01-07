using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SupportYourLocals.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using Nominatim.API.Models;
using Nominatim.API.Geocoders;

namespace SupportYourLocals.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapUtilityController : ControllerBase
    {
        private readonly IDataStorage<SellerData> sellerStorage;
        private readonly IDataStorage<MarketplaceData> marketStorage;

        public MapUtilityController(IDataStorage<SellerData> storageSeller, IDataStorage<MarketplaceData> storageMarket)
        {
            sellerStorage = storageSeller;
            marketStorage = storageMarket;
        }

        private static double GetDistance(double latitude, double longitude, double latitude2, double longitude2)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = latitude2 * (Math.PI / 180.0);
            var num2 = longitude2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))); // Distance is in meters
        }

        private async Task<GeocodeResponse> LocationToAddressInternal(double latitude, double longitude)
        {
            var geocoder = new ReverseGeocoder();
            var request = await geocoder.ReverseGeocode(new ReverseGeocodeRequest
            {
                Latitude = latitude,
                Longitude = longitude,
                BreakdownAddressElements = true
            });

            if (request.PlaceID == 0)
                return null;

            return request;
        }

        [HttpGet]
        [Route("/api/[controller]/getSellersWithinRange")]
        public async Task<List<SellerInfo>> GetSellersWithinRange(double latitude, double longitude, double range, string query = "")
        {
            var allSellers = await sellerStorage.GetAllData();
            var selectedSellers = new List<SellerInfo>();
            var queryList = query.Split(",").ToList();

            foreach (var seller in allSellers)
            {
                if (GetDistance(latitude, longitude, seller.Location.Latitude, seller.Location.Longitude) > range)
                {
                    continue;
                }

                if (queryList.Count == 1 && queryList[0] == "")
                {
                    selectedSellers.Add(new SellerInfo (seller, 1.0));
                    continue;
                }

                // Go over all product lists and see if there are any matching products
                int intersectionCount = 0;
                foreach (var productList in seller.Products.Values)
                {
                    var intersection = productList.Intersect(queryList);
                    intersectionCount += intersection.Count();
                }

                if (intersectionCount > 0)
                {
                    selectedSellers.Add(new SellerInfo (seller, (double)intersectionCount / queryList.Count));
                }
            }

            return selectedSellers;
        }

        [HttpGet]
        [Route("/api/[controller]/getSellersInMarketplace")]
        public async Task<ActionResult<List<SellerData>>> GetSellersInMarketplace(string id)
        {
            var allSellers = await sellerStorage.GetAllData();
            MarketplaceData market;
            try
            {
                market = await marketStorage.GetData(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            var marketBoundary = market.MarketBoundary;
            var selectedSellers = new List<SellerData>();

            foreach (var seller in allSellers)
            {
                if (marketBoundary.IsWithinBoundary(seller.Location))
                {
                    selectedSellers.Add(seller);
                }
            }

            return selectedSellers;
        }

        [HttpGet]
        [Route("/api/[controller]/locationToAddress")]
        public async Task<ActionResult<Tuple<string, string>>> LocationToAddress(double latitude, double longitude)
        {
            // Return value 1 - address, 2 - city/district
            var result = await LocationToAddressInternal(latitude, longitude);

            if (result == null)
            {
                return NotFound();
            }

            string city = result.Address.District;

            if (result.Address.City != null)
            {
                city = result.Address.City;
            }

            int index = result.DisplayName.IndexOf(city);
            if (index > 2)
            {
                index -= 2;
            }

            // Return the address part without the trailing ", " and the city/district
            return new Tuple<string, string>(result.DisplayName.Substring(0, index), city);
        }

        [HttpGet]
        [Route("/api/[controller]/addressToLocation")]
        public async Task<Tuple<double, double>> AddressToLocation(string address)
        {
            var geocoder = new ForwardGeocoder();
            var request = await geocoder.Geocode(new ForwardGeocodeRequest
            {
                queryString = address
            });

            if (request.Length < 1)
            {
                return null;
            }

            return new Tuple<double, double> (request[0].Latitude, request[0].Longitude);
        }
    }
}
