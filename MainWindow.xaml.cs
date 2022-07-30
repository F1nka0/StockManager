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
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Helpers;
using YahooFinanceApi;
using System.Timers;
namespace stockManager
{
    public class Stock {
        public Stock(string StockSymbols)
        {
            Symbols = StockSymbols;
            StockGraph.Title = Symbols;
            StockGraph.Values = new ChartValues<double>();
        }
        public string Symbols { get; }
        private List<double> PriceHistory { get; } = new List<double>();
        public void AddLastKnownPrice(double price) {
            PriceHistory.Add(price);
            StockGraph.Values.Add(price);
        }
        public List<double> GetPriceDataSet() {
            return PriceHistory;
        }
        public double GetLastKnownPrice() {
            return PriceHistory.Last();
        }
        public LineSeries StockGraph { get; } = new LineSeries();
    }
    public partial class MainWindow : Window
    {
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; } = (src) => "$" + src;
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public List<Stock> Stocks = new List<Stock>();
        public bool IsNeededToSave=false;
        public string SymbolsOfCurrentSelectedStock = null;
        public Timer StockPriceUpdater = new Timer(1000);
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SetupTimer();
            StockPriceUpdater.Start();
        }
        private void SetupTimer() {
            StockPriceUpdater.Elapsed += GetStockPrices;
        }

        public async void GetStockPrices(object sender, ElapsedEventArgs e) {
            IReadOnlyDictionary<string, Security> CurrentStockPrice;
            foreach (Stock stock in Stocks) {
                CurrentStockPrice = await Yahoo.Symbols(stock.Symbols).Fields(Field.RegularMarketPrice).QueryAsync();
                stock.AddLastKnownPrice(CurrentStockPrice[stock.Symbols].RegularMarketPrice);
            }
        }
        private void AddStock_Click(object sender, RoutedEventArgs e)
        {
            StockSetupWindow ver = new StockSetupWindow();
            ver.Show();
        }
        private void RequiredStocksListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (RequiredStocksListBox.SelectedItem == null) {
                return;
            }
            if (SeriesCollection.Any(stock=>stock.Title== RequiredStocksListBox.SelectedItem.ToString())) {
                SeriesCollection.Remove(SeriesCollection.First(sym=>sym.Title== RequiredStocksListBox.SelectedItem.ToString()));
                RequiredStocksListBox.UnselectAll();
                return;
            }
            SymbolsOfCurrentSelectedStock = RequiredStocksListBox.SelectedItem.ToString();
            LineSeries StockPriceData = Stocks.First(stock => stock.Symbols == SymbolsOfCurrentSelectedStock).StockGraph;
            SeriesCollection.Add(StockPriceData);
            RequiredStocksListBox.UnselectAll();
        }
    }
}
