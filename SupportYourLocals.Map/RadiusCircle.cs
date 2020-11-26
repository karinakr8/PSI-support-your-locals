using MapControl;
using System.Windows.Media;

namespace SupportYourLocals.Map
{
    class RadiusCircle : MapPath
    {
        private readonly Color BorderColor = Color.FromArgb(255, 0, 0, 255);
        private readonly Color FillColor = Color.FromArgb(70, 155, 155, 255);
        private const int BorderThickness = 1;

        public RadiusCircle(Location location, double radius)
        {
            Location = location;
            Stroke = new SolidColorBrush(BorderColor);
            StrokeThickness = BorderThickness;
            Fill = new SolidColorBrush(FillColor);
            Data = new EllipseGeometry
            {
                RadiusX = radius,
                RadiusY = radius
            };
        }

        public double Radius
        {
            get { return ((EllipseGeometry)Data).RadiusX; }
            set {
                ((EllipseGeometry)Data).RadiusX = value;
                ((EllipseGeometry)Data).RadiusY = value;
            }
        }
    }
}
