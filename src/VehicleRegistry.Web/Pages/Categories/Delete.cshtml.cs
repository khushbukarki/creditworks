using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRegistry.Core.Rules;
using VehicleRegistry.Core.Services;
using VehicleRegistry.Web.Models;

namespace VehicleRegistry.Web.Pages.Categories;

public class DeleteModel(ICategoryService categoryService) : PageModel
{
    public CategoryRange? Category { get; private set; }
    public CategoryRange? AbsorbedBy { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken) =>
        await LoadAsync(id, cancellationToken) ? Page() : NotFound();

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        var result = await categoryService.DeleteAsync(id, cancellationToken);
        switch (result.Status)
        {
            case OperationStatus.Success:
                TempData["StatusMessage"] = "Category deleted. Vehicle categories have been updated.";
                return RedirectToPage("Index");
            case OperationStatus.NotFound:
                return NotFound();
            default:
                ModelState.AddValidationErrors(result.Errors, prefix: string.Empty);
                return await LoadAsync(id, cancellationToken) ? Page() : NotFound();
        }
    }

    private async Task<bool> LoadAsync(int id, CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetCategoriesAsync(cancellationToken);
        var index = categories.ToList().FindIndex(c => c.Id == id);
        if (index < 0)
            return false;

        Category = categories[index];
        var absorbingIndex = CategoryRules.AbsorbingIndex(index, categories.Count);
        AbsorbedBy = absorbingIndex is int i ? categories[i] : null;
        return true;
    }
}