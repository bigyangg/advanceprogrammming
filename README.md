# Community Event Management System (CEMS)

**CET254 Advanced Programming – Assignment 1**

This is my ASP.NET Core MVC (.NET 8) project for the CET254 assignment. It's a web app for managing community events — creating events, assigning venues and activities to them, and letting participants browse and register. I built it around a Repository + Unit of Work data layer with a separate service layer for the actual business rules (capacity checks, duplicate registrations, etc.), and I've tried to keep the OOP side of things (inheritance, interfaces, polymorphism) genuinely doing something rather than just being there to tick a box.

---

## What it does

- **Administrators** can create/edit/delete events, venues, activities and participants, and can confirm pending registrations for an event.
- **Participants** can sign up for their own account, log in, browse events, register/cancel their bookings, and print a ticket for anything they've booked.
- **Anyone** can browse and filter upcoming events without logging in.

Login is session-based and passwordless (you just pick a person ID) — this is a demo/assignment app, not a production login system, so I kept it simple on purpose. What I did add this round is a proper self-signup flow so a new participant doesn't need an admin to create their account for them, plus a Participant/Admin tab toggle on the login page so the two account types aren't just dumped in one long list.

### Feature list
- Full CRUD for Events, Venues, Activities and Participants
- Self-service participant signup, with auto sign-in straight after creating the account
- Participant/Admin tabbed login (CSS-only toggle, no JS) alongside the original "enter your person ID" box
- Many-to-many relationships: Event↔Venue, Event↔Activity, and Event↔Participant via an explicit Registration entity
- Event capacity enforcement (seat-based, not just headcount) with duplicate-registration prevention
- Admin registration workflow — pending registrations can be confirmed from an admin screen per event
- Event filtering by date, venue and activity type
- Printable ticket page for a confirmed/pending booking
- Custom exception hierarchy + a global exception filter so domain errors turn into readable messages instead of stack traces
- Seed data so the app is populated the moment you run it

---

## Tech stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core MVC 8.0 |
| Database | SQLite (file-based, no server to install) |
| ORM | Entity Framework Core 8.0.8 |
| Testing | xUnit 2.5.3 |
| Language | C# 12 |

---

## Running it

### You'll need
- .NET SDK 8.0+
- Any OS — I developed on Windows, nothing here is Windows-specific

### Steps

```bash
# 1. Restore packages
dotnet restore

# 2. Apply migrations (creates cems.db and seeds it)
dotnet ef database update --project CEMS/CEMS.csproj --startup-project CEMS/CEMS.csproj

# 3. Run it
dotnet run --project CEMS/CEMS.csproj

# 4. Open the browser at whatever URL the console prints
#    (usually https://localhost:7089 or http://localhost:5172)
```

### Logging in

The login page has two ways in:
1. Type a person ID directly into the "sign in with a person ID" box, or
2. Use the Participant/Admin tabs to one-click sign in as one of the seeded accounts, or, on the Participant tab, open "New here? Create an account" to sign up as a brand-new participant.

**Seeded accounts:**

| Role | ID | Name |
|------|----|------|
| Administrator | 100 | Admin User |
| Participant | 1 | Anita Sharma |
| Participant | 2 | Rahul Karki |
| Participant | 3 | Mina Gurung |

Any new account created via signup shows up alongside these automatically.

---

## Project layout

```
CEMS/
├── Models/Models/       Domain entities — Person, Participant, Administrator, Event, Registration, Venue, Activity
├── Models/Models/Interfaces/  IRegistrable, INotifiable
├── Data/                 CEMSDbContext (relationships + seed data) and EF migrations
├── Repositories/          IRepository<T> / Repository<T>, IUnitOfWork / UnitOfWork
├── Services/              RegistrationService, EventFilterService
├── Exceptions/            CEMSException and the four exceptions that derive from it
├── Controllers/           7 controllers (see below)
├── Views/                 Razor views, organised by controller
├── ViewModels/             Form/binding models kept separate from the domain models
├── Filters/               CEMSExceptionFilter, RequireRoleAttribute
├── Infrastructure/        SessionUserExtensions (wraps session-based login state)
└── Program.cs             DI + middleware setup

CEMS.Tests/
├── Services/  RegistrationService, EventFilterService tests
└── Models/    Person/Participant and Administrator tests
```

**Controllers:** AccountController (login/signup/logout), EventsController (public browse + admin CRUD + registration confirmation), VenuesController, ActivitiesController, ParticipantsController (all admin CRUD), RegistrationsController (participant register/cancel/profile/ticket), HomeController.

---

## The OOP / design side, for the marking criteria

I'm putting this here because it maps fairly directly onto how the module is marked, but everything below is just describing code that's actually in the repo — nothing here is aspirational.

