﻿using System;
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
using System.Runtime.CompilerServices;

namespace SupportYourLocals.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int AddProductLineNumber = 2;

        private readonly Map.Map SYLMap;
        private readonly IDataStorage data;

        private bool updateMarketplacesWasClicked = false;

        private bool userSelectedLocation = false;


        List<double> listXCoord = new List<double>();
        List<double> listYCoord = new List<double>();
        List<int> listPersonsID = new List<int>();

        // List for StackPanel elements in Main StackPanel
        List<List<StackPanel>> listOfStackPanelListsAddProduct = new List<List<StackPanel>>();
        // List for "+" buttons in scrollviewer AddLocalSeller
        List<Button> listAddButtons = new List<Button>();

        // List for Main StackPanels
        // These stackpanels consist of other stackpanels and buttons. One Main stack panel represent one type of products, in instance- fruits
        List<StackPanel> listMainStackPanel = new List<StackPanel>();
        // Dictionaries to load scrollviews and store data based on chosen enum
        Dictionary<ProductType, ScrollViewer> dictionaryOfScrollViewsAddProduct = new Dictionary<ProductType, ScrollViewer>();
        Dictionary<ProductType, List<TextBox>> dictionaryOfTextBoxListAddProduct = new Dictionary<ProductType, List<TextBox>>();

        XMLData xmlData = new XMLData();



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

            // Filling combobox for product types and filling dictionaries
            loadAddLocalSellerFieldsAndCollections();
        }

        private void loadAddLocalSellerFieldsAndCollections()
        {
            var productTypes = Enum.GetValues(typeof(ProductType));


            foreach (Enum productType in productTypes)
            {
                // Adding elements to combobox
                ComboBoxProductType.Items.Add(productType);
                // Create first textBox and button and add to the secondary stack panel
                var stackPanel = CreateStackPanelForProductTypes(productType);
                var textBox = CreateTextFieldForProductTypes();

                var listTextBoxes = new List<TextBox>();
                listTextBoxes.Add(textBox);
                dictionaryOfTextBoxListAddProduct.Add((ProductType)productType, listTextBoxes);

                stackPanel.Children.Add((dictionaryOfTextBoxListAddProduct[(ProductType)productType])[^1]);
                var button = CreateButtonForProductTypes(productType.ToString(), "+", AddLocalSellerAddProduct1_Click, "AddLocalSellerAddTextFieldButton" + productType.ToString());
                stackPanel.Children.Add(button);
                listAddButtons.Add(button);

                //create a temporary list to store first textbox of a stack panel


                listOfStackPanelListsAddProduct.Add(new List<StackPanel>());
                listOfStackPanelListsAddProduct[^1].Add(stackPanel);

                // Create main stack panel and add the secondary stack panel
                var stackPanelMain = new StackPanel();
                stackPanelMain.Name = "stackPanelMain" + productType.ToString();
                stackPanelMain.Children.Add((listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1])[(listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1]).Count - 1]);
                listMainStackPanel.Add(stackPanelMain);

                // Create new instance of a scroll viewer for an enum and add main stack panel
                var scrollViewer = CreateScrollViewerForProductTypes();
                scrollViewer.Name = "scrollViewerMain" + productType.ToString();
                scrollViewer.Content = listMainStackPanel[^1];
                scrollViewer.Visibility = Visibility.Collapsed;

                dictionaryOfScrollViewsAddProduct.Add((ProductType)productType, scrollViewer);

                StackPanelWithScrollViewerAddSellers.Children.Add(dictionaryOfScrollViewsAddProduct[(ProductType)productType]);

            }
        }

        private void clearAddLocalSellerCollections()
        {
            dictionaryOfScrollViewsAddProduct.Clear();
            dictionaryOfTextBoxListAddProduct.Clear();
            listAddButtons.Clear();
            listOfStackPanelListsAddProduct.Clear();
            listMainStackPanel.Clear();
        }

        public void clearAddLocalSellerInputFieldsAndUserInterface()
        {
            AddLocalSellerNameTextBox.Clear();
            ComboBoxProductType.Items.Clear();
            //StackPanelWithScrollViewerAddSellers.
            foreach (var scrollviewer in dictionaryOfScrollViewsAddProduct)
            {
                scrollviewer.Value.Visibility = Visibility.Collapsed;
            }
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
            loadAddLocalSellerFieldsAndCollections();
            ErrorLabel1.Visibility = Visibility.Collapsed;
            //TextProduct.Clear();
            //TextTime.Clear();
            GridSellerAdd.Visibility = Visibility.Visible;
        }

        private void ButtonSave_Clicked(object sender, RoutedEventArgs e)
        {
            // Saving data to csv file
            //IDataStorage temp = new XmlDataProvider();
            var productTypes = Enum.GetValues(typeof(ProductType));



            if (userSelectedLocation)
            {
                //String product = TextProduct.Text;
                string productType = ComboBoxProductType.Text;
                //String time = TextTime.Text;


                //var dictionaryProducts = convertDictionaryListTextBoxToDictionaryListString(dictionaryOfTextBoxListAddProduct);
                var dictionaryListString = convertDictionaryListTextBoxToDictionaryListString(dictionaryOfTextBoxListAddProduct);

                ///change ID of a local seller
                ///Change AddedById field
                xmlData.AddData(new LocationData(1000, MainMap.TargetCenter, AddLocalSellerNameTextBox.Text, 10, DateTime.Now, dictionaryListString));

                GridSellerAdd.Visibility = Visibility.Collapsed;
                SYLMap.RemoveLastMarker();
                if (updateMarketplacesWasClicked)
                {
                    CSVData.SetMarkers(listXCoord, listYCoord, listPersonsID);
                    // Adding all markers to a map
                    for (int i = 0; i < listXCoord.Count; i++)
                        SYLMap.AddMarker(listXCoord[i], listYCoord[i], listPersonsID[i]);
                }

                // Clear everything for the next usages of "Add local seller"
                clearAddLocalSellerInputFieldsAndUserInterface();
                clearAddLocalSellerCollections();
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

            // Clean up Add local seller window
            clearAddLocalSellerInputFieldsAndUserInterface();
            clearAddLocalSellerCollections();
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
            SearchMarketplacesButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            SearchSellersButton.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void AddLocalSellerAddProduct1_Click(object sender, RoutedEventArgs e)
        {
            var stackPanel = CreateStackPanelForProductTypeElements();

            var textBox = CreateTextFieldForProductTypes();

            var productType = ComboBoxProductType.SelectedItem.ToString();
            var productTypesEnum = (ProductType)Enum.Parse(typeof(ProductType), ComboBoxProductType.SelectedValue.ToString());

            var button = CreateButtonForProductTypes(productType, "―", AddLocalSellerRemoveProduct1_Click, null);

            stackPanel.Children.Add(textBox);

            int index = ComboBoxProductType.SelectedIndex;
            // Remove "+" button from the last line before new line (textbox) is added
            (listOfStackPanelListsAddProduct[index])[listOfStackPanelListsAddProduct[index].Count - 1].Children.Remove(listAddButtons[index]);

            // Add "—" button to the last line before new line is inicialized
            (listOfStackPanelListsAddProduct[index])[listOfStackPanelListsAddProduct[index].Count - 1].Children.Add(CreateButtonForProductTypes(productType, "—", AddLocalSellerRemoveProduct1_Click, null));

            stackPanel.Children.Add(listAddButtons[index]);

            listOfStackPanelListsAddProduct[index].Add(stackPanel);

            dictionaryOfTextBoxListAddProduct[productTypesEnum].Add(textBox);

            listMainStackPanel[index].Children.Remove(listAddButtons[index]);

            listMainStackPanel[index].Children.Add(stackPanel);

            AddProductLineNumber++;
        }

        //after clicking save button, save only values of these textfields which visibility = Visible
        private void AddLocalSellerRemoveProduct1_Click(object sender, RoutedEventArgs e)
        {
            int index = ComboBoxProductType.SelectedIndex;
            var productTypesEnum = (ProductType)Enum.Parse(typeof(ProductType), ComboBoxProductType.SelectedValue.ToString());
            Button button = sender as Button;
            button.Visibility = Visibility.Collapsed;
            // Might change this later
            int i = 0;
            foreach (var stackPanel in listOfStackPanelListsAddProduct[index])
            {
                
                if (stackPanel.Children.Contains(button))
                {
                    //stackPanel.Visibility = Visibility.Collapsed;
                    (dictionaryOfTextBoxListAddProduct[productTypesEnum]).RemoveAt(i);
                    stackPanel.Children.Clear();
                    stackPanel.Margin = new Thickness(0, 0, 0, 0);
                }
                i++;
            }
        }

        private void ComboBoxProductType_SelectionChanged(object sender, EventArgs e)
        {
            if(ComboBoxProductType.SelectedValue != null) { 
                var productType = (ProductType)Enum.Parse(typeof(ProductType), ComboBoxProductType.SelectedValue.ToString());
                    LabelForScrollViewerAddLocalSeller.Visibility = Visibility.Collapsed;

                    dictionaryOfScrollViewsAddProduct[productType].Visibility = Visibility.Visible;

                    foreach (KeyValuePair<ProductType, ScrollViewer> entry in dictionaryOfScrollViewsAddProduct)
                    {
                        if (entry.Key != productType)
                        {
                            entry.Value.Visibility = Visibility.Collapsed;
                        }
                    }
            }
            else
            {
                LabelForScrollViewerAddLocalSeller.Visibility = Visibility.Visible;
            }
        }

        private Button CreateButtonForProductTypes(string enumValue, string content, Action<object, RoutedEventArgs> actionOnClickName, string buttonName)
        {
            var button = new Button
            {
                Height = 25,
                Width = 25,

                Content = content,

                FontWeight = FontWeights.Bold,
                Name = buttonName,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080")),
                BorderBrush = null,
                Background = null
            };
            button.Click += new RoutedEventHandler(actionOnClickName);
            return button;
        }

        private TextBox CreateTextFieldForProductTypes()
        {
            var textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 23,
                Width = 160,
                TextWrapping = TextWrapping.Wrap,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            return textBox;
        }

        private StackPanel CreateStackPanelForProductTypeElements()
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(40, 0, 0, 5)
            };
            return stackPanel;
        }

        private StackPanel CreateStackPanelForProductTypes(object productType)
        {
            var stackPanel = CreateStackPanelForProductTypeElements();
            return stackPanel;
        }

        private ScrollViewer CreateScrollViewerForProductTypes()
        {
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Height = 120
            };
            return scrollViewer;
        }
        private Dictionary<ProductType, List<string>> convertDictionaryListTextBoxToDictionaryListString(Dictionary<ProductType, List<TextBox>> dictionary)
        {
            var dictionaryListString = new Dictionary<ProductType, List<string>>();

            foreach (var elementTextBox in dictionary)
            {
                if (elementTextBox.Value != null)
                {
                    var listString = new List<string>();
                    foreach (var elementOfList in elementTextBox.Value)
                    {
                        if (elementOfList != null)
                        {
                            listString.Add(elementOfList.Text);
                        }
                    }
                    dictionaryListString.Add(elementTextBox.Key, listString);
                }
            }
            return dictionaryListString;
        }
    }


}
