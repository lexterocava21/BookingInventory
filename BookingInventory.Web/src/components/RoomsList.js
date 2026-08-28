import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5025/api';

const RoomsList = ({ onShowToast }) => {
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filter, setFilter] = useState('all');

  useEffect(() => {
    fetchRooms();
  }, []);

  const fetchRooms = async () => {
    try {
      setLoading(true);
      const response = await axios.get(`${API_URL}/rooms`);
      setRooms(response.data);
      setError(null);
      onShowToast?.(`Loaded ${response.data.length} rooms successfully`, 'success');
    } catch (err) {
      setError('Failed to fetch rooms: ' + err.message);
      onShowToast?.('Error loading rooms', 'danger');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const getFilteredRooms = () => {
    if (filter === 'all') return rooms;
    if (filter === 'available') return rooms.filter(r => r.capacity > 0);
    if (filter === 'high-capacity') return rooms.filter(r => r.capacity >= 3);
    return rooms;
  };

  const filteredRooms = getFilteredRooms();

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner-border" role="status"></div>
        <div className="loading-text">Loading rooms...</div>
      </div>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2>Available Rooms</h2>
          <p className="text-muted">Manage and view all hotel rooms</p>
        </div>
        <button className="btn btn-primary" onClick={fetchRooms}>
          <i className="bi bi-arrow-clockwise"></i> Refresh
        </button>
      </div>

      {error && (
        <div className="alert alert-danger alert-dismissible fade show">
          <strong>Error:</strong> {error}
          <button type="button" className="btn-close" onClick={() => setError(null)}></button>
        </div>
      )}

      {/* Filter Buttons */}
      <div className="mb-4 d-flex gap-2 flex-wrap">
        <button
          className={`btn btn-sm ${filter === 'all' ? 'btn-primary' : 'btn-outline-primary'}`}
          onClick={() => setFilter('all')}
        >
          All Rooms ({rooms.length})
        </button>
        <button
          className={`btn btn-sm ${filter === 'available' ? 'btn-success' : 'btn-outline-primary'}`}
          onClick={() => setFilter('available')}
        >
          Available ({rooms.filter(r => r.capacity > 0).length})
        </button>
        <button
          className={`btn btn-sm ${filter === 'high-capacity' ? 'btn-info' : 'btn-outline-primary'}`}
          onClick={() => setFilter('high-capacity')}
        >
          High Capacity ({rooms.filter(r => r.capacity >= 3).length})
        </button>
      </div>

      {filteredRooms.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">📦</div>
          <div className="empty-state-title">No Rooms Found</div>
          <div className="empty-state-text">
            {filter === 'all'
              ? 'Make sure the API is running and database has data.'
              : `No rooms match the "${filter}" filter.`}
          </div>
          <button className="btn btn-primary" onClick={() => setFilter('all')}>
            View All Rooms
          </button>
        </div>
      ) : (
        <div className="rooms-grid">
          {filteredRooms.map(room => (
            <div key={room.id} className="card room-card">
              <div className="card-header">
                <div className="d-flex justify-content-between align-items-center">
                  <h5 className="mb-0">Room {room.number}</h5>
                  <span className={`badge ${room.capacity > 0 ? 'badge-available' : 'badge-unavailable'}`}>
                    {room.capacity > 0 ? '✓ Available' : '✗ Full'}
                  </span>
                </div>
              </div>
              <div className="card-body">
                <div className="mb-3">
                  <small className="text-muted d-block">Hotel</small>
                  <strong className="text-dark">{room.hotel?.name || 'N/A'}</strong>
                </div>

                <div className="mb-3">
                  <small className="text-muted d-block">Room ID</small>
                  <strong className="text-dark">#{room.id}</strong>
                </div>

                <div className="mb-3">
                  <small className="text-muted d-block">Capacity</small>
                  <div className="d-flex align-items-center gap-2">
                    <strong className="text-dark">{room.capacity} guests</strong>
                    <span className="badge bg-primary">{room.capacity === 1 ? 'Single' : room.capacity === 2 ? 'Double' : 'Suite'}</span>
                  </div>
                </div>

                {room.rateHistories && room.rateHistories.length > 0 && (
                  <div className="alert alert-info py-2 px-3 mb-0">
                    <small className="d-block text-muted mb-1">Current Rate</small>
                    <strong className="text-dark">
                      ${parseFloat(room.rateHistories[0]?.baseRate || 0).toFixed(2)} / night
                    </strong>
                    <small className="d-block text-muted mt-1">
                      Effective: {new Date(room.rateHistories[0]?.effectiveDate).toLocaleDateString()}
                    </small>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default RoomsList;
