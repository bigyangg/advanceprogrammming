using CEMS.Filters;
using CEMS.Models;
using CEMS.Repositories;
using CEMS.Services;
using CEMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Controllers;

public class EventsController(IUnitOfWork unitOfWork, IEventFilterService eventFilterService) : Controller
{
    public async Task<IActionResult> Index(DateTime? date, int? venueId, int? activityId)
    {
        var events = (date.HasValue || venueId.HasValue || activityId.HasValue)
            ? await eventFilterService.FilterAsync(date, venueId, activityId)
            : await eventFilterService.GetUpcomingAsync();

        var vm = new EventIndexViewModel
        {
            Date = date,
            VenueId = venueId,
            ActivityId = activityId,
            Events = events,
            Venues = (await unitOfWork.Venues.GetAllAsync())
                .Select(v => new SelectListItem(v.Name, v.VenueId.ToString())),
            Activities = (await unitOfWork.Activities.GetAllAsync())
                .Select(a => new SelectListItem($"{a.Name} ({a.Type})", a.ActivityId.ToString()))
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var evt = await unitOfWork.Events.Query()
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .Include(e => e.Registrations)
            .ThenInclude(r => r.Participant)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (evt is null) return NotFound();
        return View(evt);
    }

    [RequireRole("Administrator")]
    public async Task<IActionResult> AdminIndex()
    {
        var events = await unitOfWork.Events.Query()
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .OrderBy(e => e.EventDate)
            .ToListAsync();
        return View(events);
    }

    [RequireRole("Administrator")]
    public async Task<IActionResult> Create()
    {
        await LoadLookupDataAsync();
        return View(new EventFormViewModel { EventDate = DateTime.UtcNow.AddDays(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireRole("Administrator")]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupDataAsync(model.SelectedVenueIds, model.SelectedActivityIds);
            return View(model);
        }

        var evt = new Event
        {
            Name = model.Name,
            EventDate = model.EventDate,
            Description = model.Description,
            Capacity = model.Capacity
        };

        var venues = (await unitOfWork.Venues.FindAsync(v => model.SelectedVenueIds.Contains(v.VenueId))).ToList();
        var activities = (await unitOfWork.Activities.FindAsync(a => model.SelectedActivityIds.Contains(a.ActivityId))).ToList();
        evt.Venues = venues;
        evt.Activities = activities;

        await unitOfWork.Events.AddAsync(evt);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(AdminIndex));
    }

    [RequireRole("Administrator")]
    public async Task<IActionResult> Edit(int id)
    {
        var evt = await unitOfWork.Events.Query()
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (evt is null) return NotFound();

        var model = new EventFormViewModel
        {
            EventId = evt.EventId,
            Name = evt.Name,
            EventDate = evt.EventDate,
            Description = evt.Description,
            Capacity = evt.Capacity,
            SelectedVenueIds = evt.Venues.Select(v => v.VenueId).ToList(),
            SelectedActivityIds = evt.Activities.Select(a => a.ActivityId).ToList()
        };
        await LoadLookupDataAsync(model.SelectedVenueIds, model.SelectedActivityIds);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireRole("Administrator")]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (id != model.EventId) return NotFound();
        if (!ModelState.IsValid)
        {
            await LoadLookupDataAsync(model.SelectedVenueIds, model.SelectedActivityIds);
            return View(model);
        }

        var evt = await unitOfWork.Events.Query()
            .Include(e => e.Venues)
            .Include(e => e.Activities)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (evt is null) return NotFound();

        evt.Name = model.Name;
        evt.EventDate = model.EventDate;
        evt.Description = model.Description;
        evt.Capacity = model.Capacity;

        var selectedVenues = (await unitOfWork.Venues.FindAsync(v => model.SelectedVenueIds.Contains(v.VenueId))).ToList();
        var selectedActivities = (await unitOfWork.Activities.FindAsync(a => model.SelectedActivityIds.Contains(a.ActivityId))).ToList();
        evt.Venues.Clear();
        evt.Activities.Clear();
        foreach (var venue in selectedVenues) evt.Venues.Add(venue);
        foreach (var activity in selectedActivities) evt.Activities.Add(activity);

        unitOfWork.Events.Update(evt);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(AdminIndex));
    }

    [RequireRole("Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var evt = await unitOfWork.Events.GetByIdAsync(id);
        if (evt is null) return NotFound();
        return View(evt);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequireRole("Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var evt = await unitOfWork.Events.GetByIdAsync(id);
        if (evt is null) return NotFound();
        unitOfWork.Events.Remove(evt);
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(AdminIndex));
    }

    private async Task LoadLookupDataAsync(IEnumerable<int>? selectedVenueIds = null, IEnumerable<int>? selectedActivityIds = null)
    {
        ViewBag.Venues = (await unitOfWork.Venues.GetAllAsync())
            .Select(v => new SelectListItem
            {
                Value = v.VenueId.ToString(),
                Text = $"{v.Name} ({v.Capacity})",
                Selected = selectedVenueIds?.Contains(v.VenueId) ?? false
            })
            .ToList();

        ViewBag.Activities = (await unitOfWork.Activities.GetAllAsync())
            .Select(a => new SelectListItem
            {
                Value = a.ActivityId.ToString(),
                Text = $"{a.Name} ({a.Type})",
                Selected = selectedActivityIds?.Contains(a.ActivityId) ?? false
            })
            .ToList();
    }
}
