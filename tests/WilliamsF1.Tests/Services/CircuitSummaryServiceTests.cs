using Moq;
using WilliamsF1.Application.Interface;
using WilliamsF1.Application.Services;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Tests.Services;

public class CircuitSummaryServiceTests
{
    private readonly Mock<ICircuitRepository> _circuitRepositoryMock;
    private readonly Mock<IRaceRepository> _raceRepositoryMock;
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<ILapTimeRepository> _lapTimeRepositoryMock;

    public CircuitSummaryServiceTests()
    {
        _circuitRepositoryMock = new Mock<ICircuitRepository>();
        _raceRepositoryMock = new Mock<IRaceRepository>();
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _lapTimeRepositoryMock = new Mock<ILapTimeRepository>();
    }

    [Fact]
    public async Task GetCircuitSummariesAsync_WithValidData_ReturnsSortedSummaries()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com"),
            new(2, "ref2", "Silverstone", "Northampton", "UK", 51.8042, -1.0469, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null),
            new(2, 2023, 2, 2, "British GP", new DateOnly(2023, 7, 9), null, "http://example.com", null, null, null, null, null, null, null, null, null, null),
            new(3, 2023, 3, 1, "Monaco GP", new DateOnly(2023, 5, 27), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch"),
            new(2, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 1, 1, "1:12.345", 72345),
            new(1, 2, 1, 2, "1:13.456", 73456),
            new(2, 2, 1, 1, "1:28.123", 88123)
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetCircuitSummariesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Monaco", result[0].CircuitName);
        Assert.Equal("Silverstone", result[1].CircuitName);
        Assert.Equal(2, result[0].TotalRaces);
        Assert.Equal("1:12.345", result[0].FastestLapTime);
        Assert.Equal("Max Verstappen", result[0].FastestLapDriver);
    }

    [Fact]
    public async Task GetCircuitSummariesAsync_WithNoLapTimes_ReturnsCircuitsWithNAValues()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetCircuitSummariesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("N/A", result[0].FastestLapTime);
        Assert.Equal("N/A", result[0].FastestLapDriver);
        Assert.Equal("-", result[0].RaceYear);
    }

    [Fact]
    public async Task GetCircuitSummariesAsync_CachesBetweenCalls()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result1 = await service.GetCircuitSummariesAsync();
        var result2 = await service.GetCircuitSummariesAsync();

        // Assert
        Assert.Same(result1, result2);
        _circuitRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchCircuitSummariesAsync_WithMatchingName_ReturnsFilteredResults()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com"),
            new(2, "ref2", "Silverstone", "Northampton", "UK", 51.8042, -1.0469, 0, "http://example.com"),
            new(3, "ref3", "Monaco B", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null),
            new(2, 2023, 2, 2, "British GP", new DateOnly(2023, 7, 9), null, "http://example.com", null, null, null, null, null, null, null, null, null, null),
            new(3, 2023, 3, 3, "Italian GP", new DateOnly(2023, 9, 3), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchCircuitSummariesAsync("Monaco");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);  // Monaco and Monaco B both match
        Assert.All(result, s => Assert.Contains("onaco", s.CircuitName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchCircuitSummariesAsync_WithCountryMatch_ReturnsFilteredResults()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com"),
            new(2, "ref2", "Silverstone", "Northampton", "UK", 51.8042, -1.0469, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null),
            new(2, 2023, 2, 2, "British GP", new DateOnly(2023, 7, 9), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchCircuitSummariesAsync("UK");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Silverstone", result[0].CircuitName);
    }

    [Fact]
    public async Task SearchCircuitSummariesAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchCircuitSummariesAsync("Nonexistent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchCircuitSummariesAsync_IsCaseInsensitive()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var resultLower = await service.SearchCircuitSummariesAsync("monaco");
        var resultUpper = await service.SearchCircuitSummariesAsync("MONACO");

        // Assert
        Assert.NotNull(resultLower);
        Assert.NotNull(resultUpper);
        Assert.Single(resultLower);
        Assert.Single(resultUpper);
    }

    [Fact]
    public async Task GetCircuitSummariesAsync_FindsFastestLapAcrossMultipleLaps()
    {
        // Arrange
        var circuits = new List<Circuit>
        {
            new(1, "ref1", "Monaco", "Monte Carlo", "Monaco", 43.7384, 7.4246, 0, "http://example.com")
        };

        var races = new List<Race>
        {
            new(1, 2023, 1, 1, "Monaco GP", new DateOnly(2023, 5, 28), null, "http://example.com", null, null, null, null, null, null, null, null, null, null)
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch"),
            new(2, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 1, 1, "1:12.345", 72345),
            new(1, 2, 1, 2, "1:13.456", 73456),
            new(1, 1, 2, 1, "1:11.234", 71234),  // Faster lap
            new(1, 2, 2, 2, "1:12.789", 72789)
        };

        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new CircuitSummaryService(
            _circuitRepositoryMock.Object,
            _raceRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetCircuitSummariesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("1:11.234", result[0].FastestLapTime);
        Assert.Equal("Max Verstappen", result[0].FastestLapDriver);
    }

    private static async IAsyncEnumerable<LapTime> StreamLapTimes(List<LapTime> lapTimes)
    {
        foreach (var lapTime in lapTimes)
        {
            yield return lapTime;
            await Task.CompletedTask;
        }
    }
}
