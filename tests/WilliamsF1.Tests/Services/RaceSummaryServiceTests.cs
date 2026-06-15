using Moq;
using WilliamsF1.Application.Interface;
using WilliamsF1.Application.Services;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Tests.Services;

public class RaceSummaryServiceTests
{
    private readonly Mock<IRaceRepository> _raceRepositoryMock;
    private readonly Mock<ICircuitRepository> _circuitRepositoryMock;
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<ILapTimeRepository> _lapTimeRepositoryMock;

    public RaceSummaryServiceTests()
    {
        _raceRepositoryMock = new Mock<IRaceRepository>();
        _circuitRepositoryMock = new Mock<ICircuitRepository>();
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _lapTimeRepositoryMock = new Mock<ILapTimeRepository>();
    }

    [Fact]
    public async Task GetRaceSummariesAsync_WithValidData_ReturnsRaces()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0)),
            CreateRace(2, 2020, 17, 2, "Abu Dhabi Grand Prix", new DateOnly(2020, 12, 13), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK"),
            CreateCircuit(2, "yas_marina", "Yas Marina Circuit", "Abu Dhabi", "UAE")
        };

        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(2, "max_verstappen", "1", "VER", "Max", "Verstappen", new DateOnly(1997, 9, 30), "Dutch")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 52, 1, "1:30.696", 90696),
            new(1, 2, 52, 2, "1:31.200", 91200),
            new(2, 1, 55, 2, "1:40.100", 100100),
            new(2, 2, 55, 1, "1:39.500", 99500)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetRaceSummariesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRaceSummariesAsync_BuildsRaceSummaryCorrectly()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "sebastian_vettel", "5", "VET", "Sebastian", "Vettel", new DateOnly(1987, 7, 3), "German"),
            new(2, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(3, "kimi_raikkonen", "7", "RAI", "Kimi", "Räikkönen", new DateOnly(1979, 10, 17), "Finnish")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 52, 1, "1:30.696", 90696),
            new(1, 2, 52, 2, "1:31.200", 91200),
            new(1, 3, 52, 3, "1:32.000", 92000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetRaceSummariesAsync();

        // Assert
        var summary = result.Single();

        Assert.Equal(1, summary.RaceId);
        Assert.Equal("British Grand Prix", summary.Name);
        Assert.Equal("Silverstone Circuit", summary.Circuit);
        Assert.Equal("2018-07-08", summary.RaceDate);
        Assert.Equal("13:10:00", summary.RaceStart);
        Assert.Equal("Sebastian Vettel", summary.Winner);
        Assert.Equal("Sebastian Vettel", summary.FastestLapDriver);
        Assert.Equal(52, summary.TotalLaps);
        Assert.Equal(3, summary.TotalDrivers);
        Assert.Equal(3, summary.RaceResults.Count);
    }

    [Fact]
    public async Task GetRaceSummariesAsync_IdentifiesFastestLapPerRace()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "sebastian_vettel", "5", "VET", "Sebastian", "Vettel", new DateOnly(1987, 7, 3), "German"),
            new(2, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 50, 1, "1:31.200", 91200),
            new(1, 2, 50, 2, "1:29.900", 89900),
            new(1, 1, 52, 1, "1:30.000", 90000),
            new(1, 2, 52, 2, "1:30.500", 90500)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetRaceSummariesAsync();

        // Assert
        var summary = result.Single();

        Assert.NotNull(summary.FastestLap);
        Assert.Equal(2, summary.FastestLap.DriverId);
        Assert.Equal("1:29.900", summary.FastestLap.Time);
        Assert.Equal(89900, summary.FastestLap.Milliseconds);
        Assert.Equal("Lewis Hamilton", summary.FastestLapDriver);
    }

    [Fact]
    public async Task GetRaceSummariesAsync_CalculatesRaceResultsCorrectly()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "sebastian_vettel", "5", "VET", "Sebastian", "Vettel", new DateOnly(1987, 7, 3), "German"),
            new(2, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British"),
            new(3, "charles_leclerc", "16", "LEC", "Charles", "Leclerc", new DateOnly(1997, 10, 16), "Monegasque")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 52, 1, "1:30.696", 90696),
            new(1, 2, 52, 2, "1:31.200", 91200),
            new(1, 3, 20, 8, "1:35.000", 95000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetRaceSummariesAsync();

        // Assert
        var raceResults = result.Single().RaceResults;

        Assert.Equal(3, raceResults.Count);

        Assert.Equal("Sebastian Vettel", raceResults[0].DriverName);
        Assert.Equal(1, raceResults[0].Position);
        Assert.Equal(52, raceResults[0].FinalLap);
        Assert.True(raceResults[0].Finished);

        Assert.Equal("Lewis Hamilton", raceResults[1].DriverName);
        Assert.Equal(2, raceResults[1].Position);
        Assert.Equal(52, raceResults[1].FinalLap);
        Assert.True(raceResults[1].Finished);

        Assert.Equal("Charles Leclerc", raceResults[2].DriverName);
        Assert.Null(raceResults[2].Position);
        Assert.Equal(20, raceResults[2].FinalLap);
        Assert.False(raceResults[2].Finished);
    }

    [Fact]
    public async Task GetRaceSummariesAsync_TreatsDriversWithinTwoLapsAsFinished()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "race_winner", "1", "WIN", "Race", "Winner", new DateOnly(1990, 1, 1), "British"),
            new(2, "lapped_driver", "2", "LAP", "Lapped", "Driver", new DateOnly(1990, 1, 1), "British"),
            new(3, "retired_driver", "3", "RET", "Retired", "Driver", new DateOnly(1990, 1, 1), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 10, 1, "1:30.000", 90000),
            new(1, 2, 8, 2, "1:31.000", 91000),
            new(1, 3, 7, 3, "1:32.000", 92000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetRaceSummariesAsync();

        // Assert
        var raceResults = result.Single().RaceResults;

        Assert.True(raceResults.First(x => x.DriverName == "Race Winner").Finished);
        Assert.True(raceResults.First(x => x.DriverName == "Lapped Driver").Finished);
        Assert.False(raceResults.First(x => x.DriverName == "Retired Driver").Finished);
    }

    [Fact]
    public async Task GetRaceSummariesAsync_UsesLatestLapForDriverFinalPosition()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 1, 10, "1:40.000", 100000),
            new(1, 1, 25, 4, "1:35.000", 95000),
            new(1, 1, 52, 1, "1:30.000", 90000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetRaceSummariesAsync();

        // Assert
        var raceResult = result.Single().RaceResults.Single();

        Assert.Equal(1, raceResult.Position);
        Assert.Equal(52, raceResult.FinalLap);
        Assert.True(raceResult.Finished);
    }

    [Fact]
    public async Task GetRaceSummariesAsync_CachesBetweenCalls()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 52, 1, "1:30.000", 90000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result1 = await service.GetRaceSummariesAsync();
        var result2 = await service.GetRaceSummariesAsync();

        // Assert
        Assert.Same(result1, result2);
        _raceRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _circuitRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _driverRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _lapTimeRepositoryMock.Verify(r => r.StreamAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_WithCircuitAndYear_ReturnsFilteredResults()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0)),
            CreateRace(2, 2019, 10, 1, "British Grand Prix", new DateOnly(2019, 7, 14), new TimeOnly(13, 10, 0)),
            CreateRace(3, 2018, 11, 2, "German Grand Prix", new DateOnly(2018, 7, 22), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK"),
            CreateCircuit(2, "hockenheimring", "Hockenheimring", "Hockenheim", "Germany")
        };

        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 52, 1, "1:30.000", 90000),
            new(2, 1, 52, 1, "1:31.000", 91000),
            new(3, 1, 67, 1, "1:20.000", 80000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchRaceSummariesAsync("Silverstone", 2018);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("British Grand Prix", result[0].Name);
        Assert.Equal("2018-07-08", result[0].RaceDate);
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_WithRaceNameAndYear_ReturnsFilteredResults()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2012, 8, 1, "European Grand Prix", new DateOnly(2012, 6, 24), new TimeOnly(12, 0, 0)),
            CreateRace(2, 2012, 10, 2, "British Grand Prix", new DateOnly(2012, 7, 8), new TimeOnly(12, 0, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "valencia", "Valencia Street Circuit", "Valencia", "Spain"),
            CreateCircuit(2, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "fernando_alonso", "14", "ALO", "Fernando", "Alonso", new DateOnly(1981, 7, 29), "Spanish")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 57, 1, "1:42.000", 102000),
            new(2, 1, 52, 1, "1:32.000", 92000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchRaceSummariesAsync("European", 2012);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("European Grand Prix", result[0].Name);
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_WithGrandPrixTerms_IgnoresGenericTerms()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2012, 8, 1, "French Grand Prix", new DateOnly(2012, 6, 24), new TimeOnly(12, 0, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "magny_cours", "Circuit de Nevers Magny-Cours", "Magny-Cours", "France")
        };

        var drivers = new List<Driver>
        {
            new(1, "fernando_alonso", "14", "ALO", "Fernando", "Alonso", new DateOnly(1981, 7, 29), "Spanish")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 70, 1, "1:17.000", 77000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchRaceSummariesAsync("French GP", 2012);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("French Grand Prix", result[0].Name);
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_WithMultiWordCircuit_ReturnsFilteredResults()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2020, 17, 1, "Abu Dhabi Grand Prix", new DateOnly(2020, 12, 13), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "yas_marina", "Yas Marina Circuit", "Abu Dhabi", "UAE")
        };

        var drivers = new List<Driver>
        {
            new(1, "max_verstappen", "1", "VER", "Max", "Verstappen", new DateOnly(1997, 9, 30), "Dutch")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 55, 1, "1:39.500", 99500)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchRaceSummariesAsync("Yas Marina", 2020);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Abu Dhabi Grand Prix", result[0].Name);
        Assert.Equal("Yas Marina Circuit", result[0].Circuit);
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 52, 1, "1:30.000", 90000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.SearchRaceSummariesAsync("Monza", 2018);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_IsCaseInsensitive()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), new TimeOnly(13, 10, 0))
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        var lapTimes = new List<LapTime>
        {
            new(1, 1, 52, 1, "1:30.000", 90000)
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(lapTimes));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var resultLower = await service.SearchRaceSummariesAsync("silverstone", 2018);
        var resultUpper = await service.SearchRaceSummariesAsync("SILVERSTONE", 2018);
        var resultMixed = await service.SearchRaceSummariesAsync("SiLvErStOnE", 2018);

        // Assert
        Assert.NotNull(resultLower);
        Assert.NotNull(resultUpper);
        Assert.NotNull(resultMixed);
        Assert.Single(resultLower);
        Assert.Single(resultUpper);
        Assert.Single(resultMixed);
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_WithEmptySearchTerm_ThrowsArgumentException()
    {
        // Arrange
        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SearchRaceSummariesAsync("", 2018));
    }

    [Fact]
    public async Task SearchRaceSummariesAsync_WithOnlyGenericRaceTerms_ThrowsArgumentException()
    {
        // Arrange
        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SearchRaceSummariesAsync("Grand Prix", 2018));
    }

    [Fact]
    public async Task GetRaceSummariesAsync_WithNoLapData_ReturnsRaceWithEmptyResults()
    {
        // Arrange
        var races = new List<Race>
        {
            CreateRace(1, 2018, 10, 1, "British Grand Prix", new DateOnly(2018, 7, 8), null)
        };

        var circuits = new List<Circuit>
        {
            CreateCircuit(1, "silverstone", "Silverstone Circuit", "Silverstone", "UK")
        };

        var drivers = new List<Driver>
        {
            new(1, "lewis_hamilton", "44", "HAM", "Lewis", "Hamilton", new DateOnly(1985, 1, 7), "British")
        };

        _raceRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(races);
        _circuitRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(circuits);
        _driverRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);
        _lapTimeRepositoryMock.Setup(r => r.StreamAllAsync(It.IsAny<CancellationToken>()))
            .Returns(StreamLapTimes(new List<LapTime>()));

        var service = new RaceSummaryService(
            _raceRepositoryMock.Object,
            _circuitRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _lapTimeRepositoryMock.Object);

        // Act
        var result = await service.GetRaceSummariesAsync();

        // Assert
        var summary = result.Single();

        Assert.Equal("British Grand Prix", summary.Name);
        Assert.Equal("Silverstone Circuit", summary.Circuit);
        Assert.Equal("2018-07-08", summary.RaceDate);
        Assert.Equal("N/A", summary.RaceStart);
        Assert.Equal("N/A", summary.Winner);
        Assert.Null(summary.FastestLap);
        Assert.Equal("N/A", summary.FastestLapDriver);
        Assert.Equal(0, summary.TotalLaps);
        Assert.Equal(0, summary.TotalDrivers);
        Assert.Empty(summary.RaceResults);
    }

    private static Race CreateRace(
        int raceId,
        int year,
        int round,
        int circuitId,
        string name,
        DateOnly date,
        TimeOnly? time)
    {
        return new Race(
            raceId,
            year,
            round,
            circuitId,
            name,
            date,
            time,
            $"https://example.com/races/{raceId}",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    private static Circuit CreateCircuit(
        int circuitId,
        string circuitRef,
        string name,
        string location,
        string country)
    {
        return new Circuit(
            circuitId,
            circuitRef,
            name,
            location,
            country,
            0,
            0,
            0,
            $"https://example.com/circuits/{circuitRef}");
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