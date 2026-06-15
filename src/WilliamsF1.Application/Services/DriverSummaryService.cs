using System.Linq;
using WilliamsF1.Application.DTO;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Application.Services;

public sealed class DriverSummaryService(
    IDriverRepository driverRepository,
    ILapTimeRepository lapTimeRepository)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyList<DriverSummary>? _cachedSummaries;

    public async Task<IReadOnlyList<DriverSummary>> GetDriverSummariesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedSummaries is not null)
            return _cachedSummaries;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            _cachedSummaries ??= await BuildDriverSummariesAsync(cancellationToken);
            return _cachedSummaries;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<DriverSummary>> SearchDriverSummariesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var summaries = await GetDriverSummariesAsync(cancellationToken);

        return [.. summaries.Where(x =>
            x.DriverName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))];
    }

    private async Task<IReadOnlyList<DriverSummary>> BuildDriverSummariesAsync(CancellationToken cancellationToken)
    {
        var drivers = await driverRepository.GetAllAsync(cancellationToken);
        var driversById = drivers.ToDictionary(x => x.DriverId);

        var maxLapByRace = new Dictionary<int, int>();
        var finalLapByDriverRace = new Dictionary<(int RaceId, int DriverId), FinalDriverLap>();

        await foreach (var lapTime in lapTimeRepository.StreamAllAsync(cancellationToken))
        {
            if (!maxLapByRace.TryGetValue(lapTime.RaceId, out var currentMaxLap) || lapTime.Lap > currentMaxLap)
                maxLapByRace[lapTime.RaceId] = lapTime.Lap;

            var key = (lapTime.RaceId, lapTime.DriverId);

            if (!finalLapByDriverRace.TryGetValue(key, out var currentFinalLap) || lapTime.Lap > currentFinalLap.Lap)
            {
                finalLapByDriverRace[key] = new FinalDriverLap(
                    lapTime.RaceId,
                    lapTime.DriverId,
                    lapTime.Lap,
                    lapTime.Position);
            }
        }

        var raceEntriesByDriverId = new Dictionary<int, int>();
        var podiumsByDriverId = new Dictionary<int, int>();

        foreach (var finalLap in finalLapByDriverRace.Values)
        {
            raceEntriesByDriverId[finalLap.DriverId] = raceEntriesByDriverId.GetValueOrDefault(finalLap.DriverId) + 1;

            var raceMaxLap = maxLapByRace[finalLap.RaceId];

            var completedRace = finalLap.Lap == raceMaxLap;
            var finishedOnPodium = finalLap.Position is >= 1 and <= 3;

            if (completedRace && finishedOnPodium)
                podiumsByDriverId[finalLap.DriverId] = podiumsByDriverId.GetValueOrDefault(finalLap.DriverId) + 1;
        }

        return drivers
            .OrderBy(x => x.Surname)
            .ThenBy(x => x.Forename)
            .Select(driver => new DriverSummary(
                $"{driver.Forename} {driver.Surname}",
                driver.Nationality,
                raceEntriesByDriverId.GetValueOrDefault(driver.DriverId),
                podiumsByDriverId.GetValueOrDefault(driver.DriverId)))
            .ToList();
    }
}