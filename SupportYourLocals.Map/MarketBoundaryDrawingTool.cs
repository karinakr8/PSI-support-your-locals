using MapControl;
using SupportYourLocals.Data;

namespace SupportYourLocals.Map
{
    public class MarketBoundaryDrawingTool
    {
        private readonly PolylineDrawer polylineDrawer;

        public MarketBoundaryDrawingTool (PolylineDrawer passedDrawer)
        {
            polylineDrawer = passedDrawer;
            polylineDrawer.ClearPolyline();
            polylineDrawer.DrawPolyline(new LocationCollection());
        }

        public void UpdateLastPoint(Location location)
        {
            polylineDrawer.UpdateLastPolylineLocation(location);
        }

        public void AddPoint(Location location)
        {
            polylineDrawer.AddLocationToPolyline(location);
            var boundary = GetBoundary();
            if (boundary != null && !boundary.IsValid())
            {
                UndoPoint();
            }
        }

        public void UndoPoint()
        {
            polylineDrawer.RemoveLastLocationFromPolyline();
        }

        public Boundary GetBoundary()
        {
            var polyline = polylineDrawer.GetPolyline();

            if (polyline != null)
            {
                return new Boundary(polylineDrawer.GetPolyline());
            }

            return null;
        }

        public void FinishDrawing()
        {
            polylineDrawer.ClearPolyline();
        }
    }
}
