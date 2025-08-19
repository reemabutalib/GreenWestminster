import { useState, useEffect, useMemo } from 'react';
import '../styling/StreakCounter.css';

const StreakCounter = ({ userId }) => {
  const [streakData, setStreakData] = useState({
    currentStreak: 0,
    maxStreak: 0,
    streakBroken: false,
    activityCalendar: []
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  const [resetNotice, setResetNotice] = useState(false);
  const [increaseNotice, setIncreaseNotice] = useState(false);

  const API_BASE_URL = (
  import.meta.env.DEV
    ? '' 
    : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
).replace(/\/$/, '');

  const todayKey = useMemo(() => {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }, []);

  const fetchStreakData = async () => {
    if (!userId) {
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      const res = await fetch(`${API_BASE_URL}/api/activities/streak/${userId}`);
      if (!res.ok) throw new Error(`Failed to fetch streak: ${res.status}`);
      const data = await res.json();
      setStreakData(data);

      // LocalStorage keys for notifications
      const resetKey = `streakResetNotified:${userId}`;
      const increaseKey = `streakIncreaseNotified:${userId}`;
      const lastSeenStreakKey = `lastSeenStreak:${userId}`;

      const lastResetNotified = localStorage.getItem(resetKey);
      const lastIncreaseNotified = localStorage.getItem(increaseKey);
      const lastSeenStreak = parseInt(localStorage.getItem(lastSeenStreakKey) || '0', 10);

      // Show reset notice if broken today
      const wasBroken = Boolean(data?.streakBroken) && Number(data?.currentStreak) === 0;
      if (wasBroken && lastResetNotified !== todayKey) {
        setResetNotice(true);
        localStorage.setItem(resetKey, todayKey);
      } else {
        setResetNotice(false);
      }

      // Show increase notice if streak increased since last seen
      const increased = data?.currentStreak > lastSeenStreak;
      if (increased && lastIncreaseNotified !== todayKey) {
        setIncreaseNotice(true);
        localStorage.setItem(increaseKey, todayKey);
      } else {
        setIncreaseNotice(false);
      }

      // Always store current streak for next comparison
      localStorage.setItem(lastSeenStreakKey, String(data?.currentStreak || 0));

    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // initial + user change
  useEffect(() => {
    if (userId) fetchStreakData();
    else setLoading(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  // refresh when streakUpdated event is fired
  useEffect(() => {
    const handle = () => { if (userId) fetchStreakData(); };
    window.addEventListener('streakUpdated', handle);
    return () => window.removeEventListener('streakUpdated', handle);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  if (loading) return <div className="streak-counter loading">Loading streak...</div>;
  if (error) return <div className="streak-counter error">Error: {error}</div>;
  if (!userId) return <div className="streak-counter no-user">Sign in to see your streak</div>;

  return (
    <div className="streak-counter-wrap">
      {resetNotice && (
        <div className="streak-notice reset">
          <span>Your streak reset to 0. Log an activity today to start again! ✨</span>
          <button onClick={() => setResetNotice(false)}>Got it</button>
        </div>
      )}
      {increaseNotice && (
        <div className="streak-notice increase">
          <span>🔥 Streak increased to {streakData.currentStreak} day{streakData.currentStreak !== 1 ? 's' : ''}!</span>
          <button onClick={() => setIncreaseNotice(false)}>Nice!</button>
        </div>
      )}

      <div className="streak-counter">
        <span className="streak-value">{streakData.currentStreak}</span>
        <span className="streak-label">
          Day{streakData.currentStreak !== 1 ? 's' : ''} Streak
          <span className="streak-flame">🔥</span>
        </span>
      </div>
    </div>
  );
};

export default StreakCounter;
