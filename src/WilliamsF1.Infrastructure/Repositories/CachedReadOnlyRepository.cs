using WilliamsF1.Application.Interface;

namespace WilliamsF1.Infrastructure.Repositories;

public class CachedReadOnlyRepository<T>(
    IReadOnlyRepository<T> inner) : IReadOnlyRepository<T>
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyList<T>? _cache;

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            _cache ??= await inner.GetAllAsync(cancellationToken);
            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }
}