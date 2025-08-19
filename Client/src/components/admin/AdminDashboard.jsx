import React from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import '../../styling/AdminDashboard.css';

const AdminDashboard = () => {
  return (
    <div className="admin-shell">
      <nav className="admin-nav" role="navigation" aria-label="Admin">
        <NavLink to="/admin/dashboard" className={({isActive}) => `admin-tab ${isActive ? 'active' : ''}`}>Overview</NavLink>
        <NavLink to="/admin/dashboard/activities" className={({isActive}) => `admin-tab ${isActive ? 'active' : ''}`}>Manage Activities</NavLink>
        <NavLink to="/admin/dashboard/events" className={({isActive}) => `admin-tab ${isActive ? 'active' : ''}`}>Manage Events</NavLink>
        <NavLink to="/admin/dashboard/challenges" className={({isActive}) => `admin-tab ${isActive ? 'active' : ''}`}>Manage Challenges</NavLink>
        <NavLink to="/admin/dashboard/review" className={({isActive}) => `admin-tab ${isActive ? 'active' : ''}`}>Review Submissions</NavLink>
      </nav>

      <main className="admin-content">
        <Outlet />
      </main>
    </div>
  );
};

export default AdminDashboard;
