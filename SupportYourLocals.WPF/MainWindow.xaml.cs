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
using System.Runtime.CompilerServices;

namespace SupportYourLocals.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i = 0;
        private int AddProductLineNumber = 2;

        private Map.Map SYLMap;
        private readonly IDataStorage data;

        private bool updateMarketplacesWasClicked = false;

        private bool userSelectedLocation = false;


        List<double> listXCoord = new List<double>();
        List<double> listYCoord = new List<double>();
        List<int> listPersonsID = new List<int>();

        // List for StackPanel elements in Main StackPanel
        //Unnecessary
        List<List<StackPanel>> listOfStackPanelListsAddProduct = new List<List<StackPanel>>();
        List<StackPanel> listStackPanelAddProduct = new List<StackPanel>();
        List<Button> listAddButtons = new List<Button>();
        //List<Button> listButtonRemoveProduct = new List<Button>();

        // List for Main StackPanels
        // These stackpanels consist of other stackpanels and buttons. One Main stack panel represent one type of products, in instance- fruits
        List<StackPanel> listMainStackPanel = new List<StackPanel>();
        // Dictionaries to load scrollviews and store data based on chosen enum
        Dictionary<string, ScrollViewer> dictionaryOfScrollViewsAddProduct = new Dictionary<string, ScrollViewer>();
        Dictionary<string, List<TextBox>> dictionaryOfTextBoxListAddProduct = new Dictionary<string, List<TextBox>>();

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


            // Adding lists to store stack panels and buttons for adding products (in AddLocalSeller)
            ///listStackPanelAddProduct.Add(StackPanelAddLocalSellerProductsOfElements1);
            ///listButtonRemoveProduct.Add(AddLocalSellerRemoveProductButton1);

            // Filling combobox for product types and filling dictionaries
            var productTypes = Enum.GetValues(typeof(Data.ProductType));

            //dictionaryOfStackPanelListsAddProduct.Add("listStackPanelAddProductfruits", new List<StackPanel>());
            ///dictionaryOfStackPanelListsAddProduct["listStackPanelAddProductfruits"].Add(StackPanelAddLocalSellerProductsOfElements1);
            foreach (var productType in productTypes)
            {
                // Adding elements to combobox
                ComboBoxProductType.Items.Add(productType);
                
                // Create first textBox and button and add to the secondary stack panel
                var stackPanel = createStackPanelForProductTypes(productType);
                var textBox = createTextFieldForProductTypes();
                stackPanel.Children.Add(textBox);
                var button = createButtonForProductTypes(productType.ToString(), "+", AddLocalSellerAddProduct1_Click, "AddLocalSellerAddTextFieldButton" + productType.ToString());
                stackPanel.Children.Add(button);
                listAddButtons.Add(button);

                //create a temporary list to store first textbox of a stack panel
                var listTextBoxes = new List<TextBox>();
                listTextBoxes.Add(textBox);
                dictionaryOfTextBoxListAddProduct.Add("textBoxList" + productType.ToString(), listTextBoxes);

                // Everything is already untill this line -------------

                // Temporary list for inner list of listOfListsAddProduct
                //(listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1]).Add(stackPanel);
                /*var temporaryListStackPanelAddProduct = new List<StackPanel>();
                temporaryListStackPanelAddProduct.Add(stackPanel);*/
                // Add secondary stack panel to secondary stack panel list of lists. Later on, we will use this list to add/remove textboxes and buttons
                listOfStackPanelListsAddProduct.Add(new List<StackPanel>());
                listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1].Add(stackPanel);
                //listOfStackPanelListsAddProduct.Add(temporaryListStackPanelAddProduct);

                // Create main stack panel and add the secondary stack panel
                var stackPanelMain = new StackPanel();
                stackPanelMain.Name = "stackPanelMain" + productType.ToString();
                //stackPanelMain.Children.Add(temporaryListStackPanelAddProduct[temporaryListStackPanelAddProduct.Count - 1]);
                stackPanelMain.Children.Add((listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1])[(listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1]).Count - 1]);
                //listMainStackPanel.Add((listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1])[(listOfStackPanelListsAddProduct[listOfStackPanelListsAddProduct.Count - 1]).Count - 1]);
                listMainStackPanel.Add(stackPanelMain);

                // Create new instance of a scroll viewer for an enum and add main stack panel
                var scrollViewer = createScrollViewerForProductTypes();
                scrollViewer.Name = "scrollViewerMain" + productType.ToString();
                scrollViewer.Content = listMainStackPanel[listMainStackPanel.Count - 1];
                scrollViewer.Visibility = Visibility.Collapsed;

                dictionaryOfScrollViewsAddProduct.Add("scrollViewerMain" + productType.ToString(), scrollViewer);

                StackPanelWithScrollViewerAddSellers.Children.Add(scrollViewer);
                //ScrollViewerAddLocalSeller.Content = stackPanelMain;
                //dictionaryOfButtonListsRemoveProduct.Add((Enum)productType, listButtonRemoveProduct);
                // Creating lists for each Enum
                //StackPanelAddLocalSellerProductsOfElements1.Tag = productType.ToString();
                /*dictionaryOfStackPanelListsAddProduct.Add("listStackPanelAddProduct" + productType.ToString(), new List<StackPanel>());
                dictionaryOfStackPanelListsAddProduct["listStackPanelAddProduct" + productType.ToString()].Add(StackPanelAddLocalSellerProductsOfElements1);*/

                /*dictionaryOfButtonListsRemoveProduct.Add("listButtonRemoveProduct" + productType.ToString(), new List<Button>());
                dictionaryOfButtonListsRemoveProduct["listButtonRemoveProduct" + productType.ToString()].Add(AddLocalSellerRemoveProductButton1);*/

                //listMainStackPanel.Add()
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
            SearchMarketplacesButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            SearchSellersButton.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void AddLocalSellerAddProduct1_Click(object sender, RoutedEventArgs e)
        {
            var stackPanel = createStackPanelForProductTypeElements();

            var textBox = createTextFieldForProductTypes();

            var productType = ComboBoxProductType.SelectedItem.ToString();

            var button = createButtonForProductTypes(productType, "―", AddLocalSellerRemoveProduct1_Click, null);

            stackPanel.Children.Add(textBox);
            //dictionaryOfScrollViewsAddProduct["scrollViewerMain" + productType.ToString()].
            
            //int index = Array.IndexOf(Enum.GetValues(ComboBoxProductType.SelectedItem.GetType()), e);
            int index = ComboBoxProductType.SelectedIndex;
            // Remove "+" button from the last line before new line (textbox) is added
            (listOfStackPanelListsAddProduct[index])[listOfStackPanelListsAddProduct[index].Count - 1].Children.Remove(listAddButtons[index]);

            // Add "—" button to the last line before new line is inicialized
            (listOfStackPanelListsAddProduct[index])[listOfStackPanelListsAddProduct[index].Count - 1].Children.Add(createButtonForProductTypes(productType, "—", AddLocalSellerRemoveProduct1_Click, null));


            //temporaryList[temporaryList.Count - 1].Children.Add(createButtonForProductTypes(productType, "—", AddLocalSellerRemoveProduct1_Click, null)); 
            stackPanel.Children.Add(listAddButtons[index]);

            listOfStackPanelListsAddProduct[index].Add(stackPanel);
            //(listOfStackPanelListsAddProduct[index])[listOfStackPanelListsAddProduct[index].Count - 1].Children.Add(listAddButtons[index]);


            //dictionaryOfScrollViewsAddProduct["scrollViewerMain" + productType.ToString()].Content = listMainStackPanel[index];


            listMainStackPanel[index].Children.Remove(listAddButtons[index]);
            //listMainStackPanel[index].Children.Add(createButtonForProductTypes(productType, "—", AddLocalSellerRemoveProduct1_Click, null));
            listMainStackPanel[index].Children.Add(stackPanel);
            

            // Add "+" button the the new line
            //stackPanel.Children.Add(listAddButtons[index]);



            //listOfStackPanelListsAddProduct[index].
            //listMainStackPanel[index].Children.Add(button);
            // index is 2
            //listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Children.Add(button); //Add minus button
            ///listButtonRemoveProduct.Add(button);
            ///listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Children.Remove(AddLocalSellerAddProductButton1);//Remove Plus button
            ///



            //listStackPanelAddProduct.Add(stackPanel);



            ///listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Children.Add(AddLocalSellerAddProductButton1);
            //StackPanelAddLocalSellerProducts.Children.Add(stackPanel);

            AddProductLineNumber++;
            //listStackPanelAddProduct[listStackPanelAddProduct.Count - 1].Margin = new Thickness(25, 0, 0, 5);


            //Reworking with dictionaries
            /*dictionaryOfStackPanelListsAddProduct.Add("listStackPanelAddProduct" + productType.ToString(), new List<StackPanel>());
            dictionaryOfStackPanelListsAddProduct["listStackPanelAddProduct" + productType.ToString()].Add(StackPanelAddLocalSellerProductsOfElements1);

            dictionaryOfButtonListsRemoveProduct.Add("listButtonRemoveProduct" + productType.ToString(), new List<Button>());
            dictionaryOfButtonListsRemoveProduct["listButtonRemoveProduct" + productType.ToString()].Add(AddLocalSellerRemoveProductButton1);*/



        }

        //after clicking save button, save only values of these textfields which visibility = Visible
        private void AddLocalSellerRemoveProduct1_Click(object sender, RoutedEventArgs e)
        {
            int index = ComboBoxProductType.SelectedIndex;
            Button button = sender as Button;
            button.Visibility = Visibility.Collapsed;
            //listOfStackPanelListsAddProduct[index].Add(stackPanel);
            foreach (var stackPanel in listOfStackPanelListsAddProduct[index])
            {
                if (stackPanel.Children.Contains(button))
                {
                    stackPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ComboBoxProductType_SelectionChanged(object sender, EventArgs e)
        {
            
            var productType = ComboBoxProductType.SelectedItem.ToString();

            LabelForScrollViewerAddLocalSeller.Visibility = Visibility.Collapsed;

            dictionaryOfScrollViewsAddProduct["scrollViewerMain" + productType.ToString()].Visibility = Visibility.Visible;
            /*foreach(var element in dictionaryOfButtonListsRemoveProduct)
            {
                if(element.Key != "scrollViewerMain" + productType.ToString())
                {
                    element[element.Key]
                }
            }*/

            foreach(KeyValuePair<string, ScrollViewer> entry in dictionaryOfScrollViewsAddProduct)
            {
                if(entry.Key != "scrollViewerMain" + productType.ToString())
                {
                    entry.Value.Visibility = Visibility.Collapsed;
                }
            }
            /*var stackPanel = new StackPanel();
            foreach(var element in dictionaryOfStackPanelListsAddProduct[productType])
            {
                stackPanel.Children.Add(element);
            }*/

            //stackPanel.Children.Add((StackPanel)dictionaryOfStackPanelListsAddProduct[productType]);
            //ScrollViewerAddLocalSeller.Content = stackPanel;
            //dictionaryOfStackPanelListsAddProduct["listStackPanelAddProduct" + productType];
            /* var childStackPanel = dictionaryOfStackPanelListsAddProduct["listStackPanelAddProduct" + productType];
             dictionaryOfStackPanelListsAddProduct["listStackPanelAddProduct" + productType].*/
        }

        private Button createButtonForProductTypes(string enumValue, string content, Action<object, RoutedEventArgs> actionOnClickName, string buttonName)
        {
            var button = new Button();
            button.Height = 25;
            button.Width = 25;

            button.Content = content;

            button.FontWeight = FontWeights.Bold;
            button.Name = buttonName;
            button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            button.BorderBrush = null;
            button.Background = null;
            button.Click += new RoutedEventHandler(actionOnClickName);
            return button;
        }
        private TextBox createTextFieldForProductTypes()
        {
            var textBox = new TextBox();
            ///textBox.Name = "AddLocalSellerAddProductTextBox" + AddProductLineNumber;
            textBox.HorizontalAlignment = HorizontalAlignment.Center;
            textBox.VerticalAlignment = VerticalAlignment.Top;
            textBox.Height = 23;
            textBox.Width = 160;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            textBox.Text = i.ToString();
            i++;
            return textBox;
        }

        private StackPanel createStackPanelForProductTypeElements()
        {
            var stackPanel = new StackPanel();
            ///stackPanel.Name = "StackPanelAddLocalSellerProductsOfElements" + AddProductLineNumber;
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            stackPanel.Margin = new Thickness(40, 0, 0, 5);
            return stackPanel;
        }
        private StackPanel createStackPanelForProductTypes(object productType)
        {
            var stackPanel = createStackPanelForProductTypeElements();
            return stackPanel;
        }

        private ScrollViewer createScrollViewerForProductTypes()
        {
            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            //scrollViewer.Margin = new Thickness(0, 150, 0, 130);
            scrollViewer.HorizontalContentAlignment = HorizontalAlignment.Center;
            scrollViewer.VerticalContentAlignment = VerticalAlignment.Center;
            scrollViewer.Height = 120;
            return scrollViewer;
        }
    }
}
