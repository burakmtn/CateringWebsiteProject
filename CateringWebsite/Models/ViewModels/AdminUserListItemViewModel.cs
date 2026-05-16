namespace CateringWebsite.Models.ViewModels;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string LocationAddress { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
