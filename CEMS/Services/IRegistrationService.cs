using CEMS.Models;

namespace CEMS.Services;

public interface IRegistrationService
{
    Task<Registration> RegisterAsync(int participantId, int eventId);
    Task<Registration> CancelAsync(int registrationId);
    Task<Registration> ConfirmAsync(int registrationId);
    Task<IEnumerable<Registration>> GetRegistrationsForParticipantAsync(int participantId);
}
