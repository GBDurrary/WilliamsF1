using WilliamsF1.Application.DTO;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Application.Services;

public sealed class CircuitSummaryService(
    ICircuitRepository circuitRepository,
    IRaceRepository raceRepository,
    IDriverRepository driverRepository,
    ILapTimeRepository lapTimeRepository)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyList<CircuitSummary>? _cachedSummaries;

    public async Task<IReadOnlyList<CircuitSummary>> GetCircuitSummariesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedSummaries is not null)
            return _cachedSummaries;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            _cachedSummaries ??= await BuildCircuitSummariesAsync(cancellationToken);
            return _cachedSummaries;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<CircuitSummary>> SearchCircuitSummariesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var summaries = await GetCircuitSummariesAsync(cancellationToken);

        return [.. summaries.Where(x => x.CircuitName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) 
                                    || x.Country.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))];
    }

    private async Task<IReadOnlyList<CircuitSummary>> BuildCircuitSummariesAsync(CancellationToken cancellationToken)
    {
        var circuits = await circuitRepository.GetAllAsync(cancellationToken);
        var races = await raceRepository.GetAllAsync(cancellationToken);
        var drivers = await driverRepository.GetAllAsync(cancellationToken);

        var racesById = races.ToDictionary(x => x.RaceId);
        var driversById = drivers.ToDictionary(x => x.DriverId);

        var circuitRaceCounts = races
            .GroupBy(x => x.CircuitId)
            .ToDictionary(x => x.Key, x => x.Count());

        var circuitIdByRaceId = races
            .ToDictionary(x => x.RaceId, x => x.CircuitId);

        var fastestLapByCircuitId = new Dictionary<int, FastestCircuitLap>();

        await foreach (var lapTime in lapTimeRepository.StreamAllAsync(cancellationToken))
        {
            if (!circuitIdByRaceId.TryGetValue(lapTime.RaceId, out var circuitId))
                continue;

            if (!fastestLapByCircuitId.TryGetValue(circuitId, out var currentFastest) || lapTime.Milliseconds < currentFastest.Milliseconds)
            {
                fastestLapByCircuitId[circuitId] = new FastestCircuitLap(
                    lapTime.RaceId,
                    lapTime.DriverId,
                    lapTime.TimeString,
                    lapTime.Milliseconds
                );
            }
        }

        return circuits
            .OrderBy(x => x.Name)
            .Select(circuit =>
            {
                fastestLapByCircuitId.TryGetValue(circuit.CircuitId, out var fastestLap);

                var driverName = fastestLap is not null &&
                                 driversById.TryGetValue(fastestLap.DriverId, out var driver)
                    ? $"{driver.Forename} {driver.Surname}"
                    : "N/A";

                var raceYear = fastestLap is not null && racesById.TryGetValue(fastestLap.RaceId, out var race)
                    ? race.Year.ToString()
                    : "-";

                return new CircuitSummary(
                    circuit.Name,
                    circuit.Country,
                    circuitRaceCounts.GetValueOrDefault(circuit.CircuitId),
                    fastestLap?.Time ?? "N/A",
                    driverName,
                    raceYear
                );
            })
            .ToList();
    }
}
