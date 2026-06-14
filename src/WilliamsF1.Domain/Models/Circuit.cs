namespace WilliamsF1.Domain.Models;

public sealed record Circuit(
    int CircuitId,
    string CircuitRef,
    string Name,
    string Location,
    string Country,
    double Lat,
    double Lng,
    int Alt,
    string Url
);
