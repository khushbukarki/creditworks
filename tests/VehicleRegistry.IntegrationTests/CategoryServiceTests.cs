using VehicleRegistry.Core.Services;

namespace VehicleRegistry.IntegrationTests;

public sealed class CategoryServiceTests(SqlServerFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Changing_a_boundary_recategorises_existing_vehicles()
    {
        // The exact example from §6 of the brief.
        await AddVehicleAsync("Jane Turei", 2200m);
        Assert.Equal("Medium", await CategoryOfAsync("Jane Turei"));

        var result = await NewCategoryService()
            .UpdateAsync(await CategoryIdAsync("Heavy"), new("Heavy", "truck", 2000m));

        Assert.True(result.Succeeded);
        Assert.Equal("Heavy", await CategoryOfAsync("Jane Turei"));
    }

    [Fact]
    public async Task Creating_a_category_splits_the_range_it_starts_in()
    {
        await AddVehicleAsync("Truckie", 12000m);

        var result = await NewCategoryService().CreateAsync(new("Extra heavy", "anvil", 10000m));

        Assert.True(result.Succeeded);
        var ranges = await NewCategoryService().GetCategoriesAsync();
        Assert.Equal(new[] { "Light", "Medium", "Heavy", "Extra heavy" }, ranges.Select(r => r.Name));
        Assert.Equal(10000m, ranges.Single(r => r.Name == "Heavy").MaxWeightKgExclusive);
        Assert.Equal("Extra heavy", await CategoryOfAsync("Truckie"));
    }

    [Fact]
    public async Task Category_starting_where_another_starts_is_rejected_and_nothing_is_saved()
    {
        var result = await NewCategoryService().CreateAsync(new("Overlap", "van", 500m));

        Assert.Equal(OperationStatus.ValidationFailed, result.Status);
        Assert.Equal(3, (await NewCategoryService().GetCategoriesAsync()).Count);
    }

    [Fact]
    public async Task Moving_the_lightest_category_away_from_zero_is_rejected_as_a_gap()
    {
        var result = await NewCategoryService()
            .UpdateAsync(await CategoryIdAsync("Light"), new("Light", "feather", 100m));

        Assert.Equal(OperationStatus.ValidationFailed, result.Status);
        Assert.Equal(0m, (await NewCategoryService().GetCategoriesAsync()).First().MinWeightKg);
    }

    [Fact]
    public async Task Deleting_a_middle_category_moves_its_vehicles_to_the_lighter_neighbour()
    {
        await AddVehicleAsync("Mid", 2200m);

        var result = await NewCategoryService().DeleteAsync(await CategoryIdAsync("Medium"));

        Assert.True(result.Succeeded);
        Assert.Equal("Light", await CategoryOfAsync("Mid"));
    }

    [Fact]
    public async Task Deleting_the_lightest_category_extends_the_next_one_down_to_zero()
    {
        await AddVehicleAsync("Small", 100m);

        var result = await NewCategoryService().DeleteAsync(await CategoryIdAsync("Light"));

        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Message)));
        var ranges = await NewCategoryService().GetCategoriesAsync();
        Assert.Equal(0m, ranges.Single(r => r.Name == "Medium").MinWeightKg);
        Assert.Equal("Medium", await CategoryOfAsync("Small"));
    }

    [Fact]
    public async Task The_only_remaining_category_cannot_be_deleted()
    {
        Assert.True((await NewCategoryService().DeleteAsync(await CategoryIdAsync("Heavy"))).Succeeded);
        Assert.True((await NewCategoryService().DeleteAsync(await CategoryIdAsync("Medium"))).Succeeded);

        var result = await NewCategoryService().DeleteAsync(await CategoryIdAsync("Light"));

        Assert.Equal(OperationStatus.ValidationFailed, result.Status);
    }

    [Fact]
    public async Task Missing_category_returns_not_found()
    {
        Assert.Equal(OperationStatus.NotFound, (await NewCategoryService().UpdateAsync(-1, new("X", "car", 10m))).Status);
        Assert.Equal(OperationStatus.NotFound, (await NewCategoryService().DeleteAsync(-1)).Status);
    }
}