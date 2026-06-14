using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class CachedDriverRepository(IDriverRepository inner)
    : CachedReadOnlyRepository<Driver>(inner), IDriverRepository;