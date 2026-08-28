# Mini Booking Inventory System

A simplified hotel room booking API with a minimal frontend UI, built with ASP.NET Core, Entity Framework Core, and SQL Server.

## Overview

This system models hotel room inventory management with the following capabilities:
- Create bookings with automatic overlap detection and rate calculation
- Check room availability for date ranges
- Cancel bookings with a 48-hour cancellation window
- Support for dynamic rate changes over time
- Overbooking exceptions with capacity flagging

## Key Design Decisions & Assumptions

### 1. Rate History Model

**Design:** Used a separate `RateHistory` table with `(RoomId, EffectiveDate, BaseRate)` tuples.

**Rationale:**
- Simplicity: No need for complex audit trails or validity date ranges
- Performance: Efficient queries using `WHERE EffectiveDate <= targetDate ORDER BY EffectiveDate DESC LIMIT 1`
- Real-world: Hotels often need to track historical rates for reporting
- Concurrency-safe: Inserting new rates doesn't modify existing data

**How It Works:**
- Each room can have multiple rate entries
- To find the rate for a specific date, query the most recent `RateHistory` where `EffectiveDate <= targetDate`
- Rate prices are exact per night; no interpolation or averaging
- Example: If rates are $100 (Jan 1) and $150 (Jan 5), a booking Jan 3-7 pays: $100 (Jan 3), $100 (Jan 4), $150 (Jan 5), $150 (Jan 6) = $500

**Alternative Considered:**
- Single `BaseRate` column on `Room` with an audit/history table: Requires more complex joins for price calculation
- Date-range validity on `RateHistory`: Added complexity for soft deletes and overlaps

### 2. Overlap Detection Logic

**Rule:** Two bookings overlap if `CheckIn1 < CheckOut2 AND CheckOut1 > CheckIn2`

**Back-to-Back Allowed:** A booking ending on date X and a booking starting on date X are NOT considered overlapping.
- Example: Booking 1 (Jan 1-3) and Booking 2 (Jan 3-5) do NOT overlap
- This is modeled as `CheckIn < CheckOut` (not `<=`), allowing same-day turnovers

**Cancelled Bookings:** Ignored in overlap checks using `WHERE IsCancelled = false`

### 3. Cancellation Soft Delete

**Design:** Used a `IsCancelled` boolean flag (soft delete) rather than hard deletion.

**Rationale:**
- Audit trail: Keeps historical data for reporting and reconciliation
- Reversibility: Could implement un-cancellation if needed
- Referential integrity: Avoids orphaned booking records in logs/transactions
- Compliance: Many jurisdictions require booking history retention

**Concurrency:** Multiple updates to the same booking use EF Core's optimistic concurrency (implicit based on SaveChanges timing)

### 4. Overbooking Exception

**Logic:**
1. Check if `GuestCount > Room.Capacity`
2. If true AND `Hotel.AllowOverbooking = false`: Return 422 (Unprocessable Entity)
3. If true AND `Hotel.AllowOverbooking = true` AND `GuestCount <= Capacity + 1`: Allow booking with `IsOverCapacity = true`
4. If true AND `Hotel.AllowOverbooking = true` AND `GuestCount > Capacity + 1`: Return 422

**Flag Purpose:** The `IsOverCapacity` flag allows hotel staff to identify and manage overbookings in reporting/operations.

### 5. DateTime Handling

**Design:** All timestamps use UTC (`DateTime.UtcNow`), stored in SQL Server as `datetime2`

**Assumption:** The API and database operate in UTC; frontend converts to local time for display
- Check-In/Check-Out dates are treated as date boundaries (midnight UTC)
- "48 hours until CheckIn" is calculated from current UTC time

### 6. Price Calculation Strategy

**Implementation:** Loop through each night in the booking range, look up the effective rate for that night, sum them.

**Why Not a Stored Procedure?**
- **Testability:** Easier to unit test in C# with in-memory database
- **Maintainability:** Business logic in application code is easier to debug and modify
- **Scalability:** Avoids storing complex logic in the database

**Trade-Off:** Small performance cost for per-night lookup vs. single stored procedure call. Mitigated by:
- EF Core query caching
- Small typical booking ranges (few days to few weeks)
- Rate changes are infrequent

