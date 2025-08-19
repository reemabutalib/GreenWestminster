import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/UserContext'; 
import '../../styling/Auth.css';

const Logout = () => {
  const navigate = useNavigate();
  const { logout } = useAuth();

  const handleLogout = () => {
    logout();                          // clears localStorage + sets currentUser(null)
    navigate('/', { replace: true });  // go to Landing/Home immediately
  };

  return (
    <button className="logout-btn" onClick={handleLogout}>
      Logout
    </button>
  );
};

export default Logout;
