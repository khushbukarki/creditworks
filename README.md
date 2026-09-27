# Vehicle Registry

This is a small web app for keeping track of vehicles and sorting them into weight categories like Light, Medium and Heavy. You can add vehicles, and you can add, change or remove the categories. When a category boundary changes, every vehicle's category updates straight away.

I built it with .NET 10, ASP.NET Core, Entity Framework Core and SQL Server.

## Getting it running

### What you need

- .NET 10 SDK
- SQL Server (LocalDB, Express, or SQL Server in Docker all work)
- Docker Desktop, but only if you want to run the integration tests
- The EF Core tool, which you can install with:

```bash
dotnet tool install --global dotnet-ef
```

### Setting up the database

First, point the connection string in `src/VehicleRegistry.Web/appsettings.json` at your SQL Server. <!-- TODO: check the connection string name -->

Then create the database:

```bash
dotnet ef database update --project src/VehicleRegistry.Infrastructure --startup-project src/VehicleRegistry.Web
```

This sets up the tables and adds the starting data: the default weight categories and the list of manufacturers.

### Building and running

```bash
dotnet build
dotnet run --project src/VehicleRegistry.Web
```

Open the address that shows up in the terminal and you're in.

### Running the tests

The unit tests don't need a database, so they're quick:

```bash
dotnet test tests/VehicleRegistry.UnitTests
```

To run everything, including the integration tests, start Docker Desktop first and then run:

```bash
dotnet test
```

The integration tests spin up a real SQL Server in a throwaway Docker container using Testcontainers. I went this way instead of using an in-memory database because I wanted to be sure things like decimal precision, constraints and sorting behave the same as they would in production. Heads up: the first run is slow because Docker has to download the SQL Server image.

## How the project is organised

I split the code into three projects:

- **VehicleRegistry.Core** holds the actual rules: what makes a valid vehicle, how categories fit together, and how to work out which category a weight belongs to. It doesn't know anything about the database or the website, which makes it easy to test.
- **VehicleRegistry.Infrastructure** handles the database side: the EF Core setup, migrations, seed data, and the services that save and load data.
- **VehicleRegistry.Web** is the website itself, with the vehicle list, the add vehicle form, and the category pages.

The tests follow the same split. The unit tests check the rules in Core on their own, and the integration tests check that everything works properly once a real database is involved.

## The database

There are three main tables.

**Vehicles** holds the owner's name, the manufacturer, the year and the weight. I store the weight as a decimal with two decimal places rather than a float, so a value like 499.99 is stored exactly and doesn't turn into 499.98999. There's also a check constraint in the database that blocks zero or negative weights, so bad data can't get in even if something skips the app's own validation.

**Manufacturers** is a fixed list that gets seeded when the database is created. Vehicles link to it, so you can't save a vehicle with a manufacturer that doesn't exist.

**WeightCategories** stores each category's name, icon, and the weight it starts at. It doesn't store where the category ends. That's just wherever the next one starts. A unique index stops two categories from starting at the same weight.

One decision worth pointing out: **vehicles don't store their category.** It gets worked out whenever it's needed. If I stored it, then every time someone moved a boundary I'd have to go back and update a bunch of vehicle rows, and there'd always be a risk of the two getting out of sync. Calculating it on the fly avoids that problem completely.

## How categories work

The way I think about it is that the categories line up end to end along a number line starting at 0, with no gaps and no overlaps. Every weight lands in exactly one of them.

To find a vehicle's category, I look for the category with the highest starting weight that's still less than or equal to the vehicle's weight.

With the default setup:

- **Light** starts at 0 kg and runs up to just under 500 kg
- **Medium** starts at 500 kg and runs up to just under where Heavy starts
- **Heavy** starts at 2500 kg <!-- TODO: check default --> and has no upper limit

### The rules

- The start of a category is included and the end isn't. So 500 kg is Medium, but 499.99 kg is still Light.
- The lightest category always has to start at 0. If you try to move Light up to 100, it gets rejected, because anything under 100 kg would have nowhere to go.
- Two categories can't start at the same weight.
- Adding a new category splits whichever range it lands in. For example, adding one at 1000 kg means Medium now stops at 1000.
- Deleting a category in the middle hands its range to the lighter category below it, so those vehicles move down one step.
- Deleting the lightest category makes the next one stretch down to 0.
- You can't delete the last category left, because then vehicles would have no category at all.

### Weight checks

A weight has to be more than zero and can have at most two decimal places. So `1850.75` is fine but `1850.755` gets rejected.

## Other bits

You can sort the vehicle list by clicking any column heading. The ▲ or ▼ shows which column it's sorted by and in which direction. The sorting happens in the database query rather than after loading everything into memory. Each category also has its own icon so it's easier to scan the list.

## Assumptions I made

- All weights are in kilograms.
- The heaviest category doesn't have an upper limit.
- The list of manufacturers is fixed, and users can't add new ones from the app.
- It's meant for a single user or a small team, so I didn't add logins.
<!-- TODO: add anything else you assumed, e.g. allowed year range -->

## What it doesn't do (yet)

- There's no login or permissions, so anyone using it can change the categories.
- The vehicle list isn't paginated, which would become a problem with lots of vehicles.
- If two people edit the same category at the same moment, the last save wins.
- Manufacturers can only be changed through the seed data or directly in the database.

## What I'd add with more time

- Paging and search on the vehicle list
- Logins, with only admins allowed to change categories
- A page for managing manufacturers
- Concurrency checks so two people can't overwrite each other's category changes
- Browser-based end-to-end tests, probably with Playwright
- An update to the Testcontainers package to clear the SSH.NET security warning that shows up during restore