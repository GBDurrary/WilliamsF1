using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class CachedRaceRepository(IRaceRepository inner)
    : CachedReadOnlyRepository<Race>(inner), IRaceRepository;