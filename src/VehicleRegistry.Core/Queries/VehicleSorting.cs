using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Services;

namespace VehicleRegistry.Core.Queries;

/// <summary>Runs as SQL ORDER BY against the database; unit-testable on an in-memory list.</summary>
public static class VehicleSorting
{
    public static IQueryable<Vehicle> Apply(IQueryable<Vehicle> query, VehicleSort sort)
    {
        var ascending = sort.Direction == SortDirection.Ascending;

        IOrderedQueryable<Vehicle> ordered = sort.Field switch
        {
            VehicleSortField.Manufacturer => ascending
                ? query.OrderBy(v => v.Manufacturer!.Name)
                : query.OrderByDescending(v => v.Manufacturer!.Name),
            VehicleSortField.Year => ascending
                ? query.OrderBy(v => v.YearOfManufacture)
                : query.OrderByDescending(v => v.YearOfManufacture),
            VehicleSortField.Weight => ascending
                ? query.OrderBy(v => v.WeightKg)
                : query.OrderByDescending(v => v.WeightKg),
            _ => ascending
                ? query.OrderBy(v => v.OwnerName)
                : query.OrderByDescending(v => v.OwnerName),
        };

        return ordered.ThenBy(v => v.Id); // stable order for equal values
    }
}