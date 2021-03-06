﻿using MapControl;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SupportYourLocals.Data
{
    public class LocalSellersData : ISellerStorage
    {
        private readonly Dictionary<string, SellerData> dictionaryLocationDataById;
        private string connectionString = ConfigurationManager.ConnectionStrings["cs"].ConnectionString;

        public LocalSellersData()
        {
            dictionaryLocationDataById = LoadData();
        }
        private Dictionary<string, SellerData> LoadData()
        {
            var localSellersDictionary = new Dictionary<string, SellerData>();
            using (var con = new MySqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = $"SELECT id, location, lname, addedById, ltime FROM localsellers;";
                    using MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var productsList = new List<string>();
                        var dictionary = new Dictionary<ProductType, List<string>>();
                        var id = reader.GetString(0);
                        var location = Location.Parse(reader.GetString(1));
                        var name = reader.GetString(2);
                        var addedById = int.Parse(reader.GetString(3));
                        var time = DateTime.Parse(reader.GetString(4));

                        var type = new List<string>();
                        TakeProductTypes(id, type);
                        foreach(string productType in type)
                        {
                            ProductType productTypeEnum = (ProductType)Enum.Parse(typeof(ProductType), productType);
                            TakeProducts(id, dictionary, productsList, productTypeEnum);
                        }                        
                        localSellersDictionary.Add(id, new SellerData(products: dictionary, addedByID: addedById, name: name, id: id, location: location, time: time));
                    }                  
                }

                con.Close();
            }
            return localSellersDictionary;
        }
        public void TakeProductTypes(string id, List<string> type)
        {
            using (var con = new MySqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"SELECT ptype FROM producttype WHERE id = \"" + id + "\";";
                    using MySqlDataReader reader1 = cmd.ExecuteReader();

                    while (reader1.Read())
                    {
                        type.Add(reader1.GetString(0));
                    }
                }
                con.Close();
            }
        }

        public void TakeProducts(string id, Dictionary<ProductType, List<string>> dictionary, List<string> productsList, ProductType productTypeEnum)
        {
            using (var con = new MySqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"SELECT product FROM product WHERE id = \"" + id + "\" AND ptype = \"" + productTypeEnum + "\";";
                    using MySqlDataReader reader2 = cmd.ExecuteReader();

                    while (reader2.Read())
                    {
                        productsList.Add(reader2.GetString(0));
                    }
                    dictionary.Add(productTypeEnum, productsList);
                }
                con.Close();
            }
        }

        public Task SaveData()
        {
            using (var con = new MySqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = "TRUNCATE localsellers";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "TRUNCATE producttype";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "TRUNCATE product";
                    cmd.ExecuteNonQuery();

                    foreach (var data in dictionaryLocationDataById.Values)
                    {
                        cmd.CommandText = "INSERT INTO localsellers(id, location, lname, addedById, ltime) VALUES(\"" + data.ID + "\", \"" + data.Location + "\", \"" + data.Name + "\", \"" + data.AddedByID + "\", \"" + data.Time + "\");";
                        cmd.ExecuteNonQuery();

                        AddProductTypesToDB(data, cmd);                        
                    }
                }

                con.Close();
            }

            return Task.CompletedTask;
        }

        private void AddProductTypesToDB(SellerData data, MySqlCommand cmd)
        {
            foreach (var productType in data.Products)
            {
                if(productType.Value.Count == 0)
                {
                    continue;
                }
                if (productType.Value[0] == "")
                {
                    continue;
                }

                cmd.CommandText = "INSERT INTO producttype(id, ptype) VALUES(\"" + data.ID + "\", \"" + productType.Key + "\");";
                cmd.ExecuteNonQuery();
                
                foreach (var product in productType.Value)
                {
                    if (product == "")
                    {
                        continue;
                    }
                    cmd.CommandText = "INSERT INTO product(id, ptype, product) VALUES(\"" + data.ID + "\", \"" + productType.Key + "\", \"" + product + "\");";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public Task<SellerData> GetData(string id) => Task.FromResult(dictionaryLocationDataById[id]);

        public Task<List<SellerData>> GetAllData() => Task.FromResult(dictionaryLocationDataById.Values.ToList());

        public Task<int> GetDataCount() => Task.FromResult(dictionaryLocationDataById.Count);

        public Task AddData(SellerData data)
        {
            dictionaryLocationDataById.Add(data.ID, data);
            return Task.CompletedTask;
        }

        public Task UpdateData(SellerData data)
        {
            dictionaryLocationDataById[data.ID] = data;
            return Task.CompletedTask;
        }

        public Task RemoveData(string id)
        {
            dictionaryLocationDataById.Remove(id);
            return Task.CompletedTask;
        }
    }
}
