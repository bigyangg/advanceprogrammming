using CEMS.Models.Interfaces;

namespace CEMS.Models
{
    /// <summary>
    /// A user who can register for events. Inherits shared identity fields
    /// from Person, implements IRegistrable to fulfil the registration contract.
    /// </summary>
    public class Participant : Person, IRegistrable
    {
        // Navigation property — EF Core will populate this from the Registrations table.
        public List<Registration> Registrations { get; set; } = new();

        public void Register(Event e, int seats = 1)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));
            if (seats < 1) throw new ArgumentOutOfRangeException(nameof(seats), "Seats must be at least 1.");

            if (Registrations.Any(r => r.EventId == e.EventId && r.Status != RegistrationStatus.Cancelled))
                throw new InvalidOperationException("Already registered for this event.");

            Registrations.Add(new Registration
            {
                ParticipantId = PersonId,
                EventId = e.EventId,
                RegistrationDate = DateTime.UtcNow,
                Status = RegistrationStatus.Pending,
                Seats = seats
            });
        }

        public void CancelRegistration(Event e)
        {
            var reg = Registrations.FirstOrDefault(r => r.EventId == e.EventId);
            if (reg is null) throw new InvalidOperationException("No registration found for this event.");
            reg.Cancel();
        }

        public List<Registration> ViewRegistrations() => Registrations;

        public override string GetRole() => "Participant";
    }
}
