import React from 'react';
import { Link, Outlet } from 'react-router-dom';
import '../../styling/AdminDashboard.css';

const AdminDashboard = () => {
  return (
    <div className="admin-dashboard">
      <nav className="admin-navigation">
        <Link to="/admin/dashboard" className="admin-nav-link">Overview</Link>
        <Link to="/admin/dashboard/activities" className="admin-nav-link">Manage Activities</Link>
        <Link to="/admin/dashboard/events" className="admin-nav-link">Manage Events</Link>
        <Link to="/admin/dashboard/challenges" className="admin-nav-link">Manage Challenges</Link>
      </nav>
      <div className="admin-content">
        <Outlet />
      </div>
    </div>
  );
};

export default AdminDashboard;