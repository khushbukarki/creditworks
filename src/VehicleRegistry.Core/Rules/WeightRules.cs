namespace VehicleRegistry.Core.Rules;

public static class WeightRules
{
    /// <summary>Largest value that fits the decimal(10,2) database column.</summary>
    public const decimal MaxWeightKg = 99_999_999.99m;

    public static bool HasAtMostTwoDecimalPlaces(decimal value) => decimal.Round(value, 2) == value;
}