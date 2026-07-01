using CEMS.Data;
using CEMS.Exceptions;
using CEMS.Models;
using CEMS.Repositories;
using CEMS.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Tests.Services;

public class RegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenValid_CreatesPendingRegistration()
    {
        await using var fixture = await TestDbFixture.CreateAsync();
        await fixture.SeedParticipantAndEventAsync(capacity: 2);

        var result = await fixture.Service.RegisterAsync(1, 10);

        Assert.Equal(RegistrationStatus.Pending, result.Status);
        Assert.Equal(1, result.ParticipantId);
        Assert.Equal(10, result.EventId);
    }

    [Fact]
    public async Task RegisterAsync_WhenDuplicate_ThrowsDuplicateRegistrationException()
    {
        await using var fixture = await TestDbFixture.CreateAsync();
        await fixture.SeedParticipantAndEventAsync(capacity: 2);
        await fixture.Context.Registrations.AddAsync(new Registration
        {
            ParticipantId = 1,
            EventId = 10,
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending
        });
        await fixture.Context.SaveChangesAsync();

        await Assert.ThrowsAsync<DuplicateRegistrationException>(() => fixture.Service.RegisterAsync(1, 10));
    }

    [Fact]
    public async Task RegisterAsync_WhenCapacityReached_ThrowsEventCapacityExceededException()
    {
        await using var fixture = await TestDbFixture.CreateAsync();
        await fixture.SeedParticipantAndEventAsync(capacity: 1);
        await fixture.Context.Participants.AddAsync(new Participant
        {
            PersonId = 200,
            Name = "Second Student",
            Email = "second@uni.edu",
            Phone = "555-1002"
        });
        await fixture.Context.Registrations.AddAsync(new Registration
        {
            ParticipantId = 200,
            EventId = 10,
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Confirmed
        });
        await fixture.Context.SaveChangesAsync();

        await Assert.ThrowsAsync<EventCapacityExceededException>(() => fixture.Service.RegisterAsync(1, 10));
    }

    [Fact]
    public async Task RegisterAsync_AfterCancel_AllowsReRegistration()
    {
        await using var fixture = await TestDbFixture.CreateAsync();
        await fixture.SeedParticipantAndEventAsync(capacity: 2);

        var created = await fixture.Service.RegisterAsync(1, 10);
        var cancelled = await fixture.Service.CancelAsync(created.RegistrationId);
        var reRegistered = await fixture.Service.RegisterAsync(1, 10);

        Assert.Equal(RegistrationStatus.Cancelled, cancelled.Status);
        Assert.Equal(RegistrationStatus.Pending, reRegistered.Status);
        Assert.NotEqual(created.RegistrationId, reRegistered.RegistrationId);
    }

    private sealed class TestDbFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public CEMSDbContext Context { get; }
        public IRegistrationService Service { get; }

        private TestDbFixture(SqliteConnection connection, CEMSDbContext context)
        {
            _connection = connection;
            Context = context;
            Service = new RegistrationService(new UnitOfWork(context));
        }

        public static async Task<TestDbFixture> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<CEMSDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new CEMSDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return new TestDbFixture(connection, context);
        }

        public async Task SeedParticipantAndEventAsync(int capacity)
        {
            if (!await Context.Participants.AnyAsync(p => p.PersonId == 1))
            {
                await Context.Participants.AddAsync(new Participant
                {
                    PersonId = 1,
                    Name = "Test Student",
                    Email = "student@uni.edu",
                    Phone = "555-1001"
                });
            }

            if (!await Context.Events.AnyAsync(e => e.EventId == 10))
            {
                await Context.Events.AddAsync(new Event
                {
                    EventId = 10,
                    Name = "Testing Event",
                    EventDate = DateTime.UtcNow.AddDays(1),
                    Description = "Event used by tests.",
                    Capacity = capacity
                });
            }

            await Context.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
