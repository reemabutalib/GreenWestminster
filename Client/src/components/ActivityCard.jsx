import { useState } from 'react';
import './styling/ActivityCard.css';

const ActivityCard = ({ activity, userId }) => {
  const [isCompleted, setIsCompleted] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleCompleteActivity = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      const response = await fetch(`/api/users/${userId}/completeActivity/${activity.id}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to complete activity');
      }
      
      setIsCompleted(true);
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className={`activity-card ${isCompleted ? 'completed' : ''}`}>
      <div className="card-tag">{activity.category}</div>
      
      <h3>{activity.title}</h3>
      <p>{activity.description}</p>
      
      <div className="activity-meta">
        <div className="points">
          <span className="points-value">+{activity.pointsValue}</span> points
        </div>
        
        <div className="frequency">
          {activity.isDaily && <span className="badge daily">Daily</span>}
          {activity.isWeekly && <span className="badge weekly">Weekly</span>}
          {activity.isOneTime && <span className="badge one-time">One-time</span>}
        </div>
      </div>
      
      {error && <div className="error-message">{error}</div>}
      
      <button 
        className="complete-btn"
        onClick={handleCompleteActivity}
        disabled={isCompleted || isLoading}
      >
        {isLoading ? 'Loading...' : isCompleted ? 'Completed' : 'Complete Activity'}
      </button>
    </div>
  );
};

export default ActivityCard;