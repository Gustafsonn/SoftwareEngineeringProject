# Sensor Configuration UML Diagrams

## Class Diagram
```mermaid
classDiagram
    class Sensor {
        +string Id
        +string Name
        +string Type
        +string Location
        +bool IsActive
        +string FirmwareVersion
    }

    class SensorConfiguration {
        +int PollingInterval
        +double AlertThreshold
        +string FirmwareVersion
    }

    class ISensorConfigurationService {
        <<interface>>
        +GetAllSensorsAsync() Task<IEnumerable<Sensor>>
        +GetSensorConfigurationAsync(string) Task<SensorConfiguration>
        +UpdateSensorConfigurationAsync(string, SensorConfiguration) Task
        +CheckFirmwareUpdateAsync(string) Task<bool>
        +UpdateFirmwareAsync(string, Action<double>) Task
    }

    class MockSensorConfigurationService {
        -List<Sensor> _mockSensors
        +GetAllSensorsAsync() Task<IEnumerable<Sensor>>
        +GetSensorConfigurationAsync(string) Task<SensorConfiguration>
        +UpdateSensorConfigurationAsync(string, SensorConfiguration) Task
        +CheckFirmwareUpdateAsync(string) Task<bool>
        +UpdateFirmwareAsync(string, Action<double>) Task
    }

    class SensorConfigurationPage {
        -ISensorConfigurationService _sensorService
        -Sensor _selectedSensor
        +LoadSensors() void
        +LoadSensorConfiguration() Task
        +OnUpdateConfigClicked() void
        +OnCheckUpdatesClicked() void
        +OnUpdateFirmwareClicked() void
    }

    ISensorConfigurationService <|.. MockSensorConfigurationService
    SensorConfigurationPage --> ISensorConfigurationService
    SensorConfigurationPage --> Sensor
    SensorConfigurationPage --> SensorConfiguration
```

## Sequence Diagram - Firmware Update
```mermaid
sequenceDiagram
    participant User
    participant SensorConfigPage
    participant SensorService
    participant Sensor

    User->>SensorConfigPage: Select Sensor
    SensorConfigPage->>SensorService: GetAllSensorsAsync()
    SensorService-->>SensorConfigPage: List<Sensor>
    SensorConfigPage->>User: Display Sensors

    User->>SensorConfigPage: Click "Check Updates"
    SensorConfigPage->>SensorService: CheckFirmwareUpdateAsync()
    SensorService-->>SensorConfigPage: Update Available
    SensorConfigPage->>User: Show Update Button

    User->>SensorConfigPage: Click "Update Firmware"
    SensorConfigPage->>SensorService: UpdateFirmwareAsync()
    SensorService->>Sensor: Update Firmware
    SensorService-->>SensorConfigPage: Progress Updates
    SensorConfigPage->>User: Show Progress
    SensorConfigPage-->>SensorConfigPage: Update Complete
    SensorConfigPage->>User: Show Success Message
```

## Component Diagram
```mermaid
componentDiagram
    component "Sensor Configuration Module" {
        [SensorConfigurationPage]
        [ISensorConfigurationService]
        [MockSensorConfigurationService]
        [Sensor]
        [SensorConfiguration]
    }

    component "Testing" {
        [SensorConfigurationTests]
    }

    [SensorConfigurationPage] --> [ISensorConfigurationService]
    [MockSensorConfigurationService] --> [ISensorConfigurationService]
    [SensorConfigurationTests] --> [MockSensorConfigurationService]
``` 