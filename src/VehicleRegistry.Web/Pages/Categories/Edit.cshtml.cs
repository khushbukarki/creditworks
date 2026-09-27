using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRegistry.Core.Services;
using VehicleRegistry.Web.Models;

namespace VehicleRegistry.Web.Pages.Categories;

public class EditModel(ICategoryService categoryService) : PageModel
{
    [BindProperty]
    public CategoryForm Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var category = await categoryService.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return NotFound();

        Input = CategoryForm.From(category);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await categoryService.UpdateAsync(id, Input.ToInput(), cancellationToken);
        switch (result.Status)
        {
            case OperationStatus.Success:
                TempData["StatusMessage"] = $"Category '{Input.Name!.Trim()}' saved. Vehicle categories have been updated.";
                return RedirectToPage("Index");
            case OperationStatus.NotFound:
                return NotFound();
            default:
                ModelState.AddValidationErrors(result.Errors, nameof(Input));
                return Page();
        }
    }
}