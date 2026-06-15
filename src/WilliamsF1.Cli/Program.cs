using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WilliamsF1.Application.DI;
using WilliamsF1.Application.Interface;
using WilliamsF1.Application.Services;
using WilliamsF1.Infrastructure.DI;
using WilliamsF1.Cli.ConsoleUi;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

using var host = builder.Build();

if (args.Length > 0)
{
    await HandleArgumentsAsync(host.Services, args);
}
else
{
    await RunMenuAsync(host.Services);
}

static async Task HandleArgumentsAsync(IServiceProvider services, string[] args)
{
    var command = args[0].ToLowerInvariant();
    var circuitSummaryService = services.GetRequiredService<CircuitSummaryService>();
    var driverSummaryService = services.GetRequiredService<DriverSummaryService>();
    var raceSummaryService = services.GetRequiredService<RaceSummaryService>();

    switch (command)
    {
        case "circuits":
            if (args.Length > 1)
            {
                var searchTerm = string.Join(" ", args.Skip(1));
                await SearchCircuitSummaryAsync(circuitSummaryService, searchTerm);
            }
            else
            {
                await ShowCircuitSummariesAsync(circuitSummaryService);
            }
            break;

        case "drivers":
            if (args.Length > 1)
            {
                var searchTerm = string.Join(" ", args.Skip(1));
                await SearchDriverSummaryAsync(driverSummaryService, searchTerm);
            }
            else
            {
                await ShowDriverSummariesAsync(driverSummaryService);
            }
            break;

        case "races":
            if (args.Length > 2)
            {
                var yearInput = args[^1];
                var searchTerm = string.Join(" ", args.Skip(1).Take(args.Length - 2));

                if (!int.TryParse(yearInput, out var year))
                {
                    Console.WriteLine("Race year must be a valid number.");
                    return;
                }

                await SearchRaceSummaryAsync(raceSummaryService, searchTerm, year);
            }
            else
            {
                Console.WriteLine("Race year must be entered.");
                return;
            }
            break;

        default:
            Console.WriteLine("Invalid command. Valid commands are:");
            Console.WriteLine("  circuits                  - Show all circuit summaries");
            Console.WriteLine("  circuits <search-term>    - Search circuit summaries");
            Console.WriteLine("  drivers                   - Show all driver summaries");
            Console.WriteLine("  drivers <search-term>     - Search driver summaries");
            Console.WriteLine("  races <search-term> <year> - Search race summaries");
            break;
    }
}

static async Task RunMenuAsync(IServiceProvider services)
{
    var circuitSummaryService = services.GetRequiredService<CircuitSummaryService>();
    var driverSummaryService = services.GetRequiredService<DriverSummaryService>();
    var raceSummaryService = services.GetRequiredService<RaceSummaryService>();

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("Options");
        Console.WriteLine("-------");
        Console.WriteLine("1. Show all circuit summaries");
        Console.WriteLine("2. Search circuit summaries");
        Console.WriteLine("3. Show all driver summaries");
        Console.WriteLine("4. Search driver summaries");
        Console.WriteLine("5. Search race summaries");
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

            case "5":
                await SearchRaceSummaryAsync(raceSummaryService);
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
    var summaries = await LoadingIndicator.RunAsync(
        "Calculating circuit summaries...",
        () => service.GetCircuitSummariesAsync());

    foreach (var summary in summaries)
    {
        Console.WriteLine($"{summary.CircuitName} ({summary.Country})");
        Console.WriteLine($"  Total races:  {summary.TotalRaces}");
        Console.WriteLine($"  Fastest lap*: {summary.FastestLapTime}");
        Console.WriteLine($"  Driver:       {summary.FastestLapDriver}");
        Console.WriteLine($"  Race year:    {summary.RaceYear}");
        Console.WriteLine();
    }

    Console.WriteLine();
    Console.WriteLine("* Within the available data");
}

static async Task SearchCircuitSummaryAsync(CircuitSummaryService service, string? searchTerm = null)
{
    searchTerm ??= PromptForCircuitSearchTerm();

    if (string.IsNullOrWhiteSpace(searchTerm))
    {
        Console.WriteLine("Search term cannot be empty.");
        return;
    }

    var summaries = await LoadingIndicator.RunAsync(
        "Searching circuit summaries...",
        () => service.SearchCircuitSummariesAsync(searchTerm));

    if (summaries.Count <= 0)
    {
        Console.WriteLine("No matching circuit found.");
        return;
    }

    foreach (var summary in summaries)
    {
        Console.WriteLine();
        Console.WriteLine($"{summary.CircuitName} ({summary.Country})");
        Console.WriteLine($"  Total races:  {summary.TotalRaces}");
        Console.WriteLine($"  Fastest lap*: {summary.FastestLapTime}");
        Console.WriteLine($"  Driver:       {summary.FastestLapDriver}");
        Console.WriteLine($"  Race year:    {summary.RaceYear}");
    }

    Console.WriteLine();
    Console.WriteLine("* Within the available data");
}

