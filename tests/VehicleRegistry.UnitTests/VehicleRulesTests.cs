using System.Globalization;
using VehicleRegistry.Core.Rules;
using VehicleRegistry.Core.Services;

namespace VehicleRegistry.UnitTests;

public class VehicleRulesTests
{
    private const int CurrentYear = 2026;

    private static VehicleInput Valid() => new("John Smith", 1, 2019, 1850.75m);

    [Fact]
    public void Valid_vehicle_has_no_errors()
    {
        Assert.Empty(VehicleRules.Validate(Valid(), CurrentYear));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Owner_name_is_required(string? ownerName)
    {
        AssertSingleErrorFor(Valid() with { OwnerName = ownerName }, nameof(VehicleInput.OwnerName));
    }

    [Fact]
    public void Manufacturer_is_required()
    {
        AssertSingleErrorFor(Valid() with { ManufacturerId = null }, nameof(VehicleInput.ManufacturerId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1885)]
    [InlineData(CurrentYear + 2)]
    public void Year_must_be_present_and_sensible(int? year)
    {
        AssertSingleErrorFor(Valid() with { YearOfManufacture = year }, nameof(VehicleInput.YearOfManufacture));
    }

    [Theory]
    [InlineData(1886)]
    [InlineData(CurrentYear)]
    [InlineData(CurrentYear + 1)]
    public void Year_boundaries_are_accepted(int year)
    {
        Assert.Empty(VehicleRules.Validate(Valid() with { YearOfManufacture = year }, CurrentYear));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1850.755")]
    [InlineData("100000000")]
    public void Invalid_weights_are_rejected(string? weight)
    {
        var parsed = weight is null ? (decimal?)null : decimal.Parse(weight, CultureInfo.InvariantCulture);
        AssertSingleErrorFor(Valid() with { WeightKg = parsed }, nameof(VehicleInput.WeightKg));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("1850.7")]
    [InlineData("1850.70")]
    [InlineData("99999999.99")]
    public void Valid_weights_are_accepted(string weight)
    {
        var input = Valid() with { WeightKg = decimal.Parse(weight, CultureInfo.InvariantCulture) };
        Assert.Empty(VehicleRules.Validate(input, CurrentYear));
    }

    private static void AssertSingleErrorFor(VehicleInput input, string field)
    {
        var error = Assert.Single(VehicleRules.Validate(input, CurrentYear));
        Assert.Equal(field, error.Field);
    }
}