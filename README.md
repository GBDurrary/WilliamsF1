# Williams F1 Dataset Tool

A .NET 10 console application for querying and analyzing Formula 1 racing data.

## Quick Start

### Running from Visual Studio
1. Clone from [GitHub](https://github.com/GBDurrary/WilliamsF1)
2. Open WilliamsF1.slnx
3. Ensure WilliamsF1.Cli is startup project
4. Press F5

### Running from Command Line
From the solution root directory, run the following commands:

$dotnet run --project .\src\WilliamsF1.Cli
$dotnet run --project .\src\WilliamsF1.Cli -- circuits
$dotnet run --project .\src\WilliamsF1.Cli -- circuits monaco
$dotnet run --project .\src\WilliamsF1.Cli -- drivers
$dotnet run --project .\src\WilliamsF1.Cli -- drivers hamilton
$dotnet run --project .\src\WilliamsF1.Cli -- races silverstone 2018

## Project Structure

- **WilliamsF1.Domain**: Core domain models
- **WilliamsF1.Application**: Business logic and services
- **WilliamsF1.Infrastructure**: Data access and repositories
- **WilliamsF1.Cli**: Console UI
- **WilliamsF1.Tests**: Unit tests

## Features

- Circuit summaries with fastest lap data
- Driver statistics and podium counts
- Race Summary including Race Results and Fastest Lap
- Case-insensitive searching
- Interactive and CLI modes

## Testing

$dotnet test

## Technology Stack

- .NET 10
- C# 14
- xUnit & Moq (testing)
- System.Text.Json

## Comments on the Project

#### Limitations of the Data Source
The Data Source is incomplete, and as such some assumptions have been made which are outlined below.
There is no live data feed, so the application is treated as access to historical data only.
There is only some historical Lap data available, as such some drivers and circuits have limited information. For example, the lap times do not exist for older races, so many older drivers show as having 0 races as there is no data entry joining them together, and many circuits have no fastest lap available.
There is also no data more recent than 2023 as far as I can see which limits the ability to provide up to date information on the current season.


#### Had I had more time, I would have liked to have added the following features:
- Drivers Standings lookup
- A User interface, probably in the form of a Typescript Web Application with more graphical representation of the data.
    - For example a lap by lap replay feature which would show the drivers and their relative positions lap by lap for an individual race.
- A more robust search feature, allowing for searching by multiple fields and with more advanced filtering options.
- Complete unit test coverage - I covered the main logic of the project but there are many areas that could do with more coverage.


## Assumptions

- Dataset provided by Williams Racing is treated as the source of truth.
- Podiums are inferred from final lap positions as official classification data is not made available.
- Sessions Dates and Times are optional as they are rarely included int he data set but do exist for some entries. These will be shown where available. 


## Resources

- [Ergast Formula 1 Database](http://ergast.com/mdb/)
- [.NET 10 Docs](https://learn.microsoft.com/en-us/dotnet/)
