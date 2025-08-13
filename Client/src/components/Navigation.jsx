import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import '../styling/Navigation.css';
import Logout from './auth/Logout';

const Navigation = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const location = useLocation();

  useEffect(() => {
    const token = localStorage.getItem('token');
    setIsLoggedIn(!!token);
    if (token) {
      try {
        const userRole = localStorage.getItem('userRole');
        setIsAdmin(userRole === 'Admin');
      } catch {
        setIsAdmin(false);
      }
    } else {
      setIsAdmin(false);
    }
  }, [location]);

  return (
    <nav className="nav-container">
      <div className="nav-logo">
        <Link to="/">
          <img
            src="/images/GW-logo.png"
            alt="Green Westminster Logo"
            style={{ height: '100px', objectFit: 'contain' }}
          />
        </Link>
      </div>

      <div className="nav-links">
        {isLoggedIn && !isAdmin ? (
          <>
            <Link
              to="/dashboard"
              className={`nav-link${location.pathname.startsWith('/dashboard') ? ' active' : ''}`}
            >
              Dashboard
            </Link>
            <Link
              to="/activities"
              className={`nav-link${location.pathname.startsWith('/activities') ? ' active' : ''}`}
            >
              Activities
            </Link>
            <Link
              to="/challenges"
              className={`nav-link${location.pathname.startsWith('/challenges') ? ' active' : ''}`}
            >
              Challenges
            </Link>
            <Link
              to="/leaderboard"
              className={`nav-link${location.pathname.startsWith('/leaderboard') ? ' active' : ''}`}
            >
              Leaderboard
            </Link>
            <Link
              to="/events"
              className={`nav-link${location.pathname.startsWith('/events') ? ' active' : ''}`}
            >
              Events
            </Link>
          </>
        ) : isAdmin ? (
          <Link to="/admin/dashboard" className={location.pathname.startsWith('/admin') ? 'active nav-link' : 'nav-link'}>
            Admin Dashboard
          </Link>
        ) : (
          <>
            <Link to="/events" className={`nav-link${location.pathname === '/events' ? ' active' : ''}`}>
              Events
            </Link>
            <Link to="/about-us" className={`nav-link${location.pathname === '/about-us' ? ' active' : ''}`}>
              About Us
            </Link>
            <Link to="/login" className="login-btn">
              Student Login
            </Link>
            <Link to="/register" className="register-btn">
              Register
            </Link>
            <Link to="/admin/login" className="admin-login-btn">
              Admin
            </Link>
          </>
        )}
      </div>

      <div className="nav-auth">
        {isLoggedIn ? (
          <>
            {isAdmin && (
              <span className="admin-badge">Admin</span>
            )}
            <Logout />
          </>
        ) : null}
      </div>
    </nav>
  );
};

export default Navigation;