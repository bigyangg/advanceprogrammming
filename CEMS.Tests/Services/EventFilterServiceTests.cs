using CEMS.Data;
using CEMS.Models;
using CEMS.Repositories;
using CEMS.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Tests.Services;

public class EventFilterServiceTests
{
    [Fact]
    public async Task FilterAsync_ByDateVenueAndActivity_ReturnsMatchingEvents()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CEMSDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new CEMSDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var venue = new Venue { VenueId = 900, Name = "V900", Address = "Addr", Capacity = 100 };
        var activity = new Activity { ActivityId = 900, Name = "A900", Type = "Workshop", Description = "desc" };
        var targetDate = new DateTime(2099, 1, 10, 9, 0, 0, DateTimeKind.Utc);
        var match = new Event
        {
            EventId = 900,
            Name = "Match",
            EventDate = targetDate,
            Description = "match",
            Capacity = 50,
            Venues = [venue],
            Activities = [activity]
        };
        var nonMatch = new Event
        {
            EventId = 901,
            Name = "NonMatch",
            EventDate = targetDate.AddDays(1),
            Description = "other",
            Capacity = 20
        };

        await context.Venues.AddAsync(venue);
        await context.Activities.AddAsync(activity);
        await context.Events.AddRangeAsync(match, nonMatch);
        await context.SaveChangesAsync();

        var service = new EventFilterService(new UnitOfWork(context));
        var result = (await service.FilterAsync(targetDate, venue.VenueId, activity.ActivityId)).ToList();

        Assert.Single(result);
        Assert.Equal(match.EventId, result[0].EventId);
        Assert.NotEmpty(result[0].Venues);
        Assert.NotEmpty(result[0].Activities);
    }

    [Fact]
    public async Task GetUpcomingAsync_ReturnsEventsWithVenuesAndActivitiesIncluded()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CEMSDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new CEMSDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var venue = new Venue { VenueId = 910, Name = "V910", Address = "Addr", Capacity = 100 };
        var activity = new Activity { ActivityId = 910, Name = "A910", Type = "Workshop", Description = "desc" };
        var upcoming = new Event
        {
            EventId = 910,
            Name = "Upcoming",
            EventDate = DateTime.UtcNow.AddDays(30),
            Description = "upcoming",
            Capacity = 50,
            Venues = [venue],
            Activities = [activity]
        };

        await context.Venues.AddAsync(venue);
        await context.Activities.AddAsync(activity);
        await context.Events.AddAsync(upcoming);
        await context.SaveChangesAsync();

        var service = new EventFilterService(new UnitOfWork(context));
        var result = (await service.GetUpcomingAsync()).ToList();

        var found = Assert.Single(result, e => e.EventId == upcoming.EventId);
        Assert.NotEmpty(found.Venues);
        Assert.NotEmpty(found.Activities);
    }
}
