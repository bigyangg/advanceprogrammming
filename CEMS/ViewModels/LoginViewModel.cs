using System.ComponentModel.DataAnnotations;

namespace CEMS.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Person ID")]
    public int PersonId { get; set; }
}
