import { useState, useEffect, useMemo } from 'react';
import '../styling/LeaderboardPage.css';

const LEVELS = ["Platinum", "Gold", "Silver", "Bronze"];

// Same thresholds you use elsewhere
const LEVEL_THRESHOLDS = [
  { level: "Platinum", min: 5000 },
  { level: "Gold",     min: 1000 },
  { level: "Silver",   min: 500 },
  { level: "Bronze",   min: 0 }
];

const LeaderboardPage = () => {
  const [usersRaw, setUsersRaw] = useState([]);
  const [timeFrame, setTimeFrame] = useState('all-time');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Replace with your actual logged-in user ID
  const currentUserId = Number(localStorage.getItem('userId'));

  useEffect(() => {
    const fetchLeaderboardData = async () => {
      setLoading(true);
      setError(null);
      try {
        const API_URL = import.meta.env.VITE_API_URL || '';
        const url = `${API_URL}/api/users/leaderboard?timeFrame=${timeFrame}`;
        const response = await fetch(url);
        if (!response.ok) throw new Error(`Failed to fetch leaderboard data: ${response.status}`);
        const data = await response.json();
        setUsersRaw(Array.isArray(data) ? data : []);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchLeaderboardData();
  }, [timeFrame]);

  // Normalize users: ensure each has level + safe fields
  const users = useMemo(() => {
    return usersRaw.map(u => ({
      id: u.id,
      username: u.username,
      points: Number(u.points ?? 0),
      currentStreak: Number(u.currentStreak ?? 0),
      level: u.level 
    }));
  }, [usersRaw]);

  // Group & sort users within each tier (by points desc, tie-breaker by streak desc, then name)
  const groups = useMemo(() => {
    const byLevel = LEVELS.map(level => ({
      level,
      users: users
        .filter(u => u.level === level)
        .sort((a, b) =>
          b.points - a.points ||
          b.currentStreak - a.currentStreak ||
          a.username.localeCompare(b.username)
        )
    }));
    return byLevel;
  }, [users]);

  // Find current user's tier + rank (within tier)
  const yourTierInfo = useMemo(() => {
    const you = users.find(u => u.id === currentUserId);
    if (!you) return null;
    const group = groups.find(g => g.level === you.level);
    if (!group) return null;
    const idx = group.users.findIndex(u => u.id === currentUserId);
    return {
      level: you.level,
      rank: idx >= 0 ? idx + 1 : null,
      totalInTier: group.users.length
    };
  }, [users, groups, currentUserId]);

  const handleTimeFrameChange = (tf) => setTimeFrame(tf);

  return (
    <div className="leaderboard-page">
      <h2>Sustainability Leaderboards</h2>
      <p className="subtitle">Each tier has its own ranking — climb your way up! 🌱</p>

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

      {!loading && !error && (
        <>
          {yourTierInfo && yourTierInfo.rank && (
            <div className={`your-rank your-rank--${yourTierInfo.level.toLowerCase()}`}>
              <div className="rank-label">Your Tier</div>
              <div className="your-tier">{yourTierInfo.level}</div>
              <div className="rank-label">Your Rank in Tier</div>
              <div className="rank-value">{yourTierInfo.rank}</div>
              <div className="rank-suffix">of {yourTierInfo.totalInTier}</div>
            </div>
          )}

          <div className="leaderboard-grid">
            {groups.map(group => (
              <section key={group.level} className={`level-card level-${group.level.toLowerCase()}`}>
                <header className="level-card__header">
                  <h3 className="level-heading">{group.level}</h3>
                  <span className="level-chip">{group.users.length} in tier</span>
                </header>

                {group.users.length === 0 ? (
                  <div className="no-data">No users in this tier yet.</div>
                ) : (
                  <div className="leaderboard-table">
                    <div className="leaderboard-header">
                      <div className="rank-header">Rank</div>
                      <div className="user-header">User</div>
                      <div className="points-header">Points</div>
                      <div className="streak-header">Streak</div>
                    </div>

                    {group.users.map((user, i) => {
                      const rank = i + 1;
                      const isYou = user.id === currentUserId;
                      return (
                        <div key={user.id} className={`leaderboard-row ${isYou ? 'current-user' : ''}`}>
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
                              day{user.currentStreak !== 1 ? 's' : ''} <span className="streak-flame">🔥</span>
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </section>
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default LeaderboardPage;
