import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './Dashboard.css';
import StreakCounter from './StreakCounter';
import DailyActivities from './DailyActivities';
import ActiveChallenges from './ActiveChallenges';

const Dashboard = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // For demo purposes, hardcoded user ID
    const userId = 1;
    
    const fetchUserData = async () => {
      try {
        const response = await fetch(`/api/users/${userId}`);
        
        if (!response.ok) {
          throw new Error('Failed to fetch user data');
        }
        
        const userData = await response.json();
        setUser(userData);
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchUserData();
  }, []);

  if (loading) return <div className="loading">Loading...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!user) return <div className="no-user">No user data available</div>;

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h2>Welcome back, {user.username}!</h2>
        <div className="user-stats">
          <div className="stat">
            <span className="stat-value">{user.points}</span>
            <span className="stat-label">Points</span>
          </div>
          <StreakCounter streak={user.currentStreak} />
        </div>
      </header>

      <section className="dashboard-content">
        <div className="section-header">
          <h3>Today's Activities</h3>
          <Link to="/activities" className="view-all">View All</Link>
        </div>
        <DailyActivities userId={user.id} />

        <div className="section-header">
          <h3>Active Challenges</h3>
          <Link to="/challenges" className="view-all">View All</Link>
        </div>
        <ActiveChallenges userId={user.id} />
      </section>
    </div>
  );
};

export default Dashboard;