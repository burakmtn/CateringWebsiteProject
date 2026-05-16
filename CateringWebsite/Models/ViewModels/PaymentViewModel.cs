using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models.ViewModels;

public class PaymentViewModel
{
    public CartViewModel Cart { get; set; } = new();

    [Required]
    [StringLength(100)]
    [Display(Name = "Card holder")]
    public string CardHolder { get; set; } = string.Empty;

    [Required]
    [CreditCard]
    [Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use MM/YY format.")]
    [Display(Name = "Expiry")]
    public string Expiry { get; set; } = string.Empty;

    [Required]
    [StringLength(4, MinimumLength = 3)]
    public string Cvv { get; set; } = string.Empty;
}
