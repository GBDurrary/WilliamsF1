using Microsoft.Extensions.Options;
using System.Text.Json;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;
using WilliamsF1.Infrastructure.JsonModels;
using WilliamsF1.Infrastructure.Options;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class JsonLapTimeRepository(IOptions<DatasetOptions> options) : ILapTimeRepository
{
    private const string FileName = "lap_times.json";
    private readonly string _filePath = Path.Combine(options.Value.DatasetPath, FileName);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async IAsyncEnumerable<LapTime> StreamAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                $"Lap times dataset file was not found at path '{_filePath}'.",
                _filePath);
        }

        await using var stream = File.OpenRead(_filePath);

        await foreach (var lapTime in JsonSerializer.DeserializeAsyncEnumerable<LapTimeJson>(
                           stream,
                           JsonOptions,
                           cancellationToken))
        {
            if (lapTime is null)
            {
                continue;
            }

            yield return new LapTime(
                lapTime.RaceId,
                lapTime.DriverId,
                lapTime.Lap,
                lapTime.Position,
                lapTime.Time,
                Convert.ToInt32(Math.Round(lapTime.Milliseconds)));
        }
    }
}