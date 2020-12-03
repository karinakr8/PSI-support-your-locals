using System.Windows;
using System.Linq;
using MapControl;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;
using System;
using System.Collections.Generic;
using SupportYourLocals.Data;

namespace SupportYourLocals.Map
{
    public class Map
    {
        private readonly MapControl.Map WPFMap;
        private readonly Marker tempMarker;
        private readonly RadiusCircle searchRadius;
        private readonly PolylineDrawer polylineDrawer;

        public Location Center
        {
            set { WPFMap.TargetCenter = value; }
            get { return WPFMap.TargetCenter;  }
        }

        public double Zoom
        {
            set { WPFMap.TargetZoomLevel = value; }
            get { return WPFMap.TargetZoomLevel; }
        }

        public Map (MapControl.Map passedMap, PolylineDrawer passedDrawer, Location center = null, double zoom = 14)
        {
            // Can't create a new map, so use the existing one
            WPFMap = passedMap;
            polylineDrawer = passedDrawer;

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

            tempMarker = new Marker2();
            tempMarker.MouseDown += new MouseButtonEventHandler(OnMarkerClick);

            searchRadius = new RadiusCircle(WPFMap.Center, 0.0);
        }

        public delegate void MarkerClickedHandler (Marker marker);
        public event MarkerClickedHandler MarkerClicked; // Gets called when any marker gets clicked
        public event MarkerClickedHandler MarkerTempClicked; // Gets called when the temporary marker gets clicked

        protected virtual void OnMarkerClicked (Marker marker)
        {
            MarkerClicked?.Invoke(marker);
        }

        protected virtual void OnTempMarkerClicked(Marker marker)
        {
            MarkerTempClicked?.Invoke(marker);
        }

        public void AddMarkerTemp (Location position)
        {
            tempMarker.Location = position;
            if (!WPFMap.Children.Contains(tempMarker))
            {
                WPFMap.Children.Add(tempMarker);
            }
        }

        public void RemoveMarkerTemp ()
        {
            if (WPFMap.Children.Contains(tempMarker))
            {
                RemoveRadiusOnTempMarker();
                WPFMap.Children.Remove(tempMarker);
            }
        }

        public Location GetMarkerTempLocation ()
        {
            return WPFMap.Children.OfType<Marker2>().FirstOrDefault()?.Location;
        }

        public void AddMarker (Location position, string id)
        {
            var marker = new Marker
            {
                Location = position,
                id = id
            };
            marker.MouseDown += new MouseButtonEventHandler(OnMarkerClick);

            WPFMap.Children.Add(marker);
        }

        public void AddMarker (double lat, double lon, string id)
        {
            AddMarker(new Location(lat, lon), id);
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

        private GeocodeResponse LocationToAddressInternal(Location location)
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

            return request.Result;
        }

        public string LocationToAddress(Location location)
        {
            return LocationToAddressInternal(location).DisplayName;
        }

        public Tuple<string, string> LocationToAddressSplit(Location location)
        {
            // Return value 1 - address, 2 - city/district
            var result = LocationToAddressInternal(location);

            string city = result.Address.District;

            if (result.Address.City != null)
                city = result.Address.City;

            int index = result.DisplayName.IndexOf(city);
            if (index > 2)
                index -= 2;
            // Return the address part without the trailing ", " and the city/district
            return new Tuple<string, string> (result.DisplayName.Substring(0, index), city);
        }

        public double GetDistance(double longitude, double latitude, double longitude2, double latitude2)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = latitude2 * (Math.PI / 180.0);
            var num2 = longitude2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))); // Distance is in meters
        }

        public double GetDistance(Location location1, Location location2)
        {
            return GetDistance(location1.Longitude, location1.Latitude, location2.Longitude, location2.Latitude);
        }

        public void DrawBoundary(Boundary boundary)
        {
            var boundaryCopy = new List<Location>(boundary);
            boundaryCopy.Add(boundaryCopy[0]); // Add the first element to the end to form an enclosed polygon
            polylineDrawer.DrawPolyline(new LocationCollection(boundaryCopy));
        }

        public void ClearBoundary()
        {
            polylineDrawer.ClearPolyline();
        }

        public void DrawRadiusOnTempMarker(double radius)
        {
            if (WPFMap.Children.OfType<Marker2>().Count() == 0)
            {
                return;
            }

            searchRadius.Radius = radius;
            searchRadius.Location = GetMarkerTempLocation();
            if (!WPFMap.Children.Contains(searchRadius))
            {
                WPFMap.Children.Add(searchRadius);
            }
        }

        public void RemoveRadiusOnTempMarker()
        {
            if (WPFMap.Children.Contains(searchRadius))
            {
                WPFMap.Children.Remove(searchRadius);
            }
        }

        private void OnMarkerClick (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (sender == tempMarker)
                OnTempMarkerClicked((Marker) sender);
            else
                OnMarkerClicked((Marker)sender); 
        }
    }
}
