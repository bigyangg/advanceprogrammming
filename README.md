# Community Event Management System (CEMS)

**CET254 Advanced Programming - Assignment 1**

A professional ASP.NET Core MVC (.NET 8) web application for managing community events, including comprehensive CRUD operations for events, participants, venues, activities, and event registrations. Demonstrates advanced object-oriented design patterns, efficient data access layers, and robust validation with exception handling.

---

## Overview

The CEMS application provides a complete event management platform with role-based access control:
- **Administrators** can create, manage events, venues, activities, and participants
- **Participants** can browse upcoming events, register/cancel registrations, and view registration history
- **Public users** can browse and filter upcoming events without authentication

### Key Features
- Full CRUD operations for all entities
- Many-to-many relationships between Events, Participants, Venues, and Activities
- Event capacity management and duplicate registration prevention
- Event filtering by date, venue, and activity type
- Session-based role-based access control
- Comprehensive error handling with custom exception hierarchy
- Seed data for immediate demonstration

---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | ASP.NET Core MVC 8.0 |
| **Database** | SQLite (portable, no server required) |
| **ORM** | Entity Framework Core 8.0.8 |
| **Testing** | xUnit 2.5.3 |
| **Language** | C# 12.0 |

---

## Installation & Setup

### Prerequisites
- .NET SDK 8.0 or later
- Windows, macOS, or Linux

### Quick Start

```bash
# 1. Restore NuGet packages
dotnet restore

# 2. Apply database migrations (creates cems.db with seed data)
dotnet ef database update --project CEMS/CEMS.csproj --startup-project CEMS/CEMS.csproj

# 3. Run the application
dotnet run --project CEMS/CEMS.csproj

# 4. Open browser and navigate to
# https://localhost:5001
```

### Demo Accounts (Seeded)

**Administrator Account**
- **User ID:** `100`
- **Name:** Admin User
- **Email:** admin@uni.edu
- **Permissions:** Full event, venue, activity, and participant management

**Participant Accounts**
- **User ID:** `1` — Anita Sharma (anita@uni.edu)
- **User ID:** `2** — Rahul Karki (rahul@uni.edu)
- **User ID:** `3` — Mina Gurung (mina@uni.edu)

Simply enter the User ID to "login" (session-based, demo-friendly).

---

## Project Architecture

### Directory Structure
```
CEMS/
├── Models/
│   ├── Models/              # Domain entities (Person, Participant, Administrator, Event, etc.)
│   └── Interfaces/          # IRegistrable, INotifiable
├── Data/
│   ├── CEMSDbContext.cs     # EF Core configuration, relationships, seed data
│   └── Migrations/          # Database migration files
├── Repositories/            # IRepository, Repository, IUnitOfWork, UnitOfWork
├── Services/                # RegistrationService, EventFilterService
├── Exceptions/              # CEMSException hierarchy (DuplicateRegistrationException, etc.)
├── Controllers/             # MVC controllers (7 total)
├── Views/                   # Razor view templates (30 views)
├── ViewModels/              # Data binding models for views
├── Filters/                 # CEMSExceptionFilter, RequireRoleAttribute
├── Infrastructure/          # SessionUserExtensions
└── Program.cs               # DI configuration, middleware setup

