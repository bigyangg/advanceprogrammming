using CEMS.Data;
using CEMS.Models;

namespace CEMS.Repositories;

public class UnitOfWork(CEMSDbContext context) : IUnitOfWork
{
    private IRepository<Event>? _events;
    private IRepository<Venue>? _venues;
    private IRepository<Activity>? _activities;
    private IRepository<Participant>? _participants;
    private IRepository<Administrator>? _administrators;
    private IRepository<Registration>? _registrations;

    public IRepository<Event> Events => _events ??= new Repository<Event>(context);
    public IRepository<Venue> Venues => _venues ??= new Repository<Venue>(context);
    public IRepository<Activity> Activities => _activities ??= new Repository<Activity>(context);
    public IRepository<Participant> Participants => _participants ??= new Repository<Participant>(context);
    public IRepository<Administrator> Administrators => _administrators ??= new Repository<Administrator>(context);
    public IRepository<Registration> Registrations => _registrations ??= new Repository<Registration>(context);

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}
