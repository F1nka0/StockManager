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
using System.Windows.Shapes;
using YahooFinanceApi;
using System.IO;

namespace stockManager
{
    /// <summary>
    /// Логика взаимодействия для StockSetupWindow.xaml
    /// </summary>
    public partial class StockSetupWindow : Window
    {
        private static MainWindow BaseWindow = (MainWindow)Application.Current.Windows[0];
        public StockSetupWindow()
        {
            InitializeComponent();
        }
        private async Task<bool> DoStockSymbolsExist(string symbols) {
            if (symbols=="") {
                return false;
            }
            var securities = await Yahoo.Symbols(symbols).Fields(Field.RegularMarketPrice).QueryAsync();
            dynamic ResultHolder;
            try
            {
                ResultHolder = securities[symbols][Field.RegularMarketPrice];
            }
            catch (Exception){
                ResultHolder = null;
            }
            return ResultHolder != null;
        }
        private async void Submit_Button_Click(object sender, RoutedEventArgs e)
        {

            bool DSSEResult = await DoStockSymbolsExist(StockSymbols.Text.ToUpper());
            bool IsPresentInStocksCollection = BaseWindow.RequiredStocksListBox.Items.Contains(StockSymbols.Text.ToUpper());
            if (DSSEResult == true && !IsPresentInStocksCollection)
            {
                StockSymbols.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x42, 0x45, 0x49));
                BaseWindow.RequiredStocksListBox.Items.Add(StockSymbols.Text.ToUpper());
                BaseWindow.RequiredStocksListBox.Items.Refresh();
                BaseWindow.IsNeededToSave = true;
                BaseWindow.Stocks.Add(new Stock(StockSymbols.Text,BaseWindow.Labels.Count));
                this.Close();
            }
            else if (DSSEResult == true && IsPresentInStocksCollection) {
                ErrorMessage.Text = "Stock is already present in the list";
                ErrorMessage.Foreground = Brushes.Red;
            }
            else {
                ErrorMessage.Text = "Stock doesn't exist";
                ErrorMessage.Foreground = Brushes.Red;
            }
        }
    }
}
