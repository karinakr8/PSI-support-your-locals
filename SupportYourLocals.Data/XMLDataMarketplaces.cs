﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using MapControl;

namespace SupportYourLocals.Data
{
    public class XMLDataMarketplaces : IMarketStorage
    {
        private const string filePath = @"./MarketPlacesData.xml";
        readonly Dictionary<string, MarketplaceData> dictionaryMarketplaceDataById;

        MarketplaceData IDataStorage<MarketplaceData>.GetData(string id) => dictionaryMarketplaceDataById[id];

        List<MarketplaceData> IDataStorage<MarketplaceData>.GetAllData() => dictionaryMarketplaceDataById.Select(d => d.Value).ToList();

        int IDataStorage<MarketplaceData>.GetDataCount() => dictionaryMarketplaceDataById.Count;

        void IDataStorage<MarketplaceData>.AddData(MarketplaceData data) => dictionaryMarketplaceDataById.Add(data.ID, data);

        void IDataStorage<MarketplaceData>.UpdateData(MarketplaceData data) => dictionaryMarketplaceDataById[data.ID] = data;

        void IDataStorage<MarketplaceData>.RemoveData(string id) => dictionaryMarketplaceDataById.Remove(id);

        public void SaveData()
        {
            XDocument doc = new XDocument(new XElement("Marketplaces"));
            foreach (var data in dictionaryMarketplaceDataById.Values)
            {
                XElement root = new XElement("Marketplace");
                root.Add(new XAttribute("ID", data.ID));
                AddBoundaryToXml(data, root);
                AddTimeTableToXml(data, root);
                doc.Element("Marketplaces").Add(root);
            }
            doc.Save(filePath);
        }

        private void AddBoundaryToXml(MarketplaceData data, XElement root)
        {
            XElement boundaryBranch = new XElement("Boundary");
            foreach (var location in data.Boundary)
            {
                XElement locationBranch = new XElement("Location");
                locationBranch.Add(content: location);
                boundaryBranch.Add(locationBranch);
            }
            root.Add(boundaryBranch);
        }

        private void AddTimeTableToXml(MarketplaceData data, XElement root)
        {
            XElement boundaryBranch = new XElement("TimeTable");
            foreach (var weekDay in data.Timetable)
            {
                XElement weekDayBranch = new XElement("WeekDay");
                weekDayBranch.Add(new XAttribute("Day", weekDay.Key));
                foreach (var day in weekDay.Value)
                {
                    XElement dayBranch = new XElement("WorkingHours");
                    dayBranch.Add(new XAttribute("StartTime", day.StartTime));
                    dayBranch.Add(new XAttribute("EndTime", day.EndTime));
                }
            }
        }

        public Dictionary<string, MarketplaceData> LoadData()
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, MarketplaceData>();
            }

            XDocument doc = XDocument.Load(filePath);
            
            var marketplaceDictionary = new Dictionary<string, MarketplaceData>();
            var locationList = new List<Location>();
            var groupElements = from elements in doc.Descendants().Elements("Marketplace") select elements;
            foreach (var element in groupElements)
            {
                var id = element.Attribute("ID").Value;
                var location = Location.Parse(element.Attribute("Location").Value);
                var name = element.Attribute("Name").Value;
                var timeTable = new Week();
                var weekDictionary = new Dictionary<WeekDays, Day>();
                var boundaryList = from boundaries in element.Elements("Boundary") select boundaries;
                foreach (var boundary in boundaryList)
                {
                    var locationCellList = from locations in boundary.Elements("Location") select locations;

                    foreach (var locationCell in locationCellList)
                    {
                        locationList.Add(Location.Parse(locationCell.Value));
                    }
                }



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
                            timePair.EndTime = new Time (workingHoursCell.Attribute("EndTime").Value);
                        }
                        timePairList.Add(timePair);
                    }
                    week.Add(weekDayName, timePairList);
                }

                marketplaceDictionary.Add(id, new MarketplaceData(location, name, week, locationList, id));
            }
            return marketplaceDictionary;
        }

    }
}
