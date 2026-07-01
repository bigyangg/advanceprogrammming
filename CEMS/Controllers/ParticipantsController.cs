using CEMS.Filters;
using CEMS.Models;
using CEMS.Repositories;
using CEMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers;

[RequireRole("Administrator")]
public class ParticipantsController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index() => View(await unitOfWork.Participants.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var participant = await unitOfWork.Participants.GetByIdAsync(id);
        return participant is null ? NotFound() : View(participant);
    }

    public IActionResult Create() => View(new ParticipantFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParticipantFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        await unitOfWork.Participants.AddAsync(new Participant
        {
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone
        });
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var participant = await unitOfWork.Participants.GetByIdAsync(id);
        if (participant is null) return NotFound();
        return View(new ParticipantFormViewModel
        {
            PersonId = participant.PersonId,
            Name = participant.Name,
            Email = participant.Email,
            Phone = participant.Phone
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ParticipantFormViewModel model)
    {
        if (id != model.PersonId) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var participant = await unitOfWork.Participants.GetByIdAsync(id);
        if (participant is null) return NotFound();
        participant.Name = model.Name;
        participant.Email = model.Email;
        participant.Phone = model.Phone;
        unitOfWork.Participants.Update(participant);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var participant = await unitOfWork.Participants.GetByIdAsync(id);
        return participant is null ? NotFound() : View(participant);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var participant = await unitOfWork.Participants.GetByIdAsync(id);
        if (participant is null) return NotFound();
        unitOfWork.Participants.Remove(participant);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
