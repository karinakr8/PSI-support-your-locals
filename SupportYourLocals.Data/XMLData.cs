﻿using MapControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SupportYourLocals.Data
{
    public class XMLData : IDataStorage
    {
        private const string filePath = @"./LocalSellersData.xml";
        private const string fileName = "LocalSellersData.xml";
        XDocument doc = null;
        readonly XmlDocument document = new XmlDocument();
        Dictionary<int, LocationData> dictionaryLocationDataById = new Dictionary<int, LocationData>();

        public XMLData()
        {


            if (!File.Exists(filePath))
            {
                //document.AppendChild(CreateElement("LocalSellers"));
                doc = new XDocument(new XElement("LocalSellers"));
                doc.Save(fileName);
            }
            else
            {
                doc = XDocument.Load(filePath);
            }
            //Data storage to dictionaryLocationDataById for easier data access in methods in this class such as GetData or GetAllData
            dictionaryLocationDataById = LoadData();

        }

        ~XMLData()
        {
            SaveData();
        }

        private Dictionary<int, LocationData> LoadData()
        {
            var localSellersDictionary = new Dictionary<int, LocationData>();
            

            var groupElements = from elements in doc.Descendants().Elements("LocalSeller") select elements;

            foreach(XElement element in groupElements)
            {
                var dictionary = new Dictionary<ProductType, List<string>>();
                var id = int.Parse(element.Attribute("ID").Value);
                var location = Location.Parse(element.Attribute("Location").Value);
                var name = element.Attribute("Name").Value;
                var addedById = int.Parse(element.Attribute("AddedByID").Value);
                var time = DateTime.Parse(element.Attribute("Time").Value);
                var productTypeList = from productTypes in element.Elements("ProductType") select productTypes;
                foreach (XElement productType in productTypeList)
                {
                    ProductType productTypeEnum = (ProductType)Enum.Parse(typeof(ProductType), productType.Attribute("type").Value);
                    var productNode = from products in productType.Elements("Product") select products;

                    var productsList = new List<string>();
                    foreach (XElement product in productNode)
                    {
                        productsList.Add(product.Value);
                    }
                    dictionary.Add(productTypeEnum, productsList);
                }
                localSellersDictionary.Add(id, new LocationData(id, location, name, addedById, time, dictionary));
                
            }
            return localSellersDictionary;
        }

        public void SaveData()
        {
            doc = new XDocument(new XElement("LocalSellers"));
            foreach (var data in dictionaryLocationDataById.Values)
            {
                XElement root = new XElement("LocalSeller");
                root.Add(new XAttribute("ID", data.ID));
                root.Add(new XAttribute("Location", data.Location));
                root.Add(new XAttribute("Name", data.Name));
                root.Add(new XAttribute("AddedByID", data.AddedByID));
                root.Add(new XAttribute("Time", data.Time));
                AddProductTypesToXml(data, root);
                doc.Element("LocalSellers").Add(root);
                doc.Save(filePath);
            }
        }

        private void AddProductTypesToXml(LocationData data, XElement root)
        {
            foreach (var productType in data.Products)
            {
                if (productType.Value[0] != "")
                {
                    XElement productTypeBranch = new XElement("ProductType");
                    productTypeBranch.Add(new XAttribute("type", productType.Key));
                    foreach (var product in productType.Value)
                    {
                        if (product != "")
                        {
                            XElement productBranch = new XElement("Product");
                            productBranch.Add(content: product);
                            productTypeBranch.Add(productBranch);
                        }
                    }
                    root.Add(productTypeBranch);
                }
            }
        }
        public void AddData(LocationData data)
        {
            dictionaryLocationDataById.Add(data.ID, data);
        }


        public List<LocationData> GetAllData()
        {
            return dictionaryLocationDataById.Select(d => d.Value).ToList();
        }

        public LocationData GetData(int id)
        {
            return dictionaryLocationDataById[id];
        }

        public int GetDataCount()
        {
            return dictionaryLocationDataById.Count;
        }

        public void RemoveData(int id)
        {
            dictionaryLocationDataById.Remove(id);
        }

        public void UpdateData(int id, LocationData data)
        {
            RemoveData(id);
            AddData(data);
        }
    }
}
