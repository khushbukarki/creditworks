namespace VehicleRegistry.Core.Rules;

public sealed record IconOption(string Key, string DisplayName);

/// <summary>Allowed icons. Each key has an SVG at wwwroot/icons/{key}.svg.</summary>
public static class CategoryIcons
{
    public static IReadOnlyList<IconOption> All { get; } =
    [
        new("feather", "Feather"),
        new("scooter", "Scooter"),
        new("car", "Car"),
        new("van", "Van"),
        new("truck", "Truck"),
        new("anvil", "Anvil"),
    ];

    public static bool IsKnown(string? key) => key is not null && All.Any(icon => icon.Key == key);
}