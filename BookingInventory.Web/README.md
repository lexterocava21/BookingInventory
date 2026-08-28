# Booking Inventory System - React Frontend

## 🌟 Features

### ✨ Enhanced Features Included:

1. **Rooms Management**
   - Browse all available rooms with detailed information
   - Filter rooms by availability and capacity
   - View room rates and pricing history
   - Real-time room capacity display

2. **Booking Management**
   - Create new bookings with validation
   - Advanced date/time selection
   - Guest count management
   - Real-time availability feedback
   - Booking instructions guide

3. **Availability Checking**
   - Check room availability for specific dates
   - Visual status indicators (available/unavailable)
   - Detailed booking conflict information
   - Helpful tips and guidelines

4. **Booking History**
   - View all your bookings
   - Filter by status (active, cancelled, upcoming)
   - Cancel bookings (48-hour advance notice required)
   - View detailed booking information
   - Real-time booking status

5. **User Experience**
   - Modern gradient UI with animations
   - Toast notifications for all actions
   - Form validation with error messages
   - Loading states and spinners
   - Responsive design for all devices
   - Professional color scheme and typography

## 📋 Project Structure

```
BookingInventory.Web/
├── public/
│   └── index.html
├── src/
│   ├── components/
│   │   ├── Navigation.js (Enhanced header)
│   │   ├── RoomsList.js (Room browsing & filtering)
│   │   ├── BookingForm.js (Create bookings)
│   │   ├── AvailabilityChecker.js (Check availability)
│   │   ├── BookingsList.js (View & manage bookings)
│   │   └── Toast.js (Notifications)
│   ├── App.js (Main app with tab navigation)
│   ├── App.css (Global enhanced styles)
│   ├── index.js (React entry point)
│   └── index.css (Base styles)
├── package.json (Dependencies)
└── README.md (This file)
```

## 🚀 Quick Start

### Prerequisites
- Node.js 14+ and npm installed
- .NET 8.0 backend running on port 5000
- SQL Server database configured

### Step 1: Install Dependencies
```bash
cd BookingInventory.Web
npm install
```

### Step 2: Start Backend API
In another terminal:
```bash
cd BookingInventory.Api
dotnet run
```
API will run on: `http://localhost:5000`

### Step 3: Start React Frontend
```bash
cd BookingInventory.Web
npm start
```
React app will open at: `http://localhost:3000`

## 📦 Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| React | 18.2.0 | UI library |
| React-DOM | 18.2.0 | React renderer |
| Bootstrap | 5.3.0 | UI framework |
| Axios | 1.6.0 | HTTP client |
| React-Scripts | 5.0.1 | Build tools |

## 🎨 UI Features

### Navigation
- Responsive sticky header
- Tab-based interface with icons
- Real-time API status indicator

### Components
- **Cards**: Elevated design with hover effects
- **Buttons**: Gradient backgrounds with smooth transitions
- **Forms**: Validated input fields with error messages
- **Alerts**: Color-coded notifications with animations
- **Badges**: Status indicators for bookings and rooms
- **Modals**: Detailed booking information popup

### Responsive Design
- Mobile-first approach
- Grid layout for rooms (auto-responsive)
- Touch-friendly button sizes
- Optimized for tablets and desktops

## 🔌 API Integration

### Base URL
```javascript
const API_URL = 'http://localhost:5000/api';
```

### Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/rooms` | GET | List all rooms |
| `/api/bookings` | POST | Create booking |
| `/api/bookings/availability` | GET | Check availability |
| `/api/bookings/{id}/cancel` | POST | Cancel booking |

## 🛠️ Environment Configuration

### Create `.env` file in root:
```env
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_API_TIMEOUT=30000
```

### Use in components:
```javascript
const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';
```

## 🎯 Component Guide

### App.js
- Main application container
- Tab state management
- Toast notification system

### Navigation.js
- Header with branding
- API status display
- Logo and description

### RoomsList.js
- Fetches rooms from API
- Filter by availability/capacity
- Grid display with card layout
- Real-time refresh

### BookingForm.js
- Form validation
- Date/time selection
- Guest count input
- Success/error handling

### AvailabilityChecker.js
- Room availability lookup
- Date range selection
- Visual status display
- Detailed information cards

### BookingsList.js
- Browse user's bookings
- Filter by status
- Cancel bookings (with 48h notice)
- Detailed booking modal

### Toast.js
- Non-intrusive notifications
- Auto-dismiss after 4 seconds
- Color-coded by type (success/danger/info/warning)

## 🔒 Features & Validation

### Form Validation
- Required field checking
- Date range validation (check-out > check-in)
- Guest count validation (must be > 0)
- Real-time error display

### API Error Handling
- Graceful error messages
- Network failure recovery
- Loading states
- Retry functionality

### Cancellation Rules
- 48-hour advance notice required
- Cannot cancel completed bookings
- Cannot cancel already cancelled bookings

## 📱 Responsive Breakpoints

```css
/* Desktop: >= 1200px */
/* Tablet: 768px - 1199px */
/* Mobile: < 768px */
```

## 🚀 Production Build

### Build for Production
```bash
npm run build
```

Creates optimized build in `build/` folder:
- Minified JavaScript
- Optimized images
- Source maps removed
- Tree-shaking enabled

### Deploy to Static Server
```bash
# Copy contents of build/ folder to your web server
cp -r build/* /var/www/html/
```

## 🔧 Troubleshooting

### Issue: Cannot Connect to API
**Solution**: 
- Verify backend is running: `dotnet run`
- Check port is 5000
- Ensure CORS is enabled in Program.cs

### Issue: Port 3000 Already in Use
**Solution**:
```bash
npm start -- --port 3001
```

### Issue: CORS Errors
**Solution**: Ensure Program.cs has:
```csharp
app.UseCors("AllowAll");
```

### Issue: Form not submitting
**Solution**:
- Check browser console for errors
- Verify API endpoint is correct
- Check request payload format

## 📊 Performance Tips

1. **Lazy Loading**: Components load only when needed
2. **Memoization**: Reduce re-renders with React.memo
3. **Code Splitting**: Use dynamic imports for large components
4. **Image Optimization**: Use appropriate image formats
5. **Caching**: Implement API response caching

## 🧪 Testing

### Run Tests
```bash
npm test
```

### Example Test Structure
```javascript
import { render, screen } from '@testing-library/react';
import RoomsList from './RoomsList';

test('renders rooms list', () => {
  render(<RoomsList />);
  expect(screen.getByText(/Available Rooms/i)).toBeInTheDocument();
});
```

## 📚 Additional Resources

- [React Documentation](https://react.dev)
- [Bootstrap Documentation](https://getbootstrap.com)
- [Axios Documentation](https://axios-http.com)
- [Create React App Guide](https://create-react-app.dev)

## 🤝 Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see LICENSE file for details.

## 👨‍💻 Developer Notes

### Code Style
- Use functional components with hooks
- Follow React best practices
- Use meaningful variable names
- Add comments for complex logic

### Naming Conventions
- Components: PascalCase
- Variables/functions: camelCase
- CSS classes: kebab-case
- API responses: camelCase

### Debugging
Enable React Developer Tools browser extension for better debugging.

---

**Created with ❤️ for efficient hotel room management**

Last Updated: 2024
Version: 1.0.0
