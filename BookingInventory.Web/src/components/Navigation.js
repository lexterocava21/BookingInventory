import React from 'react';

const Navigation = () => {
  return (
    <div className="app-header">
      <div className="container-fluid">
        <div className="row align-items-center">
          <div className="col-md-8">
            <h1>🏨 Booking Inventory System</h1>
            <p className="mb-0">Professional Hotel Room Management & Booking Platform</p>
          </div>
          <div className="col-md-4 text-end">
            <span className="badge bg-light text-dark">
              API: http://localhost:5025/api
            </span>
            <span className="badge bg-light text-dark ms-2">
              Status: Online
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Navigation;
