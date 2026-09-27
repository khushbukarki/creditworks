using VehicleRegistry.Core.Services;

namespace VehicleRegistry.Core.Rules;

public static class VehicleRules
{
    public const int MaxOwnerNameLength = 100;
    public const int EarliestYear = 1886; // first petrol car

    public static IReadOnlyList<ValidationError> Validate(VehicleInput input, int currentYear)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(input.OwnerName))
            errors.Add(new(nameof(VehicleInput.OwnerName), "Owner's name is required."));
        else if (input.OwnerName.Trim().Length > MaxOwnerNameLength)
            errors.Add(new(nameof(VehicleInput.OwnerName), $"Owner's name must be {MaxOwnerNameLength} characters or fewer."));

        if (input.ManufacturerId is null)
            errors.Add(new(nameof(VehicleInput.ManufacturerId), "Manufacturer is required."));

        var latestYear = currentYear + 1; // next year's models are sold early
        if (input.YearOfManufacture is null)
            errors.Add(new(nameof(VehicleInput.YearOfManufacture), "Year of manufacture is required."));
        else if (input.YearOfManufacture < EarliestYear || input.YearOfManufacture > latestYear)
            errors.Add(new(nameof(VehicleInput.YearOfManufacture),
                $"Year of manufacture must be between {EarliestYear} and {latestYear}."));

        if (input.WeightKg is null)
            errors.Add(new(nameof(VehicleInput.WeightKg), "Weight is required."));
        else if (input.WeightKg <= 0)
            errors.Add(new(nameof(VehicleInput.WeightKg), "Weight must be greater than 0 kg."));
        else if (input.WeightKg > WeightRules.MaxWeightKg)
            errors.Add(new(nameof(VehicleInput.WeightKg), $"Weight must be no more than {WeightRules.MaxWeightKg:#,0.00} kg."));
        else if (!WeightRules.HasAtMostTwoDecimalPlaces(input.WeightKg.Value))
            errors.Add(new(nameof(VehicleInput.WeightKg), "Weight can have at most two decimal places, for example 1850.75."));

        return errors;
    }
}