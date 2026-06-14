using WilliamsF1.Domain.Models;

namespace WilliamsF1.Application.Interface;

public interface ILapTimeRepository
{
    IAsyncEnumerable<LapTime> StreamAllAsync(CancellationToken cancellationToken = default);
}
