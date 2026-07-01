using CEMS.Exceptions;
using CEMS.Models;
using CEMS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Services;

public class RegistrationService(IUnitOfWork unitOfWork) : IRegistrationService
{
    public async Task<Registration> RegisterAsync(int participantId, int eventId)
    {
        var participant = await unitOfWork.Participants.GetByIdAsync(participantId);
        if (participant is null)
        {
            var adminWithSameId = await unitOfWork.Administrators.GetByIdAsync(participantId);
            if (adminWithSameId is not null)
            {
                throw new InvalidRegistrationStateException("Administrators cannot register for events as participants.");
            }

            throw new EntityNotFoundException($"Participant with ID {participantId} was not found.");
        }

        var targetEvent = await unitOfWork.Events.GetByIdAsync(eventId)
            ?? throw new EntityNotFoundException($"Event with ID {eventId} was not found.");

        var hasActiveRegistration = await unitOfWork.Registrations.Query().AnyAsync(r =>
            r.ParticipantId == participantId &&
            r.EventId == eventId &&
            r.Status != RegistrationStatus.Cancelled);

        if (hasActiveRegistration)
        {
            throw new DuplicateRegistrationException("Participant already has an active registration for this event.");
        }

        // Keep capacity check in SQL (COUNT(*)) instead of materializing all rows.
        var activeRegistrationsCount = await unitOfWork.Registrations.Query().CountAsync(r =>
            r.EventId == eventId &&
            r.Status != RegistrationStatus.Cancelled);

        if (activeRegistrationsCount >= targetEvent.Capacity)
        {
            throw new EventCapacityExceededException("This event is already at full capacity.");
        }

        // Delegate registration creation to the domain model to keep behavior centralized.
        participant.Register(targetEvent);
        var registration = participant.Registrations
            .OrderByDescending(r => r.RegistrationDate)
            .First();

        targetEvent.SendNotification($"New registration created for participant {participant.Name}.");
        await unitOfWork.SaveChangesAsync();
        return registration;
    }

    public async Task<Registration> CancelAsync(int registrationId)
    {
        var registration = await unitOfWork.Registrations.GetByIdAsync(registrationId)
            ?? throw new EntityNotFoundException($"Registration with ID {registrationId} was not found.");

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            throw new InvalidRegistrationStateException("This registration is already cancelled.");
        }

        registration.Cancel();
        unitOfWork.Registrations.Update(registration);
        var targetEvent = await unitOfWork.Events.GetByIdAsync(registration.EventId);
        targetEvent?.SendNotification($"Registration {registration.RegistrationId} was cancelled.");
        await unitOfWork.SaveChangesAsync();
        return registration;
    }

    public async Task<Registration> ConfirmAsync(int registrationId)
    {
        var registration = await unitOfWork.Registrations.GetByIdAsync(registrationId)
            ?? throw new EntityNotFoundException($"Registration with ID {registrationId} was not found.");

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            throw new InvalidRegistrationStateException("Cancelled registrations cannot be confirmed.");
        }

        if (registration.Status == RegistrationStatus.Confirmed)
        {
            throw new InvalidRegistrationStateException("This registration is already confirmed.");
        }

        registration.Confirm();
        unitOfWork.Registrations.Update(registration);
        var targetEvent = await unitOfWork.Events.GetByIdAsync(registration.EventId);
        targetEvent?.SendNotification($"Registration {registration.RegistrationId} was confirmed.");
        await unitOfWork.SaveChangesAsync();
        return registration;
    }

    public async Task<IEnumerable<Registration>> GetRegistrationsForParticipantAsync(int participantId)
    {
        var participant = await unitOfWork.Participants.GetByIdAsync(participantId)
            ?? throw new EntityNotFoundException($"Participant with ID {participantId} was not found.");

        return await unitOfWork.Registrations.FindAsync(r => r.ParticipantId == participant.PersonId);
    }
}
