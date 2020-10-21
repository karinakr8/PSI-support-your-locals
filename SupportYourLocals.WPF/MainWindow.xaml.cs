using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapControl;
using SupportYourLocals.Map;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace SupportYourLocals.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Map.Map SYLMap;

        private string filePath = @"./Data.csv";

        private bool updateMarketplacesWasClicked = false;

        private bool userSelectedLocation = false;

        private int personsID = 1000;

        public MainWindow()
        {
            InitializeComponent();

            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

            // Setup image cache
            var cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
            TileImageLoader.Cache = cache;

            // Setup map
            SYLMap = new Map.Map(MainMap);

            // Clean outdated image cache 2s after launch of program
            Loaded += async (s, e) =>
            {
                await Task.Delay(2000);
                await cache.Clean();
            };
        }

        private void UpdateMarketplaces_Click(object sender, RoutedEventArgs e)
        {
            updateMarketplacesWasClicked = true;
            SetMarkers(sender, e);
        }
        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Double tap on a map
            if (e.ClickCount == 2 && GridSellerAdd.Visibility == Visibility.Visible)
            {
                userSelectedLocation = true;
                MainMap.TargetCenter = MainMap.ViewToLocation(e.GetPosition(MainMap));
                SYLMap.AddMarker(MainMap.TargetCenter, "Selected location");
            }
            else
            {
                userSelectedLocation = false;
                SYLMap.RemoveLastMarker();
                if (updateMarketplacesWasClicked)
                    SetMarkers(sender, e);
            }
        }

        private void LabelAddSeller_Click(object sender, RoutedEventArgs e)
        {
            //Clean up
            ErrorLabel1.Visibility = Visibility.Collapsed;
            TextProduct.Clear();
            TextTime.Clear();
            GridSellerAdd.Visibility = Visibility.Visible;
        }

        private void ButtonSave_Clicked(object sender, RoutedEventArgs e)
        {
            // Saving data to csv file
            if (userSelectedLocation)
            {
                SaveData();
                GridSellerAdd.Visibility = Visibility.Collapsed;//------------------------------------------------------------------
                SYLMap.RemoveLastMarker();
                if (updateMarketplacesWasClicked)
                    SetMarkers(sender, e);
            }
            else
            {
                ErrorLabel1.Visibility = Visibility.Visible;
            }
        }

        private void SaveData()
        {
            // Checking person's ID
            List<double> personID = new List<double>();
            using (TextFieldParser csvParser = new TextFieldParser(filePath))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    personID.Add(Double.Parse(fields[4]));
                }
            }

            String product = TextProduct.Text;
            String time = TextTime.Text;
            var csv = new StringBuilder();


            // Setting person's ID
            personsID = personsID + personID.Count;

            var newLine = string.Format("{0},{1},{2},{3}", product, time, MainMap.TargetCenter, personsID);

            csv.AppendLine(newLine);

            File.AppendAllText(filePath, csv.ToString());
        }

        private void ButtonCancel_Clicked(object sender, RoutedEventArgs e)
        {
            GridSellerAdd.Visibility = Visibility.Collapsed;
            SYLMap.RemoveLastMarker();
            if (updateMarketplacesWasClicked)
                SetMarkers(sender, e);
        }

        private void SetMarkers(object sender, RoutedEventArgs e)
        {
            List<double> listXCoord = new List<double>();
            List<double> listYCoord = new List<double>();
            List<int> listpersonsID = new List<int>();

            using (TextFieldParser csvParser = new TextFieldParser(filePath))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Saving data to a list
                    string[] fields = csvParser.ReadFields();
                    listXCoord.Add(Double.Parse(fields[2]));
                    listYCoord.Add(Double.Parse(fields[3]));
                    listpersonsID.Add(Int32.Parse(fields[4]));
                }
            }
            // Adding all markers to a map
            for (int i = 0; i < listXCoord.Count; i++)
                SYLMap.AddMarker(listXCoord[i], listYCoord[i], listpersonsID[i].ToString());
        }

        private void SearchMarketplacesButton_Click(object sender, RoutedEventArgs e)
        {
            GridSellersSearch.Visibility = Visibility.Collapsed;
            GridMarketplacesSearch.Visibility = Visibility.Visible;
            SearchSellersButton.FontWeight = FontWeights.Normal;
            SearchMarketplacesButton.FontWeight = FontWeights.Bold;
            SearchSellersButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            SearchMarketplacesButton.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void SearchSellersButton_Click(object sender, RoutedEventArgs e)
        {
            GridSellersSearch.Visibility = Visibility.Visible;
            GridMarketplacesSearch.Visibility = Visibility.Collapsed;
            SearchMarketplacesButton.FontWeight = FontWeights.Normal;
            SearchSellersButton.FontWeight = FontWeights.Bold;
            SearchMarketplacesButton.Foreground =  new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            SearchSellersButton.Foreground = new SolidColorBrush(Colors.Black);
        }
    }
}
