namespace VehicleRegistry.Core.Entities;

/// <summary>Lookup table so manufacturers are data, not hard-coded values.</summary>
public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}