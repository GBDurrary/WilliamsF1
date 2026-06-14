namespace WilliamsF1.Infrastructure.JsonModels;

internal sealed record LapTimeJson(
    int RaceId,
    int DriverId,
    int Lap,
    int Position,
    string Time,
    double Milliseconds
);