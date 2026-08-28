import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5025/api';

const BookingsList = ({ onShowToast }) => {
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');
  const [selectedBooking, setSelectedBooking] = useState(null);
  const [showModal, setShowModal] = useState(false);

  useEffect(() => {
    fetchBookings();
  }, []);

  const fetchBookings = async () => {
    try {
      setLoading(true);
      const response = await axios.get(`${API_URL}/bookings`);
      setBookings(response.data);
      onShowToast?.(`Loaded ${response.data.length} bookings successfully`, 'success');
    } catch (err) {
      onShowToast?.('Error loading bookings', 'danger');
      console.error(err);
      setBookings([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelBooking = async (bookingId) => {
    if (!window.confirm('Are you sure you want to cancel this booking?')) {
      return;
    }

    try {
      await axios.post(`${API_URL}/bookings/${bookingId}/cancel`);
      onShowToast?.('Booking cancelled successfully', 'success');
      setShowModal(false);
      fetchBookings();
    } catch (err) {
      onShowToast?.(err.response?.data?.message || 'Failed to cancel booking', 'danger');
    }
  };

  const handleDeleteBooking = async (bookingId) => {
    if (!window.confirm('This booking will be marked as cancelled instead of being hard-deleted. Continue?')) {
      return;
    }

    try {
      await axios.delete(`${API_URL}/bookings/${bookingId}`);
      onShowToast?.('Booking marked as cancelled', 'success');
      setShowModal(false);
      fetchBookings();
    } catch (err) {
      onShowToast?.(err.response?.data?.message || 'Failed to cancel booking', 'danger');
    }
  };

  const getFilteredBookings = () => {
    if (filter === 'all') return bookings;
    if (filter === 'active') return bookings.filter(b => !b.isCancelled && !b.isCompleted);
    if (filter === 'cancelled') return bookings.filter(b => b.isCancelled || b.isCompleted);
    return bookings;
  };

  const getBookingStatus = (booking) => {
    if (booking.isCancelled) return { text: 'Cancelled', type: 'badge-cancelled' };
    if (booking.isCompleted) return { text: 'Completed', type: 'badge-unavailable' };
    const now = new Date();
    const checkIn = new Date(booking.checkIn);
    const checkOut = new Date(booking.checkOut);

    if (now < checkIn) return { text: 'Upcoming', type: 'badge-available' };
    if (now > checkOut) return { text: 'Completed', type: 'badge-unavailable' };
    return { text: 'Active', type: 'badge-available' };
  };

  const canCancelBooking = (booking) => {
    if (booking.isCancelled) return false;
    const checkIn = new Date(booking.checkIn);
    const now = new Date();
    const hoursUntilCheckIn = (checkIn - now) / (1000 * 60 * 60);
    return hoursUntilCheckIn > 48;
  };

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner-border" role="status"></div>
        <div className="loading-text">Loading bookings...</div>
      </div>
    );
  }

  const filteredBookings = getFilteredBookings();

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2>My Bookings</h2>
          <p className="text-muted">Manage your hotel reservations</p>
        </div>
        <button className="btn btn-primary" onClick={fetchBookings}>
          <i className="bi bi-arrow-clockwise"></i> Refresh
        </button>
      </div>

      {/* Filter Buttons */}
      <div className="mb-4 d-flex gap-2 flex-wrap">
        <button
          className={`btn btn-sm ${filter === 'all' ? 'btn-primary' : 'btn-outline-primary'}`}
          onClick={() => setFilter('all')}
        >
          All ({bookings.length})
        </button>
        <button
          className={`btn btn-sm ${filter === 'active' ? 'btn-success' : 'btn-outline-primary'}`}
          onClick={() => setFilter('active')}
        >
          Active ({bookings.filter(b => !b.isCancelled && !b.isCompleted).length})
        </button>
        <button
          className={`btn btn-sm ${filter === 'cancelled' ? 'btn-danger' : 'btn-outline-primary'}`}
          onClick={() => setFilter('cancelled')}
        >
          Cancelled ({bookings.filter(b => b.isCancelled || b.isCompleted).length})
        </button>
      </div>

      {bookings.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">📭</div>
          <div className="empty-state-title">No Bookings Yet</div>
          <div className="empty-state-text">
            You don't have any bookings yet. Start by creating one!
          </div>
          <p className="text-muted small mt-3">
            💡 Tip: To view your bookings, first create a booking using the "New Booking" tab.
          </p>
        </div>
      ) : (
        <div className="row">
          {filteredBookings.map(booking => {
            const status = getBookingStatus(booking);
            const canCancel = canCancelBooking(booking);

            return (
              <div key={booking.id} className="col-md-6 mb-3">
                <div className="card">
                  <div className="card-header">
                    <div className="d-flex justify-content-between align-items-center">
                      <h5 className="mb-0">Booking #{booking.id}</h5>
                      <span className={`badge ${status.type}`}>
                        {status.text}
                      </span>
                    </div>
                  </div>
                  <div className="card-body">
                    <div className="row mb-2">
                      <div className="col-6">
                        <small className="text-muted d-block">Room ID</small>
                        <strong>#{booking.roomId}</strong>
                      </div>
                      <div className="col-6">
                        <small className="text-muted d-block">Guests</small>
                        <strong>{booking.guestCount} person{booking.guestCount > 1 ? 's' : ''}</strong>
                      </div>
                    </div>
                    <div className="row mb-2">
                      <div className="col-6">
                        <small className="text-muted d-block">Check-In</small>
                        <strong>{new Date(booking.checkIn).toLocaleDateString()}</strong>
                        <br />
                        <small>{new Date(booking.checkIn).toLocaleTimeString()}</small>
                      </div>
                      <div className="col-6">
                        <small className="text-muted d-block">Check-Out</small>
                        <strong>{new Date(booking.checkOut).toLocaleDateString()}</strong>
                        <br />
                        <small>{new Date(booking.checkOut).toLocaleTimeString()}</small>
                      </div>
                    </div>
                    <div className="alert alert-info py-2 px-3 mb-2">
                      <small className="d-block text-muted mb-1">Total Price</small>
                      <strong className="text-dark">
                        ${parseFloat(booking.totalPrice).toFixed(2)}
                      </strong>
                    </div>
                    {booking.isOverCapacity && (
                      <div className="alert alert-warning py-2 px-3 mb-2">
                        <small>⚠️ This booking exceeds room capacity</small>
                      </div>
                    )}
                    <div className="d-flex gap-2">
                      <button
                        className="btn btn-sm btn-outline-primary flex-grow-1"
                        onClick={() => {
                          setSelectedBooking(booking);
                          setShowModal(true);
                        }}
                      >
                        View Details
                      </button>
                      {canCancel && (
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => handleCancelBooking(booking.id)}
                        >
                          Cancel
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Details Modal */}
      {showModal && selectedBooking && (
        <div className="modal d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Booking Details #{selectedBooking.id}</h5>
                <button type="button" className="btn-close" onClick={() => setShowModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="row mb-3">
                  <div className="col-md-6">
                    <small className="text-muted d-block">Room ID</small>
                    <strong className="h6">#{selectedBooking.roomId}</strong>
                  </div>
                  <div className="col-md-6">
                    <small className="text-muted d-block">Guest Count</small>
                    <strong className="h6">{selectedBooking.guestCount}</strong>
                  </div>
                </div>
                <div className="row mb-3">
                  <div className="col-md-6">
                    <small className="text-muted d-block">Check-In</small>
                    <strong className="h6">{new Date(selectedBooking.checkIn).toLocaleString()}</strong>
                  </div>
                  <div className="col-md-6">
                    <small className="text-muted d-block">Check-Out</small>
                    <strong className="h6">{new Date(selectedBooking.checkOut).toLocaleString()}</strong>
                  </div>
                </div>
                <div className="row mb-3">
                  <div className="col-md-6">
                    <small className="text-muted d-block">Total Price</small>
                    <strong className="h6">${parseFloat(selectedBooking.totalPrice).toFixed(2)}</strong>
                  </div>
                  <div className="col-md-6">
                    <small className="text-muted d-block">Status</small>
                    <span className={`badge ${getBookingStatus(selectedBooking).type}`}>
                      {getBookingStatus(selectedBooking).text}
                    </span>
                  </div>
                </div>
                {selectedBooking.isOverCapacity && (
                  <div className="alert alert-warning">
                    ⚠️ This booking exceeds the room's capacity
                  </div>
                )}
                {selectedBooking.isCancelled && (
                  <div className="alert alert-danger">
                    ❌ This booking has been cancelled
                  </div>
                )}
                {selectedBooking.isCompleted && !selectedBooking.isCancelled && (
                  <div className="alert alert-secondary">
                    ✅ This booking is completed and archived
                  </div>
                )}
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>
                  Close
                </button>
                {canCancelBooking(selectedBooking) && (
                  <button
                    type="button"
                    className="btn btn-warning"
                    onClick={() => {
                      handleCancelBooking(selectedBooking.id);
                    }}
                  >
                    Cancel Booking
                  </button>
                )}
                <button
                  type="button"
                  className="btn btn-danger"
                  disabled={selectedBooking.isCancelled || selectedBooking.isCompleted}
                  onClick={() => {
                    handleDeleteBooking(selectedBooking.id);
                  }}
                >
                  Cancel Booking
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};  

export default BookingsList;
