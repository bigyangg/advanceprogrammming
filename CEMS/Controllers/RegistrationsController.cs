using CEMS.Exceptions;
using CEMS.Filters;
using CEMS.Infrastructure;
using CEMS.Models;
using CEMS.Repositories;
using CEMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Controllers;

[RequireRole("Participant")]
public class RegistrationsController(IRegistrationService registrationService, IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Profile()
    {
        var participantId = HttpContext.Session.CurrentUserId();
        if (participantId is null) return RedirectToAction("Login", "Account");

        var participant = await unitOfWork.Participants.GetByIdAsync(participantId.Value);
        var registrations = await unitOfWork.Registrations.Query()
            .Include(r => r.Event)
            .Where(r => r.ParticipantId == participantId.Value)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();

        ViewBag.Participant = participant;
        return View(registrations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int eventId, int seats = 1)
    {
        var participantId = HttpContext.Session.CurrentUserId();
        if (participantId is null) return RedirectToAction("Login", "Account");

        try
        {
            await registrationService.RegisterAsync(participantId.Value, eventId, seats);
            var registeredEvent = await unitOfWork.Events.GetByIdAsync(eventId);
            TempData["FlashSuccess"] = $"You've booked {seats} seat(s) for {registeredEvent?.Name ?? "this event"}.";
            return RedirectToAction(nameof(Profile));
        }
        catch (CEMSException ex)
        {
            TempData["FlashError"] = ex.Message;
            return RedirectToAction("Details", "Events", new { id = eventId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int registrationId)
    {
        var participantId = HttpContext.Session.CurrentUserId();
        if (participantId is null) return RedirectToAction("Login", "Account");

        var registration = await unitOfWork.Registrations.GetByIdAsync(registrationId);
        if (registration is null || registration.ParticipantId != participantId.Value)
        {
            TempData["FlashError"] = "Registration not found.";
            return RedirectToAction(nameof(Profile));
        }

        try
        {
            await registrationService.CancelAsync(registrationId);
            TempData["FlashSuccess"] = "Registration cancelled.";
        }
        catch (CEMSException ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> Ticket(int registrationId)
    {
        var participantId = HttpContext.Session.CurrentUserId();
        if (participantId is null) return RedirectToAction("Login", "Account");

        var registration = await unitOfWork.Registrations.Query()
            .Include(r => r.Event)
            .ThenInclude(e => e!.Venues)
            .Include(r => r.Participant)
            .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

        if (registration is null || registration.ParticipantId != participantId.Value) return NotFound();
        if (registration.Status == RegistrationStatus.Cancelled)
        {
            TempData["FlashError"] = "Cancelled registrations don't have a ticket.";
            return RedirectToAction(nameof(Profile));
        }

        return View(registration);
    }
}
