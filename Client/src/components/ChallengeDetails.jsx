import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import '../styling/ChallengeDetails.css';

const ChallengeDetails = () => {
  const { id } = useParams();
  const [challenge, setChallenge] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [hasJoined, setHasJoined] = useState(false);
  
  // For demo purposes
  const userId = 1;

  useEffect(() => {
    const fetchChallengeDetails = async () => {
      try {
        // Fetch challenge details
        const response = await fetch(`/api/challenges/${id}`);
        
        if (!response.ok) {
          throw new Error('Failed to fetch challenge details');
        }
        
        const challengeData = await response.json();
        setChallenge(challengeData);
        
        // Check if user has joined this challenge
        try {
          const joinStatusResponse = await fetch(`/api/users/${userId}/challenges/${id}/status`);
          if (joinStatusResponse.ok) {
            const { hasJoined: joined } = await joinStatusResponse.json();
            setHasJoined(joined);
          }
        } catch (err) {
          console.error('Error checking join status:', err);
        }
        
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchChallengeDetails();
  }, [id, userId]);

  const handleJoinChallenge = async () => {
    try {
      const response = await fetch(`/api/challenges/${id}/join/${userId}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      if (!response.ok) {
        throw new Error('Failed to join challenge');
      }
      
      setHasJoined(true);
    } catch (err) {
      console.error('Error joining challenge:', err);
    }
  };

  if (loading) return <div className="loading">Loading challenge details...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!challenge) return <div className="not-found">Challenge not found</div>;

  const startDate = new Date(challenge.startDate).toLocaleDateString();
  const endDate = new Date(challenge.endDate).toLocaleDateString();
  const isActive = new Date() >= new Date(challenge.startDate) && new Date() <= new Date(challenge.endDate);
  
  return (
    <div className="challenge-details">
      <Link to="/challenges" className="back-link">
        &larr; Back to Challenges
      </Link>
      
      <header className="challenge-header">
        <div className={`challenge-badge ${challenge.category.toLowerCase().replace(' ', '-')}`}>
          {challenge.category}
        </div>
        
        <h2>{challenge.title}</h2>
        
        <div className="challenge-status">
          {isActive ? (
            <span className="badge active">Active</span>
          ) : new Date() < new Date(challenge.startDate) ? (
            <span className="badge upcoming">Upcoming</span>
          ) : (
            <span className="badge ended">Ended</span>
          )}
        </div>
      </header>
      
      <div className="challenge-info">
        <div className="info-item">
          <span className="label">Duration:</span>
          <span className="value">{startDate} to {endDate}</span>
        </div>
        
        <div className="info-item">
          <span className="label">Reward:</span>
          <span className="value points">{challenge.pointsReward} points</span>
        </div>
      </div>
      
      <div className="challenge-description">
        <h3>About this Challenge</h3>
        <p>{challenge.description}</p>
      </div>
      
      {challenge.activities && challenge.activities.length > 0 && (
        <div className="challenge-activities">
          <h3>Activities to Complete</h3>
          <ul className="activities-list">
            {challenge.activities.map(activity => (
              <li key={activity.id} className="activity-item">
                <h4>{activity.title}</h4>
                <p>{activity.description}</p>
                <div className="activity-points">+{activity.pointsValue} points</div>
              </li>
            ))}
          </ul>
        </div>
      )}
      
      {isActive && (
        <div className="challenge-actions">
          {!hasJoined ? (
            <button 
              className="join-challenge-btn"
              onClick={handleJoinChallenge}
            >
              Join This Challenge
            </button>
          ) : (
            <div className="already-joined">
              <span className="joined-icon">✓</span>
              <span>You've joined this challenge!</span>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default ChallengeDetails;