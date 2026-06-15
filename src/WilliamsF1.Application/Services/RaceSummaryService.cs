using WilliamsF1.Application.DTO;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Application.Services;

public sealed class RaceSummaryService(
    IRaceRepository raceRepository,
    ICircuitRepository circuitRepository,
    IDriverRepository driverRepository,
    ILapTimeRepository lapTimeRepository)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyList<RaceSummary>? _cachedSummaries;

    public async Task<IReadOnlyList<RaceSummary>> GetRaceSummariesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedSummaries is not null)
            return _cachedSummaries;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            _cachedSummaries ??= await BuildRaceSummariesAsync(cancellationToken);
            return _cachedSummaries;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<RaceSummary>> SearchRaceSummariesAsync(
        string raceOrCircuitSearchTerm,
        int year,
        CancellationToken cancellationToken = default)
    {
        var searchTerms = ParseRaceSearchTerms(raceOrCircuitSearchTerm);
        var summaries = await GetRaceSummariesAsync(cancellationToken);

        var yearText = year.ToString();

        return [.. summaries.Where(x =>
            x.RaceDate.StartsWith(yearText, StringComparison.OrdinalIgnoreCase)
            && (
                ContainsAllSearchTerms(x.Name, searchTerms)
                || ContainsAllSearchTerms(x.Circuit, searchTerms)
            ))];
    }

    private async Task<IReadOnlyList<RaceSummary>> BuildRaceSummariesAsync(CancellationToken cancellationToken)
    {
        var races = await raceRepository.GetAllAsync(cancellationToken);
        var circuits = await circuitRepository.GetAllAsync(cancellationToken);
        var drivers = await driverRepository.GetAllAsync(cancellationToken);

        var circuitsById = circuits.ToDictionary(x => x.CircuitId);
        var driversById = drivers.ToDictionary(x => x.DriverId);

        var maxLapByRace = new Dictionary<int, int>();
        var fastestLapByRace = new Dictionary<int, FastestCircuitLap>();
        var finalLapByDriverRace = new Dictionary<(int RaceId, int DriverId), FinalDriverLap>();

        await foreach (var lapTime in lapTimeRepository.StreamAllAsync(cancellationToken))
        {
            if (!maxLapByRace.TryGetValue(lapTime.RaceId, out var currentMaxLap) || lapTime.Lap > currentMaxLap)
                maxLapByRace[lapTime.RaceId] = lapTime.Lap;

            if (!fastestLapByRace.TryGetValue(lapTime.RaceId, out var currentFastestLap) ||
                lapTime.Milliseconds < currentFastestLap.Milliseconds)
            {
                fastestLapByRace[lapTime.RaceId] = new FastestCircuitLap(
                    lapTime.RaceId,
                    lapTime.DriverId,
                    lapTime.TimeString,
                    lapTime.Milliseconds);
            }

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

        var finalLapsByRaceId = finalLapByDriverRace.Values
            .GroupBy(x => x.RaceId)
            .ToDictionary(x => x.Key, x => x.ToList());

        return races
            .OrderByDescending(x => x.Year)
            .ThenBy(x => x.Round)
            .Select(race =>
            {
                var circuitName = circuitsById.TryGetValue(race.CircuitId, out var circuit)
                    ? circuit.Name
                    : "N/A";

                var totalLaps = maxLapByRace.GetValueOrDefault(race.RaceId);

                finalLapsByRaceId.TryGetValue(race.RaceId, out var finalLaps);
                finalLaps ??= [];

                fastestLapByRace.TryGetValue(race.RaceId, out var fastestLap);

                var raceResults = finalLaps
                    .Select(finalLap =>
                    {
                        // Account for +2 lap classifications
                        var completedRace = totalLaps > 0 && finalLap.Lap > totalLaps - 3;

                        var driverName = driversById.TryGetValue(finalLap.DriverId, out var driver)
                            ? $"{driver.Forename} {driver.Surname}"
                            : "N/A";

                        return new RaceResult(
                            finalLap.DriverId,
                            driverName,
                            completedRace ? finalLap.Position : null,
                            finalLap.Lap,
                            completedRace);
                    })
                    .OrderBy(x => x.Finished ? 0 : 1)
                    .ThenBy(x => x.Position ?? int.MaxValue)
                    .ThenByDescending(x => x.FinalLap)
                    .ThenBy(x => x.DriverName)
                    .ToList();

                var winnerDriverId = finalLaps
                    .Where(x => totalLaps > 0 && x.Lap == totalLaps)
                    .Where(x => x.Position == 1)
                    .Select(x => x.DriverId)
                    .FirstOrDefault();

                var winner = winnerDriverId != 0 && driversById.TryGetValue(winnerDriverId, out var winningDriver)
                    ? $"{winningDriver.Forename} {winningDriver.Surname}"
                    : "N/A";

                var fastestLapDriver = fastestLap is not null &&
                                        fastestLap.DriverId != 0 &&
                                        driversById.TryGetValue(fastestLap.DriverId, out var fastest)
                    ? $"{fastest.Forename} {fastest.Surname}"
                    : "N/A";

                return new RaceSummary(
                    race.RaceId,
                    race.Name,
                    circuitName,
                    race.Date.ToString("yyyy-MM-dd"),
                    race.Time?.ToString("HH:mm:ss") ?? "N/A",
                    winner,
                    fastestLap,
                    fastestLapDriver,
                    totalLaps,
                    finalLaps.Count,
                    raceResults);
            })
            .ToList();
    }

    private static IReadOnlyList<string> ParseRaceSearchTerms(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Race search must include a race or circuit name. Example: Silverstone");

        var meaningfulTerms = searchTerm
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !IsIgnoredRaceSearchTerm(x))
            .ToList();

        if (meaningfulTerms.Count == 0)
            throw new ArgumentException("Race search must include more than just GP/Grand Prix. Example: French");

        return meaningfulTerms;
    }

    private static bool ContainsAllSearchTerms(string value, IReadOnlyList<string> searchTerms)
    {
        return searchTerms.All(term =>
            value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsIgnoredRaceSearchTerm(string value)
    {
        return value.Equals("gp", StringComparison.OrdinalIgnoreCase)
            || value.Equals("grand", StringComparison.OrdinalIgnoreCase)
            || value.Equals("prix", StringComparison.OrdinalIgnoreCase);
    }
}