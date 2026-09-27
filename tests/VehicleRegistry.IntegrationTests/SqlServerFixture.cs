using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.MsSql;
using VehicleRegistry.Core.Entities;
using VehicleRegistry.Infrastructure.Data;
using VehicleRegistry.Infrastructure.Services;

namespace VehicleRegistry.IntegrationTests;

/// <summary>Starts one real SQL Server in Docker and applies the real migrations.</summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "VehicleRegistryTests",
        }.ConnectionString;

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public VehicleRegistryDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<VehicleRegistryDbContext>().UseSqlServer(_connectionString).Options);

    /// <summary>Removes all vehicles and restores the three default categories.</summary>
    public async Task ResetAsync()
    {
        await using var db = CreateDbContext();
        await db.Vehicles.ExecuteDeleteAsync();
        await db.WeightCategories.ExecuteDeleteAsync();
        db.WeightCategories.AddRange(
            new WeightCategory { Name = "Light", IconKey = "feather", MinWeightKg = 0m },
            new WeightCategory { Name = "Medium", IconKey = "car", MinWeightKg = 500m },
            new WeightCategory { Name = "Heavy", IconKey = "truck", MinWeightKg = 2500m });
        await db.SaveChangesAsync();
    }
}

internal sealed class FixedTimeProvider(int year) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(year, 6, 1, 0, 0, 0, TimeSpan.Zero);
}

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SQL Server";
}

/// <summary>Shares the container, resets data before each test, disposes DbContexts after.</summary>
[Collection(DatabaseCollection.Name)]
public abstract class DatabaseTest(SqlServerFixture fixture) : IAsyncLifetime
{
    private readonly List<VehicleRegistryDbContext> _contexts = [];

    protected SqlServerFixture Fixture { get; } = fixture;

    public Task InitializeAsync() => Fixture.ResetAsync();

    public async Task DisposeAsync()
    {
        foreach (var context in _contexts)
            await context.DisposeAsync();
    }

    protected VehicleService NewVehicleService() => new(TrackedContext(), new FixedTimeProvider(2026));
    protected CategoryService NewCategoryService() => new(TrackedContext(), NullLogger<CategoryService>.Instance);

    private VehicleRegistryDbContext TrackedContext()
    {
        var context = Fixture.CreateDbContext();
        _contexts.Add(context);
        return context;
    }

    protected async Task AddVehicleAsync(string owner, decimal weightKg, int manufacturerId = 1, int year = 2020)
    {
        var result = await NewVehicleService().AddVehicleAsync(new(owner, manufacturerId, year, weightKg));
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Message)));
    }

    protected async Task<string> CategoryOfAsync(string owner)
    {
        var vehicles = await NewVehicleService().GetVehiclesAsync(new());
        return vehicles.Single(v => v.OwnerName == owner).CategoryName;
    }

    protected async Task<int> CategoryIdAsync(string name)
    {
        await using var db = Fixture.CreateDbContext();
        return (await db.WeightCategories.SingleAsync(c => c.Name == name)).Id;
    }
}