**Inheritance & polymorphism** — `Person` is an abstract base class with `Participant` and `Administrator` inheriting from it (EF Core TPH, discriminated by a `Role` column). `GetRole()` is abstract on `Person` and overridden differently in each subclass. `Name` and `Email` are encapsulated behind property setters that actually validate on assignment rather than just having a `[Required]` attribute sitting there unused.

**Interfaces** — `IRegistrable` (`Register`, `CancelRegistration`, `ViewRegistrations`) is implemented by `Participant`. `INotifiable` (`SendNotification`) is implemented by `Event`.

**Overloading** — `Administrator.CreateEvent(...)` has two overloads with different parameter sets.

**Design pattern** — Repository + Unit of Work (`IRepository<T>` / `Repository<T>`, `IUnitOfWork` / `UnitOfWork`). The point of this, beyond "it's a known pattern", is that controllers and services never talk to `DbContext` directly, which is what makes the service-layer unit tests possible without spinning up a real web server.

**Where the actual logic lives** — `RegistrationService.RegisterAsync` does the duplicate check, the seat-capacity check (summed against `Registration.Seats`, not just a row count) and the actual registration, all wrapped in one DB transaction so two people booking the last seat at the same time can't both succeed. `EventFilterService.FilterAsync` builds up the `IQueryable` conditionally so filtering happens in SQL rather than pulling every event into memory first.

**Exception handling** — `CEMSException` is the base type, with `DuplicateRegistrationException`, `EventCapacityExceededException`, `InvalidRegistrationStateException` and `EntityNotFoundException` underneath it. `CEMSExceptionFilter` is a global `IExceptionFilter` that catches these and turns them into a flash message instead of a 500 page.

---

## Testing

15 unit tests, all passing:

| Test class | Count | What it covers |
|---|---|---|
| `RegistrationServiceTests` | 4 | Successful registration, duplicate registration throws, capacity-exceeded throws, cancel-then-reregister works |
| `EventFilterServiceTests` | 2 | Filtering by date/venue/activity returns the right event with its venues/activities loaded; `GetUpcomingAsync` does the same |
| `PersonAndParticipantTests` | 7 | Email validation (valid/invalid cases), register+cancel state transitions, registering twice for the same event throws |
| `AdministratorTests` | 2 | Both `CreateEvent` overloads |

```bash
dotnet test CEMS.sln
```

Tests run against a real (in-memory) SQLite connection rather than the EF Core in-memory provider, so transaction behaviour is actually exercised, not faked.

---

## Known limitations

Being upfront about these rather than pretending the app is bulletproof:

- **Login has no password.** It's intentionally a demo/session-based system for this assignment — anyone who knows or guesses a person ID can sign in as that person. Not something I'd ship as-is.
- **Email uniqueness on signup is checked at the application level**, not enforced by a DB constraint, so there's a small window for two simultaneous signups with the same email to both succeed. Would fix with a unique index if this went further.
- **The capacity-check transaction closes the common-case race condition**, not a fully serializable guarantee — SQLite's default transaction isolation is good enough for this assignment's purposes but I'm not claiming airtight correctness under heavy concurrent load.

---

## Build & test commands

```bash
dotnet restore
dotnet build
dotnet test CEMS.sln
dotnet ef database update --project CEMS/CEMS.csproj
dotnet run --project CEMS/CEMS.csproj

# if I change the schema again
dotnet ef migrations add <MigrationName> --project CEMS/CEMS.csproj
```

---

## Submission checklist

- [x] Source code (`CEMS/` and `CEMS.Tests/`)
- [x] Seeded `cems.db` included
- [x] 15/15 unit tests passing
- [x] Builds with 0 errors, 0 warnings
- [ ] UML document (use case + class diagram) — separate Word/PDF, not part of this repo
- [ ] Test documentation / results write-up
- [ ] Demo video link (Panopto, ≤10 min)

### Suggested demo order
1. Log in as admin (ID 100), walk through creating an event and assigning a venue/activity
2. Log out, use the new signup flow to create a participant account
3. Browse events, filter by date/venue/activity, register for one
4. Show the capacity check by trying to overbook, and the duplicate-registration check
5. Cancel a registration, then show a second participant can't cancel someone else's booking
6. Log back in as admin, confirm the pending registration
7. Generate and view a ticket
8. `dotnet test` — 15 passing

### Zip structure
```
SURNAME_FORENAME/
├── Solution/                 CEMS/ and CEMS.Tests/
├── Documentation.docx        UML diagrams + test results
└── SURNAME_FORENAME.txt      Panopto link
```

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| `dotnet` not recognised | Add the .NET SDK to PATH |
| DB error on first run | Run the `dotnet ef database update` command above |
| Port already in use | Check/change `Properties/launchSettings.json` |
| Test failures | Make sure the SQLite provider is restored (`dotnet restore`) — tests use a real in-memory SQLite connection |

---

*CET254 Advanced Programming — last touched 2026-07-02.*
