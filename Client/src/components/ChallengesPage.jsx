import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import '../styling/ChallengesPage.css';

const ChallengesPage = () => {
  const [challenges, setChallenges] = useState([]);
  const [activeTab, setActiveTab] = useState('active');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // For demo purposes - hardcoded user ID
  const userId = 1;
  
  // Add this for debugging
  console.log('ChallengesPage rendering, challenges:', challenges, 'loading:', loading, 'error:', error);

  useEffect(() => {
    console.log('ChallengesPage component mounted');
    
    const fetchChallenges = async () => {
      setLoading(true);
      setError(null);
      
      try {
        // Fetch appropriate challenges based on active tab
        const endpoint = activeTab === 'active' 
          ? '/api/challenges/active'
          : '/api/challenges';
        
        console.log(`Fetching from endpoint: ${endpoint}`);
        
        // Add API_BASE_URL to ensure correct endpoint
        const API_BASE_URL = 'http://localhost:5138'; // Update with your server port
        const fullUrl = `${API_BASE_URL}${endpoint}`;
        console.log(`Full request URL: ${fullUrl}`);
        
        const response = await fetch(fullUrl);
        
        console.log('Response status:', response.status);
        console.log('Response headers:', Object.fromEntries([...response.headers]));
        
        if (!response.ok) {
          const errorText = await response.text();
          console.error('API error response:', errorText);
          throw new Error(`Failed to fetch challenges: ${response.status} ${response.statusText}`);
        }

        // Check content type to make sure we're getting JSON
        const contentType = response.headers.get('content-type');
        if (!contentType || !contentType.includes('application/json')) {
          const text = await response.text();
          console.error('Expected JSON response but got:', text.substring(0, 200) + '...');
          throw new Error('Invalid response format from server');
        }
          
        let data = await response.json();
        console.log('Challenges data received:', data);
        
        // If we're on the upcoming tab, filter out active challenges
        if (activeTab === 'upcoming') {
          const now = new Date();
          data = data.filter(challenge => new Date(challenge.startDate) > now);
        }
        
        // If we're on the completed tab, fetch user's completed challenges
        if (activeTab === 'completed') {
          try {
            const completedResponse = await fetch(`/api/users/${userId}/challenges/completed`);
            
            if (!completedResponse.ok) {
              console.warn('Failed to fetch completed challenges, using empty array');
              data = [];
            } else {
              data = await completedResponse.json();
            }
          } catch (err) {
            console.error('Error fetching completed challenges:', err);
            data = [];
          }
        }
        
        setChallenges(data);
      } catch (err) {
        setError(err.message);
        console.error('Error fetching challenges:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchChallenges();
  }, [activeTab, userId]);
  
  
  const handleJoinChallenge = async (challengeId) => {
    try {
      const response = await fetch(`/api/challenges/${challengeId}/join/${userId}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      if (!response.ok) {
        throw new Error('Failed to join challenge');
      }
      
      // Update the UI to show the user has joined
      setChallenges(challenges.map(challenge => 
        challenge.id === challengeId 
          ? { ...challenge, hasJoined: true }
          : challenge
      ));
      
    } catch (err) {
      console.error('Error joining challenge:', err);
      // Show error message to user
    }
  };

  const renderChallengeStatus = (challenge) => {
    const now = new Date();
    const startDate = new Date(challenge.startDate);
    const endDate = new Date(challenge.endDate);
    
    if (startDate > now) {
      const daysToStart = Math.ceil((startDate - now) / (1000 * 60 * 60 * 24));
      return <span className="challenge-starts">Starts in {daysToStart} day{daysToStart !== 1 ? 's' : ''}</span>;
    } else if (endDate > now) {
      const daysLeft = Math.ceil((endDate - now) / (1000 * 60 * 60 * 24));
      return <span className="challenge-ending">Ends in {daysLeft} day{daysLeft !== 1 ? 's' : ''}</span>;
    } else {
      return <span className="challenge-ended">Ended</span>;
    }
  };

  return (
    <div className="challenges-page">
      <h2>Sustainability Challenges</h2>
      <p className="subtitle">Join challenges, complete activities, and earn bonus points!</p>
      
      <div className="challenge-tabs">
        <button 
          className={`tab ${activeTab === 'active' ? 'active' : ''}`}
          onClick={() => setActiveTab('active')}
        >
          Active Challenges
        </button>
        <button 
          className={`tab ${activeTab === 'upcoming' ? 'active' : ''}`}
          onClick={() => setActiveTab('upcoming')}
        >
          Upcoming
        </button>
        <button 
          className={`tab ${activeTab === 'completed' ? 'active' : ''}`}
          onClick={() => setActiveTab('completed')}
        >
          Completed
        </button>
      </div>
      
      {loading && <div className="loading-challenges">Loading challenges...</div>}
      
      {error && <div className="error-message">Error: {error}</div>}
      
      {!loading && !error && challenges.length === 0 && (
        <div className="no-challenges">
          <p>No {activeTab} challenges found.</p>
        </div>
      )}
      
      <div className="challenges-grid">
        {challenges.map(challenge => (
          <div key={challenge.id} className="challenge-card">
            <div className={`challenge-category ${challenge.category.toLowerCase().replace(' ', '-')}`}>
              {challenge.category}
            </div>
            
            <h3>{challenge.title}</h3>
            
            <p className="challenge-description">{challenge.description}</p>
            
            <div className="challenge-meta">
              <div className="challenge-points">
                <span className="points-icon">🏆</span>
                <span className="points-value">{challenge.pointsReward} points</span>
              </div>
              
              <div className="challenge-duration">
                {renderChallengeStatus(challenge)}
              </div>
            </div>
            
            <div className="challenge-actions">
              <Link to={`/challenges/${challenge.id}`} className="details-btn">
                View Details
              </Link>
              
              {activeTab === 'active' && !challenge.hasJoined && (
                <button 
                  className="join-btn"
                  onClick={() => handleJoinChallenge(challenge.id)}
                >
                  Join Challenge
                </button>
              )}
              
              {activeTab === 'active' && challenge.hasJoined && (
                <button className="joined-btn" disabled>
                  Joined
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default ChallengesPage;