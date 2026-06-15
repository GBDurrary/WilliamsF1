using Microsoft.Extensions.DependencyInjection;
using WilliamsF1.Application.Services;

namespace WilliamsF1.Application.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<CircuitSummaryService>();
        services.AddSingleton<DriverSummaryService>();
        services.AddSingleton<RaceSummaryService>();

        return services;
    }
}
