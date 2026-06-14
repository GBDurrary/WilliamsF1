using Microsoft.Extensions.Options;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;
using WilliamsF1.Infrastructure.Options;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class JsonDriverStandingRepository(IOptions<DatasetOptions> options)
    : JsonReadOnlyRepository<DriverStanding>(options, "driver_standings.json"), IDriverStandingRepository;