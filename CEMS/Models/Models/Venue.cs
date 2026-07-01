using System.ComponentModel.DataAnnotations;

namespace CEMS.Models
{
    public class Venue
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

        [StringLength(500)]
        public string? ImagePath { get; set; }

        // Many-to-many navigation
        public List<Event> Events { get; set; } = new();

        public bool CheckAvailability(DateTime date) =>
            !Events.Any(e => e.EventDate.Date == date.Date);
    }
}
