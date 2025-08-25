import { useState, useEffect, useMemo } from 'react';
import '../styling/LeaderboardPage.css';

const LEVELS = ['Platinum', 'Gold', 'Silver', 'Bronze'];

const LeaderboardPage = () => {
  const [usersRaw, setUsersRaw] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const currentUserId = Number(localStorage.getItem('userId'));

  useEffect(() => {
    const fetchLeaderboardData = async () => {
      setLoading(true);
      setError(null);
      try {
        const API_URL = import.meta.env.VITE_API_URL || '';
        const res = await fetch(`${API_URL}/api/users/leaderboard`);
        if (!res.ok) throw new Error(`Failed to fetch leaderboard data: ${res.status}`);
        const data = await res.json();
        setUsersRaw(Array.isArray(data) ? data : []);
      } catch (e) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    };
    fetchLeaderboardData();
  }, []);

  const users = useMemo(
    () =>
      usersRaw.map(u => ({
        id: u.id,
        username: u.username,
        points: Number(u.points ?? 0),
        currentStreak: Number(u.currentStreak ?? 0),
        level: u.level,
      })),
    [usersRaw]
  );
  const groups = useMemo(
    () =>
      LEVELS.map(level => ({
        level,
        users: users
          .filter(u => u.level === level)
          .sort(
            (a, b) =>
              b.points - a.points ||
              b.currentStreak - a.currentStreak ||
              a.username.localeCompare(b.username)
          ),
      })),
    [users]
  );

  const yourTierInfo = useMemo(() => {
    const you = users.find(u => u.id === currentUserId);
    if (!you) return null;
    const group = groups.find(g => g.level === you.level);
    if (!group) return null;
    const idx = group.users.findIndex(u => u.id === currentUserId);
    return { level: you.level, rank: idx >= 0 ? idx + 1 : null, totalInTier: group.users.length };
  }, [users, groups, currentUserId]);

  return (
    <div className="leaderboard-page">
      <h2>Sustainability Leaderboards</h2>
      <p className="subtitle">Each tier has its own ranking — climb your way up! 🌱</p>

      {loading && <div className="loading">Loading leaderboard data...</div>}
      {error && <div className="error-message">Error: {error}</div>}

      {!loading && !error && (
        <>
          {yourTierInfo?.rank && (
            <div className={`your-rank your-rank--${yourTierInfo.level.toLowerCase()}`}>
              <div className="rank-label">Your Tier</div>
              <div className="your-tier">{yourTierInfo.level}</div>
              <div className="rank-label">Your Rank in Tier</div>
              <div className="rank-value">{yourTierInfo.rank}</div>
              <div className="rank-suffix">of {yourTierInfo.totalInTier}</div>
            </div>
          )}

          {/* Single-column stack on ALL screen sizes (same as mobile) */}
          <div className="leaderboard-grid">
            {groups.map(group => (
              <section key={group.level} className={`level-card level-${group.level.toLowerCase()}`}>
                <header className="level-card__header">
                  <h3 className="level-heading">{group.level}</h3>
                  <div className="level-chip">{group.users.length} in tier</div>
                </header>

                <div className="leaderboard-table" role="table" aria-label={`${group.level} leaderboard`}>
                  <div className="leaderboard-header" role="row">
                    <div className="col-rank" role="columnheader">Rank</div>
                    <div className="col-user" role="columnheader">User</div>
                    <div className="col-points" role="columnheader">Points</div>
                    <div className="col-streak" role="columnheader">Streak</div>
                  </div>

                  {group.users.length === 0 ? (
                    <div className="no-data">No users in this tier yet.</div>
                  ) : (
                    group.users.map((user, i) => {
                      const rank = i + 1;
                      const isYou = user.id === currentUserId;
                      return (
                        <div key={user.id} className={`leaderboard-row ${isYou ? 'current-user' : ''}`} role="row">
                          <div className="col-rank" role="cell">
                            {rank <= 3 ? <span className="medal">{['🥇','🥈','🥉'][rank-1]}</span> : <span className="rank-number">{rank}</span>}
                          </div>
                          <div className="col-user" role="cell">
                            <span className="username">{user.username}</span>
                          </div>
                          <div className="col-points" role="cell">
                            <span className="points-value">{user.points}</span>
                            <span className="points-label"> points</span>
                          </div>
                          <div className="col-streak" role="cell">
                            <span className="streak-value">{user.currentStreak}</span>
                            <span className="streak-label"> day{user.currentStreak !== 1 ? 's' : ''} <span className="streak-flame">🔥</span></span>
                          </div>
                        </div>
                      );
                    })
                  )}
                </div>
              </section>
            ))}
          </div>
        </>
      )}
    </div>
  );
}


export default LeaderboardPage;