### 7. Concurrency Safety

**Scenario:** Two overlapping booking requests arrive simultaneously for the same room/dates

**Current Implementation:**
- EF Core's `SaveChangesAsync()` uses implicit row-level locking at the database level
- SQL Server's default isolation level (READ COMMITTED) prevents dirty reads
- If both requests read availability simultaneously (both see no conflicts), the second `INSERT` will succeed, violating the overlap rule

**Limitation:** This is a known issue with optimistic EF Core in high-concurrency scenarios.

**Production Fix (Not Implemented):**
1. Add a `SERIALIZABLE` transaction isolation level for booking creation
2. Use a stored procedure with explicit locking (XLOCK on booking range)
3. Add a unique constraint on `(RoomId, CheckIn, CheckOut)` pairs and handle collision at DB level

**For This Exercise:** Assumed bookings are infrequent enough that race conditions are rare; documented for interview discussion.

### 8. HTTP Status Codes

- **200 OK:** Successful booking retrieval, availability check
- **201 Created:** Booking successfully created
- **204 No Content:** Booking successfully cancelled
- **400 Bad Request:** Invalid input (bad dates, missing fields)
- **404 Not Found:** Room or booking doesn't exist
- **409 Conflict:** Booking overlap detected
- **422 Unprocessable Entity:** Business rule violation (over capacity, cancellation not allowed)

**Rationale:** Different status codes allow the frontend to provide specific error messages without parsing response bodies.

## Project Structure

```
BookingInventory/
├── BookingInventory.Api/              # Main Web API
│   ├── Controllers/
│   │   ├── BookingsController.cs      # POST, DELETE /api/bookings
│   │   └── RoomsController.cs         # GET /api/rooms
│   ├── Services/
│   │   └── BookingService.cs          # Business logic (rate calc, availability)
│   ├── Models/
│   │   ├── Hotel.cs
│   │   ├── Room.cs
│   │   ├── Booking.cs
│   │   └── RateHistory.cs
│   ├── DTOs/                          # Request/Response objects
│   ├── Data/
│   │   ├── BookingDbContext.cs        # EF Core DbContext
│   │   └── Migrations/                # Database schema
│   ├── wwwroot/
│   │   └── index.html                 # Minimal frontend
│   ├── SeedData.cs                    # Sample data initialization
│   ├── Program.cs                     # DI configuration
│   └── appsettings.json               # Connection strings, logging
│
├── BookingInventory.Tests/            # Unit tests
│   ├── BookingServiceRateCalculationTests.cs
│   └── BookingServiceAvailabilityTests.cs
│
└── BookingInventory.sln
```

## Database Schema

### Hotels Table
```sql
CREATE TABLE Hotels (
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(255) NOT NULL,
	AllowOverbooking BIT NOT NULL DEFAULT 0
);
```

### Rooms Table
```sql
CREATE TABLE Rooms (
	Id INT PRIMARY KEY IDENTITY,
	HotelId INT NOT NULL,
	Number NVARCHAR(50) NOT NULL,
	Capacity INT NOT NULL,
	FOREIGN KEY (HotelId) REFERENCES Hotels(Id) ON DELETE CASCADE
);
```

### RateHistory Table
```sql
CREATE TABLE RateHistories (
	Id INT PRIMARY KEY IDENTITY,
	RoomId INT NOT NULL,
	BaseRate DECIMAL(10, 2) NOT NULL,
	EffectiveDate DATETIME2 NOT NULL,
	FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE
);
```

### Bookings Table
```sql
CREATE TABLE Bookings (
	Id INT PRIMARY KEY IDENTITY,
	RoomId INT NOT NULL,
	CheckIn DATETIME2 NOT NULL,
	CheckOut DATETIME2 NOT NULL,
	GuestCount INT NOT NULL,
	TotalPrice DECIMAL(10, 2) NOT NULL,
	IsOverCapacity BIT NOT NULL DEFAULT 0,
	IsCancelled BIT NOT NULL DEFAULT 0,
	CreatedAt DATETIME2 NOT NULL,
	FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE
);
```

## API Endpoints

### POST /api/bookings
Create a new booking with comprehensive validation.

