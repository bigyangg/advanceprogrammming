using CEMS.Exceptions;
using CEMS.Filters;
using CEMS.Infrastructure;
using CEMS.Repositories;
using CEMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Controllers;

[RequireRole("Participant")]
public class RegistrationsController(IRegistrationService registrationService, IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> MyRegistrations()
    {
        var participantId = HttpContext.Session.CurrentUserId();
        if (participantId is null) return RedirectToAction("Login", "Account");

        var registrations = await unitOfWork.Registrations.Query()
            .Include(r => r.Event)
            .Where(r => r.ParticipantId == participantId.Value)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();

        return View(registrations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int eventId)
    {
        var participantId = HttpContext.Session.CurrentUserId();
        if (participantId is null) return RedirectToAction("Login", "Account");

        try
        {
            await registrationService.RegisterAsync(participantId.Value, eventId);
            var registeredEvent = await unitOfWork.Events.GetByIdAsync(eventId);
            TempData["FlashSuccess"] = $"You're registered for {registeredEvent?.Name ?? "this event"}.";
            return RedirectToAction(nameof(MyRegistrations));
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
        try
        {
            await registrationService.CancelAsync(registrationId);
            TempData["FlashSuccess"] = "Registration cancelled.";
        }
        catch (CEMSException ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction(nameof(MyRegistrations));
    }
}
