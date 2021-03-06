﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MapControl;
using SupportYourLocals.Map;
using SupportYourLocals.Data;
using SupportYourLocals.ExtensionMethods;
using System.Text.RegularExpressions;
using System.Threading;

namespace SupportYourLocals.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int AddProductLineNumber = 2;

        private static object mapLock = new object();

        private readonly Map.Map SYLMap;
        private MarketBoundaryDrawingTool boundaryDrawer;

        private readonly ISellerStorage sellerData = new WebData();
        private readonly IMarketStorage marketplaceData = new WebData();
        private readonly IUserStorage userLoginData = new WebData();

        private readonly MapUtilityProvider mapUtility = new MapUtilityProvider();

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

        public MainWindow()
        {
            InitializeComponent();
            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

            // Setup image cache
            var cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
            TileImageLoader.Cache = cache;

            // Setup map
            SYLMap = new Map.Map(MainMap, (PolygonDrawer) DataContext);

            // Connect to the marker clicked event
            SYLMap.MarkerClicked += new Map.Map.MarkerClickedHandler(OnMarkerClicked);

            // Clean outdated image cache 2s after launch of program
            Loaded += async (s, e) =>
            {
                await Task.Delay(2000);
                await cache.Clean();
            };

            AddWorkdaysToMarketPlaceWorkdayCombobox();
        }

        private void LoadAddLocalSellerFieldsAndCollections()
        {
            SYLMap.RemoveMarkerTemp();
            SYLMap.RemoveAllMarkers();

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

                listOfStackPanelListsAddProduct.Add(new List<StackPanel>());
                listOfStackPanelListsAddProduct[^1].Add(stackPanel);

                // Create main stack panel and add the secondary stack panel
                var stackPanelMain = new StackPanel();
                stackPanelMain.Name = "stackPanelMain" + productType.ToString();
                stackPanelMain.Children.Add((listOfStackPanelListsAddProduct[^1])[^1]);
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

        private void AddWorkdaysToMarketPlaceWorkdayCombobox()
        {
            var weekDays = Enum.GetValues(typeof(WeekDays));

            foreach (Enum weekDay in weekDays)
            {
                // Adding elements to combobox
                ComboBoxWorkDay.Items.Add(weekDay);
            }
        }

        private void ClearAddLocalSellerCollections()
        {
            dictionaryOfScrollViewsAddProduct.Clear();
            dictionaryOfTextBoxListAddProduct.Clear();
            listAddButtons.Clear();
            listOfStackPanelListsAddProduct.Clear();
            listMainStackPanel.Clear();
        }

        public void ClearAddLocalSellerInputFieldsAndUserInterface()
        {
            AddLocalSellerNameTextBox.Clear();
            ComboBoxProductType.Items.Clear();
            foreach (var scrollviewer in dictionaryOfScrollViewsAddProduct)
            {
                scrollviewer.Value.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateMarketplaces_Click(object sender, RoutedEventArgs e)
        {
            GridMarketplacesAdd.Visibility = Visibility.Visible;
            boundaryDrawer = new MarketBoundaryDrawingTool((PolygonDrawer)DataContext);
        }

        private async void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                if (boundaryDrawer == null)
                {
                    return;
                }

                boundaryDrawer.AddPoint(MainMap.ViewToLocation(e.GetPosition(MainMap)));

                return;
            }

            //Double tap on a map
            if (e.ClickCount == 2)
            {
                // Currently drawing a boundary
                if (boundaryDrawer != null)
                {
                    boundaryDrawer.UndoPoint(); // The single click event added a point, we don't need it
                    var boundary = boundaryDrawer.GetBoundary();
                    boundary.Add(boundary[0]); // Add the first point again to create an enclosed boundary
                    // Save the boundary

                    boundaryDrawer.FinishDrawing();
                    boundaryDrawer = null;

                    SYLMap.DrawBoundary(boundary);
                    return;
                }

                SYLMap.Center = MainMap.ViewToLocation(e.GetPosition(MainMap));
                SYLMap.AddMarkerTemp(SYLMap.Center);

                if (GridSellerAdd.Visibility == Visibility.Collapsed)
                {
                    if (GridSellersSearch.Visibility == Visibility.Visible)
                    {
                        SYLMap.DrawRadiusOnTempMarker(Slider1Seller.Value * 1000.0);
                    }

                    var address = await MapUtilityProvider.LocationToAddress(SYLMap.Center.Latitude, SYLMap.Center.Longitude);
                    if (GridSellersSearch.Visibility == Visibility.Visible)
                    {
                        TextBox2Seller.Text = address.Item2;
                        TextBox3Seller.Text = address.Item1;
                    }
                    else if(GridMarketplacesSearch.Visibility == Visibility.Visible && GridMarketplaceInformation.Visibility != Visibility.Visible)
                    {
                        TextBoxMarketplaceDistrict.Text = address.Item1;
                        TextBoxMarketplaceLocation.Text = address.Item2;
                    }
                }
                else
                {
                    ErrorLabel1.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (boundaryDrawer != null)
            {
                boundaryDrawer.UndoPoint();
            }
            else
            {
                SYLMap.RemoveMarkerTemp();
            }
        }

        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            if (boundaryDrawer == null)
            {
                return;
            }

            boundaryDrawer.UpdateLastPoint(MainMap.ViewToLocation(e.GetPosition(MainMap)));
        }


        private void LabelAddSeller_Click(object sender, RoutedEventArgs e)
        {
            //Clean up
            ClearSearchSellerWindow();
            GridMarkerInformation.Visibility = Visibility.Collapsed;
            LoadAddLocalSellerFieldsAndCollections();
            ErrorLabel1.Visibility = Visibility.Collapsed;
            GridSellerAdd.Visibility = Visibility.Visible;
        }

        private void ButtonSave_Clicked(object sender, RoutedEventArgs e)
        {
            if (SYLMap.GetMarkerTempLocation() == null)
            {
                DisplayErrorMessage("Select current place on a map");
                return;
            }

            var dictionaryListString = ConvertDictionaryListTextBoxToDictionaryListString(dictionaryOfTextBoxListAddProduct);

            if (!CheckAddSellerInput(AddLocalSellerNameTextBox.Text.Trim(), dictionaryListString))
            {
                return;
            }

            sellerData.AddData(new SellerData(SYLMap.GetMarkerTempLocation(), AddLocalSellerNameTextBox.Text.Trim(), 10, DateTime.Now, dictionaryListString));
            sellerData.SaveData();

            GridSellerAdd.Visibility = Visibility.Collapsed;
            SYLMap.RemoveMarkerTemp();

            // Clear everything for the next usages of "Add local seller"
            ClearAddLocalSellerInputFieldsAndUserInterface();
            ClearAddLocalSellerCollections();
        }

        private void ButtonCancel_Clicked(object sender, RoutedEventArgs e)
        {
            GridSellerAdd.Visibility = Visibility.Collapsed;

            SYLMap.RemoveMarkerTemp();

            // Clean up Add local seller window
            ClearAddLocalSellerInputFieldsAndUserInterface();
            ClearAddLocalSellerCollections();
        }

        private void SearchMarketplacesButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAddLocalSellerInputFieldsAndUserInterface();
            ClearAddLocalSellerCollections();
            GridMarkerInformation.Visibility = Visibility.Collapsed;
            GridSellersSearch.Visibility = Visibility.Collapsed;
            GridMarketplaceInformation.Visibility = Visibility.Collapsed;
            GridMarketplacesSearch.Visibility = Visibility.Visible;
            SearchSellersButton.FontWeight = FontWeights.Normal;
            SearchMarketplacesButton.FontWeight = FontWeights.Bold;
            SearchSellersButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            SearchMarketplacesButton.Foreground = new SolidColorBrush(Colors.Black);
            SYLMap.RemoveRadiusOnTempMarker();
            SYLMap.RemoveAllMarkers();
        }

        private void SearchSellersButton_Click(object sender, RoutedEventArgs e)
        {
            GridSellersSearch.Visibility = Visibility.Visible;
            GridMarketplacesSearch.Visibility = Visibility.Collapsed;
            GridMarketplaceInformation.Visibility = Visibility.Collapsed;
            GridMarketplacesAdd.Visibility = Visibility.Collapsed;
            SearchMarketplacesButton.FontWeight = FontWeights.Normal;
            SearchSellersButton.FontWeight = FontWeights.Bold;
            SearchMarketplacesButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            SearchSellersButton.Foreground = new SolidColorBrush(Colors.Black);
            SYLMap.RemoveAllMarkers();
            SYLMap.RemoveRadiusOnTempMarker();
        }

        private void AddLocalSellerAddProduct1_Click(object sender, RoutedEventArgs e)
        {
            Button buttonSender = sender as Button;
            var productType = ComboBoxProductType.SelectedItem.ToString();
            var index = ComboBoxProductType.SelectedIndex;
            var productTypesEnum = (ProductType)Enum.Parse(typeof(ProductType), ComboBoxProductType.SelectedValue.ToString());
            int i = 0;
            foreach (var stackPanelMain in listOfStackPanelListsAddProduct[index])
            {
                if (stackPanelMain.Children.Contains(buttonSender))
                {
                    if(!CheckProduct((dictionaryOfTextBoxListAddProduct[productTypesEnum])[i].Text.Trim()))
                    {
                        return;
                    }
                }
                i++;
            }
            var stackPanel = CreateStackPanelForProductTypeElements();
            var textBox = CreateTextFieldForProductTypes();

            var button = CreateButtonForProductTypes(productType, "―", AddLocalSellerRemoveProduct1_Click, null);

            stackPanel.Children.Add(textBox);
            // Remove "+" button from the last line before new line (textbox) is added
            (listOfStackPanelListsAddProduct[index])[^1].Children.Remove(listAddButtons[index]);
            // Add "—" button to the last line before new line is inicialized
            (listOfStackPanelListsAddProduct[index])[^1].Children.Add(CreateButtonForProductTypes(productType, "—", AddLocalSellerRemoveProduct1_Click, null));

            stackPanel.Children.Add(listAddButtons[index]);
            listOfStackPanelListsAddProduct[index].Add(stackPanel);
            dictionaryOfTextBoxListAddProduct[productTypesEnum].Add(textBox);

            listMainStackPanel[index].Children.Remove(listAddButtons[index]);
            listMainStackPanel[index].Children.Add(stackPanel);

            AddProductLineNumber++;
        }

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
                    (dictionaryOfTextBoxListAddProduct[productTypesEnum]).RemoveAt(i);
                    stackPanel.Children.Clear();
                    stackPanel.Margin = new Thickness(0, 0, 0, 0);
                    listOfStackPanelListsAddProduct[index].RemoveAt(i);
                    return;
                }
                i++;
            }
        }

        private void ComboBoxProductType_SelectionChanged(object sender, EventArgs e)
        {
            if (ComboBoxProductType.SelectedValue == null)
            {
                LabelForScrollViewerAddLocalSeller.Visibility = Visibility.Visible;
                return;
            }

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
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Height = 120
            };
            return scrollViewer;
        }

        private Dictionary<ProductType, List<string>> ConvertDictionaryListTextBoxToDictionaryListString(Dictionary<ProductType, List<TextBox>> dictionary)
        {
            var dictionaryListString = new Dictionary<ProductType, List<string>>();

            foreach (var elementTextBox in dictionary)
            {
                if (elementTextBox.Value == null)
                {
                    continue;
                }
                var listString = new List<string>();
                foreach (var elementOfList in elementTextBox.Value)
                {
                    if (elementOfList == null)
                    {
                        continue;
                    }
                    if (elementOfList.Text.Trim() == "")
                    {
                        continue;
                    }
                    listString.Add(elementOfList.Text.Trim());
                }
                dictionaryListString.Add(elementTextBox.Key, listString);
            }
            return dictionaryListString;
        }


        private async void FindLocation_Click(object sender, RoutedEventArgs e)
        {
            var center = await GetUserLocation(TextBox1Seller.Text.Trim());

            if (center == null)
            {
                DisplayInformationMessage("No location selected");
                return;
            }

            var searchItems = GetSearchProducts();
            var radius = Slider1Seller.Value * 1000;
            if (searchItems == null)
                searchItems = new List<string>();

            _ = AddMarkersWithinRange(center, radius, string.Join(",", searchItems));
        }


        private async Task AddMarkersWithinRange(Location center, double radius, string searchItems = "")
        {
            // UI operations need to be within the Invoke call, so they would execute on the UI thread
            Dispatcher.Invoke(() =>
            {
                SYLMap.RemoveAllMarkers();

                SYLMap.AddMarkerTemp(center);
                SYLMap.Center = center;
                SYLMap.DrawRadiusOnTempMarker(radius);
            });

            var markers = await MapUtilityProvider.GetSellersWithinRange(center.Latitude, center.Longitude, radius, searchItems);

            Dispatcher.Invoke(() =>
            {
                markers.ForEach(m => SYLMap.AddMarker(m.sellerData.Location, m.sellerData.ID));
            });

            if (searchItems != null)
            {
                DisplayInformationMessage("Local sellers in the radius were loaded by your search phrase");
                return;
            }

            DisplayInformationMessage("All local sellers in radius were loaded");
        }

        private async Task<Location> GetUserLocation(string searchPhrase)
        {
            if(!CheckSearchSellerInput(searchPhrase, TextBox2Seller.Text.Trim(), TextBox3Seller.Text.Trim()))
            {
                return null;
            }

            var location = SYLMap.GetMarkerTempLocation();

            if (location != null)
            {
                return location;
            }

            if (TextBox3Seller.Text.Trim() == "")
            {
                return null; // Show error that address box is empty
            }

            var address = TextBox3Seller.Text;
            if (TextBox2Seller.Text != "")
            {
                address = "{0}, {1}".Format(address, TextBox2Seller.Text);
            }

            var locationTouple = await MapUtilityProvider.AddressToLocation(address);
            return new Location(locationTouple.Item1, locationTouple.Item2);
        }

        private List<string> GetSearchProducts()
        {
            if (TextBox1Seller.Text.Trim() == "")
            {
                return null;
            }

            var searchItems = TextBox1Seller.Text.Split(',').ToList();

            // Cleanup the search item list
            for (int i = 0; i < searchItems.Count; i++)
            {
                searchItems[i] = searchItems[i].Trim();
                if (searchItems[i] == "")
                {
                    searchItems.RemoveAt(i);
                    i--;
                }
            }

            return searchItems;
        }

        async void OnMarkerClicked(Marker marker)
        {
            await LoadMarkerInformationWindow(marker.id);
        }

        private async Task LoadMarkerInformationWindow(string id)
        {
            GridMarkerInformation.Visibility = Visibility.Visible;
            var items = new List<MarkerInformation>();

            var locationData = await sellerData.GetData(id);
            InformationLocalSellerName.Content = locationData.Name;
            InformationLocalSellerDate.Content = locationData.Time;
            foreach (var products in locationData.Products)
            {
                foreach (var product in products.Value)
                {
                    items.Add(new MarkerInformation { ProductType = products.Key.ToString(), ProductCount = products.Value.Count, Product = product });
                }
            }
            ListViewMarkerInformation.ItemsSource = items;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewMarkerInformation.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("ProductType");
            view.GroupDescriptions.Add(groupDescription);
        }

        private async void AddSellerInformationToMarketplaceInformationWindow(string id)
        {
            var items = new List<MarkerInformation>();

            var locationData = await sellerData.GetData(id);
            foreach (var products in locationData.Products)
            {
                foreach (var product in products.Value)
                {
                    items.Add(new MarkerInformation { ProductType = products.Key.ToString(), ProductCount = products.Value.Count, Product = product });
                }
            }
            ListViewMarketplaceInformation.ItemsSource = items;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewMarketplaceInformation.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("ProductType");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void ButtonCloseMarkerInformation_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxMarketplacesInInformation.Items.Clear();
            ComboBoxChooseMarketplace.Items.Clear();
            LabelMarketplaceAddedTime.Content = "Location added (Time)";
            ListViewMarketplaceInformation.ItemsSource = null;
            GridMarketplaceInformation.Visibility = Visibility.Collapsed;
            SYLMap.RemoveAllMarkers();
        }

        private void ClearSearchSellerWindow()
        {
            TextBox1Seller.Clear();
            TextBox2Seller.Clear();
            TextBox3Seller.Clear();
        }

        private bool CheckSearchSellerInput(string searchPhrase, string city, string adress)
        {
            Regex regexSearchPhrase = new Regex(@"^[a-zA-ZĄ-ž0-9-,. ]*$");
            Regex regexLocation = new Regex(@"^[a-zA-ZĄ-ž0-9-,. ].{1,}$");
            string errorMessage = "";
            if (!regexSearchPhrase.IsMatch(searchPhrase))
            {
                errorMessage += "\"Search phrase\" ";
            }
            if (!regexLocation.IsMatch(city))
            {
                errorMessage += "\"City\" ";
            }
            if (!regexLocation.IsMatch(adress))
            {
                errorMessage += "\"Adress\" ";
            }
            if(errorMessage.Length > 0)
            {
                MessageBox.Show("Invalid input in field(s): " + errorMessage, "Invalid input",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool CheckAddSellerInput(string name, Dictionary<ProductType, List<string> > dictionaryProducts)
        {
            Regex regexSellerName = new Regex(@"^$|^[a-zA-ZĄ-ž0-9-,. ].{0,16}$");
            string errorMessage = "";
            foreach (var products in dictionaryProducts.Values)
            {
                if (!regexSellerName.IsMatch(name))
                {
                    if(name.Length > 16)
                    {
                        errorMessage += "\"Invalid name length (must be less than 16 characters)\" ";
                    }
                    else
                    {
                        errorMessage += "\"Invalid name\"";
                    }

                }
                if(ComboBoxProductType.SelectedIndex < 0)
                {
                    errorMessage += "\"Select product type\" ";
                }
                if (!CheckAddSellerProducts(products))
                {
                    errorMessage += "\"Invalid product(s)\" ";
                }
                if (errorMessage.Length > 0)
                {
                    MessageBox.Show("Input error: " + errorMessage, "Invalid input",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }

        private bool CheckAddSellerProducts(List<string> products)
        {
            Regex regexProduct = new Regex(@"^[a-zA-ZĄ-ž0-9-. ]*$");
            foreach (var product in products)
            {
                if (!regexProduct.IsMatch(product))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckProduct(string product)
        {
            Regex regexProduct = new Regex(@"^[a-zA-ZĄ-ž0-9-. ]+$");
            if (!regexProduct.IsMatch(product))
            {
                if(product.Length > 0)
                {
                    DisplayErrorMessage("Invalid action. Incorrect product characters");
                }
                else
                {
                    DisplayErrorMessage("Invalid action. Fill this textfield first");
                }
                return false;
            }
            return true;
        }

        private void DisplayInformationMessage(string message)
        {
            MessageBox.Show(message, "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisplayErrorMessage(string message)
        {
            MessageBox.Show(message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void FilterByOpenTimeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FilterByOpenTimeStackPanel.Visibility = Visibility.Visible;
        }

        private void FilterByOpenTimeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxMarketplaceSearchOpenTime.Clear();
            FilterByOpenTimeStackPanel.Visibility = Visibility.Collapsed;
        }

        private void FilterByProductsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FilterByProductsStackPanel.Visibility = Visibility.Visible;
        }

        private void FilterByProductsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxMarketplaceSearchProducts.Clear();
            FilterByProductsStackPanel.Visibility = Visibility.Collapsed;
        }

        private void ButtonFindMarketplaces_Click(object sender, RoutedEventArgs e)
        {
            ClearFindMarketplaceFields();
            LoadMarketplacesInformationGrid();
            GridMarketplaceInformation.Visibility = Visibility.Visible;
        }

        private async void LoadMarketplacesInformationGrid()
        {
            foreach (var marketplace in await marketplaceData.GetAllData())
            {
                if(SYLMap.LocationToAddressSplit(marketplace.Location).Item2 == TextBoxMarketplaceLocation.Text)
                {
                    ComboBoxMarketplacesInInformation.Items.Add(marketplace.Name);
                }
            }
        }

        private void ClearFindMarketplaceFields()
        {
            ComboBoxMarketplaceDistrict.Items.Clear();
            ComboBoxMarketplaceLocation.Items.Clear();
            ComboBoxMarketplaceDistrict.Text = "";
            ComboBoxMarketplaceLocation.Text = "";
            TextBoxMarketplaceSearchOpenTime.Clear();
            TextBoxMarketplaceSearchProducts.Clear();
            CheckBoxFilterByOpenTime.IsChecked = false;
            CheckBoxFilterByProducts.IsChecked = false;
        }
        private async void LoginButton_Clicked(object sender, RoutedEventArgs e)
        {
            LabelEmptyFieldsError.Visibility = Visibility.Collapsed;
            LabelUsernameError.Visibility = Visibility.Collapsed;
            LabelPasswordError.Visibility = Visibility.Collapsed;

            var usernameExists = false;
            string pswHash = null;
            string offlineUserHash = null;
            string username = UsernameTextBox.Text;

            var userData = await userLoginData.GetAllData();

            foreach (var user in userData)
            {
                if (user.Username != username)
                {
                    continue;
                }
                pswHash = user.PasswordHash;
                usernameExists = true;
                offlineUserHash = UserData.GenerateHash(PasswordBox.Password, user.Salt);
                break;
            }

            if (username.Length == 0 || PasswordBox.Password.Length == 0)
            {
                LabelEmptyFieldsError.Visibility = Visibility.Visible;
            }
            else if (!usernameExists)
            {
                LabelUsernameError.Visibility = Visibility.Visible;
            }
            else if(pswHash != offlineUserHash)
            {
                LabelPasswordError.Visibility = Visibility.Visible;
            }
            else
            {
                ShowUsernameLabel.Content = username;

                GridLogin.Visibility = Visibility.Collapsed;
                GridRegistration.Visibility = Visibility.Collapsed;
            }

            GridUserData.Visibility = Visibility.Collapsed;
            GridUserDataToLogin.Visibility = Visibility.Collapsed;
        }

        private async void RegisterButton_Clicked(object sender, RoutedEventArgs e)
        {
            var password = PasswordBoxR.Password;
            var username = UsernameTextBoxR.Text;

            LabelForUsedUsernameError.Visibility = Visibility.Collapsed;
            LabelForInvalidUsernameError.Visibility = Visibility.Collapsed;
            LabelForPasswordError.Visibility = Visibility.Collapsed;
            LabelForConfirmPasswordError.Visibility = Visibility.Collapsed;
            LabelForEmptyFieldsError.Visibility = Visibility.Collapsed;

            if (await CheckRegisterData(password, username))
            {
                await userLoginData.AddData(new UserData(username, password));
                await userLoginData.SaveData();

                ShowUsernameLabel.Content = username;

                GridLogin.Visibility = Visibility.Collapsed;
                GridRegistration.Visibility = Visibility.Collapsed;
            }

            GridUserData.Visibility = Visibility.Collapsed;
            GridUserDataToLogin.Visibility = Visibility.Collapsed;
        }

        private void LoginWhenRegistered_Clicked(object sender, RoutedEventArgs e)
        {
            GridLogin.Visibility = Visibility.Visible;
            GridRegistration.Visibility = Visibility.Collapsed;
        }

        public async Task<bool> CheckRegisterData(string password, string username)
        {
            var usernameExists = false;
            var userData = await userLoginData.GetAllData();

            foreach (var user in userData)
            {
                if (user.Username != username)
                {
                    continue;
                }
                usernameExists = true;
                break;
            }

            if (password.Length == 0 || username.Length == 0 || ConfirmPasswordBoxR.Password.Length == 0)
            {
                LabelForEmptyFieldsError.Visibility = Visibility.Visible;
                return false;
            }
            else if (usernameExists)
            {
                LabelForUsedUsernameError.Visibility = Visibility.Visible;
            }
            else
            {
                if(!CheckUsernameRegex(username))
                {
                    LabelForInvalidUsernameError.Visibility = Visibility.Visible;
                }
                else // if username is correct
                {
                    if(!CheckPasswordLength(password))
                    {
                        LabelForPasswordError.Visibility = Visibility.Visible;
                    }
                    else if(PasswordBoxR.Password == ConfirmPasswordBoxR.Password) // if password is correct
                    {
                        return true;
                    }
                    else
                    {
                        LabelForConfirmPasswordError.Visibility = Visibility.Visible;
                    }
                }
            }
            return false;
        }

        private bool CheckPasswordLength(string password) => password.Length >= 6;

        private bool CheckUsernameRegex(string username)
        {
            Regex regexUsername = new Regex(@"^[a-zA-ZĄ-ž0-9-. ]+$");

            if (!regexUsername.IsMatch(username))
            {
                return false;
            }
            return true;
        }

        private void NewUser_Clicked(object sender, RoutedEventArgs e)
        {
            GridLogin.Visibility = Visibility.Collapsed;
            GridRegistration.Visibility = Visibility.Visible;
        }

        private void User_Clicked(object sender, RoutedEventArgs e)
        {
            GridLogin.Visibility = Visibility.Visible;
            GridRegistration.Visibility = Visibility.Collapsed;
        }

        private void ShowLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            GridLogin.Visibility = Visibility.Visible;
            UsernameTextBox.Text = "";
            PasswordBox.Password = "";

            UsernameTextBoxR.Text = "";
            PasswordBoxR.Password = "";
            ConfirmPasswordBoxR.Password = "";

            ShowUsernameLabel.Content = "";
            GridUserData.Visibility = Visibility.Collapsed;

            GridUserData.Visibility = Visibility.Collapsed;
            GridUserDataToLogin.Visibility = Visibility.Collapsed;
        }
        private void ShowLoginButton_Clicked(object sender, RoutedEventArgs e)
        {
            LoginHeader.Visibility = Visibility.Visible;
            GridLogin.Visibility = Visibility.Visible;
            GridRegistration.Visibility = Visibility.Collapsed;

            GridUserData.Visibility = Visibility.Collapsed;
            GridUserDataToLogin.Visibility = Visibility.Collapsed;
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            if(!ShowUsernameLabel.Content.Equals(""))
            {
                if(GridUserData.Visibility == Visibility.Visible)
                {
                    GridUserData.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GridUserData.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (GridUserDataToLogin.Visibility == Visibility.Visible)
                {
                    GridUserDataToLogin.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GridUserDataToLogin.Visibility = Visibility.Visible;
                }
            }
        }

        private void Slider1Seller_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextBoxSliderValue == null)
            {
                TextBoxSliderValue = new TextBox();
            }
            TextBoxSliderValue.Text = Slider1Seller.Value.ToString("F2");
            SYLMap?.DrawRadiusOnTempMarker(e.NewValue * 1000.0);
        }

        private void ButtonAddNewMarketPlace_Click(object sender, RoutedEventArgs e)
        {
            var timePairTimes = new List<TimePair>();
            var timePair = new TimePair
            {
                StartTime = new Time(TextBoxStartTime.Text),
                EndTime = new Time(TextBoxEndTime.Text)
            };

            marketplaceData.AddData(
                new MarketplaceData(
                    SYLMap.GetMarkerTempLocation(), TextBoxNewMarketplace.Text.Trim(), null, null, null
                                    ));
            marketplaceData.SaveData();
            ClearUpdateMarketplaceWindow();
            SYLMap.RemoveAllMarkers();
            GridMarketplacesAdd.Visibility = Visibility.Collapsed;
        }

        private void ClearUpdateMarketplaceWindow()
        {
            TextBoxNewMarketplace.Text = "";
            TextBoxStartTime.Text = "";
            TextBoxEndTime.Text = "";
        }

        private async void ComboBoxMarketplaceInformatione_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteAllMarkersExceptTemp();
            ComboBoxChooseMarketplace.Items.Clear();
            var marketplaces = await marketplaceData.GetAllData();
            foreach (var seller in await sellerData.GetAllData())
            {
                foreach (var marketplace in marketplaces)
                {
                    if (ComboBoxMarketplacesInInformation.SelectedValue == null)
                    {
                        continue;
                    }
                    if (marketplace.Name != ComboBoxMarketplacesInInformation.SelectedValue.ToString())
                    {
                        continue;
                    }
                    SYLMap.AddMarkerTemp(marketplace.Location);
                    if (SYLMap.GetDistance(seller.Location, marketplace.Location) < 100)
                    {
                        ComboBoxChooseMarketplace.Items.Add(seller.Name);
                    }
                }
            }
        }

        private async void ComboBoxChooseMarketplace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var seller in await sellerData.GetAllData())
            {
                if (ComboBoxChooseMarketplace.SelectedValue == null)
                {
                    continue;
                }
                if (ComboBoxChooseMarketplace.SelectedValue.ToString() != seller.Name)
                {
                    continue;
                }
                LabelMarketplaceAddedTime.Content = seller.Time;
                AddSellerInformationToMarketplaceInformationWindow(seller.ID);
                DeleteAllMarkersExceptTemp();
                SYLMap.AddMarker(seller.Location, seller.ID);
            }
        }

        private void DeleteAllMarkersExceptTemp()
        {
            var tempMarkerLocation = SYLMap.GetMarkerTempLocation();
            SYLMap.RemoveAllMarkers();
            SYLMap.AddMarkerTemp(tempMarkerLocation);
        }

        private void ButtonCloseNewMarketPlace_Click(object sender, RoutedEventArgs e)
        {
            ClearUpdateMarketplaceWindow();
            GridMarketplacesAdd.Visibility = Visibility.Collapsed;
        }
    }

    public struct MarkerInformation
    {
        public string ProductType { get; set; }
        public int ProductCount { get; set; }
        public string Product { get; set; }
    }

    public class MarkerData
    {
        public Location Location { get; set; }
        public string ID { get; set; }

        public MarkerData (Location location, string id)
        {
            Location = location;
            ID = id;
        }
    }
}
