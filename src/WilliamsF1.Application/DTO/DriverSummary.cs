namespace WilliamsF1.Application.DTO;

public sealed record DriverSummary(
    string DriverName,
    string Nationality,
    int TotalRacesEntered,
    int Podiums
);