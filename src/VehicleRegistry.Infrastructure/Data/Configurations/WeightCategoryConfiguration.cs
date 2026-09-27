using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRegistry.Core.Entities;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Infrastructure.Data.Configurations;

public sealed class WeightCategoryConfiguration : IEntityTypeConfiguration<WeightCategory>
{
    public void Configure(EntityTypeBuilder<WeightCategory> builder)
    {
        builder.ToTable("WeightCategories", table =>
            table.HasCheckConstraint("CK_WeightCategories_MinWeightKg_NonNegative", "[MinWeightKg] >= 0"));

        builder.Property(c => c.Name).IsRequired().HasMaxLength(CategoryRules.MaxNameLength);
        builder.Property(c => c.IconKey).IsRequired().HasMaxLength(30);
        builder.Property(c => c.MinWeightKg).HasPrecision(10, 2);

        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.MinWeightKg).IsUnique();

        builder.HasData(
            new WeightCategory { Id = 1, Name = "Light", IconKey = "feather", MinWeightKg = 0m },
            new WeightCategory { Id = 2, Name = "Medium", IconKey = "car", MinWeightKg = 500m },
            new WeightCategory { Id = 3, Name = "Heavy", IconKey = "truck", MinWeightKg = 2500m });
    }
}