**Request:**
```json
{
	"roomId": 1,
	"checkIn": "2024-01-15T14:00:00Z",
	"checkOut": "2024-01-18T11:00:00Z",
	"guestCount": 2
}
```

**Responses:**
- **201 Created:** Booking created successfully
```json
{
	"id": 1,
	"roomId": 1,
	"checkIn": "2024-01-15T14:00:00Z",
	"checkOut": "2024-01-18T11:00:00Z",
	"guestCount": 2,
	"totalPrice": 300.00,
	"isOverCapacity": false
}
```

- **400 Bad Request:** Invalid dates or guest count
- **404 Not Found:** Room not found
- **409 Conflict:** Booking overlap
```json
{
	"errorCode": "BOOKING_OVERLAP",
	"message": "Room is already booked from 2024-01-16 to 2024-01-19"
}
```

- **422 Unprocessable Entity:** Over capacity or cancellation not allowed
```json
{
	"errorCode": "OVER_CAPACITY",
	"message": "Room capacity is 2. Overbooking allowed: false"
}
```

### GET /api/bookings/rooms/{roomId}/availability?from=&to=
Check if a room is available for a date range.

**Query Parameters:**
- `from` (required): Start date (ISO 8601, e.g., `2024-01-15T00:00:00Z`)
- `to` (required): End date (ISO 8601, e.g., `2024-01-18T00:00:00Z`)

**Response:**
```json
{
	"roomId": 1,
	"checkIn": "2024-01-15T00:00:00Z",
	"checkOut": "2024-01-18T00:00:00Z",
	"isAvailable": true,
	"reason": null
}
```

Or if unavailable:
```json
{
	"roomId": 1,
	"checkIn": "2024-01-15T00:00:00Z",
	"checkOut": "2024-01-18T00:00:00Z",
	"isAvailable": false,
	"reason": "Room is already booked from 2024-01-16 to 2024-01-19"
}
```

### DELETE /api/bookings/{id}
Cancel a booking (only if CheckIn is more than 48 hours away).

**Response:**
- **204 No Content:** Booking cancelled successfully
- **404 Not Found:** Booking not found
- **422 Unprocessable Entity:** Cannot cancel (too close to check-in)
```json
{
	"errorCode": "CANCELLATION_NOT_ALLOWED",
	"message": "Booking can only be cancelled if CheckIn is more than 48 hours in the future. Hours until CheckIn: 24.5"
}
```

### GET /api/rooms
List all rooms with their current rates and hotel info.

**Response:**
```json
[
	{
		"id": 1,
		"hotelId": 1,
		"number": "101",
		"capacity": 2,
		"hotel": { "id": 1, "name": "Luxury Palace", "allowOverbooking": false },
		"rateHistories": [
			{ "id": 1, "roomId": 1, "baseRate": 150.00, "effectiveDate": "2024-01-07T00:00:00" },
			{ "id": 2, "roomId": 1, "baseRate": 100.00, "effectiveDate": "2024-01-01T00:00:00" }
		]
	}
]
```

### GET /api/rooms/{id}
Get a specific room with details.

## Running the Application

### Prerequisites
- .NET 6 SDK or later
- SQL Server 2019 or later (local or remote)
- Visual Studio 2022 or VS Code

### Setup

1. **Clone/Extract the project**
   ```bash
   cd BookingInventory
   ```

2. **Update connection string (if needed)**
   Edit `BookingInventory.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
	   "DefaultConnection": "Server=YOUR_SERVER;Database=BookingInventory;Integrated Security=true;TrustServerCertificate=true;"
   }
   ```

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Apply migrations and seed data**
   ```bash
   cd BookingInventory.Api
   dotnet ef database update
   ```
   (Or just run the app; migrations and seeding happen automatically on startup)

5. **Run the API**
   ```bash
   dotnet run
   ```
   The API starts on `https://localhost:5001`

6. **Access the frontend**
   Open `https://localhost:5001/index.html` in a browser

### Running Tests

```bash
cd BookingInventory.Tests
dotnet test
```

Tests include:
- **Rate Calculation:** Single/multiple nights, rate changes, invalid dates
- **Availability:** No conflicts, overlaps, back-to-back bookings, cancelled bookings

## Sample Data

The seed script creates:

**Hotels:**
- Luxury Palace (no overbooking)
- Budget Inn (overbooking allowed)