static string PromptForCircuitSearchTerm()
{
    Console.Write("Enter circuit search term: ");
    return Console.ReadLine() ?? string.Empty;
}

static async Task ShowDriverSummariesAsync(DriverSummaryService service)
{
    var summaries = await LoadingIndicator.RunAsync(
        "Calculating driver summaries...",
        () => service.GetDriverSummariesAsync());

    foreach (var summary in summaries)
    {
        Console.WriteLine(summary.DriverName);
        Console.WriteLine($"  Nationality:    {summary.Nationality}");
        Console.WriteLine($"  Races entered*: {summary.TotalRacesEntered}");
        Console.WriteLine($"  Podiums**:      {summary.Podiums}");
        Console.WriteLine();
    }
    Console.WriteLine();
    Console.WriteLine("* Within the available data");
    Console.WriteLine("** Podiums valid at the checkered flag, Penalties may have affected race results and changed the official classification.");
}

static async Task SearchDriverSummaryAsync(DriverSummaryService service, string? searchTerm = null)
{
    searchTerm ??= PromptForDriverSearchTerm();

    if (string.IsNullOrWhiteSpace(searchTerm))
    {
        Console.WriteLine("Search term cannot be empty.");
        return;
    }

    var summaries = await LoadingIndicator.RunAsync(
        "Searching driver summaries...",
        () => service.SearchDriverSummariesAsync(searchTerm));

    if (summaries.Count <= 0)
    {
        Console.WriteLine("No matching drivers found.");
        return;
    }

    foreach (var summary in summaries)
    {
        Console.WriteLine();
        Console.WriteLine(summary.DriverName);
        Console.WriteLine($"  Nationality:    {summary.Nationality}");
        Console.WriteLine($"  Races entered*: {summary.TotalRacesEntered}");
        Console.WriteLine($"  Podiums**:      {summary.Podiums}");
    }

    Console.WriteLine();
    Console.WriteLine("* Within the available data");
    Console.WriteLine("** Podiums valid at the checkered flag, Penalties may have affected race results and changed the official classification.");
}

static async Task SearchRaceSummaryAsync(
    RaceSummaryService service,
    string? raceOrCircuitSearchTerm = null,
    int? year = null)
{
    raceOrCircuitSearchTerm ??= PromptForRaceSearchTerm();

    if (string.IsNullOrWhiteSpace(raceOrCircuitSearchTerm))
    {
        Console.WriteLine("Race or circuit search term cannot be empty.");
        return;
    }

    year ??= PromptForRaceYear();

    var summaries = await LoadingIndicator.RunAsync(
        "Searching race summaries...",
        () => service.SearchRaceSummariesAsync(raceOrCircuitSearchTerm, year.Value));

    if (summaries.Count <= 0)
    {
        Console.WriteLine("No matching race found.");
        return;
    }

    foreach (var summary in summaries)
    {
        Console.WriteLine();
        Console.WriteLine($"  Race:           {summary.Name}");
        Console.WriteLine($"  Held At:        {summary.Circuit}");
        Console.WriteLine($"  Race Date:      {summary.RaceDate} {summary.RaceStart}");
        Console.WriteLine($"  Winner*:        {summary.Winner}");

        if (summary.FastestLap is not null)
            Console.WriteLine($"  Fastest Lap**:  {summary.FastestLapDriver} - {summary.FastestLap.Time}");

        Console.WriteLine($"  Total Laps:     {summary.TotalLaps}");
        Console.WriteLine($"  Total Drivers:  {summary.TotalDrivers}");
        Console.WriteLine();

        foreach (var result in summary.RaceResults)
        {
            var resultText = result.Finished
                ? $"{result.Position}. {result.DriverName}"
                : $"DNF (Lap {result.FinalLap}). {result.DriverName}";

            if (result.Finished && result.FinalLap < summary.TotalLaps)
                resultText += $" (+ {summary.TotalLaps - result.FinalLap} Laps)";

            Console.WriteLine(resultText);
        }
    }

    Console.WriteLine();
    Console.WriteLine("* As of the checkered flag, Penalties may have affected race results and changed the official classification.");
    Console.WriteLine("** Within the available data");
}

static string PromptForRaceSearchTerm()
{
    Console.Write("Enter circuit or race name. eg. 'Silverstone', 'French GP', or 'Yas Marina': ");
    return Console.ReadLine() ?? string.Empty;
}

static int PromptForRaceYear()
{
    while (true)
    {
        Console.Write("Enter race year. eg. '2018': ");
        var input = Console.ReadLine();

        if (int.TryParse(input, out var year))
            return year;

        Console.WriteLine("Invalid year. Please enter a number, for example 2018.");
        Console.WriteLine();
    }
}

static string PromptForDriverSearchTerm()
{
    Console.Write("Enter driver search term: ");
    return Console.ReadLine() ?? string.Empty;
}