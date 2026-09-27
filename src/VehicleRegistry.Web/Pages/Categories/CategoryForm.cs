using System.ComponentModel.DataAnnotations;
using VehicleRegistry.Core.Rules;
using VehicleRegistry.Core.Services;

namespace VehicleRegistry.Web.Pages.Categories;

public sealed class CategoryForm
{
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Display(Name = "Icon")]
    public string? IconKey { get; set; }

    [Display(Name = "Starting weight")]
    public decimal? MinWeightKg { get; set; }

    public CategoryInput ToInput() => new(Name, IconKey, MinWeightKg);

    public static CategoryForm From(CategoryRange range) =>
        new() { Name = range.Name, IconKey = range.IconKey, MinWeightKg = range.MinWeightKg };
}