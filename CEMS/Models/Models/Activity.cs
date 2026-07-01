using System.ComponentModel.DataAnnotations;

namespace CEMS.Models
{
    public class Activity
    {
        public int ActivityId { get; set; }
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(60)]
        public string Type { get; set; } = string.Empty; // workshop, talk, game, ...
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ImagePath { get; set; } = "/images/activities/img1_talk.png";

        // Many-to-many navigation
        public List<Event> Events { get; set; } = new();

        public string GetDetails() => $"{Name} ({Type}) — {Description}";
    }
}
