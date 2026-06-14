using Moq;
using WilliamsF1.Application.Interface;
using WilliamsF1.Application.Services;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Tests.Services;

public class DriverSummaryServiceTests
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<ILapTimeRepository> _lapTimeRepositoryMock;

    public DriverSummaryServiceTests()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _lapTimeRepositoryMock = new Mock<ILapTimeRepository>();
    }

    [Fact]
    public async Task GetDriverSummariesAsync_WithValidData_ReturnsDrivers()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch"),
            new(3, "charles_leclerc", "16", "LEC", "Charles", "Leclerc", new DateOnly(1997, 10, 16), "Monegasque")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 45, 1, "1:23.456", 83456),  // Lewis finishes race 1
            new(1, 2, 45, 2, "1:24.567", 84567),  // Max finishes race 1
            new(2, 1, 45, 2, "1:34.567", 94567), // Lewis completes race 2
            new(2, 2, 45, 1, "1:33.456", 93456), // Max completes race 2
            new(3, 3, 10, 4, "1:45.678", 105678) // Charles DNF
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetDriverSummariesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetDriverSummariesAsync_CountsRaceEntriesCorrectly()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 1, 1, "1:23.456", 83456),
            new(1, 2, 1, 2, "1:24.567", 84567),
            new(2, 1, 1, 3, "1:34.567", 94567),
            new(2, 2, 1, 4, "1:33.456", 93456),
            new(3, 1, 1, 2, "1:45.678", 105678),
            new(3, 2, 1, 1, "1:44.567", 104567)
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetDriverSummariesAsync();

        // Assert
        Assert.Equal(3, result.First(d => d.DriverName == "Lewis Hamilton").TotalRacesEntered);
        Assert.Equal(3, result.First(d => d.DriverName == "Max Verstappen").TotalRacesEntered);
    }

    [Fact]
    public async Task GetDriverSummariesAsync_CountsRaceEntriesForIncompletedRaces()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "jenson_button", "1", "BUT", "Jenson", "Button", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            // Race 1: completed race with 45 laps max
            new(1, 1, 45, 1, "1:23.456", 83456),  // Lewis finishes 1st (podium)
            new(1, 2, 45, 2, "1:33.999", 94568),

            // Race 2: 45 laps max, but Lewis DNF at lap 30
            new(2, 1, 30, 1, "1:34.567", 94567),  // Lewis position 1st but only completed 30 laps - No podium to prove DNF registered correctly
            new(2, 2, 45, 1, "1:33.999", 94568),
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetDriverSummariesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[1].Podiums);  // Ensure that the podium count is correct for the completed race and that the DNF has been registered correctly
        Assert.Equal(2, result[1].TotalRacesEntered);  // But both races are counted as entered
    }

    [Fact]
    public async Task GetDriverSummariesAsync_CountsPodiumsCorrectly()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        var lapTimes = new List<LapTime>
        {
            // Race 1: 45 laps
            new(1, 1, 45, 1, "1:23.456", 83456),  // Lewis finishes 1st (podium)
            new(1, 2, 45, 2, "1:24.567", 84567),  // Max finishes 2nd (podium)

            // Race 2: 45 laps
            new(2, 1, 45, 4, "1:34.567", 94567),  // Lewis finishes 4th (no podium)
            new(2, 2, 45, 3, "1:33.456", 93456),  // Max finishes 3rd (podium)

            // Race 3: 45 laps
            new(3, 1, 10, 2, "1:45.678", 105678), // Lewis DNF at lap 10 (no podium)
            new(3, 2, 45, 1, "1:44.567", 104567)  // Max finishes 1st (podium)
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetDriverSummariesAsync();

        // Assert
        Assert.Equal(1, result.First(d => d.DriverName == "Lewis Hamilton").Podiums);
        Assert.Equal(3, result.First(d => d.DriverName == "Max Verstappen").Podiums);
    }

    [Fact]
    public async Task GetDriverSummariesAsync_OnlyCountsPodiumsForCompletedRaces()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "jenson_button", "1", "BUT", "Jenson", "Button", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            // Race 1: completed race with 45 laps max
            new(1, 1, 45, 1, "1:23.456", 83456),  // Lewis finishes 1st (podium)
            new(1, 2, 45, 2, "1:33.999", 94568),

            // Race 2: 45 laps max, but Lewis DNF at lap 30
            new(2, 1, 30, 1, "1:34.567", 94567),  // Lewis position 1st but only completed 30 laps - no podium
            new(2, 2, 45, 1, "1:33.999", 94568),
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetDriverSummariesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[1].Podiums);  // Only the completed race counts as a podium
        Assert.Equal(2, result[1].TotalRacesEntered);
    }

    [Fact]
    public async Task GetDriverSummariesAsync_CachesBetweenCalls()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 45, 1, "1:23.456", 83456)
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result1 = await service.GetDriverSummariesAsync();
        var result2 = await service.GetDriverSummariesAsync();

        // Assert
        Assert.Same(result1, result2);
        _driverRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _lapTimeRepositoryMock.Verify(r => r.StreamAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchDriverSummariesAsync_WithMatchingName_ReturnsFilteredResults()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch"),
            new(3, "lewis_rojas", "99", "LEW", "Lewis", "Rojas", new DateOnly(1990, 5, 15), "Spanish")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 45, 1, "1:23.456", 83456),
            new(1, 2, 45, 2, "1:24.567", 84567),
            new(1, 3, 45, 3, "1:25.678", 85678)
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchDriverSummariesAsync("Lewis");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.Contains("Lewis", d.DriverName));
    }

    [Fact]
    public async Task SearchDriverSummariesAsync_WithPartialMatch_ReturnsFilteredResults()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "max_verstappen", "1", "MAX", "Max", "Verstappen", new DateOnly(1997, 12, 31), "Dutch")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 45, 1, "1:23.456", 83456),
            new(1, 2, 45, 2, "1:24.567", 84567)
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchDriverSummariesAsync("Ham");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Lewis Hamilton", result[0].DriverName);
    }

    [Fact]
    public async Task SearchDriverSummariesAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 45, 1, "1:23.456", 83456)
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchDriverSummariesAsync("Nonexistent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchDriverSummariesAsync_IsCaseInsensitive()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 45, 1, "1:23.456", 83456)
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var resultLower = await service.SearchDriverSummariesAsync("lewis");
        var resultUpper = await service.SearchDriverSummariesAsync("LEWIS");
        var resultMixed = await service.SearchDriverSummariesAsync("LeWiS");

        // Assert
        Assert.NotNull(resultLower);
        Assert.NotNull(resultUpper);
        Assert.NotNull(resultMixed);
        Assert.Single(resultLower);
        Assert.Single(resultUpper);
        Assert.Single(resultMixed);
    }

    [Fact]
    public async Task GetDriverSummariesAsync_WithNoRaceData_ReturnsDriversWithZeroCounts()
    {
        // Arrange
        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new DriverSummaryService(
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetDriverSummariesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Lewis Hamilton", result[0].DriverName);
        Assert.Equal(0, result[0].TotalRacesEntered);
        Assert.Equal(0, result[0].Podiums);
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
