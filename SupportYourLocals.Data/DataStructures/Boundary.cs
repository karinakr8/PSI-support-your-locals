using MapControl;
using System.Collections.Generic;
using System;

namespace SupportYourLocals.Data
{
    public class Boundary : List<Location>
    {
        public Boundary(LocationCollection locations) : base(locations) { }
        public Boundary(List<Location> locations) : base(locations) { }

        public static bool IsCounterClockwise(Location A, Location B, Location C)
        {
            return (C.Longitude - A.Longitude) * (B.Latitude - A.Latitude) > (B.Longitude - A.Longitude) * (C.Latitude - A.Latitude);
        }

        public static bool DoLinesIntersect(Location A, Location B, Location C, Location D)
        {
            return (IsCounterClockwise(A, C, D) != IsCounterClockwise(B, C, D)) &&
                   (IsCounterClockwise(A, B, C) != IsCounterClockwise(A, B, D));
        }

        // Check if all lines of the boundary do not intersect eachother
        public bool IsValid()
        {
            for (int i = 0; i < Count; i++)
            {
                Location point1 = this[i];
                Location point2 = ((i == Count - 1) ? this[0] : this[i + 1]);

                for (int j = 0; j < Count; j++)
                {
                    Location point1i = this[j];
                    Location point2i = (j == Count - 1) ? this[0] : this[j];

                    if (DoLinesIntersect(point1, point2, point1i, point2i))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Checks if given location is within the boundary
        public bool IsWithinBoundary(Location point)
        {
            double minX = this[0].Latitude;
            double maxX = this[0].Latitude;
            double minY = this[0].Longitude;
            double maxY = this[0].Longitude;
            for (int i = 1; i < Count; i++)
            {
                Location q = this[i];
                minX = Math.Min(q.Latitude, minX);
                maxX = Math.Max(q.Latitude, maxX);
                minY = Math.Min(q.Longitude, minY);
                maxY = Math.Max(q.Longitude, maxY);
            }

            if (point.Latitude < minX || point.Latitude > maxX || point.Longitude < minY || point.Longitude > maxY)
            {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = Count - 1; i < Count; j = i++)
            {
                if (((this[i]).Longitude > point.Longitude) != ((this[j]).Longitude > point.Longitude) &&
                     point.Latitude < (this[j].Latitude - this[i].Latitude) * (point.Longitude - this[i].Longitude) / (this[j].Longitude - this[i].Longitude) + this[j].Latitude)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
