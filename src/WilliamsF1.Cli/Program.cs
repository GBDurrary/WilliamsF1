using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WilliamsF1.Application.DI;
using WilliamsF1.Application.Interface;
using WilliamsF1.Application.Services;
using WilliamsF1.Infrastructure.DI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

using var host = builder.Build();

await DisplayStartupSummaryAsync(host.Services);
await RunMenuAsync(host.Services);

static async Task DisplayStartupSummaryAsync(IServiceProvider services)
{
    var drivers = await services.GetRequiredService<IDriverRepository>().GetAllAsync();
    var circuits = await services.GetRequiredService<ICircuitRepository>().GetAllAsync();
    var races = await services.GetRequiredService<IRaceRepository>().GetAllAsync();
    var standings = await services.GetRequiredService<IDriverStandingRepository>().GetAllAsync();

    Console.Clear();
    Console.WriteLine("Williams F1 Dataset Tool");
    Console.WriteLine("========================");
    Console.WriteLine();
    Console.WriteLine($"Drivers loaded:          {drivers.Count}");
    Console.WriteLine($"Circuits loaded:         {circuits.Count}");
    Console.WriteLine($"Races loaded:            {races.Count}");
    Console.WriteLine($"Driver standings loaded: {standings.Count}");
    Console.WriteLine();
}

static async Task RunMenuAsync(IServiceProvider services)
{
    var circuitSummaryService = services.GetRequiredService<CircuitSummaryService>();
    var driverSummaryService = services.GetRequiredService<DriverSummaryService>();

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("Options");
        Console.WriteLine("-------");
        Console.WriteLine("1. Show all circuit summaries");
        Console.WriteLine("2. Search circuit summary");
        Console.WriteLine("3. Show all driver summaries");
        Console.WriteLine("4. Search driver summary");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        Console.Write("Select an option: ");

        var input = Console.ReadLine();

        Console.WriteLine();

        switch (input)
        {
            case "1":
                await ShowCircuitSummariesAsync(circuitSummaryService);
                break;

            case "2":
                await SearchCircuitSummaryAsync(circuitSummaryService);
                break;

            case "3":
                await ShowDriverSummariesAsync(driverSummaryService);
                break;

            case "4":
                await SearchDriverSummaryAsync(driverSummaryService);
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }
}

static async Task ShowCircuitSummariesAsync(CircuitSummaryService service)
{
    Console.WriteLine("Calculating circuit summaries...");
    Console.WriteLine();

    var summaries = await service.GetCircuitSummariesAsync();

    foreach (var summary in summaries)
    {
        Console.WriteLine($"{summary.CircuitName} ({summary.Country})");
        Console.WriteLine($"  Total races: {summary.TotalRaces}");
        Console.WriteLine($"  Fastest lap*: {summary.FastestLapTime}");
        Console.WriteLine($"  Driver:      {summary.FastestLapDriver}");
        Console.WriteLine($"  Race year:   {summary.RaceYear}");
        Console.WriteLine();
    }

    Console.WriteLine();
    Console.WriteLine("* Within the available data");
}

static async Task SearchCircuitSummaryAsync(CircuitSummaryService service)
{
    Console.Write("Enter circuit search term: ");
    var searchTerm = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(searchTerm))
    {
        Console.WriteLine("Search term cannot be empty.");
        return;
    }

    var summaries = await service.SearchCircuitSummariesAsync(searchTerm);

    if (summaries is null)
    {
        Console.WriteLine("No matching circuit found.");
        return;
    }

    foreach (var summary in summaries)
    {
        Console.WriteLine();
        Console.WriteLine($"{summary.CircuitName} ({summary.Country})");
        Console.WriteLine($"  Total races: {summary.TotalRaces}");
        Console.WriteLine($"  Fastest lap*: {summary.FastestLapTime}");
        Console.WriteLine($"  Driver:      {summary.FastestLapDriver}");
        Console.WriteLine($"  Race year:   {summary.RaceYear}");
    }

    Console.WriteLine();
    Console.WriteLine("* Within the available data");
}

static async Task ShowDriverSummariesAsync(DriverSummaryService service)
{
    Console.WriteLine("Calculating driver summaries...");
    Console.WriteLine();

    var summaries = await service.GetDriverSummariesAsync();

    foreach (var summary in summaries)
    {
        Console.WriteLine(summary.DriverName);
        Console.WriteLine($"  Nationality:   {summary.Nationality}");
        Console.WriteLine($"  Races entered*: {summary.TotalRacesEntered}");
        Console.WriteLine($"  Podiums**:       {summary.Podiums}");
        Console.WriteLine();
    }
    Console.WriteLine();
    Console.WriteLine("* Within the available data");
    Console.WriteLine("** Podiums valid at the checkered flag, Penalties may have affected race results and changed the official classification.");
}

static async Task SearchDriverSummaryAsync(DriverSummaryService service)
{
    Console.Write("Enter driver search term: ");
    var searchTerm = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(searchTerm))
    {
        Console.WriteLine("Search term cannot be empty.");
        return;
    }

    var summaries = await service.SearchDriverSummariesAsync(searchTerm);

    if (summaries.Count <= 0)
    {
        Console.WriteLine("No matching drivers found.");
        return;
    }

    foreach (var summary in summaries)
    {

        Console.WriteLine();
        Console.WriteLine(summary.DriverName);
        Console.WriteLine($"  Nationality:   {summary.Nationality}");
        Console.WriteLine($"  Races entered*: {summary.TotalRacesEntered}");
        Console.WriteLine($"  Podiums**:       {summary.Podiums}");
    }

    Console.WriteLine();
    Console.WriteLine("* Within the available data");
    Console.WriteLine("** Podiums valid at the checkered flag, Penalties may have affected race results and changed the official classification.");
}