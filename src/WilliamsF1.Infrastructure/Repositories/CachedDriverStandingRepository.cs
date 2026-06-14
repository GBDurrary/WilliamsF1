using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class CachedDriverStandingRepository(IDriverStandingRepository inner)
    : CachedReadOnlyRepository<DriverStanding>(inner), IDriverStandingRepository;