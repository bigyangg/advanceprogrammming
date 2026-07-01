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

        await registrationService.RegisterAsync(participantId.Value, eventId);
        return RedirectToAction(nameof(MyRegistrations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int registrationId)
    {
        await registrationService.CancelAsync(registrationId);
        return RedirectToAction(nameof(MyRegistrations));
    }
}
