namespace WilliamsF1.Domain.Models;

public sealed record DriverStanding(
    int DriverStandingsId,
    int RaceId,
    int DriverId,
    decimal Points,
    int Position,
    int? PositionText, // Seemingly a misnomer in the datasets. This is always an integer and always identical to position
    int Wins
);
