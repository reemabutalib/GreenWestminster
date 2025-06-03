import { useState, useEffect } from 'react';
import './DailyActivities.css';

const DailyActivities = ({ userId }) => {
  const [activities, setActivities] = useState([]);
  const [completedActivities, setCompletedActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchDailyActivities = async () => {
      try {
        // Fetch all daily activities
        const activitiesResponse = await fetch('/api/activities');
        
        if (!activitiesResponse.ok) {
          throw new Error('Failed to fetch activities');
        }
        
        const activitiesData = await activitiesResponse.json();
        const dailyActivities = activitiesData.filter(activity => activity.isDaily);
        setActivities(dailyActivities);
        
        // Fetch user's completed activities for today
        const today = new Date().toISOString().split('T')[0];
        const completionsResponse = await fetch(`/api/users/${userId}/activities/completed?date=${today}`);
        
        if (!completionsResponse.ok) {
          throw new Error('Failed to fetch completed activities');
        }
        
        const completionsData = await completionsResponse.json();
        setCompletedActivities(completionsData.map(completion => completion.activityId));
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchDailyActivities();
  }, [userId]);
  
  const handleCompleteActivity = async (activityId) => {
    try {
      const response = await fetch(`/api/users/${userId}/completeActivity/${activityId}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      if (!response.ok) {
        throw new Error('Failed to complete activity');
      }
      
      setCompletedActivities([...completedActivities, activityId]);
    } catch (err) {
      console.error('Error completing activity:', err);
    }
  };

  if (loading) return <div className="loading-activities">Loading activities...</div>;
  if (error) return <div className="error-activities">Error: {error}</div>;
  if (!activities.length) return <div className="no-activities">No daily activities available</div>;

  return (
    <div className="daily-activities">
      {activities.slice(0, 4).map((activity) => {
        const isCompleted = completedActivities.includes(activity.id);
        
        return (
          <div 
            key={activity.id} 
            className={`activity-card ${isCompleted ? 'completed' : ''}`}
          >
            <div className="activity-content">
              <h4>{activity.title}</h4>
              <p>{activity.description}</p>
              <div className="activity-points">+{activity.pointsValue} points</div>
            </div>
            
            <button 
              className="complete-btn"
              disabled={isCompleted}
              onClick={() => handleCompleteActivity(activity.id)}
            >
              {isCompleted ? 'Completed' : 'Complete'}
            </button>
          </div>
        );
      })}
    </div>
  );
};

export default DailyActivities;