namespace VehicleRegistry.Core.Rules;

/// <summary>A category with its derived range: [MinWeightKg, MaxWeightKgExclusive).</summary>
public sealed record CategoryRange(
    int Id,
    string Name,
    string IconKey,
    decimal MinWeightKg,
    decimal? MaxWeightKgExclusive)
{
    public bool Contains(decimal weightKg) =>
        weightKg >= MinWeightKg && (MaxWeightKgExclusive is null || weightKg < MaxWeightKgExclusive);
}