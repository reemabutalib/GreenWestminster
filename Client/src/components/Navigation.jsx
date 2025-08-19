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

  const homeTo = isLoggedIn ? (isAdmin ? '/admin/dashboard' : '/dashboard') : '/';

  return (
    <nav className="nav-container">
      {/* Left: logo + primary links */}
      <div className="nav-left">
        <div className="nav-logo">
          <Link to={homeTo} className="nav-logo-link">
            <img
              src="/images/GW-logo.png"
              alt="Green Westminster Logo"
              className="nav-logo-img"
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
            <Link
              to="/admin/dashboard"
              className={`nav-link${location.pathname.startsWith('/admin') ? ' active' : ''}`}
            >
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
            </>
          )}
        </div>
      </div>

      {/* Right: actions (login/register/admin OR logout) */}
      <div className="nav-right">
        {isLoggedIn ? (
          <>
            {isAdmin && <span className="admin-badge">Admin</span>}
            <Logout /> {/* keep your existing logout button component */}
          </>
        ) : (
          <>
            <Link to="/login" className="pill pill-outline">Student Login</Link>
            <Link to="/register" className="pill pill-solid">Register</Link>
            <Link to="/admin/login" className="pill pill-ghost">Admin</Link>
          </>
        )}
      </div>
    </nav>
  );
};

export default Navigation;
