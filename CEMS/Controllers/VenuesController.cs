using CEMS.Filters;
using CEMS.Models;
using CEMS.Repositories;
using CEMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers;

[RequireRole("Administrator")]
public class VenuesController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index() => View(await unitOfWork.Venues.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var venue = await unitOfWork.Venues.GetByIdAsync(id);
        return venue is null ? NotFound() : View(venue);
    }

    public IActionResult Create() => View(new VenueFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VenueFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await unitOfWork.Venues.AddAsync(new Venue
        {
            Name = model.Name,
            Address = model.Address,
            Capacity = model.Capacity
        });
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var venue = await unitOfWork.Venues.GetByIdAsync(id);
        if (venue is null) return NotFound();
        return View(new VenueFormViewModel
        {
            VenueId = venue.VenueId,
            Name = venue.Name,
            Address = venue.Address,
            Capacity = venue.Capacity
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VenueFormViewModel model)
    {
        if (id != model.VenueId) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var venue = await unitOfWork.Venues.GetByIdAsync(id);
        if (venue is null) return NotFound();
        venue.Name = model.Name;
        venue.Address = model.Address;
        venue.Capacity = model.Capacity;
        unitOfWork.Venues.Update(venue);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var venue = await unitOfWork.Venues.GetByIdAsync(id);
        return venue is null ? NotFound() : View(venue);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venue = await unitOfWork.Venues.GetByIdAsync(id);
        if (venue is null) return NotFound();
        unitOfWork.Venues.Remove(venue);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
