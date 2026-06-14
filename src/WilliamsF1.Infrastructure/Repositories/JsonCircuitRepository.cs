using Microsoft.Extensions.Options;
using WilliamsF1.Application.Interface;
using WilliamsF1.Domain.Models;
using WilliamsF1.Infrastructure.Options;

namespace WilliamsF1.Infrastructure.Repositories;

public sealed class JsonCircuitRepository(IOptions<DatasetOptions> options)
    : JsonReadOnlyRepository<Circuit>(options, "circuits.json"), ICircuitRepository;