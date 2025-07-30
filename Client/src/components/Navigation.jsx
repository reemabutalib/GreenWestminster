import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import '../styling/Navigation.css';
import Logout from './auth/Logout';

const Navigation = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const location = useLocation();
  
  useEffect(() => {
    // Check if user is logged in by looking for token
    const token = localStorage.getItem('token');
    setIsLoggedIn(!!token);
    
    // Check if user is an admin
    if (token) {
      try {
        // Get user role from local storage or parse from token
        const userRole = localStorage.getItem('userRole');
        setIsAdmin(userRole === 'Admin');
      } catch (error) {
        console.error('Error checking admin status:', error);
        setIsAdmin(false);
      }
    } else {
      setIsAdmin(false);
    }
  }, [location]); // Re-check when location changes (after login/logout)

  return (
    <nav className="nav-container">
      <div className="nav-logo">
        <Link to="/">Green Westminster</Link>
      </div>
      
      <div className="nav-links">
        {/* Events is visible to everyone */}
        <Link to="/events" className={location.pathname === '/events' ? 'active' : ''}>
          Events
        </Link>
        
        {/* These links are only visible when logged in */}
        {isLoggedIn && !isAdmin && (
          <>
            <Link to="/dashboard" className={location.pathname === '/dashboard' ? 'active' : ''}>
              Dashboard
            </Link>
            <Link to="/activities" className={location.pathname === '/activities' ? 'active' : ''}>
              Activities
            </Link>
            <Link to="/challenges" className={location.pathname === '/challenges' ? 'active' : ''}>
              Challenges
            </Link>
            <Link to="/leaderboard" className={location.pathname === '/leaderboard' ? 'active' : ''}>
              Leaderboard
            </Link>
            
          </>
        )}
        
        {/* Admin links */}
        {isAdmin && (
          <Link to="/admin/dashboard" className={location.pathname.startsWith('/admin') ? 'active' : ''}>
            Admin Dashboard
          </Link>
        )}
      </div>
      
      <div className="nav-auth">
        {isLoggedIn ? (
          <>
            {!isAdmin && (
              <Link to="/profile" className="profile-link">
                <div className="profile-icon">
                  <i className="fa fa-user"></i>
                </div>
                Profile
              </Link>
            )}
            {isAdmin && (
              <span className="admin-badge">Admin</span>
            )}
            <Logout />
          </>
        ) : (
          <>
            <Link to="/login" className="login-btn">
              Student Login
            </Link>
            <Link to="/register" className="register-btn">
              Register
            </Link>
            <Link to="/admin/login" className="admin-login-btn">
              Admin
            </Link>
            <Link to="/about-us">About Us</Link>
          </>
        )}
      </div>
    </nav>
  );
};

export default Navigation;