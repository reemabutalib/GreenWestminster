import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import '../styling/Navigation.css';
import Logout from './auth/Logout';

const Navigation = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const location = useLocation();
  
  useEffect(() => {
    // Check if user is logged in by looking for token
    const token = localStorage.getItem('token');
    setIsLoggedIn(!!token);
  }, [location]); // Re-check when location changes (after login/logout)

  return (
    <nav className="nav-container">
      <div className="nav-logo">
        <Link to="/">Green Westminster</Link>
      </div>
      
      <div className="nav-links">
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
      </div>
      
      <div className="nav-auth">
        {isLoggedIn ? (
          <>
            <Link to="/profile" className="profile-link">
              <div className="profile-icon">
                <i className="fa fa-user"></i>
              </div>
              Profile
            </Link>
            <Logout />
          </>
        ) : (
          <>
            <Link to="/login" className="login-btn">
              Login
            </Link>
            <Link to="/register" className="register-btn">
              Register
            </Link>
          </>
        )}
      </div>
    </nav>
  );
};

export default Navigation;                                     