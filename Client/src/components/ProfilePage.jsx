import { useState, useEffect } from 'react';
import './styling/ProfilePage.css';

const ProfilePage = () => {
  const [user, setUser] = useState(null);
  const [stats, setStats] = useState(null);
  const [recentActivities, setRecentActivities] = useState([]);
  const [activeTab, setActiveTab] = useState('activities');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // For demo purposes
  const userId = 1;

  useEffect(() => {
    const fetchUserData = async () => {
      setLoading(true);
      setError(null);
      
      try {
        // Fetch user profile data
        const userResponse = await fetch(`/api/users/${userId}`);
        
        if (!userResponse.ok) {
          throw new Error('Failed to fetch user data');
        }
        
        const userData = await userResponse.json();
        setUser(userData);
        
        // Fetch user stats
        const statsResponse = await fetch(`/api/users/${userId}/stats`);
        
        if (statsResponse.ok) {
          const statsData = await statsResponse.json();
          setStats(statsData);
        }
        
        // Fetch recent activities
        const activitiesResponse = await fetch(`/api/users/${userId}/activities/recent`);
        
        if (activitiesResponse.ok) {
          const activitiesData = await activitiesResponse.json();
          setRecentActivities(activitiesData);
        }
        
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchUserData();
  }, [userId]);

  if (loading) return <div className="loading">Loading profile data...</div>;
  if (error) return <div className="error-message">Error: {error}</div>;
  if (!user) return <div className="not-found">User profile not found</div>;

  return (
    <div className="profile-page">
      <div className="profile-header">
        <div className="profile-info">
          <h2>{user.username}</h2>
          <p className="subtitle">{user.email}</p>
          
          <div className="user-stats">
            <div className="stat-card">
              <div className="stat-value">{user.points}</div>
              <div className="stat-label">Total Points</div>
            </div>
            
            <div className="stat-card">
              <div className="stat-value">{user.currentStreak} <span className="streak-flame">🔥</span></div>
              <div className="stat-label">Day Streak</div>
            </div>
            
            <div className="stat-card">
              <div className="stat-value">{user.maxStreak}</div>
              <div className="stat-label">Best Streak</div>
            </div>
          </div>
        </div>
        
        <div className="profile-actions">
          <button className="edit-profile-btn">Edit Profile</button>
        </div>
      </div>
      
      {stats && (
        <div className="impact-summary">
          <h3>Your Environmental Impact</h3>
          <div className="impact-grid">
            <div className="impact-item">
              <div className="impact-icon">🌱</div>
              <div className="impact-data">
                <div className="impact-value">{stats.treesPlanted || 0}</div>
                <div className="impact-label">Trees Planted</div>
              </div>
            </div>
            
            <div className="impact-item">
              <div className="impact-icon">♻️</div>
              <div className="impact-data">
                <div className="impact-value">{stats.wasteRecycled || 0}kg</div>
                <div className="impact-label">Waste Recycled</div>
              </div>
            </div>
            
            <div className="impact-item">
              <div className="impact-icon">🚲</div>
              <div className="impact-data">
                <div className="impact-value">{stats.sustainableCommutes || 0}</div>
                <div className="impact-label">Sustainable Commutes</div>
              </div>
            </div>
            
            <div className="impact-item">
              <div className="impact-icon">💧</div>
              <div className="impact-data">
                <div className="impact-value">{stats.waterSaved || 0}L</div>
                <div className="impact-label">Water Saved</div>
              </div>
            </div>
          </div>
        </div>
      )}
      
      <div className="profile-tabs">
        <button 
          className={`tab-btn ${activeTab === 'activities' ? 'active' : ''}`}
          onClick={() => setActiveTab('activities')}
        >
          Recent Activities
        </button>
        <button 
          className={`tab-btn ${activeTab === 'challenges' ? 'active' : ''}`}
          onClick={() => setActiveTab('challenges')}
        >
          Challenges
        </button>
        <button 
          className={`tab-btn ${activeTab === 'achievements' ? 'active' : ''}`}
          onClick={() => setActiveTab('achievements')}
        >
          Achievements
        </button>
      </div>
      
      {activeTab === 'activities' && (
        <div className="profile-activities">
          {recentActivities.length === 0 ? (
            <div className="no-data">No recent activities found.</div>
          ) : (
            <div className="activity-timeline">
              {recentActivities.map(activity => (
                <div key={activity.id} className="timeline-item">
                  <div className="timeline-date">
                    {new Date(activity.completedAt).toLocaleDateString()}
                  </div>
                  <div className="timeline-content">
                    <h4>{activity.activity.title}</h4>
                    <p>{activity.activity.description}</p>
                    <div className="timeline-points">+{activity.pointsEarned} points</div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
      
      {activeTab === 'challenges' && (
        <div className="profile-challenges">
          <div className="no-data">Coming soon: View your challenge history here.</div>
        </div>
      )}
      
      {activeTab === 'achievements' && (
        <div className="profile-achievements">
          <div className="no-data">Coming soon: View your earned achievements here.</div>
        </div>
      )}
    </div>
  );
};

export default ProfilePage;