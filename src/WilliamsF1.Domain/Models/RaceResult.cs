namespace WilliamsF1.Domain.Models;
public sealed record RaceResult(
    int DriverId,
    string DriverName,
    int? Position,
    int FinalLap,
    bool Finished
);
