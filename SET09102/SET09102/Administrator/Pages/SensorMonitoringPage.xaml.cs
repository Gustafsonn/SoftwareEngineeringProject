using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    public partial class SensorMonitoringPage : ContentPage, INotifyPropertyChanged
    {
        private readonly SensorService _sensorService;
        private readonly SensorSettingsService _sensorSettingsService;
        private ObservableCollection<Sensor> _allSensors = new();
        private ObservableCollection<Sensor> _filteredSensors = new();
        private Sensor _selectedSensor;
        private bool _hasSelectedSensor;
        private string _selectedSensorType = "All Types";
        private string _selectedStatus = "All Status";
        private bool _showActiveOnly = true;
        
        // Status counts for dashboard
        private int _operationalCount;
        private int _maintenanceCount;
        private int _offlineCount;

        public SensorMonitoringPage()
        {
            try
            {
                Debug.WriteLine("SensorMonitoringPage constructor started");
                
                // Skip InitializeComponent() - build UI programmatically instead
                
                string dbPath = new DatabaseService().GetDatabasePath();
                _sensorService = new SensorService(dbPath);
                _sensorSettingsService = new SensorSettingsService(new DatabaseService());
                
                BindingContext = this;
                
                // Build a simplified UI to avoid XAML issues
                BuildSimplifiedUI();
                
                // Load sensors
                Task.Run(async () => await LoadSensorsAsync());
                
                Debug.WriteLine("SensorMonitoringPage constructor completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing SensorMonitoringPage: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                // Create a simple error UI
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(20),
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = "Error Loading Sensor Monitoring",
                            FontSize = 24,
                            TextColor = Colors.Red,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = ex.Message,
                            FontSize = 16,
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 20, 0, 0)
                        },
                        new Button
                        {
                            Text = "Go Back",
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 20, 0, 0)
                        }
                    }
                };
            }
        }

        private void BuildSimplifiedUI()
        {
            // Create a simple loading UI to start with
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Sensor Operational Status Monitoring",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Loading sensor data...",
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    },
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    }
                }
            };
        }

        public ObservableCollection<Sensor> Sensors
        {
            get => _filteredSensors;
            set
            {
                _filteredSensors = value;
                OnPropertyChanged();
            }
        }

        public Sensor SelectedSensor
        {
            get => _selectedSensor;
            set
            {
                _selectedSensor = value;
                HasSelectedSensor = (_selectedSensor != null);
                OnPropertyChanged();
            }
        }

        public bool HasSelectedSensor
        {
            get => _hasSelectedSensor;
            set
            {
                _hasSelectedSensor = value;
                OnPropertyChanged();
            }
        }

        private async Task LoadSensorsAsync()
        {
            try
            {
                Debug.WriteLine("Loading sensors");
                _allSensors = await _sensorService.GetSensorsAsync();
                
                // Make sure _allSensors is never null to avoid crashes
                if (_allSensors == null)
                    _allSensors = new ObservableCollection<Sensor>();
                
                FilterSensors();
                UpdateStatusCounts();
                
                // Update UI on the main thread
                await MainThread.InvokeOnMainThreadAsync(() => {
                    BuildCompleteUI();
                });
                
                Debug.WriteLine($"Loaded {_allSensors.Count} sensors");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadSensorsAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                // Set empty collections to avoid null reference exceptions
                _allSensors = new ObservableCollection<Sensor>();
                Sensors = new ObservableCollection<Sensor>();
                
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await DisplayAlert("Error", $"Failed to load sensors: {ex.Message}", "OK");
                });
            }
        }

        private void BuildCompleteUI()
        {
            // Build a more complex UI once data is loaded
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },
                Padding = new Thickness(20)
            };

            // Header
            grid.Add(new Label
            {
                Text = "Sensor Operational Status Monitoring",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 20)
            }, 0, 0);

            // Main content - just a simple list for now
            var sensorList = new ListView
            {
                ItemsSource = _filteredSensors,
                HasUnevenRows = true,
                SeparatorVisibility = SeparatorVisibility.Default,
                SeparatorColor = Colors.LightGray,
                Margin = new Thickness(0, 10)
            };

            sensorList.ItemTemplate = new DataTemplate(() =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    Padding = new Thickness(10)
                };

                var statusIndicator = new Frame
                {
                    WidthRequest = 16,
                    HeightRequest = 16,
                    CornerRadius = 8,
                    Padding = 0,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                statusIndicator.SetBinding(BackgroundColorProperty, new Binding("StatusColor", converter: new StatusColorConverter()));

                var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
                nameLabel.SetBinding(Label.TextProperty, "Name");

                var typeLabel = new Label { FontSize = 12 };
                typeLabel.SetBinding(Label.TextProperty, "Type");

                var locationLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
                locationLabel.SetBinding(Label.TextProperty, "Location");

                var infoStack = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children = { nameLabel, typeLabel, locationLabel }
                };

                var statusLabel = new Label { FontSize = 12, HorizontalOptions = LayoutOptions.End };
                statusLabel.SetBinding(Label.TextProperty, "Status");
                statusLabel.SetBinding(Label.TextColorProperty, new Binding("Status", converter: new StatusTextColorConverter()));

                var calibratedLabel = new Label { FontSize = 10, TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.End };
                calibratedLabel.SetBinding(Label.TextProperty, "LastCalibratedText");

                var rightStack = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.End,
                    Children = { statusLabel, calibratedLabel }
                };

                grid.Add(statusIndicator, 0, 0);
                grid.Add(infoStack, 1, 0);
                grid.Add(rightStack, 2, 0);

                return new ViewCell { View = grid };
            });

            // Add a ScrollView for the content 
            var scrollView = new ScrollView
            {
                Content = sensorList
            };
            grid.Add(scrollView, 0, 1);

            // Navigation bar (simplified)
            var navBar = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                BackgroundColor = Colors.White,
                Padding = new Thickness(10)
            };

            var dashboardBtn = new Button
            {
                Text = "🏠 Dashboard",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Black
            };
            dashboardBtn.Clicked += async (s, e) => await Shell.Current.GoToAsync("//MainPage");

            var adminHomeBtn = new Button
            {
                Text = "Admin Home",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Black
            };
            adminHomeBtn.Clicked += async (s, e) => await Shell.Current.GoToAsync("//Administrator/MainPage");

            var dataStorageBtn = new Button
            {
                Text = "📊 Data Storage",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Black
            };
            dataStorageBtn.Clicked += async (s, e) => await Shell.Current.GoToAsync("//Administrator/DataStoragePage");

            var sensorMonitorBtn = new Button
            {
                Text = "🔌 Sensor Monitor",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Black
            };
            sensorMonitorBtn.Clicked += async (s, e) => await Shell.Current.GoToAsync("//Administrator/SensorMonitoringPage");

            var settingsBtn = new Button
            {
                Text = "⚙️ Settings",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Black
            };
            settingsBtn.Clicked += async (s, e) => await Shell.Current.GoToAsync("//Administrator/SettingsPage");

            navBar.Add(dashboardBtn, 0, 0);
            navBar.Add(adminHomeBtn, 1, 0);
            navBar.Add(dataStorageBtn, 2, 0);
            navBar.Add(sensorMonitorBtn, 3, 0);
            navBar.Add(settingsBtn, 4, 0);

            grid.Add(navBar, 0, 2);

            // Set the content
            Content = grid;
        }

        private void FilterSensors()
        {
            if (_allSensors == null)
                return;

            // Apply type filter
            var filtered = _allSensors.AsEnumerable();
            
            if (_selectedSensorType != "All Types")
            {
                filtered = filtered.Where(s => s.Type == _selectedSensorType);
            }
            
            // Apply status filter
            if (_selectedStatus != "All Status")
            {
                filtered = filtered.Where(s => s.Status.ToLower() == _selectedStatus.ToLower());
            }
            
            // Apply active only filter
            if (_showActiveOnly)
            {
                filtered = filtered.Where(s => s.IsActive);
            }
            
            Sensors = new ObservableCollection<Sensor>(filtered);
        }
        
        private void UpdateStatusCounts()
        {
            // Get status counts from all sensors (not filtered)
            var sensorsToCount = _showActiveOnly 
                ? _allSensors.Where(s => s.IsActive) 
                : _allSensors;
                
            _operationalCount = sensorsToCount.Count(s => s.Status.ToLower() == "operational");
            _maintenanceCount = sensorsToCount.Count(s => s.Status.ToLower() == "maintenance");
            _offlineCount = sensorsToCount.Count(s => s.Status.ToLower() == "offline");
            
            Debug.WriteLine($"Status counts: Operational: {_operationalCount}, Maintenance: {_maintenanceCount}, Offline: {_offlineCount}");
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple converter for status colors
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "operational" => Colors.Green,
                    "maintenance" => Colors.Orange,
                    "offline" => Colors.Red,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Simple converter for status text colors
    public class StatusTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "operational" => Colors.Green,
                    "maintenance" => Colors.Orange,
                    "offline" => Colors.Red,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}