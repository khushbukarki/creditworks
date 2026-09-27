using Microsoft.EntityFrameworkCore;
using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Services;

namespace VehicleRegistry.IntegrationTests;

public sealed class VehicleServiceTests(SqlServerFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Seeded_manufacturers_are_available()
    {
        var manufacturers = await NewVehicleService().GetManufacturersAsync();
        Assert.Equal(new[] { "Ferrari", "Honda", "Mazda", "Mercedes", "Toyota" }, manufacturers.Select(m => m.Name));
    }

    [Fact]
    public async Task Added_vehicle_is_stored_with_two_decimal_places_and_categorised()
    {
        await AddVehicleAsync("John Smith", 1850.75m);

        var vehicle = Assert.Single(await NewVehicleService().GetVehiclesAsync(new()));
        Assert.Equal(1850.75m, vehicle.WeightKg);
        Assert.Equal("Medium", vehicle.CategoryName);
    }

    [Theory]
    [InlineData(499.99, "Light")]
    [InlineData(500.00, "Medium")]
    [InlineData(2500.00, "Heavy")]
    public async Task Boundary_weights_resolve_to_exactly_one_category(double weight, string expected)
    {
        await AddVehicleAsync("Owner", (decimal)weight);
        Assert.Equal(expected, await CategoryOfAsync("Owner"));
    }

    [Fact]
    public async Task Unknown_manufacturer_is_rejected()
    {
        var result = await NewVehicleService().AddVehicleAsync(new("Jane", 999, 2020, 1000m));

        Assert.Equal(OperationStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, e => e.Field == nameof(VehicleInput.ManufacturerId));
    }

    [Fact]
    public async Task Invalid_vehicle_is_not_saved()
    {
        var result = await NewVehicleService().AddVehicleAsync(new("", 1, 2020, -1m));

        Assert.False(result.Succeeded);
        Assert.Empty(await NewVehicleService().GetVehiclesAsync(new()));
    }

    [Fact]
    public async Task Database_rejects_a_non_positive_weight_even_if_application_validation_is_bypassed()
    {
        await using var db = Fixture.CreateDbContext();
        db.Vehicles.Add(new Vehicle { OwnerName = "Bypass", ManufacturerId = 1, YearOfManufacture = 2020, WeightKg = -1m });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Vehicles_are_sorted_in_the_database()
    {
        await AddVehicleAsync("Aroha", 3000m, manufacturerId: 5, year: 2021); // Toyota
        await AddVehicleAsync("Bella", 450m, manufacturerId: 4, year: 2015);  // Ferrari
        await AddVehicleAsync("Chen", 1200m, manufacturerId: 1, year: 2009);  // Mazda

        var service = NewVehicleService();
        var byWeightDesc = await service.GetVehiclesAsync(new(VehicleSortField.Weight, SortDirection.Descending));
        var byManufacturer = await service.GetVehiclesAsync(new(VehicleSortField.Manufacturer));

        Assert.Equal(new[] { "Aroha", "Chen", "Bella" }, byWeightDesc.Select(v => v.OwnerName));
        Assert.Equal(new[] { "Bella", "Chen", "Aroha" }, byManufacturer.Select(v => v.OwnerName));
    }
}