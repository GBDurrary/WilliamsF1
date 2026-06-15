using WilliamsF1.Domain.Models;

namespace WilliamsF1.Application.DTO;

public sealed record RaceSummary(
    int RaceId,
    string Name,
    string Circuit,
    string RaceDate,
    string RaceStart,
    string Winner,
    FastestCircuitLap? FastestLap,
    string FastestLapDriver,
    int TotalLaps,
    int TotalDrivers,
    IReadOnlyList<RaceResult> RaceResults
);