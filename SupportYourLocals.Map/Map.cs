using System.Windows;
using System.Linq;
using MapControl;

namespace SupportYourLocals.Map
{
    public class Map
    {
        private readonly MapControl.Map WPFMap;

        public Map (MapControl.Map passed_map)
        {
            WPFMap = passed_map;

            WPFMap.ZoomLevel = 15;
            WPFMap.MaxZoomLevel = 19;
            WPFMap.Center = new Location(54.675083, 25.273633);

            // Add OSM layer
            WPFMap.MapLayer = MapTileLayer.OpenStreetMapTileLayer;

            // Add map scale marker
            WPFMap.Children.Add(new MapScale
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            });
        }

        public void AddMarker(Location position)
        {
            WPFMap.Children.Add(new Marker
            {
                Location = position
            });
        }

        public void AddMarker (double lat, double lon)
        {
            AddMarker(new Location(lat, lon));
        }

        public void RemoveLastMarker()
        {
            var toRemove = WPFMap.Children.OfType<Pushpin>().ToList();
            if (toRemove.Count != 0)
            {
                WPFMap.Children.Remove(toRemove[toRemove.Count - 1]);
            }
        }

        public void RemoveAllMarkers ()
        {
            var toRemove = WPFMap.Children.OfType<Marker>().ToList();

            foreach (var item in toRemove)
                WPFMap.Children.Remove(item);
        }
    }
}
