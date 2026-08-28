import React, { useState } from 'react';
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5025/api';

const AvailabilityChecker = ({ onShowToast }) => {
  const [formData, setFormData] = useState({
    roomId: '',
    from: '',
    to: '',
  });
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [searchPerformed, setSearchPerformed] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.roomId || !formData.from || !formData.to) {
      onShowToast?.('Please fill in all fields', 'warning');
      return;
    }

    try {
      setLoading(true);
      const response = await axios.get(`${API_URL}/bookings/availability`, {
        params: {
          roomId: parseInt(formData.roomId),
          from: formData.from,
          to: formData.to,
        }
      });

      setResult(response.data);
      setSearchPerformed(true);

      if (response.data.isAvailable) {
        onShowToast?.('Room is available for your dates!', 'success');
      } else {
        onShowToast?.('Room is not available for selected dates', 'warning');
      }
    } catch (err) {
      onShowToast?.(err.response?.data?.message || 'Failed to check availability', 'danger');
      setResult(null);
      setSearchPerformed(true);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const today = new Date().toISOString().slice(0, 16);

  return (
    <div>
      <h2>Check Room Availability</h2>
      <p className="text-muted">Verify if a room is available for your desired dates</p>

      <div className="row">
        <div className="col-md-8">
          <div className="card">
            <div className="card-header">
              <h5 className="mb-0">Availability Search</h5>
            </div>
            <div className="card-body">
              <form onSubmit={handleSubmit}>
                <div className="row">
                  <div className="col-md-12 mb-3">
                    <label className="form-label">Room ID *</label>
                    <input
                      type="number"
                      className="form-control"
                      name="roomId"
                      value={formData.roomId}
                      onChange={handleChange}
                      placeholder="Enter room ID to check"
                      min="1"
                    />
                  </div>
                </div>

                <div className="row">
                  <div className="col-md-6 mb-3">
                    <label className="form-label">From Date & Time *</label>
                    <input
                      type="datetime-local"
                      className="form-control"
                      name="from"
                      value={formData.from}
                      onChange={handleChange}
                      min={today}
                    />
                  </div>
                  <div className="col-md-6 mb-3">
                    <label className="form-label">To Date & Time *</label>
                    <input
                      type="datetime-local"
                      className="form-control"
                      name="to"
                      value={formData.to}
                      onChange={handleChange}
                      min={today}
                    />
                  </div>
                </div>

                <div className="d-flex gap-2">
                  <button 
                    type="submit" 
                    className="btn btn-primary btn-lg flex-grow-1"
                    disabled={loading}
                  >
                    {loading ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2"></span>
                        Checking...
                      </>
                    ) : (
                      <>
                        <i className="bi bi-search"></i> Check Availability
                      </>
                    )}
                  </button>
                  <button 
                    type="reset" 
                    className="btn btn-outline-primary btn-lg"
                    onClick={() => {
                      setFormData({ roomId: '', from: '', to: '' });
                      setResult(null);
                      setSearchPerformed(false);
                    }}
                  >
                    Clear
                  </button>
                </div>
              </form>
            </div>
          </div>

          {searchPerformed && result && (
            <div className="mt-4">
              {result.isAvailable ? (
                <div className="alert alert-success alert-lg">
                  <div className="d-flex align-items-center">
                    <div className="flex-grow-1">
                      <h5 className="mb-2">✓ Room Available</h5>
                      <p className="mb-0">
                        The room is available for the selected dates. You can proceed with booking.
                      </p>
                    </div>
                    <div className="ms-3">
                      <span style={{ fontSize: '2rem' }}>✓</span>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="alert alert-warning alert-lg">
                  <div className="d-flex align-items-center">
                    <div className="flex-grow-1">
                      <h5 className="mb-2">✗ Room Not Available</h5>
                      <p className="mb-1">
                        <strong>Reason:</strong> {result.reason}
                      </p>
                      <p className="mb-0 text-muted small">
                        Please select different dates or another room.
                      </p>
                    </div>
                    <div className="ms-3">
                      <span style={{ fontSize: '2rem' }}>✕</span>
                    </div>
                  </div>
                </div>
              )}

              <div className="card mt-3">
                <div className="card-body">
                  <h6 className="card-title">Search Details</h6>
                  <div className="row">
                    <div className="col-md-4">
                      <small className="text-muted d-block">Room ID</small>
                      <strong>#{result.roomId || formData.roomId}</strong>
                    </div>
                    <div className="col-md-4">
                      <small className="text-muted d-block">Check-In</small>
                      <strong>{new Date(result.checkIn || formData.from).toLocaleString()}</strong>
                    </div>
                    <div className="col-md-4">
                      <small className="text-muted d-block">Check-Out</small>
                      <strong>{new Date(result.checkOut || formData.to).toLocaleString()}</strong>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="col-md-4">
          <div className="card bg-light">
            <div className="card-header bg-primary text-white">
              <h5 className="mb-0">💡 Tips</h5>
            </div>
            <div className="card-body small">
              <ul className="list-unstyled">
                <li className="mb-2">
                  ✓ Check availability before making a booking
                </li>
                <li className="mb-2">
                  ✓ Make sure your dates don't overlap with existing bookings
                </li>
                <li className="mb-2">
                  ✓ Cancellations must be made 48 hours in advance
                </li>
                <li className="mb-2">
                  ✓ Check-in and check-out times are important
                </li>
                <li>
                  ✓ See the "My Bookings" tab to manage your reservations
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AvailabilityChecker;
