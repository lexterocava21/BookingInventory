# 🚀 Complete Setup & Deployment Guide

## Full Stack: Booking Inventory Management System

### Architecture Overview
```
┌─────────────────┐
│  React Frontend │ (Port 3000)
│  BookingInventory.Web
└────────┬────────┘
		 │ API Calls (http://localhost:5000)
		 ▼
┌─────────────────┐
│  .NET 8.0 API   │ (Port 5000)
│  BookingInventory.Api
└────────┬────────┘
		 │ Entity Framework Core
		 ▼
┌─────────────────┐
│  SQL Server     │
│  Database       │
└─────────────────┘
```

---

## 📋 Prerequisites

- **Node.js** 14+ with npm
- **.NET 8.0 SDK**
- **SQL Server** (Local or Express)
- **Git** (Optional)

---

## 🔧 Step-by-Step Setup

### Phase 1: Backend Setup (30 min)

#### 1.1 Open Solution in Visual Studio
```
File → Open → Solution
Select: BookingInventory.sln
```

#### 1.2 Verify Database Configuration
```
BookingInventory.Api/appsettings.json
ConnectionString: Server=.;Database=BookingInventory;...
```

#### 1.3 Build & Restore NuGet Packages
```powershell
cd C:\Users\Lexter Capule\Desktop\BookingManagementSys
dotnet restore BookingInventory.Api/BookingInventory.Api.csproj
```

#### 1.4 Apply Migrations
```powershell
dotnet ef database update --project BookingInventory.Api --startup-project BookingInventory.Api
```

#### 1.5 Verify Database Creation
- Open SQL Server Management Studio
- Connect to: `.` (localhost)
- Database: `BookingInventory`
- Tables: Hotels, Rooms, Bookings, RateHistories

#### 1.6 Run Backend Server
```powershell
cd BookingInventory.Api
dotnet run
```
✓ API should be running on `http://localhost:5000`
✓ Swagger UI: `http://localhost:5000/swagger/ui`

---

### Phase 2: Frontend Setup (20 min)

#### 2.1 Open Terminal (new PowerShell window)
```powershell
cd C:\Users\Lexter Capule\Desktop\BookingManagementSys\BookingInventory.Web
```

#### 2.2 Install Dependencies
```bash
npm install
```
Wait for completion (~2-3 minutes)

#### 2.3 Start React Development Server
```bash
npm start
```
✓ Browser opens to `http://localhost:3000`

---

## ✅ Verification Checklist

### Backend Verification
- [ ] API running on `http://localhost:5000`
- [ ] Swagger UI accessible at `http://localhost:5000/swagger/ui`
- [ ] Database tables created in SQL Server
- [ ] Seed data loaded (Hotels, Rooms, RateHistories)

### Frontend Verification
- [ ] React app running on `http://localhost:3000`
- [ ] Navigation header displays
- [ ] Can switch between tabs
- [ ] No console errors

### Integration Test
- [ ] Click "Rooms" tab → Rooms load from API ✓
- [ ] Click "New Booking" → Form displays ✓
- [ ] Click "Check Availability" → Checker displays ✓
- [ ] Click "My Bookings" → Bookings display ✓

---

## 🎯 Quick Start Commands

### Terminal 1: Backend
```powershell
cd C:\Users\Lexter Capule\Desktop\BookingManagementSys
dotnet run --project BookingInventory.Api
```

### Terminal 2: Frontend
```powershell
cd C:\Users\Lexter Capule\Desktop\BookingManagementSys\BookingInventory.Web
npm start
```

### Access Points
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000/api
- **Swagger Docs**: http://localhost:5000/swagger/ui
- **Database**: SQL Server (localhost)

---

## 📊 API Testing

### Test Endpoints with Swagger UI

1. Go to: `http://localhost:5000/swagger/ui`

2. **GET /api/rooms**
   - Retrieve all rooms
   - Expected: Array of room objects

3. **POST /api/bookings**
   ```json
   {
	 "roomId": 1,
	 "checkIn": "2024-09-15T14:00:00",
	 "checkOut": "2024-09-17T11:00:00",
	 "guestCount": 2
   }
   ```

4. **GET /api/bookings/availability**
   - Query params: `roomId`, `from`, `to`
   - Returns: availability status

---

## 🎨 Frontend Testing Workflow

### 1. View Rooms
- Click "Rooms" tab
- Browse available rooms
- Use filters (All, Available, High Capacity)

### 2. Create Booking
- Click "New Booking" tab
- Fill form: Room ID, Check-in, Check-out, Guests
- Click "Create Booking"
- Verify success notification

### 3. Check Availability
- Click "Check Availability" tab
- Enter Room ID and dates
- Click "Check Availability"
- See result (Available/Unavailable)

### 4. Manage Bookings
- Click "My Bookings" tab
- View your bookings
- Filter by status
- Cancel if eligible (48h+ before check-in)

---

## 🐛 Common Issues & Solutions

### Issue: API won't start
```
Error: Cannot bind to port 5000
```
**Solution**:
```powershell
# Check what's using port 5000
netstat -ano | findstr :5000

# Kill process if needed
taskkill /PID <PID> /F

# Or change port in launchSettings.json
```

