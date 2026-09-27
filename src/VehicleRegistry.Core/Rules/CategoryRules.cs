using VehicleRegistry.Core.Entities;

namespace VehicleRegistry.Core.Rules;

/// <summary>
/// Boundary rule: every range is lower-inclusive, upper-exclusive, i.e. [min, next min).
/// Default set: 499.99 kg = Light, 500.00 kg = Medium, 2500.00 kg = Heavy.
/// </summary>
public static class CategoryRules
{
    public const int MaxNameLength = 50;
    public const decimal LowestWeightKg = 0m;

    public static IReadOnlyList<ValidationError> ValidateCategory(string? name, string? iconKey, decimal minWeightKg)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new(nameof(WeightCategory.Name), "Category name is required."));
        else if (name.Trim().Length > MaxNameLength)
            errors.Add(new(nameof(WeightCategory.Name), $"Category name must be {MaxNameLength} characters or fewer."));

        if (string.IsNullOrWhiteSpace(iconKey))
            errors.Add(new(nameof(WeightCategory.IconKey), "Choose an icon for the category."));
        else if (!CategoryIcons.IsKnown(iconKey))
            errors.Add(new(nameof(WeightCategory.IconKey), "Choose one of the available icons."));

        if (minWeightKg < LowestWeightKg)
            errors.Add(new(nameof(WeightCategory.MinWeightKg), "Starting weight can't be negative."));
        else if (minWeightKg > WeightRules.MaxWeightKg)
            errors.Add(new(nameof(WeightCategory.MinWeightKg), $"Starting weight must be no more than {WeightRules.MaxWeightKg:#,0.00} kg."));
        else if (!WeightRules.HasAtMostTwoDecimalPlaces(minWeightKg))
            errors.Add(new(nameof(WeightCategory.MinWeightKg), "Starting weight can have at most two decimal places."));

        return errors;
    }

    /// <summary>A valid configuration guarantees every weight >= 0 resolves to exactly one category.</summary>
    public static IReadOnlyList<ValidationError> ValidateConfiguration(IEnumerable<WeightCategory> categories)
    {
        var list = categories.ToList();
        if (list.Count == 0)
            return [new(string.Empty, "At least one category is required so that every vehicle has a category.")];

        var errors = list
            .SelectMany(c => ValidateCategory(c.Name, c.IconKey, c.MinWeightKg))
            .ToList();

        // 5.1 No gap below the first category.
        if (!list.Any(c => c.MinWeightKg == LowestWeightKg))
            errors.Add(new(nameof(WeightCategory.MinWeightKg),
                "The lightest category must start at 0 kg, otherwise lighter vehicles would have no category."));

        // 5.2 Two categories starting at the same weight would overlap.
        foreach (var duplicate in list.GroupBy(c => c.MinWeightKg).Where(g => g.Count() > 1))
            errors.Add(new(nameof(WeightCategory.MinWeightKg),
                $"Only one category can start at {duplicate.Key:#,0.##} kg ({string.Join(", ", duplicate.Select(c => c.Name))})."));

        foreach (var duplicate in list
                     .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                     .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1))
            errors.Add(new(nameof(WeightCategory.Name), $"A category called '{duplicate.Key}' already exists."));

        return errors;
    }

    /// <summary>Derives each category's full range. Consecutive ranges always meet exactly.</summary>
    public static IReadOnlyList<CategoryRange> ToRanges(IEnumerable<WeightCategory> categories)
    {
        var ordered = categories.OrderBy(c => c.MinWeightKg).ToList();
        return ordered
            .Select((c, i) => new CategoryRange(
                c.Id, c.Name, c.IconKey, c.MinWeightKg,
                i + 1 < ordered.Count ? ordered[i + 1].MinWeightKg : null))
            .ToList();
    }

    /// <summary>Returns the category with the greatest start <= weight.</summary>
    public static WeightCategory Resolve(decimal weightKg, IEnumerable<WeightCategory> categories)
    {
        WeightCategory? match = null;
        foreach (var category in categories)
        {
            if (category.MinWeightKg <= weightKg && (match is null || category.MinWeightKg > match.MinWeightKg))
                match = category;
        }

        return match ?? throw new InvalidOperationException(
            $"The category configuration does not cover a weight of {weightKg} kg.");
    }

    /// <summary>
    /// When the category at index is deleted, the lighter neighbour takes over its range,
    /// or the next heavier one if it was the lightest. Null if it is the only category.
    /// </summary>
    public static int? AbsorbingIndex(int index, int count) =>
        count <= 1 ? null : index > 0 ? index - 1 : 1;
}