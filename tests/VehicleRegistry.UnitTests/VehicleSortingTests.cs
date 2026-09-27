using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Queries;
using VehicleRegistry.Core.Services;

namespace VehicleRegistry.UnitTests;

public class VehicleSortingTests
{
    private static readonly Manufacturer Mazda = new() { Id = 1, Name = "Mazda" };
    private static readonly Manufacturer Toyota = new() { Id = 5, Name = "Toyota" };
    private static readonly Manufacturer Honda = new() { Id = 3, Name = "Honda" };

    private static IQueryable<Vehicle> Vehicles() => new List<Vehicle>
    {
        new() { Id = 1, OwnerName = "Bella", Manufacturer = Toyota, YearOfManufacture = 2015, WeightKg = 1200m },
        new() { Id = 2, OwnerName = "Aroha", Manufacturer = Mazda, YearOfManufacture = 2021, WeightKg = 3000.5m },
        new() { Id = 3, OwnerName = "Chen", Manufacturer = Honda, YearOfManufacture = 2009, WeightKg = 450m },
    }.AsQueryable();

    [Theory]
    [InlineData(VehicleSortField.OwnerName, SortDirection.Ascending, new[] { "Aroha", "Bella", "Chen" })]
    [InlineData(VehicleSortField.OwnerName, SortDirection.Descending, new[] { "Chen", "Bella", "Aroha" })]
    [InlineData(VehicleSortField.Manufacturer, SortDirection.Ascending, new[] { "Chen", "Aroha", "Bella" })]
    [InlineData(VehicleSortField.Year, SortDirection.Descending, new[] { "Aroha", "Bella", "Chen" })]
    [InlineData(VehicleSortField.Weight, SortDirection.Ascending, new[] { "Chen", "Bella", "Aroha" })]
    [InlineData(VehicleSortField.Weight, SortDirection.Descending, new[] { "Aroha", "Bella", "Chen" })]
    public void Sorts_by_the_requested_field_and_direction(VehicleSortField field, SortDirection direction, string[] expectedOwners)
    {
        var owners = VehicleSorting.Apply(Vehicles(), new VehicleSort(field, direction)).Select(v => v.OwnerName);
        Assert.Equal(expectedOwners, owners);
    }

    [Fact]
    public void Equal_values_are_ordered_by_id_so_the_order_is_stable()
    {
        var vehicles = new List<Vehicle>
        {
            new() { Id = 2, OwnerName = "Same", Manufacturer = Mazda, YearOfManufacture = 2020, WeightKg = 1000m },
            new() { Id = 1, OwnerName = "Same", Manufacturer = Mazda, YearOfManufacture = 2020, WeightKg = 1000m },
        }.AsQueryable();

        var ids = VehicleSorting.Apply(vehicles, new VehicleSort(VehicleSortField.OwnerName)).Select(v => v.Id);

        Assert.Equal(new[] { 1, 2 }, ids);
    }
}