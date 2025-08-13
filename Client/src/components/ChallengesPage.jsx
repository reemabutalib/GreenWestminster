import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import '../styling/ChallengesPage.css';

const ChallengesPage = () => {
  const [challenges, setChallenges] = useState([]);
  const [activeTab, setActiveTab] = useState('active'); // 'active' | 'upcoming' | 'past'
  const [loading, setLoading] = useState(true);
  const [joining, setJoining] = useState({}); // { [challengeId]: boolean }
  const [error, setError] = useState(null);

  const userId = Number(localStorage.getItem('userId')) || 1; // replace with auth context when ready
  const token = localStorage.getItem('token');
  const API_BASE_URL = (import.meta.env.VITE_API_URL || 'http://localhost:80').replace(/\/$/, '');

  const fetchChallenges = async () => {
    setLoading(true);
    setError(null);
    try {
      let endpoint = '/api/challenges';
      if (activeTab === 'active') endpoint = '/api/challenges/active';
      else if (activeTab === 'past') endpoint = '/api/challenges/past';

      const res = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          ...(token ? { Authorization: `Bearer ${token}` } : {})
        }
      });

      const raw = await res.text();
      if (!res.ok) {
        let msg;
        try { msg = JSON.parse(raw)?.message || JSON.parse(raw)?.error; } catch {
          msg = null;
       }
        throw new Error(msg || raw || `Failed to fetch challenges (HTTP ${res.status})`);
      }

      const contentType = res.headers.get('content-type') || '';
      if (!contentType.includes('application/json')) {
        throw new Error('Invalid response format from server');
      }

      let data = raw ? JSON.parse(raw) : [];

      // Client-side filter for "upcoming" (server gives all/active/past)
      if (activeTab === 'upcoming') {
        const now = new Date();
        data = data.filter(c => new Date(c.startDate) > now);
      }

      setChallenges(Array.isArray(data) ? data : []);
    } catch (e) {
      setError(e.message || 'Failed to load challenges');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchChallenges();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab, userId]);

  const handleJoinChallenge = async (challengeId) => {
    setJoining(prev => ({ ...prev, [challengeId]: true }));
    try {
      const res = await fetch(`${API_BASE_URL}/api/challenges/${challengeId}/join`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {})
        },
        body: JSON.stringify({ userId })
      });

      const raw = await res.text();
      if (!res.ok) {
        let msg;
        try { msg = JSON.parse(raw)?.message || JSON.parse(raw)?.error; } catch {
          msg = null;
        }
        throw new Error(msg || raw || `Failed to join challenge (HTTP ${res.status})`);
      }

      // Either refetch so hasJoined comes from server…
      await fetchChallenges();

      // …or optimistic update:
      // setChallenges(prev => prev.map(c => (c.id === challengeId ? { ...c, hasJoined: true } : c)));
    } catch (e) {
      console.error('Error joining challenge:', e);
      alert(e.message || 'Failed to join challenge');
    } finally {
      setJoining(prev => ({ ...prev, [challengeId]: false }));
    }
  };

  const renderChallengeStatus = (challenge) => {
    const now = new Date();
    const start = new Date(challenge.startDate);
    const end = new Date(challenge.endDate);

    if (start > now) {
      const days = Math.ceil((start - now) / (1000 * 60 * 60 * 24));
      return <span className="challenge-starts">Starts in {days} day{days !== 1 ? 's' : ''}</span>;
    }
    if (end > now) {
      const days = Math.ceil((end - now) / (1000 * 60 * 60 * 24));
      return <span className="challenge-ending">Ends in {days} day{days !== 1 ? 's' : ''}</span>;
    }
    return <span className="challenge-ended">Ended</span>;
  };

  return (
    <div className="challenges-page">
      <h2>Sustainability Challenges</h2>
      <p className="subtitle">Join challenges, complete activities, and earn bonus points!</p>

      <div className="challenge-tabs">
        <button className={`tab ${activeTab === 'active' ? 'active' : ''}`} onClick={() => setActiveTab('active')}>
          Active Challenges
        </button>
        <button className={`tab ${activeTab === 'upcoming' ? 'active' : ''}`} onClick={() => setActiveTab('upcoming')}>
          Upcoming
        </button>
        <button className={`tab ${activeTab === 'past' ? 'active' : ''}`} onClick={() => setActiveTab('past')}>
          Past
        </button>
      </div>

      {loading && <div className="loading-challenges">Loading challenges...</div>}
      {error && <div className="error-message">Error: {error}</div>}
      {!loading && !error && challenges.length === 0 && (
        <div className="no-challenges"><p>No {activeTab} challenges found.</p></div>
      )}

      <div className="challenges-grid">
        {challenges.map((challenge) => (
          <div key={challenge.id} className="challenge-card">
            <div className={`challenge-category ${challenge.category ? challenge.category.toLowerCase().replace(/\s+/g, '-') : 'general'}`}>
              {challenge.category || 'General'}
            </div>

            <h3>{challenge.title}</h3>
            <p className="challenge-description">{challenge.description}</p>

            <div className="challenge-meta">
              <div className="challenge-points">
                <span className="points-icon">🏆</span>
                <span className="points-value">{challenge.pointsReward} points</span>
              </div>
              <div className="challenge-duration">{renderChallengeStatus(challenge)}</div>
            </div>

            <div className="challenge-actions">
              <Link to={`/challenges/${challenge.id}`} className="details-btn">View Details</Link>

              {activeTab === 'active' && !challenge.hasJoined && (
                <button
                  className="join-btn"
                  onClick={() => handleJoinChallenge(challenge.id)}
                  disabled={!!joining[challenge.id]}
                  title={!token ? 'Sign in to join' : undefined}
                >
                  {joining[challenge.id] ? 'Joining…' : 'Join Challenge'}
                </button>
              )}

              {activeTab === 'active' && challenge.hasJoined && (
                <button className="joined-btn" disabled>Joined</button>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default ChallengesPage;
