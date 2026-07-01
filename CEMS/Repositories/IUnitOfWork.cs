using CEMS.Models;

namespace CEMS.Repositories;

/// <summary>
/// Unit of Work coordinates repositories behind one commit boundary.
/// This keeps controllers/services decoupled from EF Core internals, makes data access
/// easier to mock in tests, and ensures multi-entity business operations commit atomically.
/// </summary>
public interface IUnitOfWork
{
    IRepository<Event> Events { get; }
    IRepository<Venue> Venues { get; }
    IRepository<Activity> Activities { get; }
    IRepository<Participant> Participants { get; }
    IRepository<Administrator> Administrators { get; }
    IRepository<Registration> Registrations { get; }
    Task<int> SaveChangesAsync();
}
