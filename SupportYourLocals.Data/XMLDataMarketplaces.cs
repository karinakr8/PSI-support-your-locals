﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using MapControl;
using System.Configuration;
using System.Threading.Tasks;

namespace SupportYourLocals.Data
{
    public class XMLDataMarketplaces : IMarketStorage
    {
        private readonly string filePath = ConfigurationManager.AppSettings.Get("XMLDataMarketplacesFilePath");
        private readonly Dictionary<string, MarketplaceData> dictionaryMarketplaceData;

        public XMLDataMarketplaces()
        {
            dictionaryMarketplaceData = LoadData();
        }



        private void AddBoundaryToXml(MarketplaceData data, XElement root)
        {
            if (data.MarketBoundary == null)
            {
                return;
            }
            XElement boundaryBranch = new XElement("Boundary");
            foreach (var location in data.MarketBoundary)
            {
                XElement locationBranch = new XElement("Location");
                locationBranch.Add(content: location);
                boundaryBranch.Add(locationBranch);
            }
            root.Add(boundaryBranch);
        }

        private void AddTimeTableToXml(MarketplaceData data, XElement root)
        {
            if (data.Timetable == null)
            {
                return;
            }
            XElement boundaryBranch = new XElement("TimeTable");
            foreach (var weekDay in data.Timetable)
            {
                XElement weekDayBranch = new XElement("WeekDay");
                weekDayBranch.Add(new XAttribute("Day", weekDay.Key));
                foreach (var day in weekDay.Value)
                {
                    XElement dayBranch = new XElement("WorkingHours");
                    dayBranch.Add(new XAttribute("StartTime", $"{day.StartTime.Hours}:{day.StartTime.Minutes}"));
                    dayBranch.Add(new XAttribute("EndTime", $"{day.EndTime.Hours}:{day.EndTime.Minutes}"));
                    weekDayBranch.Add(dayBranch);
                }
                boundaryBranch.Add(weekDayBranch);
            }
            root.Add(boundaryBranch);
        }

        private Dictionary<string, MarketplaceData> LoadData()
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, MarketplaceData>();
            }

            XDocument doc = XDocument.Load(filePath);
            var marketplaceDictionary = new Dictionary<string, MarketplaceData>();

            var groupElements = from elements in doc.Descendants().Elements("Marketplace") select elements;
            foreach (var element in groupElements)
            {
                var id = element.Attribute("ID").Value;
                var location = Location.Parse(element.Attribute("Location").Value);
                var name = element.Attribute("Name").Value;

                marketplaceDictionary.Add(id, new MarketplaceData(location, name, LoadTimeTable(element), new Boundary(LoadBoundary(element)), id));
            }
            return marketplaceDictionary;
        }

        private List<Location> LoadBoundary(XElement element)
        {
            var locationList = new List<Location>();
            var boundaryList = from boundaries in element.Elements("Boundary") select boundaries;
            foreach (var boundary in boundaryList)
            {
                var locationCellList = from locations in boundary.Elements("Location") select locations;

                foreach (var locationCell in locationCellList)
                {
                    locationList.Add(Location.Parse(locationCell.Value));
                }
            }
            return locationList;
        }

        private Week LoadTimeTable(XElement element)
        {
            var timetableList = from timetables in element.Elements("TimeTable") select timetables;
            Week week = new Week();
            foreach (var timetable in timetableList)
            {
                var timePairList = new Day();
                WeekDays weekDayName = new WeekDays();
                var weekdayList = from weekdays in timetable.Elements("WeekDay") select weekdays;
                foreach (var weekday in weekdayList)
                {
                    var timePair = new TimePair();
                    weekDayName = (WeekDays)Enum.Parse(typeof(WeekDays), weekday.Attribute("Day").Value);
                    var workingHoursList = from workingHours in weekday.Elements("WorkingHours") select workingHours;
                    foreach (var workingHoursCell in workingHoursList)
                    {
                        timePair.StartTime = new Time(workingHoursCell.Attribute("StartTime").Value);
                        timePair.EndTime = new Time(workingHoursCell.Attribute("EndTime").Value);
                    }
                    timePairList.Add(timePair);
                }
                week.Add(weekDayName, timePairList);
            }
            return week;
        }

        public Task SaveData()
        {
            XDocument doc = new XDocument(new XElement("Marketplaces"));
            foreach (var data in dictionaryMarketplaceData.Values)
            {
                XElement root = new XElement("Marketplace");
                root.Add(new XAttribute("ID", data.ID));
                root.Add(new XAttribute("Location", data.Location));
                root.Add(new XAttribute("Name", data.Name));
                AddBoundaryToXml(data, root);
                AddTimeTableToXml(data, root);
                doc.Element("Marketplaces").Add(root);
            }
            doc.Save(filePath);

            return Task.CompletedTask;
        }

        public Task<MarketplaceData> GetData(string id) => Task.FromResult(dictionaryMarketplaceData[id]);

        public Task<List<MarketplaceData>> GetAllData() => Task.FromResult(dictionaryMarketplaceData.Values.ToList());

        public Task<int> GetDataCount() => Task.FromResult(dictionaryMarketplaceData.Count);

        public Task AddData(MarketplaceData data)
        {
            dictionaryMarketplaceData.Add(data.ID, data);
            return Task.CompletedTask;
        }

        public Task UpdateData(MarketplaceData data)
        {
            dictionaryMarketplaceData[data.ID] = data;
            return Task.CompletedTask;
        }

        public Task RemoveData(string id)
        {
            dictionaryMarketplaceData.Remove(id);
            return Task.CompletedTask;
        }
    }
}
