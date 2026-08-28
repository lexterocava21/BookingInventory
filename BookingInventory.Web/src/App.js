import React from 'react';
import './App.css';
import Navigation from './components/Navigation';
import RoomsList from './components/RoomsList';
import BookingForm from './components/BookingForm';
import AvailabilityChecker from './components/AvailabilityChecker';
import BookingsList from './components/BookingsList';
import Toast from './components/Toast';

function App() {
  const [activeTab, setActiveTab] = React.useState('rooms');
  const [toast, setToast] = React.useState(null);

  const showToast = (message, type = 'info') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 4000);
  };

  return (
    <div className="App">
      <Navigation />
      {toast && <Toast message={toast.message} type={toast.type} />}

      <div className="container-fluid mt-4 pb-5">
        <div className="row">
          <div className="col-md-12">
            <div className="nav-tabs-wrapper mb-4">
              <ul className="nav nav-tabs nav-pills">
                <li className="nav-item">
                  <button 
                    className={`nav-link ${activeTab === 'rooms' ? 'active' : ''}`}
                    onClick={() => setActiveTab('rooms')}
                  >
                    <i className="bi bi-door-closed"></i> Rooms
                  </button>
                </li>
                <li className="nav-item">
                  <button 
                    className={`nav-link ${activeTab === 'booking' ? 'active' : ''}`}
                    onClick={() => setActiveTab('booking')}
                  >
                    <i className="bi bi-plus-circle"></i> New Booking
                  </button>
                </li>
                <li className="nav-item">
                  <button 
                    className={`nav-link ${activeTab === 'availability' ? 'active' : ''}`}
                    onClick={() => setActiveTab('availability')}
                  >
                    <i className="bi bi-search"></i> Check Availability
                  </button>
                </li>
                <li className="nav-item">
                  <button 
                    className={`nav-link ${activeTab === 'bookings' ? 'active' : ''}`}
                    onClick={() => setActiveTab('bookings')}
                  >
                    <i className="bi bi-list-check"></i> My Bookings
                  </button>
                </li>
              </ul>
            </div>

            <div className="tab-content">
              {activeTab === 'rooms' && <RoomsList onShowToast={showToast} />}
              {activeTab === 'booking' && <BookingForm onShowToast={showToast} />}
              {activeTab === 'availability' && <AvailabilityChecker onShowToast={showToast} />}
              {activeTab === 'bookings' && <BookingsList onShowToast={showToast} />}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
