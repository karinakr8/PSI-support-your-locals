using MapControl;
using SupportYourLocals.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SupportYourLocals.Map
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Polyline
    {
        public LocationCollection Locations { get; set; }
    }

    public class PolylineDrawer : ViewModelBase
    {
        public ObservableCollection<Polyline> Polylines { get; } = new ObservableCollection<Polyline>();
        // Currently this class can only draw a single polyline at a time, since that's all that we need.
        // The ability to draw multiple polylines can easily be added if needed.

        public PolylineDrawer() { }

        public LocationCollection GetPolyline()
        {
            if (Polylines.Count == 0)
            {
                return null;
            }

            return Polylines[0].Locations;
        }

        public void DrawPolyline(string polyline)
        {
            DrawPolyline(LocationCollection.Parse(polyline));
        }

        public void DrawPolyline(LocationCollection locations)
        {
            if (Polylines.Count != 0)
            {
                return;
            }

            Polylines.Add(new Polyline
            {
                Locations = locations
            });
        }

        public void AddLocationToPolyline(Location location)
        {
            if (Polylines.Count == 0)
            {
                return;
            }

            Polylines[0].Locations.Add(location);
            var polyline = Polylines[0];
            Polylines.Remove(polyline);
            Polylines.Add(polyline);
        }

        public void RemoveLastLocationFromPolyline()
        {
            if (Polylines.Count == 0)
            {
                return;
            }

            if (Polylines[0].Locations.Count == 0)
            {
                return;
            }    

            Polylines[0].Locations.RemoveAt(Polylines[0].Locations.Count - 1);
            var polyline = Polylines[0];
            Polylines.Remove(polyline);
            Polylines.Add(polyline);
        }

        public void UpdateLastPolylineLocation(Location location)
        {
            RemoveLastLocationFromPolyline();
            AddLocationToPolyline(location);
        }

        public void ClearPolyline()
        {
            Polylines.Clear();
        }
    }
}
