namespace CateringWebsite.Models.ViewModels;

public class SystemLogListViewModel
{
    public IReadOnlyList<SystemLogListItemViewModel> Logs { get; set; } = [];
}
