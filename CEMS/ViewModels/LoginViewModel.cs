using System.ComponentModel.DataAnnotations;

namespace CEMS.ViewModels;

public class LoginViewModel
{
    [Required]
    public int PersonId { get; set; }
}
