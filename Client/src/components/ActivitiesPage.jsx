import { useState, useEffect } from 'react';
import './styling/ActivitiesPage.css';
import ActivityCard from './ActivityCard';

const ActivitiesPage = () => {
  const [activities, setActivities] = useState([]);
  const [categories, setCategories] = useState([]);
  const [activeCategory, setActiveCategory] = useState('all');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const userId = 1; // For demo purposes, hardcoded user ID

  useEffect(() => {
    const fetchActivities = async () => {
      try {
        const response = await fetch('/api/activities');
        
        if (!response.ok) {
          throw new Error('Failed to fetch activities');
        }
        
        const activitiesData = await response.json();
        setActivities(activitiesData);
        
        // Extract unique categories
        const uniqueCategories = [...new Set(activitiesData.map(activity => activity.category))];
        setCategories(uniqueCategories);
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };
    
    fetchActivities();
  }, []);

  const filteredActivities = activeCategory === 'all'
    ? activities
    : activities.filter(activity => activity.category === activeCategory);

  if (loading) return <div className="loading">Loading...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="activities-page">
      <h2>Sustainable Activities</h2>
      
      <div className="category-filters">
        <button 
          className={`category-btn ${activeCategory === 'all' ? 'active' : ''}`}
          onClick={() => setActiveCategory('all')}
        >
          All
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

      <div className="activities-grid">
        {filteredActivities.length > 0 ? (
          filteredActivities.map(activity => (
            <ActivityCard 
              key={activity.id} 
              activity={activity} 
              userId={userId}
            />
          ))
        ) : (
          <p className="no-activities">No activities found for this category.</p>
        )}
      </div>
    </div>
  );
};

export default ActivitiesPage;