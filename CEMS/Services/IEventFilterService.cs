using CEMS.Models;

namespace CEMS.Services;

public interface IEventFilterService
{
    Task<IEnumerable<Event>> FilterAsync(DateTime? date, int? venueId, int? activityId);
    Task<IEnumerable<Event>> GetUpcomingAsync();
}
