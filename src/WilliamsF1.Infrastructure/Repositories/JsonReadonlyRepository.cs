using Microsoft.Extensions.Options;
using System.Text.Json;
using WilliamsF1.Application.Interface;
using WilliamsF1.Infrastructure.Options;

namespace WilliamsF1.Infrastructure.Repositories;

public abstract class JsonReadOnlyRepository<T>(
    IOptions<DatasetOptions> options,
    string fileName) : IReadOnlyRepository<T>
{
    private readonly string _filePath = Path.Combine(options.Value.DatasetPath, fileName);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var datasetDir = Path.GetFullPath(options.Value.DatasetPath);

        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                $"{typeof(T).Name} dataset file was not found at path '{_filePath}'.",
                _filePath);
        }

        await using var stream = File.OpenRead(_filePath);

        var items = await JsonSerializer.DeserializeAsync<List<T>>(
            stream,
            JsonOptions,
            cancellationToken);

        return items ?? [];
    }
}