using MapControl;
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
        readonly XDocument doc = null;
        readonly XmlDocument document = new XmlDocument();
        Dictionary<int, LocationData> dictionaryLocationDataById = new Dictionary<int, LocationData>();

        public XMLData()
        {
            //Data storage to dictionaryLocationDataById for easier data access in methods in this class such as GetData or GetAllData
            var tempListOfLocationData = GetAllData();
            foreach (var location in tempListOfLocationData)
            {
                dictionaryLocationDataById.Add(location.ID, location);
            }

            if (!File.Exists(filePath))
            {
                doc = new XDocument(new XElement("LocalSellers"));
                doc.Save(fileName);
            }
            else
            {
                doc = XDocument.Load(filePath);
            }
        }

        ~XMLData()
        {
            SaveData();
        }

        public void AddData(LocationData data)
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

            dictionaryLocationDataById.Add(data.ID, data);
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

        public List<LocationData> GetAllData()
        {
            var localSellers = new List<LocationData>();
            var dictionary = new Dictionary<ProductType, List<string>>();

            XmlNodeList localSellerNode = document.GetElementsByTagName("LocalSeller");
            foreach (XmlNode localSeller in localSellerNode)
             {
                var id = int.Parse(localSeller.Attributes[0].Value);
                var location = Location.Parse(localSeller.Attributes[1].Value);
                var name = localSeller.Attributes[2].Value;
                var addedById = int.Parse(localSeller.Attributes[3].Value);
                var time = DateTime.Parse(localSeller.Attributes[4].Value);
                XmlNodeList productTypeNode = localSeller.ChildNodes;
                foreach (XmlNode productType in productTypeNode)
                {
                    ProductType productTypeEnum = (ProductType)Enum.Parse(typeof(ProductType), productType.Attributes[0].Value);
                    XmlNodeList productNode = productType.ChildNodes;

                    var productsList = new List<string>();
                    foreach (XmlNode product in productNode)
                    {
                        productsList.Add(product.InnerText);
                    }
                    dictionary.Add(productTypeEnum, productsList);
                }
                localSellers.Add(new LocationData(id, location, name, addedById, time, dictionary));
            }
            return localSellers;
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
            document.Load(fileName);
            XmlNodeList localSellerNode = document.GetElementsByTagName("LocalSeller");
            foreach (XmlNode localSeller in localSellerNode)
            {
                if(int.Parse(localSeller.Attributes[0].Value) == id)
                {
                    localSeller.ParentNode.RemoveChild(localSeller);
                    break;
                }
            }
            document.Save(fileName);

            dictionaryLocationDataById.Remove(id);
        }

        public void SaveData()
        {
            doc.Save(filePath);
        }

        public void UpdateData(int id, LocationData data)
        {
            RemoveData(id);
            AddData(data);
        }
    }
}
