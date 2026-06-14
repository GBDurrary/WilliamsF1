using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WilliamsF1.Application.Interface;
using WilliamsF1.Infrastructure.Options;
using WilliamsF1.Infrastructure.Repositories;

namespace WilliamsF1.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatasetOptions>(
            configuration.GetSection("Dataset"));

        services.AddSingleton<JsonDriverRepository>();
        services.AddSingleton<IDriverRepository>(sp =>
            new CachedDriverRepository(
                sp.GetRequiredService<JsonDriverRepository>()));

        services.AddSingleton<JsonCircuitRepository>();
        services.AddSingleton<ICircuitRepository>(sp =>
            new CachedCircuitRepository(
                sp.GetRequiredService<JsonCircuitRepository>()));

        services.AddSingleton<JsonRaceRepository>();
        services.AddSingleton<IRaceRepository>(sp =>
            new CachedRaceRepository(
                sp.GetRequiredService<JsonRaceRepository>()));

        services.AddSingleton<JsonDriverStandingRepository>();
        services.AddSingleton<IDriverStandingRepository>(sp =>
            new CachedDriverStandingRepository(
                sp.GetRequiredService<JsonDriverStandingRepository>()));

        services.AddSingleton<ILapTimeRepository, JsonLapTimeRepository>();

        return services;
    }
}