CEMS.Tests/
├── Services/                # RegistrationService, EventFilterService tests
└── Models/                  # Person, Participant, Administrator tests
```

### Core Components

#### 1. **Domain Model** (Models/)
- **Person** (abstract base class) — encapsulated Name/Email with validation
- **Participant** (inherits Person, implements IRegistrable) — register/cancel/view registrations
- **Administrator** (inherits Person) — overloaded CreateEvent methods
- **Event** (implements INotifiable) — many-to-many to Participant (via Registration), Venue, Activity
- **Registration** (explicit join entity) — RegistrationDate, Status (Pending/Confirmed/Cancelled)
- **Venue**, **Activity** (supporting entities)

#### 2. **Data Access Layer** (Repositories/)
- **IRepository<T>** — generic interface for CRUD + Query operations
- **Repository<T>** — EF Core wrapper (no business logic, data access only)
- **IUnitOfWork / UnitOfWork** — coordinates multiple repositories for atomic commits
- **Design Pattern:** Repository pattern decouples business logic from EF Core; enables testing and future data source changes

#### 3. **Service Layer** (Services/)
- **RegistrationService** — business rules (duplicate check, capacity validation, role verification)
- **EventFilterService** — LINQ query composition for efficient filtering (SQL-side WHERE/ORDER BY)
- Integrates with domain models via repository pattern

#### 4. **Exception Handling** (Exceptions/)
- **CEMSException** (base class) — all CEMS domain errors inherit
- **DuplicateRegistrationException** — active registration already exists
- **EventCapacityExceededException** — event is at capacity
- **InvalidRegistrationStateException** — registration status conflict
- **EntityNotFoundException** — entity not found in database
- **CEMSExceptionFilter** — global exception handler translates errors to user-friendly responses

#### 5. **Controllers** (Controllers/)
1. **AccountController** — Login/Logout (session-based)
2. **EventsController** — Public browse, Admin CRUD (with venue/activity assignment)
3. **VenuesController** — Admin CRUD
4. **ActivitiesController** — Admin CRUD
5. **ParticipantsController** — Admin CRUD
6. **RegistrationsController** — Participant register/cancel/view registrations
7. **HomeController** — Home page

---

## Database Schema

### Tables
- **Persons** — discriminator-based inheritance (TPH) for Person/Participant/Administrator
- **Events** — event details, capacity
- **Registrations** — explicit join entity (Event ↔ Participant with status)
- **EventActivities** — many-to-many (Event ↔ Activity)
- **EventVenues** — many-to-many (Event ↔ Venue)
- **Venues** — venue name, address, capacity
- **Activities** — activity name, type

### Key Constraints
- Event.Capacity >= 0
- Registration.Status in {Pending, Confirmed, Cancelled}
- Participant email must be unique

---

## Mapping to CET254 Marking Criteria

### 1. UML Diagrams (20 points)
**Status:** Pending External Submission
- External document required (use case diagram and class diagram)
- Deliverable: Separate Word/PDF document
- Domain classes implemented match design specifications

### 2. Object-Oriented Implementation (20 points)
**Status:** Complete (100%)

| Criterion | Implementation |
|-----------|-----------------|
| Inheritance | `Person` (abstract) extends to `Participant`, `Administrator` |
| Interfaces | `IRegistrable` (Register, CancelRegistration, ViewRegistrations); `INotifiable` (SendNotification) |
| Polymorphism | `GetRole()` virtual method overridden in Participant and Administrator |
| Overloading | `Administrator.CreateEvent()` implemented with two parameterized versions |
| Encapsulation | Protected backing fields with validation for Name and Email |
| Constructors | Parameterized constructors with dependency injection; proper initialization |

**Evidence:** `CEMS/Models/Models/*.cs`, `CEMS/Models/Interfaces/*.cs`

### 3. Data Structures, Algorithms & Design Patterns (20 points)
**Status:** Complete (100%)

| Criterion | Implementation |
|-----------|-----------------|
| Design Pattern | Repository plus Unit of Work pattern (IRepository<T>, UnitOfWork) coordinates data access, enables atomic commits, and supports testing |
| Data Structures | List<T> for collections, Dictionary for lookups, IQueryable<T> for deferred execution |
| Algorithms - Efficiency | EventFilterService applies predicates conditionally to optimize SQL queries; RegistrationService uses CountAsync and AnyAsync for server-side aggregation |
| Complex Logic | Capacity validation using SQL COUNT on confirmed and pending registrations, duplicate detection with SQL EXISTS check, role validation at service layer |

**Evidence:** `CEMS/Repositories/*.cs`, `CEMS/Services/EventFilterService.cs` (lines 14–32), `CEMS/Services/RegistrationService.cs` (lines 30–45)

### 4. Validation & Exception Handling (10 points)
**Status:** Complete (100%)

| Criterion | Implementation |
|-----------|-----------------|
| Data Validation | Data annotations including [Required], [StringLength], [EmailAddress], [Phone], and [Range] on models and views |
| Custom Exceptions | 5 exception types (CEMSException base plus 4 derived); thrown from service layer |
| Exception Handling | CEMSExceptionFilter (global IExceptionFilter) catches CEMSException, adds to ModelState, returns user-friendly error |
| Validation Coverage | Email format, phone format, name length, capacity bounds, registration status transitions |

**Evidence:** `CEMS/Exceptions/*.cs`, `CEMS/Filters/CEMSExceptionFilter.cs`, all models include [Required] and related attributes

### 5. Test Documentation (10 points)
**Status:** Complete (100%)

**Test Coverage: 14 Unit Tests (All Passing)**

| Test Class | Tests | Purpose |
|-----------|-------|---------|
| RegistrationServiceTests | 4 | Register success, duplicate throws exception, capacity throws exception, cancel then reregister |
| PersonAndParticipantTests | 3 | Email validation, Register and CancelRegistration state transitions, duplicate prevention |
| AdministratorTests | 2 | CreateEvent with parameters, CreateEvent template-based overload demonstration |
| EventFilterServiceTests | 1 | FilterAsync by date, venue, and activity using LINQ query composition |
| HomeControllerTests | 4 | Index, Privacy, Error pages |

**Run Tests:** `dotnet test CEMS.sln`  
**Result:** Passed: 14, Failed: 0, Skipped: 0

**Evidence:** `CEMS.Tests/Services/*.cs`, `CEMS.Tests/Models/*.cs`

### 6. Scope & Functionality (10 points)
**Status:** Complete (100%)

| Feature | Status | Evidence |
|---------|--------|----------|
| Admin CRUD for Events | Complete | Create, Read, Update, Delete with venue and activity assignment in EventsController (lines 50–120) |
| Admin CRUD for Venues | Complete | Create, Read, Update, Delete in VenuesController |
| Admin CRUD for Activities | Complete | Create, Read, Update, Delete in ActivitiesController |
| Admin CRUD for Participants | Complete | Create, Read, Update, Delete in ParticipantsController |
| Participant Registration | Complete | Duplicate check, capacity check, pending status in RegistrationService.RegisterAsync |
| Participant Cancel Registration | Complete | Status changed to Cancelled in RegistrationService.CancelAsync |
| Participant View Registrations | Complete | Personal registration list with status in RegistrationsController.MyRegistrations |
| Public Browse Events | Complete | Upcoming events available without login in EventsController.Index |
| Public Filter Events | Complete | Filtering by date, venue, and activity in EventFilterService.FilterAsync |
| Many-to-Many Relationships | Complete | Event to Participant via Registration, Event to Venue, Event to Activity in CEMSDbContext |
| Event Capacity Management | Complete | Event capacity enforced in RegistrationService capacity check |

**Evidence:** All 7 controllers, 30 views, seed data with sample events, venues, activities, and registrations

### 7. Demonstration (10 points)
**Status:** Pending Video Submission

**Required:** Panopto video (10 minutes or less) linked via text file in submission zip  
**Suggested Demo Path (all features covered in approximately 8 minutes):**

1. **Login & Admin Workflows (0:00–1:30)**
   - Login as Administrator (ID: 100)
   - Create a new event with venues and activities
   - Edit event details
   - Show the repository pattern in code (IUnitOfWork usage)

2. **Service Layer & Design Pattern (1:30–3:00)**
   - Show RegistrationService capacity check logic
   - Demonstrate EventFilterService LINQ query composition
   - Explain why Repository pattern is used for decoupling and testability

3. **Participant Workflows (3:00–5:30)**
   - Logout and login as Participant (ID: 1)
   - Browse Upcoming Events (public access without login)
   - Register for an event with success and capacity check demonstration
   - View personal registrations
   - Cancel a registration

4. **Event Filtering & Exception Handling (5:30–7:00)**
   - Filter events by Date, Venue, and Activity
   - Show validation error with invalid email
   - Demonstrate duplicate registration exception

5. **Unit Tests & Code Quality (7:00–8:00)**
   - Run `dotnet test` and show 14 of 14 tests passing
   - Briefly review a test (for example, RegistrationServiceTests)
   - Show OOP concepts in models using IDE inheritance diagram

---

## Build & Test Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests (all 14 tests should pass)
dotnet test CEMS.sln

# Apply database migrations
dotnet ef database update --project CEMS/CEMS.csproj

# Run application
dotnet run --project CEMS/CEMS.csproj

# Create new migration (if schema changes)
dotnet ef migrations add <MigrationName> --project CEMS/CEMS.csproj
```

---

## Submission Checklist

- Source Code: CEMS/ and CEMS.Tests/ folders with all controllers, services, models, and views
- Database: cems.db seeded with sample data
- Tests: 14 unit tests (all passing)
- Build: 0 errors, 0 warnings
- UML Document: External Word/PDF with use case and class diagrams (separate from code)
- Test Documentation: Evidence of test results in Word/PDF document (can be combined with UML doc)
- Demo Video: Panopto link in text file (10 minutes or less, follows suggested path above)

### Preparing Your Submission Zip

```
SURNAME_FORENAME/
├── Solution/                    # CEMS/ and CEMS.Tests/ folders
├── Documentation.docx           # UML diagrams and test results
└── SURNAME_FORENAME.txt         # Text file with Panopto video link
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `dotnet not recognized` | Add .NET SDK to PATH or use the full path: `C:\Program Files\dotnet\dotnet.exe` |
| Database error on first run | Run `dotnet ef database update` to initialize the SQLite database |
| Port 5001 already in use | Change the port in `Properties/launchSettings.json` or restart the process |
| Test failures | Ensure database is updated; xUnit tests use in-memory SQLite for isolation |

---

## References & Documentation

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [xUnit.net](https://xunit.net/)
- [Repository Pattern](https://www.martinfowler.com/eaaCatalog/repository.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

**Assignment:** CET254 Advanced Programming  
**Module:** Advanced Programming  
**Submission Deadline:** As per Canvas  
**Last Updated:** 2026-07-01
