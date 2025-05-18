using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    /// <summary>
    /// Provides real-time monitoring of sensor operational status with filtering,
    /// sorting, and status visualization capabilities.
    /// </summary>
    /// <remarks>
    /// This page builds its UI programmatically at runtime due to the dynamic nature
    /// of sensor monitoring features. It implements INotifyPropertyChanged to support
    /// real-time UI updates as sensor data changes.
    /// </remarks>
    public partial class SensorMonitoringPage : ContentPage, INotifyPropertyChanged
    {
        // Services for data access
        private readonly SensorService _sensorService;
        private readonly SensorSettingsService _sensorSettingsService;
        
        // Collections for data binding
        private ObservableCollection<Sensor> _allSensors = new();
        private ObservableCollection<Sensor> _filteredSensors = new();
        
        // State tracking properties
        private Sensor _selectedSensor;
        private bool _hasSelectedSensor;
        private string _selectedSensorType = "All Types";
        private string _selectedStatus = "All Status";
        private bool _showActiveOnly = true;
        
        // Status counts for dashboard visualization
        private int _operationalCount;
        private int _maintenanceCount;
        private int _offlineCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorMonitoringPage"/> class.
        /// Sets up services, initializes data, and builds the UI programmatically.
        /// </summary>
        public SensorMonitoringPage()
        {
            try
            {
                Debug.WriteLine("SensorMonitoringPage constructor started");
                
                // Skip InitializeComponent() - build UI programmatically instead
                
                // Initialize services
                string dbPath = new DatabaseService().GetDatabasePath();
                _sensorService = new SensorService(dbPath);
                _sensorSettingsService = new SensorSettingsService(new DatabaseService());
                
                // Set binding context for data binding
                BindingContext = this;
                
                // Build a simplified UI to avoid XAML issues
                BuildSimplifiedUI();
                
                // Load sensors data
                Task.Run(async () => await LoadSensorsAsync());
                
                Debug.WriteLine("SensorMonitoringPage constructor completed");
            }
            catch (Exception ex)
            {
                // Log detailed error information for debugging
                Debug.WriteLine($"Error initializing SensorMonitoringPage: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                // Create a simple error UI when initialization fails
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

        /// <summary>
        /// Builds a simplified loading UI while data is being retrieved.
        /// </summary>
        /// <remarks>
        /// This method creates a basic UI with a title, loading message, and spinner
        /// that will be displayed until the full UI is constructed after data loading.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the filtered collection of sensors to display.
        /// </summary>
        public ObservableCollection<Sensor> Sensors
        {
            get => _filteredSensors;
            set
            {
                _filteredSensors = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected sensor.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether a sensor is currently selected.
        /// </summary>
        public bool HasSelectedSensor
        {
            get => _hasSelectedSensor;
            set
            {
                _hasSelectedSensor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Asynchronously loads all sensor data from the service.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadSensorsAsync()
        {
            try
            {
                Debug.WriteLine("Loading sensors");
                
                // Get all sensors from the service
                _allSensors = await _sensorService.GetSensorsAsync();
                
                // Make sure _allSensors is never null to avoid crashes
                if (_allSensors == null)
                    _allSensors = new ObservableCollection<Sensor>();
                
                // Apply current filters to the data
                FilterSensors();
                
                // Update sensor status counts for dashboard
                UpdateStatusCounts();
                
                // Update UI on the main thread
                await MainThread.InvokeOnMainThreadAsync(() => {
                    BuildCompleteUI();
                });
                
                Debug.WriteLine($"Loaded {_allSensors.Count} sensors");
            }
            catch (Exception ex)
            {
                // Log detailed error information
                Debug.WriteLine($"Error in LoadSensorsAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                // Set empty collections to avoid null reference exceptions
                _allSensors = new ObservableCollection<Sensor>();
                Sensors = new ObservableCollection<Sensor>();
                
                // Show error to user on main thread
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await DisplayAlert("Error", $"Failed to load sensors: {ex.Message}", "OK");
                });
            }
        }

        /// <summary>
        /// Builds the complete UI after data is loaded.
        /// </summary>
        /// <remarks>
        /// This method creates the full-featured UI with sensor list, status indicators,
        /// and navigation controls once the sensor data has been successfully loaded.
        /// </remarks>
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

            // Main content - sensor list with status indicators
            var sensorList = new ListView
            {
                ItemsSource = _filteredSensors,
                HasUnevenRows = true,
                SeparatorVisibility = SeparatorVisibility.Default,
                SeparatorColor = Colors.LightGray,
                Margin = new Thickness(0, 10)
            };

            // Create the template for each sensor item
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

                // Status color indicator
                var statusIndicator = new Frame
                {
                    WidthRequest = 16,
                    HeightRequest = 16,
                    CornerRadius = 8,
                    Padding = 0,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                statusIndicator.SetBinding(BackgroundColorProperty, new Binding("StatusColor", converter: new StatusColorConverter()));

                // Sensor name
                var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
                nameLabel.SetBinding(Label.TextProperty, "Name");

                // Sensor type
                var typeLabel = new Label { FontSize = 12 };
                typeLabel.SetBinding(Label.TextProperty, "Type");

                // Sensor location
                var locationLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
                locationLabel.SetBinding(Label.TextProperty, "Location");

                // Group sensor info in a vertical stack
                var infoStack = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children = { nameLabel, typeLabel, locationLabel }
                };

                // Sensor status text
                var statusLabel = new Label { FontSize = 12, HorizontalOptions = LayoutOptions.End };
                statusLabel.SetBinding(Label.TextProperty, "Status");
                statusLabel.SetBinding(Label.TextColorProperty, new Binding("Status", converter: new StatusTextColorConverter()));

                // Last calibrated info
                var calibratedLabel = new Label { FontSize = 10, TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.End };
                calibratedLabel.SetBinding(Label.TextProperty, "LastCalibratedText");

                // Group status info in a vertical stack
                var rightStack = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.End,
                    Children = { statusLabel, calibratedLabel }
                };

                // Add all elements to the grid
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

            // Navigation buttons with event handlers
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

            // Add navigation buttons to navbar
            navBar.Add(dashboardBtn, 0, 0);
            navBar.Add(adminHomeBtn, 1, 0);
            navBar.Add(dataStorageBtn, 2, 0);
            navBar.Add(sensorMonitorBtn, 3, 0);
            navBar.Add(settingsBtn, 4, 0);

            // Add navbar to main grid
            grid.Add(navBar, 0, 2);

            // Set the content
            Content = grid;
        }

        /// <summary>
        /// Applies current filter settings to the sensor collection.
        /// </summary>
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
            
            // Update sensors collection
            Sensors = new ObservableCollection<Sensor>(filtered);
        }
        
        /// <summary>
        /// Updates the status count statistics for the dashboard.
        /// </summary>
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

        /// <summary>
        /// INotifyPropertyChanged implementation for property change notifications.
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Converts sensor status to appropriate color for visual indication.
    /// </summary>
    public class StatusColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts sensor status string to a color.
        /// </summary>
        /// <param name="value">The status string value.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>A color representing the status.</returns>
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

        /// <summary>
        /// Not implemented: conversion from color back to status.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts sensor status to appropriate text color for visual indication.
    /// </summary>
    public class StatusTextColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts sensor status string to a text color.
        /// </summary>
        /// <param name="value">The status string value.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>A color for the status text.</returns>
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

        /// <summary>
        /// Not implemented: conversion from color back to status.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}