using System.ComponentModel.DataAnnotations;

namespace CEMS.ViewModels;

public class ActivityFormViewModel
{
    public int ActivityId { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Type { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}
