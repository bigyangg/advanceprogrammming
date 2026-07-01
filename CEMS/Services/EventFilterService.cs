using CEMS.Models;
using CEMS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Services;

public class EventFilterService(IUnitOfWork unitOfWork) : IEventFilterService
{
    public async Task<IEnumerable<Event>> FilterAsync(DateTime? date, int? venueId, int? activityId)
    {
        // Compose the query conditionally so EF emits one SQL query with only needed WHERE clauses,
        // and ORDER BY is also pushed to SQL rather than sorting in memory.
        IQueryable<Event> query = unitOfWork.Events.Query();

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(e => e.EventDate.Date == targetDate);
        }

        if (venueId.HasValue)
        {
            var targetVenueId = venueId.Value;
            query = query.Where(e => e.Venues.Any(v => v.VenueId == targetVenueId));
        }

        if (activityId.HasValue)
        {
            var targetActivityId = activityId.Value;
            query = query.Where(e => e.Activities.Any(a => a.ActivityId == targetActivityId));
        }

        return await query.OrderBy(e => e.EventDate).ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetUpcomingAsync()
    {
        var now = DateTime.UtcNow;
        return await unitOfWork.Events.Query()
            .Where(e => e.EventDate >= now)
            .OrderBy(e => e.EventDate)
            .ToListAsync();
    }
}
