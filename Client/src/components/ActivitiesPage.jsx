import { useState, useEffect } from 'react';
import '../styling/ActivitiesPage.css';
import ActivityCard from './ActivityCard';

const ActivitiesPage = () => {
  // State management
  const [activities, setActivities] = useState([]);
  const [categories, setCategories] = useState([]);
  const [activeCategory, setActiveCategory] = useState('all');
  const [activeFrequency, setActiveFrequency] = useState('all');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Modal state
  const [showCompletionModal, setShowCompletionModal] = useState(false);
  const [selectedActivity, setSelectedActivity] = useState(null);
  const [confirmationChecked, setConfirmationChecked] = useState(false);
  const [notes, setNotes] = useState('');
  const [image, setImage] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  // Constants
  const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';
  const userId = localStorage.getItem('userId') || 1;

  // Helper function to check properties with different case variations
  // ONLY DEFINE THIS FUNCTION ONCE
  const getPropertyValue = (obj, propNames) => {
    if (!obj) return false;
    
    for (const prop of propNames) {
      // First check if the property exists at all
      if (obj[prop] !== undefined) {
        // Convert string "true" (case insensitive) to boolean true
        if (typeof obj[prop] === 'string') {
          return obj[prop].toLowerCase() === 'true';
        }
        // Convert numeric 1 to boolean true
        if (typeof obj[prop] === 'number') {
          return obj[prop] === 1;
        }
        // Keep boolean true as true
        return Boolean(obj[prop]);
      }
    }
    return false;
  };

  // Fetch activities from the API
  useEffect(() => {
    const fetchActivities = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/activities`);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch activities: ${response.status}`);
        }
        
        const activitiesData = await response.json();
        setActivities(activitiesData);
        
        // Extract unique categories
        const uniqueCategories = [...new Set(activitiesData.map(activity => activity.category))];
        setCategories(uniqueCategories);
        setLoading(false);
      } catch (err) {
        console.error('Error fetching activities:', err);
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchActivities();
  }, [API_BASE_URL]);

 // Debug function to see exactly what's in the activities data
 useEffect(() => {
  if (activities.length > 0 && activeFrequency === 'weekly') {
    console.log('All activities count:', activities.length);
    console.log('Raw activities data:', JSON.stringify(activities, null, 2));
    
    // Check each activity for weekly properties with different formats
    activities.forEach(activity => {
      console.log(`Activity: ${activity.title}`);
      console.log(`  isWeekly (direct): ${activity.isWeekly}`);
      console.log(`  IsWeekly (capital): ${activity.IsWeekly}`);
      console.log(`  Type of isWeekly: ${typeof activity.isWeekly}`);
      // Add more comprehensive debug for all properties
      console.log(`  All properties:`, Object.keys(activity));
    });

    const weeklyActivities = activities.filter(a => 
      getPropertyValue(a, ['isWeekly', 'isweekly', 'IsWeekly', 'ISWEEKLY'])
    );
    console.log('Weekly activities found:', weeklyActivities.length);
  }
}, [activities, activeFrequency]);

 // Update your filter function to remove one-time handling
