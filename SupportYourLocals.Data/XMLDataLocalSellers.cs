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
    public class XMLDataLocalSellers : ISellerStorage
    {
        private const string filePath = @"./LocalSellersData.xml";
        private readonly Dictionary<string, SellerData> dictionaryLocationDataById;
        // TODO: Add a lock object once we start working on files asynchronously

        public XMLDataLocalSellers()
        {
            dictionaryLocationDataById = LoadData();
        }

        private Dictionary<string, SellerData> LoadData()
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, SellerData>();
            }

            XDocument doc = XDocument.Load(filePath);
            var localSellersDictionary = new Dictionary<string, SellerData>();
            var groupElements = from elements in doc.Descendants().Elements("LocalSeller") select elements;

            foreach(XElement element in groupElements)
            {
                var dictionary = new Dictionary<ProductType, List<string>>();
                var id = element.Attribute("ID").Value;
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
                localSellersDictionary.Add(id, new SellerData(products: dictionary, addedByID: addedById, name: name, id: id, location: location, time: time));
            }
            return localSellersDictionary;
        }

        public void SaveData()
        {
            XDocument doc = new XDocument(new XElement("LocalSellers"));
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
            }
            doc.Save(filePath);
        }

        private void AddProductTypesToXml(SellerData data, XElement root)
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
                XElement productTypeBranch = new XElement("ProductType");
                productTypeBranch.Add(new XAttribute("type", productType.Key));
                foreach (var product in productType.Value)
                {
                    if (product == "")
                    {
                        continue;
                    }
                    XElement productBranch = new XElement("Product");
                    productBranch.Add(content: product);
                    productTypeBranch.Add(productBranch);
                }
                root.Add(productTypeBranch);
            }
        }
        SellerData IDataStorage<SellerData>.GetData(string id) => dictionaryLocationDataById[id];

        List<SellerData> IDataStorage<SellerData>.GetAllData() => dictionaryLocationDataById.Select(d => d.Value).ToList();

        int IDataStorage<SellerData>.GetDataCount() => dictionaryLocationDataById.Count;
        
        void IDataStorage<SellerData>.AddData(SellerData data) => dictionaryLocationDataById.Add(data.ID, data);

        void IDataStorage<SellerData>.UpdateData(SellerData data) => dictionaryLocationDataById[data.ID] = data;

        void IDataStorage<SellerData>.RemoveData(string id) => dictionaryLocationDataById.Remove(id);
    }
}
