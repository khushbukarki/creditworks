using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRegistry.Core.Rules;
using VehicleRegistry.Core.Services;

namespace VehicleRegistry.Web.Pages.Categories;

public class IndexModel(ICategoryService categoryService) : PageModel
{
    public IReadOnlyList<CategoryRange> Categories { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Categories = await categoryService.GetCategoriesAsync(cancellationToken);
}
