namespace WilliamsF1.Domain.Models;

public sealed record Race(
    int RaceId,
    int Year,
    int Round,
    int CircuitId,
    string Name,
    DateOnly Date,
    TimeOnly? Time,
    string Url,
    DateOnly? Fp1Date,
    TimeOnly? Fp1Time,
    DateOnly? Fp2Date,
    TimeOnly? Fp2Time,
    DateOnly? Fp3Date,
    TimeOnly? Fp3Time,
    DateOnly? QualiDate,
    TimeOnly? QualiTime,
    DateOnly? SprintDate,
    TimeOnly? SprintTime
);
