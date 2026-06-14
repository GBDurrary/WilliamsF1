using System.Text.Json;
using Microsoft.Extensions.Options;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;
using WilliamsF1.Infrastructure.JsonModels;
using WilliamsF1.Infrastructure.Options;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class JsonRaceRepository(IOptions<DatasetOptions> options) : IRaceRepository
{
    private readonly string _filePath = Path.Combine(options.Value.DatasetPath, "races.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Race>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                $"Race dataset file was not found at path '{_filePath}'.",
                _filePath);
        }

        await using var stream = File.OpenRead(_filePath);

        var races = await JsonSerializer.DeserializeAsync<List<RaceJson>>(
            stream,
            JsonOptions,
            cancellationToken);

        return races?
            .Select(MapToDomain)
            .ToList()
            ?? [];
    }

    private static Race MapToDomain(RaceJson race)
    {
        return new Race(
            race.RaceId,
            race.Year,
            race.Round,
            race.CircuitId,
            race.Name,
            race.Date,
            ParseOptionalTime(race.Time),
            race.Url,
            ParseOptionalDate(race.Fp1Date),
            ParseOptionalTime(race.Fp1Time),
            ParseOptionalDate(race.Fp2Date),
            ParseOptionalTime(race.Fp2Time),
            ParseOptionalDate(race.Fp3Date),
            ParseOptionalTime(race.Fp3Time),
            ParseOptionalDate(race.QualiDate),
            ParseOptionalTime(race.QualiTime),
            ParseOptionalDate(race.SprintDate),
            ParseOptionalTime(race.SprintTime));
    }

    private static DateOnly? ParseOptionalDate(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || value == "\\N"
            ? null
            : DateOnly.Parse(value);
    }

    private static TimeOnly? ParseOptionalTime(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || value == "\\N"
            ? null
            : TimeOnly.Parse(value);
    }
}