import { useState, useEffect } from 'react';
import '../styling/DailyActivities.css';

// API base URL from environment variables
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';

const DailyActivities = ({ userId }) => {
  const [activities, setActivities] = useState([]);
  const [completedActivities, setCompletedActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  // Add state to track if we should skip fetching completed activities
  const [skipCompletedFetch, setSkipCompletedFetch] = useState(false);

  // Modify the fetchDailyActivities function to use a simpler approach for tracking completed activities

useEffect(() => {
  const fetchDailyActivities = async () => {
    if (!userId) {
      setLoading(false);
      return;
    }

    try {
      // Fetch all daily activities with proper API URL
      const token = localStorage.getItem('token');
      const headers = {
        'Authorization': `Bearer ${token}`
      };

      console.log('Fetching activities from:', `${API_BASE_URL}/api/activities/daily`);
      const activitiesResponse = await fetch(`${API_BASE_URL}/api/activities/daily`, {
        headers
      });

      if (!activitiesResponse.ok) {
        throw new Error(`Failed to fetch activities: ${activitiesResponse.status}`);
      }

      const dailyActivities = await activitiesResponse.json();
      console.log('Activities data:', dailyActivities);
      setActivities(dailyActivities || []);

      // Track completed activities locally in localStorage if API fails
      const localStorageKey = `user_${userId}_completed_activities_${new Date().toISOString().split('T')[0]}`;
      
      // Try to get completed activities from localStorage first as a fallback
      const localCompletedActivities = JSON.parse(localStorage.getItem(localStorageKey) || "[]");
      
      // Only attempt to fetch from server if we haven't decided to skip
      if (!skipCompletedFetch) {
        try {
          // Fetch user's completed activities for today with a timeout
          const today = new Date().toISOString().split('T')[0];
          const fetchUrl = `${API_BASE_URL}/api/activities/completed/${userId}?date=${today}`;
          console.log('Fetching completed activities from:', fetchUrl);

          const controller = new AbortController();
          const timeoutId = setTimeout(() => controller.abort(), 3000); // 3 second timeout

          try {
            const completionsResponse = await fetch(fetchUrl, {
              headers,
              signal: controller.signal
            });

            clearTimeout(timeoutId);

            if (completionsResponse.ok) {
              const completionsData = await completionsResponse.json();
              console.log('Completed activities:', completionsData);

              // Extract activity IDs with fallbacks for different casings
              const serverCompletedIds = Array.isArray(completionsData)
                ? completionsData.map(completion => {
                    if (!completion) return null;
                    return completion.activityId ||
                      completion.activityid ||
                      completion.ActivityId ||
                      (completion.activity && completion.activity.id) ||
                      null;
                  }).filter(Boolean)
                : [];
              
              // Combine server data with local data
              const allCompletedIds = [...new Set([...serverCompletedIds, ...localCompletedActivities])];
              setCompletedActivities(allCompletedIds);
              
              // Update localStorage with latest data
              localStorage.setItem(localStorageKey, JSON.stringify(allCompletedIds));
            } else {
              console.warn(`Failed to fetch completed activities: ${completionsResponse.status}`);
              
              // Use local data as fallback
              setCompletedActivities(localCompletedActivities);
              
              // Skip future fetches if we get a 500 error
              if (completionsResponse.status === 500) {
                console.log('Setting skipCompletedFetch to true due to 500 error');
                setSkipCompletedFetch(true);
              }
            }
          } catch (fetchError) {
            clearTimeout(timeoutId);
            
            if (fetchError.name === 'AbortError') {
              console.warn('Fetch for completed activities timed out');
            } else {
              console.warn('Network error fetching completed activities:', fetchError);
            }
            
            // Use local data as fallback
            setCompletedActivities(localCompletedActivities);
            
            // Skip future fetches if we have network errors
            setSkipCompletedFetch(true);
          }
        } catch (completionErr) {
          console.warn('Error in completed activities logic:', completionErr);
          // Use local data as fallback
          setCompletedActivities(localCompletedActivities);
        }
      } else {
        console.log('Skipping completed activities fetch due to previous errors');
        // Use local data when skipping server fetch
        setCompletedActivities(localCompletedActivities);
      }

      setLoading(false);
    } catch (err) {
      console.error('Error in fetchDailyActivities:', err);
      setError(err.message);
      setLoading(false);
    }
  };

  fetchDailyActivities();
}, [userId, skipCompletedFetch]);

  // Update the handleCompleteActivity function to use completedAt instead of completionDate

 const handleCompleteActivity = async (activityId) => {
  try {
    const token = localStorage.getItem('token');
    console.log(`Completing activity ${activityId} for user ${userId}`);

    // Ensure we're using UTC ISO string format for the completedAt date
    const completedAtUTC = new Date().toISOString();

    const response = await fetch(`${API_BASE_URL}/api/activities/${activityId}/complete`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        userId: userId,
        completedAt: completedAtUTC // This ensures we send a proper UTC ISO string
      })
    });

    // Store completion locally regardless of server response
    const today = new Date().toISOString().split('T')[0];
    const localStorageKey = `user_${userId}_completed_activities_${today}`;
    const localCompletedActivities = JSON.parse(localStorage.getItem(localStorageKey) || "[]");
    
    if (!localCompletedActivities.includes(activityId)) {
      localCompletedActivities.push(activityId);
      localStorage.setItem(localStorageKey, JSON.stringify(localCompletedActivities));
    }

    if (!response.ok) {
  try {
    const errorData = await response.text();
    console.error('Error completing activity:', errorData);
  } catch {
    // Ignore parse error
  }
  
  // Even if server request fails, update UI
  setCompletedActivities(prev => [...new Set([...prev, activityId])]);
  
  // Show a more user-friendly message but still alert the user
  alert(`Activity marked as completed locally. Server sync failed (${response.status})`);
  return; // Exit early but don't throw error - UI still updates
}

    // Parse the response to get updated user data
    const result = await response.json();
    console.log('Activity completed successfully:', result);

    // Update local state to reflect completion
    setCompletedActivities(prev => [...new Set([...prev, activityId])]);

    // Display success message or update UI
    alert(`Activity completed! You earned ${result.pointsEarned} points.`);
  } catch (err) {
    console.error('Error completing activity:', err);
    
    // Still update the UI even if there was an error
    setCompletedActivities(prev => {
      if (!prev.includes(activityId)) {
        return [...prev, activityId];
      }
      return prev;
    });
    
    alert('Network error. Activity marked as completed locally.');
  }
};

  if (loading) return <div className="loading-activities">Loading activities...</div>;
  if (error) return <div className="error-activities">Error: {error}</div>;
  if (!activities || !activities.length) return <div className="no-activities">No daily activities available</div>;

  return (
    <div className="daily-activities">
      {activities.slice(0, 4).map((activity) => {
        // Handle different casing in properties that may come from PostgreSQL
        const id = activity.id || activity.Id;
        const title = activity.title || activity.Title;
        const description = activity.description || activity.Description;
        const pointsValue = activity.pointsValue || activity.pointsvalue || activity.PointsValue || 0;

        // Check if activity is completed
        const isCompleted = completedActivities.includes(id);

        return (
          <div
            key={id}
            className={`activity-card ${isCompleted ? 'completed' : ''}`}
          >
            <div className="activity-content">
              <h4>{title}</h4>
              <p>{description}</p>
              <div className="activity-points">+{pointsValue} points</div>
            </div>

            <button
              className="complete-btn"
              disabled={isCompleted}
              onClick={() => handleCompleteActivity(id)}
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