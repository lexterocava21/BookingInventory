import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5025/api';

const BookingForm = ({ onShowToast }) => {
  const [formData, setFormData] = useState({
    hotelId: '',
    roomId: '',
    checkIn: '',
    checkOut: '',
    guestCount: '',
  });
  const [hotels, setHotels] = useState([]);
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(false);
  const [loadingHotels, setLoadingHotels] = useState(false);
  const [loadingRooms, setLoadingRooms] = useState(false);
  const [validation, setValidation] = useState({});

  useEffect(() => {
    const fetchHotels = async () => {
      try {
        setLoadingHotels(true);
        const response = await axios.get(`${API_URL}/hotels`);
        const hotelList = response.data || [];
        setHotels(hotelList);

        if (formData.hotelId) {
          const selectedHotel = hotelList.find(hotel => hotel.id === Number(formData.hotelId));
          const selectedRooms = selectedHotel?.rooms || [];
          setRooms(selectedRooms);
          setFormData(prev => ({
            ...prev,
            roomId: selectedRooms[0]?.id ? String(selectedRooms[0].id) : ''
          }));
        }
      } catch (err) {
        console.error('Failed to load hotels', err);
        onShowToast?.('Failed to load hotels', 'danger');
      } finally {
        setLoadingHotels(false);
      }
    };

    fetchHotels();
  }, [onShowToast]);

  useEffect(() => {
    if (!formData.hotelId) {
      setRooms([]);
      setFormData(prev => ({ ...prev, roomId: '' }));
      return;
    }

    const selectedHotel = hotels.find(hotel => hotel.id === Number(formData.hotelId));
    const filteredRooms = selectedHotel?.rooms || [];
    setRooms(filteredRooms);
    setFormData(prev => ({
      ...prev,
      roomId: filteredRooms[0]?.id ? String(filteredRooms[0].id) : ''
    }));
  }, [formData.hotelId, hotels]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    // Clear validation error for this field
    if (validation[name]) {
      setValidation(prev => ({
        ...prev,
        [name]: null
      }));
    }
  };

  const validateForm = () => {
    const errors = {};

    if (!formData.hotelId) errors.hotelId = 'Hotel is required';
    if (!formData.roomId) errors.roomId = 'Room is required';
    if (!formData.checkIn) errors.checkIn = 'Check-in date is required';
    if (!formData.checkOut) errors.checkOut = 'Check-out date is required';
    if (!formData.guestCount) errors.guestCount = 'Guest count is required';

    if (formData.checkIn && formData.checkOut) {
      const checkIn = new Date(formData.checkIn);
      const checkOut = new Date(formData.checkOut);
      if (checkIn >= checkOut) {
        errors.checkOut = 'Check-out must be after check-in';
      }
    }

    if (formData.guestCount && parseInt(formData.guestCount) <= 0) {
      errors.guestCount = 'Guest count must be greater than 0';
    }

    setValidation(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      onShowToast?.('Please fill in all fields correctly', 'warning');
      return;
    }

    try {
      setLoading(true);
      const response = await axios.post(`${API_URL}/bookings`, {
        roomId: parseInt(formData.roomId),
        checkIn: formData.checkIn,
        checkOut: formData.checkOut,
        guestCount: parseInt(formData.guestCount),
      });

      onShowToast?.(`Booking created successfully! ID: ${response.data.id}`, 'success');

      // Reset form
      setFormData({
        hotelId: '',
        roomId: '',
        checkIn: '',
        checkOut: '',
        guestCount: '',
      });
      setRooms([]);
      setValidation({});
    } catch (err) {
      onShowToast?.(err.response?.data?.message || 'Failed to create booking', 'danger');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const today = new Date().toISOString().slice(0, 16);

  return (
    <div>
      <h2>Create New Booking</h2>
      <p className="text-muted">Reserve a room for your guests</p>

      <div className="row">
        <div className="col-md-8">
          <div className="card">
            <div className="card-header">
              <h5 className="mb-0">Booking Details</h5>
            </div>
            <div className="card-body">
              <form onSubmit={handleSubmit}>
                <div className="row">
                  <div className="col-md-6 mb-3">
                    <label className="form-label">Hotel *</label>
                    <select
                      className={`form-select ${validation.hotelId ? 'is-invalid' : ''}`}
                      name="hotelId"
                      value={formData.hotelId}
                      onChange={handleChange}
                      disabled={loadingHotels}
                    >
                      <option value="">Select a hotel</option>
                      {hotels.map(hotel => (
                        <option key={hotel.id} value={hotel.id}>{hotel.name}</option>
                      ))}
                    </select>
                    {validation.hotelId && <div className="invalid-feedback d-block">{validation.hotelId}</div>}
                  </div>

                  <div className="col-md-6 mb-3">
                    <label className="form-label">Room *</label>
                    <select
                      className={`form-select ${validation.roomId ? 'is-invalid' : ''}`}
                      name="roomId"
                      value={formData.roomId}
                      onChange={handleChange}
                      disabled={!formData.hotelId || loadingRooms || rooms.length === 0}
                    >
                      {!formData.hotelId && <option value="">Select a hotel first</option>}
                      {formData.hotelId && rooms.length === 0 && !loadingRooms && (
                        <option value="">No rooms available for this hotel</option>
                      )}
                      {rooms.map(room => (
                        <option key={room.id} value={room.id}>Room {room.number} (Capacity {room.capacity})</option>
                      ))}
                    </select>
                    {validation.roomId && <div className="invalid-feedback d-block">{validation.roomId}</div>}
                  </div>
                </div>

                <div className="row">
                  <div className="col-md-6 mb-3">
                    <label className="form-label">Number of Guests *</label>
                    <input
                      type="number"
                      className={`form-control ${validation.guestCount ? 'is-invalid' : ''}`}
                      name="guestCount"
                      value={formData.guestCount}
                      onChange={handleChange}
                      placeholder="Number of guests"
                      min="1"
                    />
                    {validation.guestCount && <div className="invalid-feedback d-block">{validation.guestCount}</div>}
                  </div>
                </div>

                <div className="row">
                  <div className="col-md-6 mb-3">
                    <label className="form-label">Check-In Date & Time *</label>
                    <input
                      type="datetime-local"
                      className={`form-control ${validation.checkIn ? 'is-invalid' : ''}`}
                      name="checkIn"
                      value={formData.checkIn}
                      onChange={handleChange}
                      min={today}
                    />
                    {validation.checkIn && <div className="invalid-feedback d-block">{validation.checkIn}</div>}
                  </div>

                  <div className="col-md-6 mb-3">
                    <label className="form-label">Check-Out Date & Time *</label>
                    <input
                      type="datetime-local"
                      className={`form-control ${validation.checkOut ? 'is-invalid' : ''}`}
                      name="checkOut"
                      value={formData.checkOut}
                      onChange={handleChange}
                      min={today}
                    />
                    {validation.checkOut && <div className="invalid-feedback d-block">{validation.checkOut}</div>}
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
                        Creating Booking...
                      </>
                    ) : (
                      <>
                        <i className="bi bi-check-circle"></i> Create Booking
                      </>
                    )}
                  </button>
                  <button 
                    type="reset" 
                    className="btn btn-outline-primary btn-lg"
                    onClick={() => {
                      setFormData({ roomId: '', checkIn: '', checkOut: '', guestCount: '' });
                      setValidation({});
                    }}
                  >
                    Clear
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card bg-light">
            <div className="card-header bg-primary text-white">
              <h5 className="mb-0">📋 Instructions</h5>
            </div>
            <div className="card-body small">
              <ul className="list-unstyled">
                <li className="mb-2">
                  <strong>1. Pick Hotel:</strong><br />
                  Choose the hotel first to filter valid rooms
                </li>
                <li className="mb-2">
                  <strong>2. Select Room:</strong><br />
                  The room dropdown is populated from the selected hotel
                </li>
                <li className="mb-2">
                  <strong>3. Guest Count:</strong><br />
                  Number of people staying
                </li>
                <li className="mb-2">
                  <strong>4. Check-In:</strong><br />
                  Date and time of arrival
                </li>
                <li className="mb-2">
                  <strong>5. Check-Out:</strong><br />
                  Date and time of departure
                </li>
                <li>
                  <strong>6. Submit:</strong><br />
                  Review and create booking
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookingForm;
