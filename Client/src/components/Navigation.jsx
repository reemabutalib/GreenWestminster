import { useState, useEffect, useRef, Fragment } from 'react';
import { Link, useLocation } from 'react-router-dom';
import '../styling/Navigation.css';
import Logout from './auth/Logout';

const Navigation = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const location = useLocation();
  const navRef = useRef(null);
  const menuRef = useRef(null);
  const toggleRef = useRef(null);

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

  useEffect(() => { if (isMenuOpen) setIsMenuOpen(false); }, [location.pathname]);

  useEffect(() => {
    document.body.style.overflow = isMenuOpen ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [isMenuOpen]);

  useEffect(() => {
    const setTopVar = () => {
      if (!navRef.current || !menuRef.current) return;
      const h = navRef.current.getBoundingClientRect().height;
      menuRef.current.style.setProperty('--nav-h', `${h}px`);
    };
    setTopVar();
    window.addEventListener('resize', setTopVar);
    return () => window.removeEventListener('resize', setTopVar);
  }, []);

  useEffect(() => {
    if (!isMenuOpen) return;
    const onKey = (e) => e.key === 'Escape' && setIsMenuOpen(false);
    const onClickOutside = (e) => {
      if (!menuRef.current || !toggleRef.current) return;
      const inMenu = menuRef.current.contains(e.target);
      const onToggle = toggleRef.current.contains(e.target);
      if (!inMenu && !onToggle) setIsMenuOpen(false);
    };
    document.addEventListener('keydown', onKey);
    document.addEventListener('click', onClickOutside);
    return () => {
      document.removeEventListener('keydown', onKey);
      document.removeEventListener('click', onClickOutside);
    };
  }, [isMenuOpen]);

  const homeTo = isLoggedIn ? (isAdmin ? '/admin/dashboard' : '/dashboard') : '/';

  const PrimaryLinks = () => (
    <Fragment>
      {isLoggedIn && !isAdmin ? (
        <Fragment>
          <Link to="/dashboard"   className={`nav-link${location.pathname.startsWith('/dashboard') ? ' active' : ''}`}>Dashboard</Link>
          <Link to="/activities"  className={`nav-link${location.pathname.startsWith('/activities') ? ' active' : ''}`}>Activities</Link>
          <Link to="/challenges"  className={`nav-link${location.pathname.startsWith('/challenges') ? ' active' : ''}`}>Challenges</Link>
          <Link to="/leaderboard" className={`nav-link${location.pathname.startsWith('/leaderboard') ? ' active' : ''}`}>Leaderboard</Link>
          <Link to="/events"      className={`nav-link${location.pathname.startsWith('/events') ? ' active' : ''}`}>Events</Link>
        </Fragment>
      ) : isAdmin ? (
        <Link to="/admin/dashboard" className={`nav-link${location.pathname.startsWith('/admin') ? ' active' : ''}`}>Admin Dashboard</Link>
      ) : (
        <Fragment>
          <Link to="/events"    className={`nav-link${location.pathname === '/events' ? ' active' : ''}`}>Events</Link>
          <Link to="/about-us"  className={`nav-link${location.pathname === '/about-us' ? ' active' : ''}`}>About Us</Link>
        </Fragment>
      )}
    </Fragment>
  );

  const Actions = () => (
    <Fragment>
      {isLoggedIn ? (
        <Fragment>
          {isAdmin && <span className="admin-badge">Admin</span>}
          <Logout />
        </Fragment>
      ) : (
        <Fragment>
          <Link to="/login" className="pill pill-outline">Student Login</Link>
          <Link to="/register" className="pill pill-solid">Register</Link>
          <Link to="/admin/login" className="pill pill-ghost">Admin</Link>
        </Fragment>
      )}
    </Fragment>
  );

  return (
    <Fragment>
      <nav className="nav-container" ref={navRef}>
        <div className="nav-left">
          <div className="nav-logo">
            <Link to={homeTo} className="nav-logo-link">
              <img src="/images/GW-logo.png" alt="Green Westminster Logo" className="nav-logo-img" />
            </Link>
          </div>

          <div className="nav-links">
            <PrimaryLinks />
          </div>
        </div>

        <div className="nav-right">
          <Actions />
          <button
            ref={toggleRef}
            className="nav-toggle"
            aria-expanded={isMenuOpen ? 'true' : 'false'}
            aria-controls="mobile-menu"
            aria-label={isMenuOpen ? 'Close menu' : 'Open menu'}
            onClick={() => setIsMenuOpen(v => !v)}
            type="button"
          >
            <span className="nav-toggle-icon" aria-hidden="true" />
          </button>
        </div>
      </nav>

      {/* MOBILE PANEL */}
      <div
        id="mobile-menu"
        className="mobile-menu"
        ref={menuRef}
        hidden={!isMenuOpen}
      >
        <nav className="mobile-links" aria-label="Mobile">
          <PrimaryLinks />
        </nav>
      </div>
    </Fragment>
  );
};

export default Navigation;
