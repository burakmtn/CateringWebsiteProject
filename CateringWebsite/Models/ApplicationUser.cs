using Microsoft.AspNetCore.Identity;

namespace CateringWebsite.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public string? LocationAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
