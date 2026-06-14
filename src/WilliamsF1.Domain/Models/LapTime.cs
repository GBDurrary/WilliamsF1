namespace WilliamsF1.Domain.Models;

public sealed record LapTime(
    int RaceId,
    int DriverId,
    int Lap,
    int Position,
    string TimeString,
    int Milliseconds
);
