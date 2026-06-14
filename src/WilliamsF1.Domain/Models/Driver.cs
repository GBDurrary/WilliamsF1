using System.Text.Json.Serialization;

namespace WilliamsF1.Domain.Models;

public sealed record Driver(
    int DriverId,
    string DriverRef,
    string? Number,
    string? Code,
    string Forename,
    string Surname,
    [property: JsonPropertyName("dob")] DateOnly DateOfBirth,
    string Nationality
);
