using System.Windows;
using System.Linq;
using MapControl;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SupportYourLocals.Map
{
    public class Map
    {
        private readonly MapControl.Map WPFMap;

        public Location Center
        {
            set { WPFMap.TargetCenter = value; }
            get { return WPFMap.Center;  }
        }

        public double Zoom
        {
            set { WPFMap.TargetZoomLevel = value; }
            get { return WPFMap.ZoomLevel; }
        }

        public Map (MapControl.Map passed_map, Location center = null, double zoom = 14)
        {
            // Can't create a new map, so use the existing one
            WPFMap = passed_map;

            WPFMap.ZoomLevel = zoom;
            WPFMap.MaxZoomLevel = 19; // Any closer and the map starts getting blurry

            // Setup easing function for centering and changing zoom level
            WPFMap.AnimationEasingFunction = new CubicEase();
            WPFMap.AnimationEasingFunction.EasingMode = EasingMode.EaseInOut;

            // Default center is at MIF
            WPFMap.Center = center ?? new Location(54.675083, 25.273633);

            // Add OSM layer
            WPFMap.MapLayer = MapTileLayer.OpenStreetMapTileLayer;

            // Add map scale marker
            WPFMap.Children.Add(new MapScale
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            });
        }

        public delegate void MarkerClickedHandler(Marker marker);
        public event MarkerClickedHandler MarkerClicked; // Gets called when any marker gets clicked

        protected virtual void OnMarkerClicked(Marker marker)
        {
            MarkerClicked?.Invoke(marker);
        }

        public void AddMarker(Location position, string label)
        {
            var marker = new Marker
            {
                Location = position,
                Label = label
            };
            marker.MouseDown += new MouseButtonEventHandler(onMarkerClick);

            WPFMap.Children.Add(marker);
        }

        public void AddMarker (double lat, double lon, string label)
        {
            AddMarker(new Location(lat, lon), label);
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

        public void SetCenterFromCoordinates(double lat, double lon)
        {
            Center = new Location(lat, lon);
        }

        private void onMarkerClick (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            OnMarkerClicked((Marker) sender);
        }
    }
}
