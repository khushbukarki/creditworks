using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRegistry.Core.Services;

namespace VehicleRegistry.Web.Pages;

public class IndexModel(IVehicleService vehicleService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public VehicleSortField Sort { get; set; } = VehicleSortField.OwnerName;

    [BindProperty(SupportsGet = true)]
    public SortDirection Dir { get; set; } = SortDirection.Ascending;

    public IReadOnlyList<VehicleListItem> Vehicles { get; private set; } = [];

    public static IReadOnlyList<(VehicleSortField Field, string Label)> Columns { get; } =
    [
        (VehicleSortField.OwnerName, "Owner's name"),
        (VehicleSortField.Manufacturer, "Manufacturer"),
        (VehicleSortField.Year, "Year"),
        (VehicleSortField.Weight, "Weight"),
    ];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(Sort)) Sort = VehicleSortField.OwnerName;
        if (!Enum.IsDefined(Dir)) Dir = SortDirection.Ascending;

        Vehicles = await vehicleService.GetVehiclesAsync(new VehicleSort(Sort, Dir), cancellationToken);
    }

    public string SortLabel => Columns.First(c => c.Field == Sort).Label.ToLowerInvariant();

    public SortDirection NextDirection(VehicleSortField field) =>
        field == Sort && Dir == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;

    public string? AriaSort(VehicleSortField field) =>
        field != Sort ? null : Dir == SortDirection.Ascending ? "ascending" : "descending";

    public string SortIndicator(VehicleSortField field) =>
        field != Sort ? "" : Dir == SortDirection.Ascending ? "▲" : "▼";
}