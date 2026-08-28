# 🚀 Quick Reference Card

## 📱 Start Development in 2 Minutes

### Step 1: Terminal 1 - Backend
```powershell
cd C:\Users\Lexter Capule\Desktop\BookingManagementSys
dotnet run --project BookingInventory.Api
```
✓ Runs on: http://localhost:5000

### Step 2: Terminal 2 - Frontend  
```powershell
cd BookingInventory.Web
npm install  # (First time only)
npm start
```
✓ Runs on: http://localhost:3000

---

## 🎯 Access Points

| Service | URL | Purpose |
|---------|-----|---------|
| Frontend | http://localhost:3000 | React app |
| Backend API | http://localhost:5000/api | API endpoints |
| Swagger Docs | http://localhost:5000/swagger/ui | API documentation |
| Database | localhost:1433 | SQL Server |

---

## 📝 Available NPM Commands

```bash
npm start          # Start dev server
npm run build      # Build for production
npm test           # Run tests
npm run eject      # Eject from Create React App (⚠️ irreversible)
npm install        # Install dependencies
```

---

## 🖥️ Available dotnet Commands

```bash
dotnet run                              # Run backend
dotnet build                            # Build solution
dotnet test                             # Run tests
dotnet ef migrations list               # Show migrations
dotnet ef migrations add MigrationName  # Create migration
dotnet ef database update               # Apply migrations
dotnet ef database drop                 # Drop database
```

---

## 📋 API Endpoints Quick List

```
GET    /api/rooms
GET    /api/rooms/{id}
POST   /api/bookings
POST   /api/bookings/{id}/cancel
GET    /api/bookings/availability?roomId=1&from=DATE&to=DATE
```

---

## 🎨 React Components

| Component | Purpose | File |
|-----------|---------|------|
| App | Main container | src/App.js |
| Navigation | Header | components/Navigation.js |
| RoomsList | Browse rooms | components/RoomsList.js |
| BookingForm | Create booking | components/BookingForm.js |
| AvailabilityChecker | Check dates | components/AvailabilityChecker.js |
| BookingsList | Manage bookings | components/BookingsList.js |
| Toast | Notifications | components/Toast.js |

---

## 🗂️ File Locations

```
Frontend:
  - Config: BookingInventory.Web/package.json
  - Styles: BookingInventory.Web/src/App.css
  - Components: BookingInventory.Web/src/components/

Backend:
  - API Config: BookingInventory.Api/Program.cs
  - Database: BookingInventory.Api/Data/BookingDbContext.cs
  - Controllers: BookingInventory.Api/Controllers/
  - Settings: BookingInventory.Api/appsettings.json

Database:
  - Connection: Server=.;Database=BookingInventory
  - Migrations: BookingInventory.Api/Migrations/
```

---

## 🐛 Common Issues & Fixes

### Port Already in Use
```powershell
# Find what's using the port
netstat -ano | findstr :5000

# Kill the process
taskkill /PID <PID> /F

# Or use different port
npm start -- --port 3001
```

### Can't Connect to API
```
✓ Check backend is running
✓ Verify port is 5000
✓ Check firewall
✓ Ensure CORS is enabled
```

### Database Issues
```powershell
# Recreate database
dotnet ef database drop -f
dotnet ef database update
```

---

## 🎯 Feature Quick Access

| Feature | Tab | Action |
|---------|-----|--------|
| View Rooms | Rooms | Click refresh to load |
| Create Booking | New Booking | Fill form & submit |
| Check Dates | Check Availability | Enter room & dates |
| Manage Bookings | My Bookings | View & cancel |

---

## 🔍 Testing Workflow

1. **Rooms Tab** → Verify rooms load
2. **Check Availability** → Enter Room 1, dates, check
3. **New Booking** → Create booking for available slot
4. **My Bookings** → See created booking
5. **Backend Swagger** → Test API directly

---

## 📊 Database Quick Check

```sql
-- Connect to SQL Server (localhost)
-- Database: BookingInventory

SELECT COUNT(*) FROM Hotels;
SELECT COUNT(*) FROM Rooms;
SELECT COUNT(*) FROM Bookings;
SELECT COUNT(*) FROM RateHistories;
```

---

## 💡 Tips & Tricks

### Quick Clean
```powershell
# Clean backend
dotnet clean

# Clean frontend
rm -r node_modules build
npm install
```

### Debug Backend
```csharp
System.Diagnostics.Debug.WriteLine("Your message");
// Check Output window in Visual Studio
```

### Debug Frontend
```javascript
console.log('Your message');
// Check browser console (F12)
```

### API Testing Tool
```
Use Swagger UI: http://localhost:5000/swagger/ui
Great for testing without frontend
```

---

## 🚀 Deployment Checklist

- [ ] Backend builds successfully (`dotnet build`)
- [ ] Frontend builds successfully (`npm run build`)
- [ ] Database migrations applied
- [ ] API endpoints tested
- [ ] UI components responsive
- [ ] No console errors
- [ ] Environment variables configured
- [ ] Database backup created

---

## 📚 Documentation Files

| File | Location | Content |
|------|----------|---------|
| README.md | BookingInventory.Web/ | Feature overview |
| SETUP_GUIDE.md | BookingInventory.Web/ | Complete setup |
| UI_FEATURES.md | BookingInventory.Web/ | Design system |
| PROJECT_SUMMARY.md | BookingInventory.Web/ | Full overview |
| This File | BookingInventory.Web/ | Quick reference |

---

## 🎓 Learning Path

1. **First Time?** → Read SETUP_GUIDE.md
2. **Understand Features?** → Read PROJECT_SUMMARY.md  
3. **Modify UI?** → Read UI_FEATURES.md
4. **Need Help?** → Check this file or SETUP_GUIDE.md

---

## ✅ Success Indicators

- ✅ Frontend loads at http://localhost:3000
- ✅ Backend runs at http://localhost:5000
- ✅ Rooms display in browser
- ✅ Can create booking
- ✅ Notifications appear
- ✅ No red errors in console

---

## 📞 Quick Help

**API won't start?**
```
→ Check port 5000 is free
→ Verify SQL Server is running
→ Check appsettings.json connection string
```

**Frontend won't load?**
```
→ Check npm install completed
→ Verify node_modules folder exists
→ Try: npm start -- --port 3001
```

**Database issues?**
```
→ Open SQL Server Management Studio
→ Connect to: localhost
→ Find BookingInventory database
→ Check tables exist
```

---

## 🔗 Useful Links

- React Docs: https://react.dev
- Bootstrap: https://getbootstrap.com
- .NET Docs: https://learn.microsoft.com/dotnet/
- Axios: https://axios-http.com

---

## 🎉 You're Ready!

Everything is set up and working. 
Start coding and building awesome features!

**Questions?** Check the documentation files or browser console for errors.

---

**Last Updated**: September 2024
**Quick Reference v1.0**