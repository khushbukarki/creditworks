using System.Globalization;
using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.UnitTests;

public class CategoryRulesTests
{
    private static List<WeightCategory> DefaultCategories() =>
    [
        new() { Id = 1, Name = "Light", IconKey = "feather", MinWeightKg = 0m },
        new() { Id = 2, Name = "Medium", IconKey = "car", MinWeightKg = 500m },
        new() { Id = 3, Name = "Heavy", IconKey = "truck", MinWeightKg = 2500m },
    ];

    [Theory]
    [InlineData("0.01", "Light")]
    [InlineData("499.99", "Light")]
    [InlineData("500.00", "Medium")]
    [InlineData("500.01", "Medium")]
    [InlineData("2499.99", "Medium")]
    [InlineData("2500.00", "Heavy")]
    [InlineData("99999999.99", "Heavy")]
    public void Resolve_uses_inclusive_lower_and_exclusive_upper_bounds(string weight, string expectedCategory)
    {
        var category = CategoryRules.Resolve(decimal.Parse(weight, CultureInfo.InvariantCulture), DefaultCategories());
        Assert.Equal(expectedCategory, category.Name);
    }

    [Fact]
    public void Resolve_does_not_depend_on_the_order_categories_are_supplied_in()
    {
        var reversed = DefaultCategories().AsEnumerable().Reverse();
        Assert.Equal("Medium", CategoryRules.Resolve(2200m, reversed).Name);
    }

    [Fact]
    public void Resolve_reflects_changed_category_boundaries()
    {
        var categories = DefaultCategories();
        Assert.Equal("Medium", CategoryRules.Resolve(2200m, categories).Name);

        categories.Single(c => c.Name == "Heavy").MinWeightKg = 2000m;

        Assert.Equal("Heavy", CategoryRules.Resolve(2200m, categories).Name);
    }

    [Fact]
    public void Resolve_throws_when_no_category_covers_the_weight()
    {
        var withoutLight = DefaultCategories().Where(c => c.Name != "Light");
        Assert.Throws<InvalidOperationException>(() => CategoryRules.Resolve(100m, withoutLight));
    }

    [Fact]
    public void ToRanges_produces_contiguous_ranges_starting_at_zero_with_an_open_ended_last_range()
    {
        var ranges = CategoryRules.ToRanges(DefaultCategories());

        Assert.Equal(0m, ranges[0].MinWeightKg);
        for (var i = 0; i < ranges.Count - 1; i++)
            Assert.Equal(ranges[i + 1].MinWeightKg, ranges[i].MaxWeightKgExclusive);
        Assert.Null(ranges[^1].MaxWeightKgExclusive);
    }

    [Fact]
    public void Every_weight_falls_in_exactly_one_range()
    {
        var ranges = CategoryRules.ToRanges(DefaultCategories());

        for (var weight = 0.01m; weight <= 3000m; weight += 0.49m)
            Assert.Single(ranges, r => r.Contains(weight));

        foreach (var boundary in new[] { 500m, 2500m })
            Assert.Single(ranges, r => r.Contains(boundary));
    }

    [Fact]
    public void Default_configuration_is_valid()
    {
        Assert.Empty(CategoryRules.ValidateConfiguration(DefaultCategories()));
    }

    [Fact]
    public void Configuration_without_a_category_starting_at_zero_is_rejected_as_a_gap()
    {
        var categories = DefaultCategories();
        categories.Single(c => c.Name == "Light").MinWeightKg = 100m;

        var errors = CategoryRules.ValidateConfiguration(categories);

        Assert.Contains(errors, e => e.Field == nameof(WeightCategory.MinWeightKg) && e.Message.Contains("0 kg"));
    }

    [Fact]
    public void Two_categories_starting_at_the_same_weight_are_rejected_as_an_overlap()
    {
        var categories = DefaultCategories();
        categories.Add(new WeightCategory { Name = "Van", IconKey = "van", MinWeightKg = 500m });

        var errors = CategoryRules.ValidateConfiguration(categories);

        Assert.Contains(errors, e => e.Message.Contains("Only one category can start at 500 kg"));
    }

    [Fact]
    public void Empty_configuration_is_rejected()
    {
        Assert.NotEmpty(CategoryRules.ValidateConfiguration([]));
    }

    [Fact]
    public void Duplicate_names_are_rejected_regardless_of_case()
    {
        var categories = DefaultCategories();
        categories.Add(new WeightCategory { Name = " light ", IconKey = "van", MinWeightKg = 5000m });

        var errors = CategoryRules.ValidateConfiguration(categories);

        Assert.Contains(errors, e => e.Field == nameof(WeightCategory.Name));
    }

    [Theory]
    [InlineData("", "car", "10", nameof(WeightCategory.Name))]
    [InlineData("   ", "car", "10", nameof(WeightCategory.Name))]
    [InlineData("Van", "", "10", nameof(WeightCategory.IconKey))]
    [InlineData("Van", "rocket", "10", nameof(WeightCategory.IconKey))]
    [InlineData("Van", "car", "-1", nameof(WeightCategory.MinWeightKg))]
    [InlineData("Van", "car", "10.555", nameof(WeightCategory.MinWeightKg))]
    public void Invalid_category_fields_are_rejected(string name, string iconKey, string minWeight, string expectedField)
    {
        var errors = CategoryRules.ValidateCategory(name, iconKey, decimal.Parse(minWeight, CultureInfo.InvariantCulture));
        Assert.Contains(errors, e => e.Field == expectedField);
    }

    [Theory]
    [InlineData(0, 3, 1)]
    [InlineData(1, 3, 0)]
    [InlineData(2, 3, 1)]
    public void AbsorbingIndex_picks_the_neighbour_that_takes_over_the_range(int index, int count, int expected)
    {
        Assert.Equal(expected, CategoryRules.AbsorbingIndex(index, count));
    }

    [Fact]
    public void AbsorbingIndex_is_null_for_the_only_category()
    {
        Assert.Null(CategoryRules.AbsorbingIndex(0, 1));
    }
}