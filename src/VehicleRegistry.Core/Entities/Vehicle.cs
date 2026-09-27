namespace VehicleRegistry.Core.Entities;

/// <summary>
/// A vehicle. Its category is deliberately NOT stored: it is calculated from WeightKg
/// and the current categories every time, so it can never become out of date.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int ManufacturerId { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public int YearOfManufacture { get; set; }
    public decimal WeightKg { get; set; }
}