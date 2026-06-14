namespace WilliamsF1.Domain.Models;

public sealed record FastestCircuitLap(
    int RaceId,
    int DriverId,
    string Time,
    int Milliseconds
);
