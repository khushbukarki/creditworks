using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRegistry.Core.Services;
using VehicleRegistry.Web.Models;

namespace VehicleRegistry.Web.Pages.Vehicles;

public class CreateModel(IVehicleService vehicleService) : PageModel
{
    [BindProperty]
    public VehicleForm Input { get; set; } = new();

    public IReadOnlyList<ManufacturerOption> Manufacturers { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Manufacturers = await vehicleService.GetManufacturersAsync(cancellationToken);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        // ModelState only holds binding errors (e.g. "abc" as weight).
        // All business validation happens on the server in the service.
        if (ModelState.IsValid)
        {
            var result = await vehicleService.AddVehicleAsync(Input.ToInput(), cancellationToken);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = $"Vehicle added for {Input.OwnerName!.Trim()}.";
                return RedirectToPage("/Index");
            }

            ModelState.AddValidationErrors(result.Errors, nameof(Input));
        }

        Manufacturers = await vehicleService.GetManufacturersAsync(cancellationToken);
        return Page();
    }

    public sealed class VehicleForm
    {
        [Display(Name = "Owner's name")]
        public string? OwnerName { get; set; }

        [Display(Name = "Manufacturer")]
        public int? ManufacturerId { get; set; }

        [Display(Name = "Year of manufacture")]
        public int? YearOfManufacture { get; set; }

        [Display(Name = "Weight")]
        public decimal? WeightKg { get; set; }

        public VehicleInput ToInput() => new(OwnerName, ManufacturerId, YearOfManufacture, WeightKg);
    }
}