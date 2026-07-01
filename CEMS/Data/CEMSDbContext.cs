using CEMS.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Data;

public class CEMSDbContext(DbContextOptions<CEMSDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.PersonId);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(120);
            entity.Property(p => p.Email).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Phone).HasMaxLength(30);

            entity
                .HasDiscriminator<string>("Role")
                .HasValue<Participant>("Participant")
                .HasValue<Administrator>("Administrator");
        });

        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.Property(a => a.Permissions)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v == "" ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(500)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    a => a.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                    a => a.ToList()));
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Capacity).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_Events_Capacity", "Capacity > 0"));
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(a => a.ActivityId);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(120);
            entity.Property(a => a.Type).IsRequired().HasMaxLength(60);
            entity.Property(a => a.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(v => v.VenueId);
            entity.Property(v => v.Name).IsRequired().HasMaxLength(120);
            entity.Property(v => v.Address).IsRequired().HasMaxLength(250);
            entity.Property(v => v.Capacity).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_Venues_Capacity", "Capacity > 0"));
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(r => r.RegistrationId);
            entity.Property(r => r.RegistrationDate).IsRequired();
            entity.Property(r => r.Status).IsRequired();

            entity.HasOne(r => r.Participant)
                .WithMany(p => p.Registrations)
                .HasForeignKey(r => r.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Activities)
            .WithMany(a => a.Events)
            .UsingEntity<Dictionary<string, object>>(
                "EventActivity",
                right => right.HasOne<Activity>().WithMany().HasForeignKey("ActivityId"),
                left => left.HasOne<Event>().WithMany().HasForeignKey("EventId"),
                join =>
                {
                    join.ToTable("EventActivities");
                    join.HasKey("EventId", "ActivityId");
                    join.HasData(
                        new { EventId = 1, ActivityId = 1 },
                        new { EventId = 1, ActivityId = 2 },
                        new { EventId = 2, ActivityId = 3 });
                });

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Venues)
            .WithMany(v => v.Events)
            .UsingEntity<Dictionary<string, object>>(
                "EventVenue",
                right => right.HasOne<Venue>().WithMany().HasForeignKey("VenueId"),
                left => left.HasOne<Event>().WithMany().HasForeignKey("EventId"),
                join =>
                {
                    join.ToTable("EventVenues");
                    join.HasKey("EventId", "VenueId");
                    join.HasData(
                        new { EventId = 1, VenueId = 1 },
                        new { EventId = 2, VenueId = 2 });
                });

        modelBuilder.Entity<Activity>().HasData(
            new Activity { ActivityId = 1, Name = "AI Fundamentals", Type = "Workshop", Description = "Hands-on intro to machine learning basics." },
            new Activity { ActivityId = 2, Name = "Career Talk", Type = "Talk", Description = "Industry alumni share hiring and portfolio tips." },
            new Activity { ActivityId = 3, Name = "Coding Challenge", Type = "Game", Description = "Team-based programming challenge with prizes." });

        modelBuilder.Entity<Venue>().HasData(
            new Venue { VenueId = 1, Name = "Innovation Hall", Address = "Block A, Main Campus", Capacity = 200 },
            new Venue { VenueId = 2, Name = "Computer Lab 3", Address = "Block C, Room 301", Capacity = 60 },
            new Venue { VenueId = 3, Name = "Open Ground", Address = "North Field", Capacity = 500 });

        modelBuilder.Entity<Participant>().HasData(
            new Participant { PersonId = 1, Name = "Anita Sharma", Email = "anita.sharma@uni.edu", Phone = "555-0101" },
            new Participant { PersonId = 2, Name = "Rahul Karki", Email = "rahul.karki@uni.edu", Phone = "555-0102" },
            new Participant { PersonId = 3, Name = "Mina Gurung", Email = "mina.gurung@uni.edu", Phone = "555-0103" });

        modelBuilder.Entity<Administrator>().HasData(
            new Administrator { PersonId = 100, Name = "Admin User", Email = "admin@uni.edu", Phone = "555-0000" });

        modelBuilder.Entity<Event>().HasData(
            new Event
            {
                EventId = 1,
                Name = "Tech Connect 2026",
                EventDate = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc),
                Description = "University-wide community event with talks and workshops.",
                Capacity = 180
            },
            new Event
            {
                EventId = 2,
                Name = "Code Sprint Weekend",
                EventDate = new DateTime(2026, 10, 5, 9, 30, 0, DateTimeKind.Utc),
                Description = "A collaborative coding sprint for students and local mentors.",
                Capacity = 50
            });

        modelBuilder.Entity<Registration>().HasData(
            new Registration
            {
                RegistrationId = 1,
                ParticipantId = 1,
                EventId = 1,
                RegistrationDate = new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc),
                Status = RegistrationStatus.Confirmed
            },
            new Registration
            {
                RegistrationId = 2,
                ParticipantId = 2,
                EventId = 1,
                RegistrationDate = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
                Status = RegistrationStatus.Pending
            });
    }
}
