using System.Text.Json.Serialization;

namespace WilliamsF1.Infrastructure.JsonModels;

internal sealed record RaceJson(
    int RaceId,
    int Year,
    int Round,
    int CircuitId,
    string Name,
    DateOnly Date,
    string? Time,
    string Url,
    [property: JsonPropertyName("fp1_date")] string? Fp1Date,
    [property: JsonPropertyName("fp1_time")] string? Fp1Time,
    [property: JsonPropertyName("fp2_date")] string? Fp2Date,
    [property: JsonPropertyName("fp2_time")] string? Fp2Time,
    [property: JsonPropertyName("fp3_date")] string? Fp3Date,
    [property: JsonPropertyName("fp3_time")] string? Fp3Time,
    [property: JsonPropertyName("quali_date")] string? QualiDate,
    [property: JsonPropertyName("quali_time")] string? QualiTime,
    [property: JsonPropertyName("sprint_date")] string? SprintDate,
    [property: JsonPropertyName("sprint_time")] string? SprintTime
);