using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models.ViewModels;

public class LocationViewModel
{
    [Required]
    [StringLength(260)]
    [Display(Name = "Location address")]
    public string LocationAddress { get; set; } = string.Empty;

    public string RoleLabel { get; set; } = string.Empty;
}
