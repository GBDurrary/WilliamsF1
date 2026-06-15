namespace WilliamsF1.Domain.Models;

public sealed record FinalDriverLap(
    int RaceId,
    int DriverId,
    int Lap,
    int Position
);
