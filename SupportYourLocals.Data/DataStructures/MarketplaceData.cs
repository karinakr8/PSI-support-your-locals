using MapControl;
using System.Collections.Generic;
using SupportYourLocals.ExtensionMethods;
using System;

namespace SupportYourLocals.Data
{
    public enum WeekDays
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    public class Time
    {
        private int hours = 0;
        private int minutes = 0;

        public int Hours
        {
            get { return hours; }
            set 
            { 
                if (value < 0 || value > 23)
                {
                    throw new Exception("Hours cannot be less than 0 or more than 23");
                }
                hours = value;
            }
        }

        public int Minutes
        {
            get { return minutes; }
            set
            {
                if (value < 0 || value > 59)
                {
                    throw new Exception("Minutes cannot be less than 0 or more than 59");
                }
                minutes = value;
            }
        }

        public Time (int hours, int minutes)
        {
            Hours = hours;
            Minutes = minutes;
        }

        public Time (string time)
        {
            var splitTime = time.Split(':');
            if (splitTime.Length != 2)
            {
                throw new Exception("Time passed in invalid format. Must be HH:MM");
            }

            Hours = int.Parse(splitTime[0]);
            Minutes = int.Parse(splitTime[1]);
        }

        public string AsText() => "{0}:{1}".Format(hours, minutes);
    }

    public class TimePair
    {
        public Time StartTime { get; set; }
        public Time EndTime { get; set; }
    }

    public class Day : List<TimePair> { }

    public class Week : Dictionary<WeekDays, Day> 
    { 
        public new Day this[WeekDays day]
        {
            get { return ContainsKey(day) ? base[day] : new Day(); }
            set { base[day] = value; }
        }
    }

    public class MarketplaceData : LocationData
    {
        public List<Location> Boundary { get; set; }
        public Week Timetable { get; set; }

        public MarketplaceData(Location location, string name, Week timetable, List<Location> boundary = null, string id = null) : base(location, name, id)
        {
            Timetable = timetable;
            Boundary = boundary;
        }
    }
}
