# Williams F1 Dataset Tool

A .NET 10 console application for querying and analyzing Formula 1 racing data.

## Quick Start

### Running from Visual Studio
1. Clone from [GitHub](https://github.com/GBDurrary/WilliamsF1)
2. Open WilliamsF1.slnx
3. Set WilliamsF1.Cli as startup project
4. Press F5

### Running from Command Line
From the solution root directory, run the following commands:

#### Interactive Menu
$dotnet run --project .\src\WilliamsF1.Cli

#### View All Circuits
$dotnet run --project .\src\WilliamsF1.Cli -- circuits

#### Search Circuits
$dotnet run --project .\src\WilliamsF1.Cli -- circuits monaco

#### View All Drivers
$dotnet run --project .\src\WilliamsF1.Cli -- drivers

#### Search Drivers
$dotnet run --project .\src\WilliamsF1.Cli -- drivers hamilton

## Project Structure

- **WilliamsF1.Domain**: Core domain models
- **WilliamsF1.Application**: Business logic and services
- **WilliamsF1.Infrastructure**: Data access and repositories
- **WilliamsF1.Cli**: Console UI
- **WilliamsF1.Tests**: Unit tests

## Features

- Circuit summaries with fastest lap data
- Driver statistics and podium counts
- Case-insensitive searching
- Interactive and CLI modes

## Testing

$dotnet test

## Technology Stack

- .NET 10
- C# 14
- xUnit & Moq (testing)
- System.Text.Json



## Resources

- [Ergast Formula 1 Database](http://ergast.com/mdb/)
- [.NET 10 Docs](https://learn.microsoft.com/en-us/dotnet/)
