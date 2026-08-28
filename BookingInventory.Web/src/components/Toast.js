import React from 'react';

const Toast = ({ message, type = 'info' }) => {
  const getIcon = () => {
    switch (type) {
      case 'success':
        return '✓';
      case 'danger':
        return '✕';
      case 'warning':
        return '⚠';
      default:
        return 'ℹ';
    }
  };

  return (
    <div className="toast-container">
      <div className={`toast ${type}`}>
        <span className="toast-icon">{getIcon()}</span>
        <span>{message}</span>
      </div>
    </div>
  );
};

export default Toast;