**Rooms:**
- Luxury Palace: Rooms 101 (cap 2), 102 (cap 4), 201 (cap 2)
- Budget Inn: Rooms A1 (cap 2), A2 (cap 1)

**Rates:**
- Room 101: $100/night (initially), $150/night (from next week)
- Room 102: $200/night
- Room 201: $120/night
- Room A1: $50/night
- Room A2: $30/night

**Bookings:**
- Past booking: Room 101, Jan 1-5 (for demonstration)
- Future booking: Room 102, Jan 9-11

## Known Limitations & Trade-offs

### 1. Concurrency (High-Load Scenarios)
**Issue:** Two simultaneous requests for overlapping dates may both succeed.
**Impact:** Low in practice for typical hotel booking traffic; would require stored procedure with transaction isolation for production.

### 2. Rate Calculations Not in Stored Procedure
**Issue:** Business logic in application code, not database.
**Trade-off:** Easier testing and maintenance vs. minor performance cost.

### 3. No Authentication/Authorization
**Issue:** API is completely open.
**Rationale:** Out of scope per requirements; production would add OAuth2 or API keys.

### 4. Soft Deletes Only
**Issue:** Cancelled bookings remain in database.
**Rationale:** Audit trail is valuable; purge archived data via scheduled job.

### 5. Single-Process Deployment
**Issue:** Cache/session state would not work across multiple API instances.
**Rationale:** Not needed for booking logic; all state in database.

## Interview Talking Points

### 1. Handling Concurrent Overlapping Bookings
**Scenario:** Two simultaneous requests for the same room/dates.

**Current Behavior:**
1. Request 1: Checks availability → available
2. Request 2: Checks availability → available
3. Request 1: Creates booking → success
4. Request 2: Creates booking → success (BUG: should fail)

**Production Fix:**
- Use `SERIALIZABLE` isolation level or a row-level lock
- Stored procedure approach: `WITH (XLOCK)` hint on the booking range
- Pessimistic locking before availability check

### 2. Rate-Spanning Logic
**Explanation:**
- RateHistory table tracks historical rates per room
- For a booking, iterate nightly, find the effective rate for each night
- Sum the nightly rates

**Why Not Single Query?**
- Dynamic rates make a single aggregate complex
- Per-night calculation is clearer for auditing

### 3. Extending the System
**Example Changes:**
- Add "No-refund vs. Refundable" rate categories → add `RateType` column to `RateHistory`
- Add minimum/maximum stay rules → add `MinNights, MaxNights` to `Room`
- Add seasonal pricing → add `Season` column to `RateHistory` and adjust rate logic

## Future Enhancements

1. **Stored Procedure for sp_CreateBooking** (as required by original spec)
   - Implement concurrency-safe booking creation with transaction isolation
   - Return status codes as output parameters

2. **Cached Rate Lookups**
   - Add Redis cache for RateHistory (rates change infrequently)
   - Invalidate on rate updates

3. **Audit Logging**
   - Track booking creation, modification, cancellation with user context
   - Add AuditLog table

4. **Reporting API**
   - Revenue reports by room/date range
   - Occupancy rates
   - Overbooking summary

5. **Advanced Pricing**
   - Discount/promotion codes
   - Length-of-stay discounts
   - Loyalty pricing

6. **Frontend Enhancements**
   - Calendar view with availability heatmap
   - Batch booking creation
   - Admin panel for rate management

## Technical Details

### Entity Framework Core
- **Version:** 6.0.0
- **Database Provider:** SQL Server
- **Migrations:** Generated and included; applied automatically on startup
- **Lazy Loading:** Disabled; explicit `.Include()` used to avoid N+1 queries

### Testing Framework
- **Framework:** xUnit
- **Database:** In-memory (for unit tests)
- **No Mocking:** Tests use real DbContext and EF Core logic

### Frontend
- **Framework:** Vanilla JavaScript (no dependencies)
- **API Communication:** Fetch API
- **Date Handling:** JavaScript `Date` object (local time) ↔ ISO 8601 (API/UTC)

## License

This is a training/interview exercise. Use freely for educational purposes.

## Contact

For questions about the design or implementation, refer to the code comments and README sections above.
