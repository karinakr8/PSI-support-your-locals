using System.Windows;
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
    }
}
