import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import '../styling/ActiveChallenges.css';

const ActiveChallenges = ({ userId }) => {
  const [challenges, setChallenges] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchActiveChallenges = async () => {
      try {
        // Fetch all active challenges
        const response = await fetch('/api/challenges/active');
        
        if (!response.ok) {
          throw new Error('Failed to fetch challenges');
        }
        
        const challengesData = await response.json();
        setChallenges(challengesData);
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchActiveChallenges();
  }, [userId]);

  if (loading) return <div className="loading-challenges">Loading challenges...</div>;
  if (error) return <div className="error-challenges">Error: {error}</div>;
  if (!challenges.length) return <div className="no-challenges">No active challenges available</div>;

  return (
    <div className="active-challenges">
      {challenges.slice(0, 2).map((challenge) => {
        const endDate = new Date(challenge.endDate);
        const daysLeft = Math.ceil((endDate - new Date()) / (1000 * 60 * 60 * 24));
        
        return (
          <div key={challenge.id} className="challenge-card">
            <h4>{challenge.title}</h4>
            <p>{challenge.description}</p>
            
            <div className="challenge-meta">
              <span className="challenge-reward">+{challenge.pointsReward} bonus points</span>
              <span className="challenge-time">{daysLeft} day{daysLeft !== 1 ? 's' : ''} left</span>
            </div>
            
            <Link to={`/challenges/${challenge.id}`} className="challenge-details-btn">
              View Details
            </Link>
          </div>
        );
      })}
    </div>
  );
};

export default ActiveChallenges;