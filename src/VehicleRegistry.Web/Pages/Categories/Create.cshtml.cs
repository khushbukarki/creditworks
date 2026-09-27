using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRegistry.Core.Services;
using VehicleRegistry.Web.Models;

namespace VehicleRegistry.Web.Pages.Categories;

public class CreateModel(ICategoryService categoryService) : PageModel
{
    [BindProperty]
    public CategoryForm Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await categoryService.CreateAsync(Input.ToInput(), cancellationToken);
        if (result.Succeeded)
        {
            TempData["StatusMessage"] = $"Category '{Input.Name!.Trim()}' added. Vehicle categories have been updated.";
            return RedirectToPage("Index");
        }

        ModelState.AddValidationErrors(result.Errors, nameof(Input));
        return Page();
    }
}