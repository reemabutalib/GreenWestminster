import { useState } from 'react';
import { Link } from 'react-router-dom';
import './Navigation.css';

const Navigation = () => {
  const [isOpen, setIsOpen] = useState(false);
  
  return (
    <nav className="navigation">
      <div className="nav-brand">
        <Link to="/">
          <h1>Green Westminster</h1>
        </Link>
      </div>
      
      <div className="nav-toggle" onClick={() => setIsOpen(!isOpen)}>
        <span className="hamburger"></span>
      </div>
      
      <ul className={`nav-menu ${isOpen ? 'active' : ''}`}>
        <li>
          <Link to="/dashboard" className="nav-link" onClick={() => setIsOpen(false)}>
            Dashboard
          </Link>
        </li>
        <li>
          <Link to="/activities" className="nav-link" onClick={() => setIsOpen(false)}>
            Activities
          </Link>
        </li>
        <li>
          <Link to="/challenges" className="nav-link" onClick={() => setIsOpen(false)}>
            Challenges
          </Link>
        </li>
        <li>
          <Link to="/leaderboard" className="nav-link" onClick={() => setIsOpen(false)}>
            Leaderboard
          </Link>
        </li>
        <li>
          <Link to="/profile" className="nav-link" onClick={() => setIsOpen(false)}>
            Profile
          </Link>
        </li>
      </ul>
    </nav>
  );
};

export default Navigation;