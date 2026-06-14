using WilliamsF1.Domain.Models;

namespace WilliamsF1.Application.Interface;

public interface IReadOnlyRepository<T>
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IDriverRepository : IReadOnlyRepository<Driver>;
public interface IDriverStandingRepository : IReadOnlyRepository<DriverStanding>;
public interface ICircuitRepository : IReadOnlyRepository<Circuit>;
public interface IRaceRepository : IReadOnlyRepository<Race>;
