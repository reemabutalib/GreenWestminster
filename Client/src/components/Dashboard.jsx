import { useState, useEffect } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from './context/UserContext';
import '../styling/Dashboard.css';
import StreakCounter from './StreakCounter';
import ActiveChallenges from './ActiveChallenges';
import ActivitiesPage from './ActivitiesPage';

// API base URL from environment variables
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';

const Dashboard = () => {
  const { currentUser } = useAuth(); // Use the authenticated user from context
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchUserData = async () => {
      // If no user is authenticated, don't attempt to fetch data
      if (!currentUser) {
        setLoading(false);
        return;
      }
      
      try {
        // Get userId from authentication context or localStorage
        const userId = currentUser.id || localStorage.getItem('userId');
        
        if (!userId) {
          throw new Error('No user ID available');
        }
        
        // Use the correct API URL with appropriate headers
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        
        console.log(`Fetching user data for ID: ${userId}, Status: ${response.status}`);
        
       if (!response.ok) {
  // Try to get error details from response
  let errorMessage = 'Failed to fetch user data';
  try {
    const errorData = await response.json();
    errorMessage = errorData.message || errorMessage;
  } catch {
    // If response is not JSON, use status text
    errorMessage = `${errorMessage} (${response.status}: ${response.statusText})`;
  }
  throw new Error(errorMessage);
}
        
        const userData = await response.json();
        console.log('User data fetched successfully:', userData);
        setUserData(userData);
        setLoading(false);
      } catch (err) {
        console.error('Error fetching user data:', err);
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchUserData();
  }, [currentUser]);

  // Redirect to login if not authenticated
  if (!currentUser && !loading) {
    return <Navigate to="/login" />;
  }

  if (loading) return <div className="loading-container"><div className="loading">Loading...</div></div>;
  if (error) return <div className="error-container"><div className="error">Error: {error}</div></div>;
  if (!userData) return <div className="no-user-container"><div className="no-user">No user data available</div></div>;

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h2>Welcome back, {userData.username}!</h2>
        <div className="user-stats">
          <div className="stat">
            <span className="stat-value">{userData.points}</span>
            <span className="stat-label">Points</span>
          </div>
          <StreakCounter streak={userData.currentStreak} />
        </div>
      </header>

      <section className="dashboard-content">
        <div className="section-header">
          <h3>Today's Activities</h3>
          <Link to="/activities" className="view-all">View All</Link>
        </div>
        <ActivitiesPage userId={userData.id} />

        <div className="section-header">
          <h3>Active Challenges</h3>
          <Link to="/challenges" className="view-all">View All</Link>
        </div>
        <ActiveChallenges userId={userData.id} />
      </section>
    </div>
  );
};

export default Dashboard;