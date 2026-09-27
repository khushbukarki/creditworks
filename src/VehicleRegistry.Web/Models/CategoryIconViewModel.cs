namespace VehicleRegistry.Web.Models;

public class CategoryIconViewModel
{
    public string IconKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    public CategoryIconViewModel() { }

    public CategoryIconViewModel(string iconKey, string label)
    {
        IconKey = iconKey;
        Label = label;
    }
}