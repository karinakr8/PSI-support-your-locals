using MapControl;
using System;
using System.Collections.Generic;
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
        private static Dictionary<string, BitmapImage> images = new Dictionary<string, BitmapImage>();
        public string id;

        public Marker ()
        {
            Source = GetImage();

            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Bottom;

            MapPanel.InitMapElement(this);
        }

        private BitmapImage GetImage()
        {
            if (!images.ContainsKey(DefaultImageSrc))
            {
                images[DefaultImageSrc] = new BitmapImage();
                images[DefaultImageSrc].BeginInit();
                images[DefaultImageSrc].DecodePixelWidth = MarkerSizeX;
                images[DefaultImageSrc].DecodePixelHeight = MarkerSizeY;
                images[DefaultImageSrc].UriSource = new Uri(DefaultImageSrc, UriKind.Relative);
                images[DefaultImageSrc].EndInit();
            }

            return images[DefaultImageSrc];
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
