import { useNavigate } from 'react-router-dom';
import '../../styling/Auth.css';

const Logout = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    // Remove the authentication token from localStorage
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    
    // Redirect to the login page
    navigate('/login');
  };

  return (
    <button className="logout-btn" onClick={handleLogout}>
      Logout
    </button>
  );
};

export default Logout;