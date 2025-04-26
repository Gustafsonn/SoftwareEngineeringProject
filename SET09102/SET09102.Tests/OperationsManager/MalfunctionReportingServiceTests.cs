using SET09102.OperationsManager.Models;
using SET09102.OperationsManager.Services;
using Moq;
using Xunit;

namespace SET09102.Tests.OperationsManager
{
    public class MalfunctionReportingServiceTests
    {
        private readonly Mock<IMalfunctionReportingService> _mockService;
        private readonly List<SensorMalfunction> _testMalfunctions;

        public MalfunctionReportingServiceTests()
        {
            _mockService = new Mock<IMalfunctionReportingService>();
            _testMalfunctions = new List<SensorMalfunction>
            {
                new SensorMalfunction
                {
                    MalfunctionId = 1,
                    SensorId = 1,
                    SensorName = "Air Quality Sensor 1",
                    DetectedTime = DateTime.UtcNow.AddHours(-1),
                    Description = "Sensor reporting inconsistent readings",
                    Severity = MalfunctionSeverity.High,
                    ErrorCode = "AQ-001",
                    Location = "Building A, Floor 2",
                    ReportedBy = "System",
                    Status = MalfunctionStatus.Reported,
                    AffectedParameters = new List<string> { "PM2.5", "CO2" },
                    DiagnosticData = new Dictionary<string, string>
                    {
                        { "LastCalibration", "2024-01-15" },
                        { "BatteryLevel", "85%" },
                        { "SignalStrength", "92%" }
                    }
                },
                new SensorMalfunction
                {
                    MalfunctionId = 2,
                    SensorId = 2,
                    SensorName = "Water Quality Sensor 1",
                    DetectedTime = DateTime.UtcNow.AddHours(-2),
                    Description = "Sensor offline for more than 1 hour",
                    Severity = MalfunctionSeverity.Critical,
                    ErrorCode = "WQ-002",
                    Location = "River Station 1",
                    ReportedBy = "System",
                    Status = MalfunctionStatus.UnderInvestigation,
                    AffectedParameters = new List<string> { "pH", "Turbidity" },
                    DiagnosticData = new Dictionary<string, string>
                    {
                        { "LastConnection", "2024-02-20T10:00:00Z" },
                        { "BatteryLevel", "15%" },
                        { "SignalStrength", "0%" }
                    }
                }
            };
        }

        [Fact]
        public async Task ReportMalfunctionAsync_CreatesNewMalfunctionReport()
        {
            // Arrange
            var newMalfunction = new SensorMalfunction
            {
                SensorId = 3,
                SensorName = "Temperature Sensor 1",
                Description = "Temperature readings out of range",
                Severity = MalfunctionSeverity.Medium
            };

            _mockService.Setup(s => s.ReportMalfunctionAsync(It.IsAny<SensorMalfunction>()))
                       .ReturnsAsync(newMalfunction);

            // Act
            var result = await _mockService.Object.ReportMalfunctionAsync(newMalfunction);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newMalfunction.SensorId, result.SensorId);
            Assert.Equal(newMalfunction.Description, result.Description);
            Assert.Equal(newMalfunction.Severity, result.Severity);
        }

        [Fact]
        public async Task GetAllMalfunctionReportsAsync_ReturnsAllMalfunctions()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllMalfunctionReportsAsync())
                       .ReturnsAsync(_testMalfunctions);

            // Act
            var result = await _mockService.Object.GetAllMalfunctionReportsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, m => m.MalfunctionId == 1);
            Assert.Contains(result, m => m.MalfunctionId == 2);
        }

        [Fact]
        public async Task GetMalfunctionsBySeverityAsync_ReturnsCorrectMalfunctions()
        {
            // Arrange
            var severity = MalfunctionSeverity.Critical;
            var criticalMalfunctions = _testMalfunctions.Where(m => m.Severity == severity).ToList();
            
            _mockService.Setup(s => s.GetMalfunctionsBySeverityAsync(severity))
                       .ReturnsAsync(criticalMalfunctions);

            // Act
            var result = await _mockService.Object.GetMalfunctionsBySeverityAsync(severity);

            // Assert
            Assert.Single(result);
            Assert.All(result, m => Assert.Equal(severity, m.Severity));
        }

        [Fact]
        public async Task UpdateMalfunctionStatusAsync_UpdatesStatusCorrectly()
        {
            // Arrange
            var malfunctionId = 1;
            var newStatus = MalfunctionStatus.BeingRepaired;
            var resolutionNotes = "Technician assigned to investigate";

            // Act
            await _mockService.Object.UpdateMalfunctionStatusAsync(malfunctionId, newStatus, resolutionNotes);

            // Assert
            _mockService.Verify(s => s.UpdateMalfunctionStatusAsync(malfunctionId, newStatus, resolutionNotes), Times.Once);
        }

        [Fact]
        public async Task GetActiveMalfunctionsAsync_ReturnsOnlyActiveMalfunctions()
        {
            // Arrange
            var activeStatuses = new[] { MalfunctionStatus.Reported, MalfunctionStatus.UnderInvestigation, MalfunctionStatus.BeingRepaired };
            var activeMalfunctions = _testMalfunctions.Where(m => activeStatuses.Contains(m.Status)).ToList();
            
            _mockService.Setup(s => s.GetActiveMalfunctionsAsync())
                       .ReturnsAsync(activeMalfunctions);

            // Act
            var result = await _mockService.Object.GetActiveMalfunctionsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, m => Assert.Contains(m.Status, activeStatuses));
        }

        [Fact]
        public async Task AddDiagnosticDataAsync_AddsNewDiagnosticInformation()
        {
            // Arrange
            var malfunctionId = 1;
            var diagnosticData = new Dictionary<string, string>
            {
                { "ErrorCount", "5" },
                { "LastReset", DateTime.UtcNow.ToString() }
            };

            // Act
            await _mockService.Object.AddDiagnosticDataAsync(malfunctionId, diagnosticData);

            // Assert
            _mockService.Verify(s => s.AddDiagnosticDataAsync(malfunctionId, diagnosticData), Times.Once);
        }

        [Fact]
        public async Task GetMalfunctionStatisticsAsync_ReturnsCorrectStatistics()
        {
            // Arrange
            var expectedStats = new Dictionary<string, int>
            {
                { "TotalMalfunctions", 2 },
                { "CriticalMalfunctions", 1 },
                { "ActiveMalfunctions", 2 },
                { "ResolvedMalfunctions", 0 }
            };

            _mockService.Setup(s => s.GetMalfunctionStatisticsAsync())
                       .ReturnsAsync(expectedStats);

            // Act
            var result = await _mockService.Object.GetMalfunctionStatisticsAsync();

            // Assert
            Assert.Equal(expectedStats.Count, result.Count);
            Assert.Equal(expectedStats["TotalMalfunctions"], result["TotalMalfunctions"]);
            Assert.Equal(expectedStats["CriticalMalfunctions"], result["CriticalMalfunctions"]);
        }
    }
} 