using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Models
{
    public class SensorSettings : INotifyPropertyChanged
    {
        private string _dataCollectionInterval;
        private bool _autoCalibration;
        private bool _alertNotifications;
        private int _calibrationIntervalDays;
        private double _alertThreshold;
        private bool _enableRemoteControl;
        private string _firmwareUpdatePolicy;

        public SensorSettings()
        {
            _dataCollectionInterval = "1 hour";
            _autoCalibration = true;
            _alertNotifications = true;
            _calibrationIntervalDays = 90; // 3 months
            _alertThreshold = 0.75; // 75% of max threshold
            _enableRemoteControl = true;
            _firmwareUpdatePolicy = "Manual";
        }

        public string DataCollectionInterval
        {
            get => _dataCollectionInterval;
            set
            {
                _dataCollectionInterval = value;
                OnPropertyChanged();
            }
        }

        public bool AutoCalibration
        {
            get => _autoCalibration;
            set
            {
                _autoCalibration = value;
                OnPropertyChanged();
            }
        }

        public bool AlertNotifications
        {
            get => _alertNotifications;
            set
            {
                _alertNotifications = value;
                OnPropertyChanged();
            }
        }

        public int CalibrationIntervalDays
        {
            get => _calibrationIntervalDays;
            set
            {
                _calibrationIntervalDays = value;
                OnPropertyChanged();
            }
        }

        public double AlertThreshold
        {
            get => _alertThreshold;
            set
            {
                _alertThreshold = value;
                OnPropertyChanged();
            }
        }

        public bool EnableRemoteControl
        {
            get => _enableRemoteControl;
            set
            {
                _enableRemoteControl = value;
                OnPropertyChanged();
            }
        }

        public string FirmwareUpdatePolicy
        {
            get => _firmwareUpdatePolicy;
            set
            {
                _firmwareUpdatePolicy = value;
                OnPropertyChanged();
            }
        }

        // Helper methods
        public int GetDataCollectionIntervalMinutes()
        {
            return _dataCollectionInterval switch
            {
                "1 minute" => 1,
                "5 minutes" => 5,
                "15 minutes" => 15,
                "30 minutes" => 30,
                "1 hour" => 60,
                _ => 60, // Default to 1 hour
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}