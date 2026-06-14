namespace WilliamsF1.Application.DTO;

public sealed record CircuitSummary(
    string CircuitName,
    string Country,
    int TotalRaces,
    string FastestLapTime,
    string FastestLapDriver,
    string RaceYear
);
