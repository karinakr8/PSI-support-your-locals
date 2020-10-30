using MapControl;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SupportYourLocals.Map
{
    public class Marker : Image
    {
        protected virtual string DefaultImageSrc => "marker.png";
        protected virtual int MarkerSizeX => 35;
        protected virtual int MarkerSizeY => 57;
        private static BitmapImage defaultImage = null;
        public int id;

        public Marker ()
        {
            Source = GetImage();

            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Bottom;

            MapPanel.InitMapElement(this);
        }

        private BitmapImage GetImage()
        {
            if (defaultImage == null)
            {
                defaultImage = new BitmapImage();
                defaultImage.BeginInit();
                defaultImage.DecodePixelWidth = MarkerSizeX;
                defaultImage.DecodePixelHeight = MarkerSizeY;
                defaultImage.UriSource = new Uri(DefaultImageSrc, UriKind.Relative);
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

    public class Marker2 : Marker
    {
        protected override string DefaultImageSrc => "marker2.png";
    }
}
