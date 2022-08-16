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
using System.IO;
namespace stockManager
{
    public class Stock {
        public Stock(string StockSymbols, int Offset)
        {
            Symbols = StockSymbols;
            StockGraph.Title = Symbols;
            StockGraph.Values = new ChartValues<double>();
            this.Offset = Offset;
            for (int StartOffset = 0; StartOffset < Offset; StartOffset++) {
                StockGraph.Values.Add(Double.NaN);
            }
        }
        public int Offset{ get; }
        public string Symbols { get; }
        public void AddLastKnownPrice(double price){
            StockGraph.Values.Add(price);
        }
        public LineSeries StockGraph { get; } = new LineSeries();
    }
    public partial class MainWindow : Window
    {
        public List<string> Labels { get; set; } = new List<string>();
        public Func<double, string> YFormatter { get; set; } = (src) => "$" + src;
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public List<Stock> Stocks { get; set; } = new List<Stock>();
        public List<string> NeededToSaveCollection = new List<string>();
        public string SymbolsOfCurrentSelectedStock = null;
        public Timer StockPriceUpdateTimer = new Timer(1000);
        public MainWindow()
        {
            InitializeComponent();
            RetrieveStockInfo();
            (this as Window).Closed += SaveStockInfo;
            DataContext = this;
            BaseChart.AnimationsSpeed = new TimeSpan(0,0,0,0,150);
            SetupTimer(StockPriceUpdateTimer);
        }
        private void SaveStockInfo(object sender, EventArgs e) {
            if (NeededToSaveCollection != null)
            {
                using (StreamWriter SaveFile = new StreamWriter(File.Open($"{Directory.GetCurrentDirectory()}\\SavedStocks.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                {//TODO - go from txt to xml
                    foreach (string SymbolSet in NeededToSaveCollection)
                    {
                        SaveFile.Write($"{SymbolSet}@");
                    }
                }
            }
        }
        private void RetrieveStockInfo() {
            string Path = $"{Directory.GetCurrentDirectory()}\\SavedStocks.txt";
            try
            {
                using (StreamReader SaveFile = new StreamReader(File.Open(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)))
                {//TODO - go from txt to xml
                    foreach (string SplittedSymbols in SaveFile.ReadToEnd().Split(new char[] { '@' },StringSplitOptions.RemoveEmptyEntries))
                    {
                        Stocks.Add(new Stock(SplittedSymbols, 0));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            finally
            {
                foreach (Stock Stock in Stocks)
                {
                    RequiredStocksListBox.Items.Add(Stock.Symbols);
                    RequiredStocksListBox.Items.Refresh();
                }
            }
        }
        private void SetupTimer(Timer BaseTimer) {
            StockPriceUpdateTimer.Elapsed += GetStockPrices;
            BaseTimer.Start();
        }

        public async void GetStockPrices(object sender, ElapsedEventArgs e)
        {
            IReadOnlyDictionary<string, Security> CurrentStockPrice;
            if (RequiredStocksListBox.Items.Count != 0)
            {
                foreach (Stock stock in Stocks)
                {
                    CurrentStockPrice = await Yahoo.Symbols(stock.Symbols).Fields(Field.RegularMarketPrice).QueryAsync();
                    stock.AddLastKnownPrice(CurrentStockPrice[stock.Symbols].RegularMarketPrice);

                }
                Labels.Add(DateTime.Now.ToString("HH:mm:ss"));
            }
        }
        private void AddStock_Click(object sender, RoutedEventArgs e)
        {
            StockSetupWindow StockAdditonWindow = new StockSetupWindow();
            StockAdditonWindow.Owner = this;
            StockAdditonWindow.Show();
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
            StockPriceData.PointGeometrySize = 5;
            SeriesCollection.Add(StockPriceData);
            
            RequiredStocksListBox.UnselectAll();
        }

        private void BaseChart_DataClick(object sender, ChartPoint chartPoint)
        {
            
            foreach (var XAxis in BaseChart.AxisX)
            {
                XAxis.MinValue = Stocks.First(stock=>stock.Symbols == chartPoint.SeriesView.Title).Offset;
                XAxis.SetRange(XAxis.MinValue, double.NaN);
            }
            foreach (var YAxis in BaseChart.AxisY)
            {
                YAxis.MinValue = double.NaN;
                YAxis.SetRange(double.NaN, double.NaN);
            }
        }

        private void RemoveStock_Click(object sender, RoutedEventArgs e)
        {
            //Stocks.Remove(Stocks.Where(stock => stock.Symbols == )
        }
    }
}
