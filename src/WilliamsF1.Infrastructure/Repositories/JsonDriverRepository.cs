using Microsoft.Extensions.Options;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;
using WilliamsF1.Infrastructure.Options;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class JsonDriverRepository(IOptions<DatasetOptions> options)
    : JsonReadOnlyRepository<Driver>(options, "drivers.json"), IDriverRepository;