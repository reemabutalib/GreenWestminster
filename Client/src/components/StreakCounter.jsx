import { useState, useEffect } from 'react';
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

  // API base URL
  const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';
  
  const fetchStreakData = async () => {
    // Check if userId is valid before making the request
    if (!userId) {
      console.warn('No userId provided to StreakCounter component');
      setLoading(false);
      return;
    }
    
    try {
      setLoading(true);
      
      const response = await fetch(`${API_BASE_URL}/api/activities/streak/${userId}`);
      
      if (!response.ok) {
        throw new Error(`Failed to fetch streak: ${response.status}`);
      }
      
      const data = await response.json();
      setStreakData(data);
    } catch (err) {
      console.error('Error fetching streak:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Only fetch data when userId is available
    if (userId) {
      fetchStreakData();
    } else {
      setLoading(false); // Stop loading if there's no userId
    }
  }, [userId, API_BASE_URL]);

  // Listen for streak update events
  useEffect(() => {
    const handleStreakUpdate = () => {
      if (userId) {
        fetchStreakData();
      }
    };
    
    window.addEventListener('streakUpdated', handleStreakUpdate);
    
    return () => {
      window.removeEventListener('streakUpdated', handleStreakUpdate);
    };
  }, [userId]); // Added userId to dependency array

  if (loading) return <div className="streak-counter loading">Loading streak...</div>;
  if (error) return <div className="streak-counter error">Error: {error}</div>;
  if (!userId) return <div className="streak-counter no-user">Sign in to see your streak</div>;

  return (
    <div className="streak-counter">
      <span className="streak-value">{streakData.currentStreak}</span>
      <span className="streak-label">
        Day{streakData.currentStreak !== 1 ? 's' : ''} Streak
        <span className="streak-flame">🔥</span>
      </span>
      
    </div>
  );
};

export default StreakCounter;