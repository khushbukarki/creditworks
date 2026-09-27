using System.Globalization;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Web.Models;

public static class DisplayFormat
{
    public static string Kg(decimal value) =>
        value.ToString("#,0.##", CultureInfo.InvariantCulture) + " kg";

    public static string Range(CategoryRange range) =>
        range.MaxWeightKgExclusive is decimal max
            ? $"From {Kg(range.MinWeightKg)} up to, but not including, {Kg(max)}"
            : $"{Kg(range.MinWeightKg)} and above";
}
