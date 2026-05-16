namespace CateringWebsite.Models.ViewModels;

public class SystemLogListViewModel
{
    public PagedListViewModel<SystemLogListItemViewModel> Logs { get; set; } = new();
}
