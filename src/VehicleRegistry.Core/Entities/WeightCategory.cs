namespace VehicleRegistry.Core.Entities;

/// <summary>
/// Only the inclusive LOWER bound is stored. A category ends where the next one starts,
/// and the heaviest one is open-ended, so gaps and overlaps cannot be represented.
/// </summary>
public class WeightCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public decimal MinWeightKg { get; set; }
}