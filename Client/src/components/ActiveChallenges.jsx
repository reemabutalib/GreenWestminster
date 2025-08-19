import { useState, useEffect } from 'react';
import '../styling/ActiveChallenges.css';

// API base URL from environment variables
const API_BASE_URL = (
  import.meta.env.DEV
    ? '' 
    : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
).replace(/\/$/, '');

const ActiveChallenges = ({ userId }) => {
  const [challenges, setChallenges] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchActiveChallenges = async () => {
      if (!userId) {
        setLoading(false);
        return;
      }
      
      try {
        const token = localStorage.getItem('token');
        const headers = {
          'Authorization': `Bearer ${token}`
        };
        
        console.log('Fetching active challenges from:', `${API_BASE_URL}/api/challenges/active`);
        const response = await fetch(`${API_BASE_URL}/api/challenges/active`, {
          headers
        });
        
        if (!response.ok) {
          throw new Error(`Failed to fetch challenges: ${response.status}`);
        }
        
        const data = await response.json();
        console.log('Active challenges data:', data);
        setChallenges(data);
        setLoading(false);
      } catch (err) {
        console.error('Error in fetchActiveChallenges:', err);
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchActiveChallenges();
  }, [userId]);
  
 const handleJoinChallenge = async (challengeId) => {
  try {
    const token = localStorage.getItem('token');
    console.log(`Joining challenge ${challengeId} for user ${userId}`);
    
    const response = await fetch(`${API_BASE_URL}/api/challenges/${challengeId}/join`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ userId: userId })
    });
    
    const responseData = await response.json();
    
    if (!response.ok) {
      console.error('Server error response:', responseData);
      throw new Error(responseData.message || 'Failed to join challenge');
    }
    
    console.log('Successfully joined challenge:', responseData);
    
    // Update UI to reflect the joined state
    setChallenges(challenges.map(challenge => 
      challenge.id === challengeId 
        ? { ...challenge, isJoined: true } 
        : challenge
    ));
  } catch (err) {
    console.error('Error joining challenge:', err);
    setError(`Failed to join challenge: ${err.message}`);
  }
};

  if (loading) return <div className="loading-challenges">Loading challenges...</div>;
  if (error) return <div className="error-challenges">Error: {error}</div>;
  if (!challenges.length) return <div className="no-challenges">No active challenges available</div>;

  return (
    <div className="active-challenges">
      {challenges.slice(0, 2).map((challenge) => (
        <div key={challenge.id} className="challenge-card">
          <div className="challenge-content">
            <h4>{challenge.title}</h4>
            <p>{challenge.description}</p>
            <div className="challenge-meta">
              <span className="challenge-points">+{challenge.pointsReward} points</span>
              <span className="challenge-dates">
                Ends {new Date(challenge.endDate).toLocaleDateString()}
              </span>
            </div>
          </div>
          
          <button 
            className="join-btn"
            disabled={challenge.isJoined}
            onClick={() => handleJoinChallenge(challenge.id)}
          >
            {challenge.isJoined ? 'Joined' : 'Join Challenge'}
          </button>
        </div>
      ))}
    </div>
  );
};

export default ActiveChallenges;