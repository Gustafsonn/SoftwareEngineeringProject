using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Models
{
    /// <summary>
    /// Represents configuration settings for an environmental sensor.
    /// </summary>
    /// <remarks>
    /// This class allows for real-time updates of settings values in the interface.
    /// </remarks>
    public class SensorSettings : INotifyPropertyChanged
    {
        private string _dataCollectionInterval;
        private bool _autoCalibration;
        private bool _alertNotifications;
        private int _calibrationIntervalDays;
        private double _alertThreshold;
        private bool _enableRemoteControl;
        private string _firmwareUpdatePolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorSettings"/> class with default values.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the data collection interval as a string representation.
        /// </summary>
        /// <value>
        /// A string representing the data collection interval (e.g., "1 minute", "1 hour").
        /// </value>
        /// <remarks>
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public string DataCollectionInterval
        {
            get => _dataCollectionInterval;
            set
            {
                _dataCollectionInterval = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto-calibration is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if auto-calibration is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, the sensor will automatically calibrate itself at the specified interval.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public bool AutoCalibration
        {
            get => _autoCalibration;
            set
            {
                _autoCalibration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether alert notifications are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if alert notifications are enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, the system will send notifications when sensor readings exceed specified thresholds.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public bool AlertNotifications
        {
            get => _alertNotifications;
            set
            {
                _alertNotifications = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the number of days between sensor calibrations.
        /// </summary>
        /// <value>
        /// The calibration interval in days.
        /// </value>
        /// <remarks>
        /// This interval is used for scheduling both automatic and manual calibrations.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public int CalibrationIntervalDays
        {
            get => _calibrationIntervalDays;
            set
            {
                _calibrationIntervalDays = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the alert threshold as a percentage of the maximum threshold.
        /// </summary>
        /// <value>
        /// A value between 0.0 and 1.0 representing the percentage of the maximum threshold.
        /// </value>
        /// <remarks>
        /// This value is used to determine when alerts should be triggered. For example,
        /// a value of 0.75 means alerts are triggered when readings reach 75% of the maximum threshold.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public double AlertThreshold
        {
            get => _alertThreshold;
            set
            {
                _alertThreshold = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether remote control of the sensor is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if remote control is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, operators can remotely adjust sensor settings and trigger operations.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public bool EnableRemoteControl
        {
            get => _enableRemoteControl;
            set
            {
                _enableRemoteControl = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the firmware update policy.
        /// </summary>
        /// <value>
        /// A string representing the firmware update policy (e.g., "Manual", "Automatic", "Scheduled").
        /// </value>
        /// <remarks>
        /// This determines how firmware updates are applied to the sensor.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public string FirmwareUpdatePolicy
        {
            get => _firmwareUpdatePolicy;
            set
            {
                _firmwareUpdatePolicy = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Converts the <see cref="DataCollectionInterval"/> string representation to minutes.
        /// </summary>
        /// <returns>The data collection interval in minutes.</returns>
        /// <remarks>
        /// This method provides a consistent way to interpret the interval string
        /// for use in scheduling data collection.
        /// </remarks>
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

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}