using CEMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CEMS.ViewModels;

public class EventIndexViewModel
{
    public DateTime? Date { get; set; }
    public int? VenueId { get; set; }
    public int? ActivityId { get; set; }
    public IEnumerable<Event> Events { get; set; } = [];
    public IEnumerable<SelectListItem> Venues { get; set; } = [];
    public IEnumerable<SelectListItem> Activities { get; set; } = [];
}
