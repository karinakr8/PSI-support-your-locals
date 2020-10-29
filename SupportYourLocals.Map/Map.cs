using System.Windows;
using System.Linq;
using MapControl;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;

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

        public Map (MapControl.Map passedMap, Location center = null, double zoom = 14)
        {
            // Can't create a new map, so use the existing one
            WPFMap = passedMap;

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

        public delegate void MarkerClickedHandler (Marker marker);
        public event MarkerClickedHandler MarkerClicked; // Gets called when any marker gets clicked

        protected virtual void OnMarkerClicked (Marker marker)
        {
            MarkerClicked?.Invoke(marker);
        }

        public void AddMarker (Location position, int id)
        {
            var marker = new Marker
            {
                Location = position,
                id = id
            };
            marker.MouseDown += new MouseButtonEventHandler(OnMarkerClick);

            WPFMap.Children.Add(marker);
        }

        public void AddMarker (double lat, double lon, int id)
        {
            AddMarker(new Location(lat, lon), id);
        }

        public void RemoveLastMarker ()
        {
            var toRemove = WPFMap.Children.OfType<Marker>().LastOrDefault();
            if (toRemove != null)
                WPFMap.Children.Remove(toRemove);
        }

        public void RemoveAllMarkers ()
        {
            var toRemove = WPFMap.Children.OfType<Marker>().ToList();

            foreach (Marker item in toRemove)
                WPFMap.Children.Remove(item);
        }

        public void SetCenterFromCoordinates (double lat, double lon)
        {
            Center = new Location(lat, lon);
        }

        public Location AddressToLocation (string address)
        {
            var geocoder = new ForwardGeocoder();
            var request = geocoder.Geocode(new ForwardGeocodeRequest
            {
                queryString = address
            });
            request.Wait();

            if (request.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                return null;

            if (request.Result.Length < 1)
                return null;

            return new Location(request.Result[0].Latitude, request.Result[0].Longitude);
        }

        public string LocationToAddress(Location location)
        {
            var geocoder = new ReverseGeocoder();
            var request = geocoder.ReverseGeocode(new ReverseGeocodeRequest
            {
                Longitude = location.Longitude,
                Latitude = location.Latitude,
                BreakdownAddressElements = true
            });
            request.Wait();

            if (request.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                return null;

            if (request.Result.PlaceID == 0)
                return null;

            return request.Result.DisplayName;
        }

        private void OnMarkerClick (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            OnMarkerClicked((Marker) sender);
        }
    }
}
