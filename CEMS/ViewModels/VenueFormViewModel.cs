using System.ComponentModel.DataAnnotations;

namespace CEMS.ViewModels;

public class VenueFormViewModel
{
    public int VenueId { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}
