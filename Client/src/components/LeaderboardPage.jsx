import { useState, useEffect } from 'react';
import '../styling/LeaderboardPage.css';

const LeaderboardPage = () => {
  const [users, setUsers] = useState([]);
  const [timeFrame, setTimeFrame] = useState('all-time');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentUserRank, setCurrentUserRank] = useState(null);
  
  // For demo purposes
  const currentUserId = 1;

  useEffect(() => {
    const fetchLeaderboardData = async () => {
      setLoading(true);
      setError(null);
      
      try {
        // URL includes timeFrame as a query parameter
        const url = `http://localhost:80/api/users/leaderboard?timeFrame=${timeFrame}`;
        const response = await fetch(url);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch leaderboard data: ${response.status}`);
        }
        
        const data = await response.json();
        setUsers(data);
        
        // Find current user's rank
        const userRank = data.findIndex(user => user.id === currentUserId);
        if (userRank !== -1) {
          setCurrentUserRank(userRank + 1); // +1 because index is zero-based
        } else {
          setCurrentUserRank(null);
        }
        
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchLeaderboardData();
  }, [timeFrame, currentUserId]);

  const handleTimeFrameChange = (newTimeFrame) => {
    setTimeFrame(newTimeFrame);
  };

  return (
    <div className="leaderboard-page">
      <h2>Sustainability Leaderboard</h2>
      <p className="subtitle">See who's making the biggest impact!</p>
      
      <div className="leaderboard-filters">
        <button 
          className={`filter-btn ${timeFrame === 'all-time' ? 'active' : ''}`}
          onClick={() => handleTimeFrameChange('all-time')}
        >
          All Time
        </button>
        <button 
          className={`filter-btn ${timeFrame === 'month' ? 'active' : ''}`}
          onClick={() => handleTimeFrameChange('month')}
        >
          This Month
        </button>
        <button 
          className={`filter-btn ${timeFrame === 'week' ? 'active' : ''}`}
          onClick={() => handleTimeFrameChange('week')}
        >
          This Week
        </button>
      </div>
      
      {loading && <div className="loading">Loading leaderboard data...</div>}
      
      {error && <div className="error-message">Error: {error}</div>}
      
      {!loading && !error && users.length === 0 && (
        <div className="no-data">No leaderboard data available.</div>
      )}
      
      {!loading && !error && users.length > 0 && (
        <>
          {currentUserRank && (
            <div className="your-rank">
              <div className="rank-label">Your Rank</div>
              <div className="rank-value">{currentUserRank}</div>
              <div className="rank-suffix">of {users.length}</div>
            </div>
          )}
          
          <div className="leaderboard-container">
            <div className="leaderboard-header">
              <div className="rank-header">Rank</div>
              <div className="user-header">User</div>
              <div className="points-header">Points</div>
              <div className="streak-header">Streak</div>
            </div>
            
            <div className="leaderboard-users">
              {users.map((user, index) => {
                const rank = index + 1;
                const isCurrentUser = user.id === currentUserId;
                
                return (
                  <div 
                    key={user.id} 
                    className={`leaderboard-row ${isCurrentUser ? 'current-user' : ''}`}
                  >
                    <div className="rank-cell">
                      {rank === 1 && <span className="medal gold">🥇</span>}
                      {rank === 2 && <span className="medal silver">🥈</span>}
                      {rank === 3 && <span className="medal bronze">🥉</span>}
                      {rank > 3 && <span className="rank-number">{rank}</span>}
                    </div>
                    
                    <div className="user-cell">
                      <div className="username">{user.username}</div>
                    </div>
                    
                    <div className="points-cell">
                      <div className="points-value">{user.points}</div>
                      <div className="points-label">points</div>
                    </div>
                    
                    <div className="streak-cell">
                      <div className="streak-value">{user.currentStreak}</div>
                      <div className="streak-label">
                        day{user.currentStreak !== 1 ? 's' : ''} 
                        <span className="streak-flame">🔥</span>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default LeaderboardPage;