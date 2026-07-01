using System.ComponentModel.DataAnnotations;
using CEMS.Models.Interfaces;

namespace CEMS.Models
{
    /// <summary>
    /// The core entity of the system. Implements INotifiable so it can announce
    /// changes (cancellations, updates) to registered participants.
    /// </summary>
    public class Event : INotifiable
    {
        public int EventId { get; set; }
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public DateTime EventDate { get; set; }
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; } = "/images/events/img1_workshop.png";

        // Many-to-many navigations
        public List<Activity> Activities { get; set; } = new();
        public List<Venue> Venues { get; set; } = new();

        // Composition: Registrations belong to this Event
        public List<Registration> Registrations { get; set; } = new();

        public void AddActivity(Activity a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (!Activities.Contains(a)) Activities.Add(a);
        }

        public void RemoveActivity(Activity a)
        {
            Activities.Remove(a);
        }

        public List<Participant> GetParticipants() =>
            Registrations
                .Where(r => r.Status != RegistrationStatus.Cancelled)
                .Select(r => r.Participant)
                .Where(p => p is not null)
                .ToList()!;

        public bool FilterByVenue(Venue v) => Venues.Any(venue => venue.VenueId == v.VenueId);

        public void SendNotification(string message)
        {
            // Wired up to a real notification/email service later —
            // for now this is the extension point INotifiable promises.
            Console.WriteLine($"[Event: {Name}] {message}");
        }
    }
}