const filteredActivities = activities.filter(activity => {
  // Apply category filter
  const matchesCategory = activeCategory === 'all' || activity.category === activeCategory;
  
  // Apply frequency filter with enhanced debugging
  let matchesFrequency = true;
  
  if (activeFrequency === 'all') {
    // Keep all frequencies
    matchesFrequency = true;
  } 
  else if (activeFrequency === 'weekly') {
    // If there's no explicit isWeekly property, consider activities with isDaily=false as weekly
    if (activity.isWeekly !== undefined) {
      // If isWeekly property exists, use it directly
      matchesFrequency = Boolean(activity.isWeekly);
    } else {
      // Otherwise, infer from isDaily property
      matchesFrequency = activity.isDaily === false;
    }
    
    if (matchesFrequency) {
      console.log(`Weekly activity: ${activity.title}`);
    }
  }
  else if (activeFrequency === 'daily') {
    matchesFrequency = Boolean(activity.isDaily);
  }
  // Removed the one-time case
  
  return matchesCategory && matchesFrequency;
});

  // Handler to open the completion modal
  const handleCompleteClick = (activity) => {
    setSelectedActivity(activity);
    setShowCompletionModal(true);
    setConfirmationChecked(false);
    setNotes('');
    setImage(null);
    setImagePreview(null);
  };

  // Handler for image upload
  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setImage(file);
      
      // Create a preview URL
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  // Get confirmation text based on activity frequency
  const getConfirmationText = (activity) => {
    if (getPropertyValue(activity, ['isDaily', 'isdaily', 'IsDaily', 'ISDAILY'])) 
      return "I confirm I completed this activity today";
    
    if (getPropertyValue(activity, ['isWeekly', 'isweekly', 'IsWeekly', 'ISWEEKLY'])) 
      return "I confirm I completed this activity this week";
    
    return "I confirm I completed this activity";
  };

  // Handler for form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!confirmationChecked) {
      let timeframe = "";
      
      if (getPropertyValue(selectedActivity, ['isDaily', 'isdaily', 'IsDaily', 'ISDAILY'])) {
        timeframe = "today";
      } else if (getPropertyValue(selectedActivity, ['isWeekly', 'isweekly', 'IsWeekly', 'ISWEEKLY'])) {
        timeframe = "this week";
      }
      
      alert(`Please confirm that you completed this activity ${timeframe}.`);
      return;
    }
    
    setSubmitting(true);
    
    try {
      // Create form data for the API request
      const formData = new FormData();
      formData.append('userId', userId);
      formData.append('completedAt', new Date().toISOString());
      
      if (notes) {
        formData.append('notes', notes);
      }
      
      if (image) {
        formData.append('image', image);
      }
      
      // Send the completion request
      const response = await fetch(`${API_BASE_URL}/api/activities/${selectedActivity.id}/complete`, {
        method: 'POST',
        body: formData,
        // Important: Do not set Content-Type header when sending FormData
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to complete activity');
      }
      
      const result = await response.json();
      
      // Show success message
      alert(`Activity completed! You earned ${result.pointsEarned} points.`);
      
      // Close the modal
      setShowCompletionModal(false);
      
    } catch (err) {
      console.error('Error completing activity:', err);
      alert(`Error: ${err.message}`);
    } finally {
      setSubmitting(false);
    }
  };

  // Loading and error states
  if (loading) return <div className="loading">Loading...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  // Render the component
  return (
    <div className="activities-page">
      <h2>Sustainable Activities</h2>
      
      {/* Frequency filters */}
      <div className="frequency-filters">
  <button 
    className={`frequency-btn ${activeFrequency === 'all' ? 'active' : ''}`}
    onClick={() => setActiveFrequency('all')}
  >
    All Frequencies
  </button>
  <button 
    className={`frequency-btn ${activeFrequency === 'daily' ? 'active' : ''}`}
    onClick={() => setActiveFrequency('daily')}
  >
    Daily
  </button>
  <button 
    className={`frequency-btn ${activeFrequency === 'weekly' ? 'active' : ''}`}
    onClick={() => setActiveFrequency('weekly')}
  >
    Weekly
  </button>
</div>
      
      {/* Category filters */}
      <div className="category-filters">
        <button 
          className={`category-btn ${activeCategory === 'all' ? 'active' : ''}`}
          onClick={() => setActiveCategory('all')}
        >
          All Categories
        </button>
        
        {categories.map(category => (
          <button 
            key={category}
            className={`category-btn ${activeCategory === category ? 'active' : ''}`}
            onClick={() => setActiveCategory(category)}
          >
            {category}
          </button>
        ))}
      </div>

      {/* Activities grid */}
      <div className="activities-grid">
        {filteredActivities.length > 0 ? (
          filteredActivities.map(activity => (
            <ActivityCard 
              key={activity.id || activity.Id} 
              activity={activity}
              onCompleteClick={() => handleCompleteClick(activity)}
            />
          ))
        ) : (
          <p className="no-activities">No activities found for the selected filters.</p>
        )}
      </div>
      
      {/* Activity Completion Modal */}
      {showCompletionModal && selectedActivity && (
        <div className="modal-overlay">
          <div className="completion-modal">
            <h3>Complete Activity</h3>
            <h4>{selectedActivity.title}</h4>
            
            <form onSubmit={handleSubmit}>
              <div className="confirmation-checkbox">
                <input 
                  type="checkbox" 
                  id="confirmation" 
                  checked={confirmationChecked} 
                  onChange={(e) => setConfirmationChecked(e.target.checked)}
                />
                <label htmlFor="confirmation">
                  {getConfirmationText(selectedActivity)}
                </label>
              </div>
              
              <div className="form-group">
                <label htmlFor="notes">Notes (optional)</label>
                <textarea 
                  id="notes" 
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Add any details about how you completed this activity..."
                  rows="3"
                ></textarea>
              </div>
              
              <div className="form-group">
                <label htmlFor="image">Upload Image (optional)</label>
                <input 
                  type="file" 
                  id="image" 
                  accept="image/*"
                  onChange={handleImageChange}
                />
                
                {imagePreview && (
                  <div className="image-preview">
                    <img src={imagePreview} alt="Preview" />
                  </div>
                )}
              </div>
              
              <div className="modal-actions">
                <button 
                  type="button" 
                  className="cancel-btn" 
                  onClick={() => setShowCompletionModal(false)}
                  disabled={submitting}
                >
                  Cancel
                </button>
                <button 
                  type="submit" 
                  className="submit-btn" 
                  disabled={submitting || !confirmationChecked}
                >
                  {submitting ? 'Submitting...' : 'Submit'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ActivitiesPage;