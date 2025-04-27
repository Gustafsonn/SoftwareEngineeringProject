using SET09102.Models;
using SET09102.Services;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Microsoft.Maui.Controls;

namespace SET09102.EnvironmentalScientist.Pages
{
    public partial class HistoricalDataPage : ContentPage
    {
        private readonly HistoricalDataViewModel _viewModel;
        private readonly DatabaseService _databaseService;

        public HistoricalDataPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _viewModel = new HistoricalDataViewModel(_databaseService);
            BindingContext = _viewModel;
            
            // Set default dates
            StartDatePicker.Date = DateTime.Now.AddDays(-7);
            EndDatePicker.Date = DateTime.Now;
            
            // Set default data type if not already set
            if (DataTypePicker.SelectedIndex < 0)
                DataTypePicker.SelectedIndex = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Load data if we already have filter selections
            if (DataTypePicker.SelectedIndex >= 0)
            {
                _ = LoadDataAsync();
            }
        }

        private async void OnDataTypeChanged(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }
        
        private async void OnDateRangeChanged(object sender, DateChangedEventArgs e)
        {
            await LoadDataAsync();
        }
        
        private async void OnMetricChanged(object sender, EventArgs e)
        {
            if (MetricPicker.SelectedIndex >= 0)
            {
                string? metric = MetricPicker.SelectedItem?.ToString();
                if (metric != null)
                {
                    await _viewModel.UpdateSelectedMetricAsync(metric);
                    StatsPanel.IsVisible = _viewModel.HasData;
                }
            }
        }
        
        private async Task LoadDataAsync()
        {
            if (DataTypePicker.SelectedIndex < 0 || StartDatePicker.Date > EndDatePicker.Date)
                return;
                
            string? dataType = DataTypePicker.SelectedItem?.ToString();
            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date;
            
            NoDataLabel.IsVisible = false;
            
            if (dataType != null)
            {
                await _viewModel.LoadDataAsync(dataType, startDate, endDate);
            }
            
            // Show empty state message if no data
            NoDataLabel.IsVisible = !_viewModel.HasData;
            StatsPanel.IsVisible = _viewModel.HasData;
            
            // Setup the metrics picker based on data type
            if (_viewModel.HasData && dataType != null)
            {
                SetupMetricPicker(dataType);
            }
        }

        private void SetupMetricPicker(string dataType)
        {
            MetricPicker.Items.Clear();
            
            switch (dataType)
            {
                case "Air Quality":
                    MetricPicker.Items.Add("Nitrogen Dioxide");
                    MetricPicker.Items.Add("Sulphur Dioxide");
                    MetricPicker.Items.Add("PM2.5 Particulate Matter");
                    MetricPicker.Items.Add("PM10 Particulate Matter");
                    break;
                case "Water Quality":
                    MetricPicker.Items.Add("Nitrate");
                    MetricPicker.Items.Add("Nitrite");
                    MetricPicker.Items.Add("Phosphate");
                    MetricPicker.Items.Add("E. Coli");
                    break;
                case "Weather":
                    MetricPicker.Items.Add("Temperature");
                    MetricPicker.Items.Add("Relative Humidity");
                    MetricPicker.Items.Add("Wind Speed");
                    MetricPicker.Items.Add("Wind Direction");
                    break;
            }
            
            if (MetricPicker.Items.Count > 0)
            {
                MetricPicker.SelectedIndex = 0;
                OnMetricChanged(this, EventArgs.Empty);
            }
        }
        
        // Add the missing event handler with correct signature
        private async void OnDataItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;
                
            var selectedItem = e.CurrentSelection[0] as EnvironmentalDataPoint;
            
            // Deselect the item
            ((CollectionView)sender).SelectedItem = null;
            
            // Show detail view if item selected
            if (selectedItem != null)
            {
                string detailMessage = $"Date/Time: {selectedItem.Timestamp:g}\n" +
                                      $"Location: {selectedItem.Location}\n" +
                                      $"Value: {selectedItem.Value:F2} {selectedItem.Unit}\n" +
                                      $"Status: {selectedItem.Status}\n\n" +
                                      $"Data Type: {selectedItem.DataType}\n" +
                                      $"Sensor: {selectedItem.SensorName}\n";
                
                // Add comparison to thresholds if available
                if (_viewModel.CurrentThreshold.HasValue)
                {
                    detailMessage += $"\nSafe Threshold: {_viewModel.CurrentThreshold:F2} {selectedItem.Unit}\n";
                    
                    if (selectedItem.Value > _viewModel.CurrentThreshold)
                    {
                        detailMessage += $"⚠️ Value exceeds safe threshold by {selectedItem.Value - _viewModel.CurrentThreshold:F2} {selectedItem.Unit}";
                    }
                    else
                    {
                        detailMessage += $"✓ Value is within safe threshold";
                    }
                }
                
                await DisplayAlert($"{selectedItem.DataType} Details", detailMessage, "Close");
            }
        }
        
        private async void OnExportDataClicked(object sender, EventArgs e)
        {
            if (!_viewModel.HasData)
                return;
                
            try
            {
                string? dataType = DataTypePicker.SelectedItem?.ToString();
                string? metric = MetricPicker.SelectedItem?.ToString();
                
                if (dataType == null || metric == null)
                    return;
                    
                string fileName = $"{dataType}_{metric}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                StringBuilder csv = new StringBuilder();
                
                // Add header
                csv.AppendLine("Date,Time,Location,Value,Unit,Status");
                
                // Add data rows
                foreach (var dataPoint in _viewModel.HistoricalData)
                {
                    csv.AppendLine($"{dataPoint.Timestamp:yyyy-MM-dd},{dataPoint.Timestamp:HH:mm:ss},{dataPoint.Location},{dataPoint.Value},{dataPoint.Unit},{dataPoint.Status}");
                }
                
                // Save file to app's data directory
                string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                File.WriteAllText(filePath, csv.ToString());
                
                await DisplayAlert("Export Successful", $"Data exported to:\n{filePath}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Export Failed", $"Error exporting data: {ex.Message}", "OK");
            }
        }
    }
}