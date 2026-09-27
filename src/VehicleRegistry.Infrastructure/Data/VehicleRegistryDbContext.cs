using Microsoft.EntityFrameworkCore;
using VehicleRegistry.Core.Entities;

namespace VehicleRegistry.Infrastructure.Data;

public class VehicleRegistryDbContext(DbContextOptions<VehicleRegistryDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<WeightCategory> WeightCategories => Set<WeightCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleRegistryDbContext).Assembly);
}