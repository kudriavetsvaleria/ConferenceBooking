# ConferenceBooking

A backend service for booking conference rooms: room search and booking, optional add-on services (projector, Wi-Fi, sound) with inventory tracking, client/organizer roles, and JWT authentication.

## Tech Stack

- **.NET 9**, ASP.NET Core Web API
- **Entity Framework Core** + SQL Server (LocalDB for development)
- **JWT Bearer** authentication, **BCrypt** for password hashing
- **Swagger / OpenAPI** for API documentation and manual testing
- **xUnit** + **Moq** for unit tests

## Architecture

The project follows **Clean Architecture** — dependencies point strictly inward, toward `Domain`:

```
API ──────► Application ◄────── Infrastructure
                  │
                  ▼
               Domain
```

| Layer | Contents |
|---|---|
| `ConferenceBooking.Domain` | Entities (`User`, `Room`, `Service`, `Booking`, `BookingService`) and enums. Depends on nothing. |
| `ConferenceBooking.Application` | Business logic (services), DTOs, repository/service interfaces. Depends only on `Domain`. |
| `ConferenceBooking.Infrastructure` | EF Core (`AppDbContext`, migrations), repository implementations, JWT and BCrypt. Depends on `Application`. |
| `ConferenceBooking.API` | Controllers, DI and Swagger configuration, entry point. |

Repository interfaces are declared in `Application` (not `Domain`) — so `Infrastructure` implements contracts defined by the business logic, rather than the other way around (Dependency Inversion Principle).

## Domain Model

- **User** — client or organizer (single table, role as a field)
- **Room** — a room: capacity, price per hour
- **Service** — an add-on (Projector/Wi-Fi/Sound) with its own inventory (`TotalQuantity`), not tied to any specific room
- **Booking** — a booking: room, client, time period, status (`Confirmed → Paid`, or `Cancelled`)
- **BookingService** — booking↔service link (many-to-many) with quantity; part of the `Booking` aggregate, has no repository of its own

## Business Rules

- There is a single organizer who manages the entire platform; created manually (seed), not through public registration
- A booking is auto-confirmed if the room and services are available for the requested period
- Before creating a booking, the system checks: (1) whether the period overlaps with another active booking of the same room, (2) whether there's enough inventory for each requested service given what's already booked for that period
- The total price is calculated on the fly (not stored in the database) — room price for the duration plus the price of the selected services
- A booking can be cancelled no later than 48 hours before it starts; cancelling a paid booking includes a (mock) refund message
- Payment is a mock endpoint that moves a booking from `Confirmed` to `Paid`

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server LocalDB (installed together with Visual Studio)

### Steps

```bash
git clone <repo-url>
cd ConferenceBooking
dotnet restore
```

Apply migrations (creates the `ConferenceBookingDb` database in LocalDB):
```bash
dotnet ef database update --project src/ConferenceBooking.Infrastructure --startup-project src/ConferenceBooking.API
```

Run the API:
```bash
dotnet run --project src/ConferenceBooking.API
```

Open Swagger UI: `https://localhost:<port>/swagger`

### First Organizer

Registering through the API always creates a `Client`. The organizer has to be added to the database manually with a single SQL statement (the password is a BCrypt hash):
```sql
INSERT INTO Users (Name, Email, PasswordHash, Role)
VALUES ('Organizer', 'organizer@conferencebooking.com', '<bcrypt-hash>', 1);
```

## API

| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/api/Auth/register` | Public | Register a client |
| POST | `/api/Auth/login` | Public | Log in, returns a JWT |
| GET | `/api/Rooms` | Public | Search rooms (capacity/date/price) |
| GET | `/api/Rooms/{id}` | Public | Get a room by id |
| POST/PUT/DELETE | `/api/Rooms` | Organizer | Room CRUD |
| GET | `/api/Services` | Public | Service catalog |
| POST/PUT/DELETE | `/api/Services` | Organizer | Service CRUD |
| POST | `/api/Bookings` | Authenticated | Create a booking (with availability checks) |
| GET | `/api/Bookings/my` | Authenticated | Get own bookings |
| POST | `/api/Bookings/{id}/cancel` | Authenticated (owner) | Cancel a booking (48h deadline) |
| POST | `/api/Bookings/{id}/pay` | Authenticated (owner) | Mock-pay for a booking |

## Tests

```bash
dotnet test tests/ConferenceBooking.Tests
```

Unit tests for `BookingManagementService` (with mocked repositories via Moq): time validation, room conflicts, insufficient inventory, and correct price calculation.
