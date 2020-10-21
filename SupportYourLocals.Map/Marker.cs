using MapControl;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SupportYourLocals.Map
{
    public class Marker : Image
    {
        private const string defaultImageSrc = "marker.png";
        private const int markerSizeX = 35;
        private const int markerSizeY = 57;
        private static BitmapImage defaultImage = null;
        public int id;

        public Marker ()
        {
            Source = getImage();

            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Bottom;

            MapPanel.InitMapElement(this);
        }

        private static BitmapImage getImage()
        {
            if (defaultImage == null)
            {
                defaultImage = new BitmapImage();
                defaultImage.BeginInit();
                defaultImage.DecodePixelWidth = markerSizeX;
                defaultImage.DecodePixelHeight = markerSizeY;
                defaultImage.UriSource = new Uri(defaultImageSrc, UriKind.Relative);
                defaultImage.EndInit();
            }

            return defaultImage;
        }

        public Location Location
        {
            get { return MapPanel.GetLocation(this); }
            set { MapPanel.SetLocation(this, value); }
        }
    }
}
