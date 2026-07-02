using System.ComponentModel.DataAnnotations;

namespace CEMS.ViewModels;

public class SignupViewModel
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;
}
