using Microsoft.EntityFrameworkCore;
using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Queries;
using VehicleRegistry.Core.Rules;
using VehicleRegistry.Core.Services;
using VehicleRegistry.Infrastructure.Data;

namespace VehicleRegistry.Infrastructure.Services;

public sealed class VehicleService(VehicleRegistryDbContext db, TimeProvider clock) : IVehicleService
{
    public async Task<IReadOnlyList<VehicleListItem>> GetVehiclesAsync(VehicleSort sort, CancellationToken cancellationToken = default)
    {
        // Category is calculated at read time, so it always reflects the current configuration (§6).
        var categories = await db.WeightCategories.AsNoTracking().ToListAsync(cancellationToken);

        var vehicles = await VehicleSorting.Apply(db.Vehicles.AsNoTracking(), sort)
            .Select(v => new { v.Id, v.OwnerName, Manufacturer = v.Manufacturer!.Name, v.YearOfManufacture, v.WeightKg })
            .ToListAsync(cancellationToken);

        return vehicles
            .Select(v =>
            {
                var category = CategoryRules.Resolve(v.WeightKg, categories);
                return new VehicleListItem(v.Id, v.OwnerName, v.Manufacturer, v.YearOfManufacture, v.WeightKg,
                    category.Name, category.IconKey);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ManufacturerOption>> GetManufacturersAsync(CancellationToken cancellationToken = default) =>
        await db.Manufacturers
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerOption(m.Id, m.Name))
            .ToListAsync(cancellationToken);

    public async Task<OperationResult> AddVehicleAsync(VehicleInput input, CancellationToken cancellationToken = default)
    {
        var errors = VehicleRules.Validate(input, clock.GetLocalNow().Year).ToList();

        if (input.ManufacturerId is int manufacturerId &&
            !await db.Manufacturers.AnyAsync(m => m.Id == manufacturerId, cancellationToken))
        {
            errors.Add(new(nameof(VehicleInput.ManufacturerId), "Choose a manufacturer from the list."));
        }

        if (errors.Count > 0)
            return OperationResult.Invalid(errors);

        db.Vehicles.Add(new Vehicle
        {
            OwnerName = input.OwnerName!.Trim(),
            ManufacturerId = input.ManufacturerId!.Value,
            YearOfManufacture = input.YearOfManufacture!.Value,
            WeightKg = input.WeightKg!.Value,
        });
        await db.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}