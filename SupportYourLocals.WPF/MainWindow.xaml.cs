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
using SupportYourLocals.Data;
using MaterialDesignThemes.Wpf;

namespace SupportYourLocals.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int AddProductLineNumber = 2;

        private Map.Map SYLMap;
        private readonly IDataStorage data;

        private bool updateMarketplacesWasClicked = false;

        private bool userSelectedLocation = false;

        List<StackPanel> listStackPanelAddProduct = new List<StackPanel>();
        List<Button> listButtonRemoveProduct = new List<Button>();
        List<double> listXCoord = new List<double>();
        List<double> listYCoord = new List<double>();
        List<int> listPersonsID = new List<int>();

        private int personsID = 1000;

        
        public MainWindow()
        {
            // data = new CSVDataStorage() or smth like that
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

            listStackPanelAddProduct.Add(StackPanelAddLocalSellerProductsOfElements1);
            listButtonRemoveProduct.Add(AddLocalSellerRemoveProductButton1);
        }

        private void UpdateMarketplaces_Click(object sender, RoutedEventArgs e)
        {
            updateMarketplacesWasClicked = true;
            CSVData.SetMarkers(listXCoord, listYCoord, listPersonsID);
            // Adding all markers to a map
            for (int i = 0; i < listXCoord.Count; i++)
                SYLMap.AddMarker(listXCoord[i], listYCoord[i], listPersonsID[i]);
        }
        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Double tap on a map
            if (e.ClickCount == 2 && GridSellerAdd.Visibility == Visibility.Visible)
            {
                userSelectedLocation = true;
                MainMap.TargetCenter = MainMap.ViewToLocation(e.GetPosition(MainMap));
                SYLMap.AddMarker(MainMap.TargetCenter, personsID);
            }
            else
            {
                userSelectedLocation = false;
                SYLMap.RemoveLastMarker();
                if (updateMarketplacesWasClicked)
                {
                    CSVData.SetMarkers(listXCoord, listYCoord, listPersonsID);
                    // Adding all markers to a map
                    for (int i = 0; i < listXCoord.Count; i++)
                        SYLMap.AddMarker(listXCoord[i], listYCoord[i], listPersonsID[i]);
                }
            }
        }

        private void LabelAddSeller_Click(object sender, RoutedEventArgs e)
        {
            //Clean up
            ErrorLabel1.Visibility = Visibility.Collapsed;
            //TextProduct.Clear();
            //TextTime.Clear();
            GridSellerAdd.Visibility = Visibility.Visible;
        }

        private void ButtonSave_Clicked(object sender, RoutedEventArgs e)
        {
            // Saving data to csv file
            if (userSelectedLocation)
            {
                //String product = TextProduct.Text;
                String product = "Testing value";
                //String time = TextTime.Text;

                CSVData.SaveData(product, MainMap.TargetCenter);

                GridSellerAdd.Visibility = Visibility.Collapsed;//------------------------------------------------------------------
                SYLMap.RemoveLastMarker();
                if (updateMarketplacesWasClicked)
                {
                    CSVData.SetMarkers(listXCoord, listYCoord, listPersonsID);
                    // Adding all markers to a map
                    for (int i = 0; i < listXCoord.Count; i++)
                        SYLMap.AddMarker(listXCoord[i], listYCoord[i], listPersonsID[i]);
                }
            }
            else
            {
                ErrorLabel1.Visibility = Visibility.Visible;
            }
        }

        private void ButtonCancel_Clicked(object sender, RoutedEventArgs e)
        {
            GridSellerAdd.Visibility = Visibility.Collapsed;
            SYLMap.RemoveLastMarker();
            if (updateMarketplacesWasClicked)
                CSVData.SetMarkers(listXCoord, listYCoord, listPersonsID);
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

        private void AddLocalSellerAddProduct1_Click(object sender, RoutedEventArgs e) 
        {
            var stackPanel = new StackPanel();
            stackPanel.Name = "StackPanelAddLocalSellerProductsOfElements" + AddProductLineNumber;
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            stackPanel.Margin = new Thickness(0, 0, 0, 5);

            var textBox = new TextBox();
            textBox.Name = "AddLocalSellerAddProductTextBox" + AddProductLineNumber;
            textBox.HorizontalAlignment = HorizontalAlignment.Center;
            textBox.VerticalAlignment = VerticalAlignment.Top;
            textBox.Height = 23;
            textBox.Width = 160;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.HorizontalContentAlignment = HorizontalAlignment.Center;

            var button = new Button();
            button.Height = 25;
            button.Width = 25;
            button.Content = "―";

            button.FontWeight = FontWeights.Bold;
            button.Name = "AddLocalSellerRemoveProductButton" + AddProductLineNumber;
            button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            button.BorderBrush = null;
            button.Background = null;
            button.Click += new RoutedEventHandler(AddLocalSellerRemoveProduct1_Click);

            stackPanel.Children.Add(textBox);
            listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Children.Add(button); //Add minus button
            listButtonRemoveProduct.Add(button);
            listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Children.Remove(AddLocalSellerAddProductButton1);//Remove Plus button
            listStackPanelAddProduct.Add(stackPanel);
            listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Children.Add(AddLocalSellerAddProductButton1);
            StackPanelAddLocalSellerProducts.Children.Add(stackPanel);
           
            AddProductLineNumber++;
            listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Margin = new Thickness(25, 0, 0, 5);
        }

        private void AddLocalSellerRemoveProduct1_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.Visibility = Visibility.Collapsed;
            foreach(var stackPanel in listStackPanelAddProduct)
            {
                if (stackPanel.Children.Contains(button))
                {
                    stackPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

    }
}