### Issue: React can't connect to API
```
Error: Failed to fetch from http://localhost:5000
CORS error
```
**Solution**:
- Ensure backend is running
- Verify CORS is enabled in Program.cs
- Check firewall settings

### Issue: Database connection fails
```
Error: Cannot open database 'BookingInventory'
```
**Solution**:
```powershell
# Verify SQL Server is running
Get-Service "MSSQLSERVER" | Start-Service

# Recreate database
dotnet ef database drop -p BookingInventory.Api
dotnet ef database update -p BookingInventory.Api
```

### Issue: npm packages won't install
```
Error: ERESOLVE unable to resolve dependency tree
```
**Solution**:
```bash
npm install --legacy-peer-deps
```

---

## 📈 Performance Optimization

### Backend Optimization
```csharp
// Program.cs
builder.Services.AddControllers()
	.ConfigureApiBehaviorOptions(options =>
	{
		options.SuppressConsumesConstraintForFormFileBindingOnMultipartFormData = true;
	});

// Add caching
builder.Services.AddMemoryCache();
```

### Frontend Optimization
```javascript
// Use React.memo for expensive components
const RoomsList = React.memo(({ rooms }) => {
  return ...
});

// Implement lazy loading
const BookingForm = lazy(() => import('./BookingForm'));
```

---

## 🚀 Deployment Options

### Option 1: Azure App Service
```powershell
# Publish backend
dotnet publish -c Release

# Deploy using Azure CLI
az webapp up --name my-booking-api --runtime dotnet:8.0
```

### Option 2: Docker
```dockerfile
# Dockerfile (Backend)
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/BookingInventory.Api.dll"]
```

### Option 3: Local IIS
```powershell
# Publish backend
dotnet publish -c Release -o "C:\inetpub\wwwroot\booking-api"

# Build React production
cd BookingInventory.Web
npm run build

# Copy to IIS
Copy-Item -Path "build\*" -Destination "C:\inetpub\wwwroot\booking-web" -Recurse
```

---

## 📝 Database Schema

### Hotels Table
```sql
CREATE TABLE Hotels (
	Id INT PRIMARY KEY IDENTITY(1,1),
	Name NVARCHAR(255) NOT NULL,
	AllowOverbooking BIT NOT NULL
);
```

### Rooms Table
```sql
CREATE TABLE Rooms (
	Id INT PRIMARY KEY IDENTITY(1,1),
	HotelId INT NOT NULL,
	Number NVARCHAR(50) NOT NULL,
	Capacity INT NOT NULL,
	FOREIGN KEY (HotelId) REFERENCES Hotels(Id)
);
```

### Bookings Table
```sql
CREATE TABLE Bookings (
	Id INT PRIMARY KEY IDENTITY(1,1),
	RoomId INT NOT NULL,
	CheckIn DATETIME2 NOT NULL,
	CheckOut DATETIME2 NOT NULL,
	GuestCount INT NOT NULL,
	TotalPrice DECIMAL(10,2) NOT NULL,
	IsOverCapacity BIT NOT NULL,
	IsCancelled BIT NOT NULL,
	CreatedAt DATETIME2 NOT NULL,
	FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
);
```

### RateHistories Table
```sql
CREATE TABLE RateHistories (
	Id INT PRIMARY KEY IDENTITY(1,1),
	RoomId INT NOT NULL,
	BaseRate DECIMAL(10,2) NOT NULL,
	EffectiveDate DATETIME2 NOT NULL,
	FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
);
```

---

## 🔐 Security Checklist

- [ ] CORS properly configured for allowed origins
- [ ] Input validation on all endpoints
- [ ] SQL injection prevention (using EF Core)
- [ ] Authentication/Authorization (if needed)
- [ ] HTTPS in production
- [ ] Secure database connection strings
- [ ] API rate limiting
- [ ] Request validation & sanitization

---

## 📞 Support & Troubleshooting

### Useful Commands
```powershell
# Build solution
dotnet build

# Run tests
dotnet test

# Format code
dotnet format

# Check dependencies
dotnet list package --outdated

# Create migration
dotnet ef migrations add MigrationName --project BookingInventory.Api

# Update database
dotnet ef database update --project BookingInventory.Api
```

### Useful Resources
- [.NET 8 Documentation](https://learn.microsoft.com/dotnet/)
- [React Documentation](https://react.dev)
- [Bootstrap Documentation](https://getbootstrap.com)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

---

## 🎉 Success!

Your Booking Inventory Management System is now:
- ✅ Database configured with real data
- ✅ Backend API running with full CRUD operations
- ✅ Frontend React app with modern UI
- ✅ Real-time API integration
- ✅ Complete booking management system

**Ready for production or further development!**

---

## 📞 Quick Reference

| Component | Port | URL |
|-----------|------|-----|
| Frontend | 3000 | http://localhost:3000 |
| Backend | 5000 | http://localhost:5000 |
| API | 5000 | http://localhost:5000/api |
| Swagger | 5000 | http://localhost:5000/swagger/ui |
| Database | 1433 | localhost (SQL Server) |

---

**Last Updated**: September 2024
**Version**: 1.0.0