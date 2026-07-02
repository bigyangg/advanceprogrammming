using System.ComponentModel.DataAnnotations;

namespace CEMS.Models
{
    /// <summary>
    /// Junction entity for the many-to-many between Participant and Event,
    /// carrying its own data (date, status) — which is exactly why it's a
    /// full entity rather than an implicit EF Core join table.
    /// </summary>
    public class Registration
    {
        public int RegistrationId { get; set; }
        [Required]
        public DateTime RegistrationDate { get; set; }
        [Required]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
        [Range(1, int.MaxValue, ErrorMessage = "Seats must be at least 1.")]
        public int Seats { get; set; } = 1;

        public int ParticipantId { get; set; }
        public Participant? Participant { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        public void Confirm() => Status = RegistrationStatus.Confirmed;

        public void Cancel() => Status = RegistrationStatus.Cancelled;
    }
}
