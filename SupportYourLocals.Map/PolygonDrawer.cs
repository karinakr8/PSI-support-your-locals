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

    public class Polygon
    {
        public LocationCollection Locations { get; set; }
    }

    public class PolygonDrawer : ViewModelBase
    {
        public ObservableCollection<Polygon> Polygons { get; } = new ObservableCollection<Polygon>();
        // Currently this class can only draw a single Polygon at a time, since that's all that we need.
        // The ability to draw multiple Polygons can easily be added if needed.

        public PolygonDrawer() { }

        public LocationCollection GetPolygon()
        {
            if (Polygons.Count == 0)
            {
                return null;
            }

            return Polygons[0].Locations;
        }

        public void DrawPolygon(string polygon)
        {
            DrawPolygon(LocationCollection.Parse(polygon));
        }

        public void DrawPolygon(LocationCollection locations)
        {
            if (Polygons.Count != 0)
            {
                return;
            }

            Polygons.Add(new Polygon
            {
                Locations = locations
            });
        }

        public void AddLocationToPolygon(Location location)
        {
            if (Polygons.Count == 0)
            {
                return;
            }

            Polygons[0].Locations.Add(location);
            var polygon = Polygons[0];
            Polygons.Remove(polygon);
            Polygons.Add(polygon);
        }

        public void RemoveLastLocationFromPolygon()
        {
            if (Polygons.Count == 0)
            {
                return;
            }

            if (Polygons[0].Locations.Count == 0)
            {
                return;
            }    

            Polygons[0].Locations.RemoveAt(Polygons[0].Locations.Count - 1);
            var polygon = Polygons[0];
            Polygons.Remove(polygon);
            Polygons.Add(polygon);
        }

        public void UpdateLastPolygonLocation(Location location)
        {
            RemoveLastLocationFromPolygon();
            AddLocationToPolygon(location);
        }

        public void ClearPolygon()
        {
            Polygons.Clear();
        }
    }
}
