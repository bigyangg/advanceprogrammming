using System.ComponentModel.DataAnnotations;

namespace CEMS.ViewModels;

public class EventFormViewModel
{
    public int EventId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime EventDate { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    public List<int> SelectedVenueIds { get; set; } = new();
    public List<int> SelectedActivityIds { get; set; } = new();
}
