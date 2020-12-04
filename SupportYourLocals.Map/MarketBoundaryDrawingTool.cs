using MapControl;
using SupportYourLocals.Data;

namespace SupportYourLocals.Map
{
    public class MarketBoundaryDrawingTool
    {
        private readonly PolygonDrawer polygonDrawer;

        public MarketBoundaryDrawingTool (PolygonDrawer passedDrawer)
        {
            polygonDrawer = passedDrawer;
            polygonDrawer.ClearPolygon();
            polygonDrawer.DrawPolygon(new LocationCollection());
        }

        public void UpdateLastPoint(Location location)
        {
            polygonDrawer.UpdateLastPolygonLocation(location);
        }

        public void AddPoint(Location location)
        {
            polygonDrawer.AddLocationToPolygon(location);
            var boundary = GetBoundary();
            if (boundary != null && !boundary.IsValid())
            {
                UndoPoint();
            }
        }

        public void UndoPoint()
        {
            polygonDrawer.RemoveLastLocationFromPolygon();
        }

        public Boundary GetBoundary()
        {
            var Polygon = polygonDrawer.GetPolygon();

            if (Polygon != null)
            {
                return new Boundary(polygonDrawer.GetPolygon());
            }

            return null;
        }

        public void FinishDrawing()
        {
            polygonDrawer.ClearPolygon();
        }
    }
}
