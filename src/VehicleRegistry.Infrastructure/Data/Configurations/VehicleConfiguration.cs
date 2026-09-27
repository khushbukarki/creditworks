using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Infrastructure.Data.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles", table =>
        {
            // Last line of defence if data bypasses the application.
            table.HasCheckConstraint("CK_Vehicles_WeightKg_Positive", "[WeightKg] > 0");
            table.HasCheckConstraint("CK_Vehicles_YearOfManufacture", $"[YearOfManufacture] >= {VehicleRules.EarliestYear}");
        });

        builder.Property(v => v.OwnerName).IsRequired().HasMaxLength(VehicleRules.MaxOwnerNameLength);
        builder.Property(v => v.WeightKg).HasPrecision(10, 2);

        builder.HasOne(v => v.Manufacturer)
            .WithMany()
            .HasForeignKey(v => v.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}