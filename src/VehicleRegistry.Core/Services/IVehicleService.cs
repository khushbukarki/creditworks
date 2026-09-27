namespace VehicleRegistry.Core.Services;

public sealed record VehicleInput(string? OwnerName, int? ManufacturerId, int? YearOfManufacture, decimal? WeightKg);

public sealed record VehicleListItem(
    int Id, string OwnerName, string Manufacturer, int YearOfManufacture,
    decimal WeightKg, string CategoryName, string CategoryIconKey);

public sealed record ManufacturerOption(int Id, string Name);

public enum VehicleSortField { OwnerName, Manufacturer, Year, Weight }

public enum SortDirection { Ascending, Descending }

public sealed record VehicleSort(
    VehicleSortField Field = VehicleSortField.OwnerName,
    SortDirection Direction = SortDirection.Ascending);

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleListItem>> GetVehiclesAsync(VehicleSort sort, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManufacturerOption>> GetManufacturersAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> AddVehicleAsync(VehicleInput input, CancellationToken cancellationToken = default);
}