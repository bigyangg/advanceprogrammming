using CEMS.Filters;
using CEMS.Models;
using CEMS.Repositories;
using CEMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers;

[RequireRole("Administrator")]
public class ActivitiesController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index() => View(await unitOfWork.Activities.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var activity = await unitOfWork.Activities.GetByIdAsync(id);
        return activity is null ? NotFound() : View(activity);
    }

    public IActionResult Create() => View(new ActivityFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActivityFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        await unitOfWork.Activities.AddAsync(new Activity
        {
            Name = model.Name,
            Type = model.Type,
            Description = model.Description
        });
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var activity = await unitOfWork.Activities.GetByIdAsync(id);
        if (activity is null) return NotFound();
        return View(new ActivityFormViewModel
        {
            ActivityId = activity.ActivityId,
            Name = activity.Name,
            Type = activity.Type,
            Description = activity.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActivityFormViewModel model)
    {
        if (id != model.ActivityId) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var activity = await unitOfWork.Activities.GetByIdAsync(id);
        if (activity is null) return NotFound();
        activity.Name = model.Name;
        activity.Type = model.Type;
        activity.Description = model.Description;
        unitOfWork.Activities.Update(activity);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var activity = await unitOfWork.Activities.GetByIdAsync(id);
        return activity is null ? NotFound() : View(activity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var activity = await unitOfWork.Activities.GetByIdAsync(id);
        if (activity is null) return NotFound();
        unitOfWork.Activities.Remove(activity);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
