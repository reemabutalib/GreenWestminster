import { useState, useEffect } from 'react';
import '../styling/ActivitiesPage.css';
import ActivityCard from './ActivityCard';

const ActivitiesPage = () => {
  const [activities, setActivities] = useState([]);
  const [categories, setCategories] = useState([]);
  const [activeCategory, setActiveCategory] = useState('all');
  const [activeFrequency, setActiveFrequency] = useState('all');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [showCompletionModal, setShowCompletionModal] = useState(false);
  const [selectedActivity, setSelectedActivity] = useState(null);
  const [confirmationChecked, setConfirmationChecked] = useState(false);
  const [completedAt, setCompletedAt] = useState('');
  const [notes, setNotes] = useState('');
  const [quantity, setQuantity] = useState('');
  const [image, setImage] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const [pendingActivities, setPendingActivities] = useState([]);

  const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';
  const userId = localStorage.getItem('userId') || 1;

  useEffect(() => {
    const savedPending = localStorage.getItem('pendingActivities');
    if (savedPending) {
      setPendingActivities(JSON.parse(savedPending));
    }
  }, []);

  useEffect(() => {
    localStorage.setItem('pendingActivities', JSON.stringify(pendingActivities));
  }, [pendingActivities]);

  const getPropertyValue = (obj, propNames) => {
    if (!obj) return false;
    for (const prop of propNames) {
      if (obj[prop] !== undefined) {
        if (typeof obj[prop] === 'string') return obj[prop].toLowerCase() === 'true';
        if (typeof obj[prop] === 'number') return obj[prop] === 1;
        return Boolean(obj[prop]);
      }
    }
    return false;
  };

  useEffect(() => {
    const fetchActivities = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/activities`);
        if (!response.ok) throw new Error(`Failed to fetch activities: ${response.status}`);
        const activitiesData = await response.json();
        setActivities(Array.isArray(activitiesData) ? activitiesData : []);
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

  const filteredActivities = activities.filter(activity => {
    const matchesCategory = activeCategory === 'all' || activity.category === activeCategory;

    let matchesFrequency = true;
    if (activeFrequency === 'weekly') {
      if (activity.isWeekly !== undefined) {
        matchesFrequency = Boolean(activity.isWeekly);
      } else {
        matchesFrequency = activity.isDaily === false;
      }
    } else if (activeFrequency === 'daily') {
      matchesFrequency = Boolean(activity.isDaily);
    }

    return matchesCategory && matchesFrequency;
  });

  const handleCompleteClick = (activity) => {
    setSelectedActivity(activity);
    setShowCompletionModal(true);
    setConfirmationChecked(false);
    setCompletedAt('');
    setNotes('');
    setImage(null);
    setImagePreview(null);
    setQuantity('');
  };

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setImage(file);
      const reader = new FileReader();
      reader.onloadend = () => setImagePreview(reader.result);
      reader.readAsDataURL(file);
    }
  };

  const getConfirmationText = (activity) => {
    if (getPropertyValue(activity, ['isDaily', 'isdaily', 'IsDaily', 'ISDAILY']))
      return "I confirm I completed this activity today";
    if (getPropertyValue(activity, ['isWeekly', 'isweekly', 'IsWeekly', 'ISWEEKLY']))
      return "I confirm I completed this activity this week";
    return "I confirm I completed this activity";
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!confirmationChecked) {
      const timeframe = getPropertyValue(selectedActivity, ['isDaily']) ? "today" : "this week";
      alert(`Please confirm that you completed this activity ${timeframe}.`);
      return;
    }

    if (!completedAt) {
      alert("Please select the date you completed the activity.");
      return;
    }

    // Quantity must be a positive number
    const qty = Number(quantity);
    if (Number.isNaN(qty) || qty <= 0) {
      alert("Please enter a valid quantity greater than 0.");
      return;
    }

    // Image is required now
    if (!image) {
      alert("Please upload a photo as evidence.");
      return;
    }

    setSubmitting(true);

    try {
      const isoDate = new Date(completedAt).toISOString();
      console.log("📅 Submitting completedAt:", completedAt);
      console.log("📦 ISO formatted date:", isoDate);

      const formData = new FormData();
      formData.append('userId', userId);
      formData.append('completedAt', isoDate);
      if (notes) formData.append('notes', notes);
      if (quantity) formData.append('quantity', quantity);
      if (image) formData.append('image', image);

      const response = await fetch(`${API_BASE_URL}/api/activities/${selectedActivity.id}/complete`, {
        method: 'POST',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to complete activity');
      }

      const result = await response.json();

      // Points logic
      const co2Points = Math.max(0, Math.floor((Number(result.co2eReduction) || 0) / 0.5));
      let waterPoints = 0;
      let waterSaved = result.waterSaved || 0;

      // Check if this is a water activity
      if (
        selectedActivity.category &&
        selectedActivity.category.toLowerCase().includes('water')
      ) {
        // 1 point per 10 liters saved (adjust as needed)
        waterPoints = Math.floor(Number(waterSaved) / 10);
      }

      const estimatedPoints = co2Points + waterPoints;

      alert(
        `Activity submitted for review!\n` +
        `CO₂ Reduction: ${Number(result.co2eReduction || 0).toFixed(2)} kg\n` +
        (waterSaved > 0 ? `Water Saved: ${Number(waterSaved).toFixed(2)} liters\n` : '') +
        `Estimated points on approval: ${estimatedPoints}`
      );

      // Avoid duplicate pending IDs
      setPendingActivities(prev => {
        const set = new Set(prev);
        set.add(selectedActivity.id);
        return Array.from(set);
      });

      setShowCompletionModal(false);
    } catch (err) {
      console.error('❌ Error completing activity:', err);
      alert(`Error: ${err.message}`);
    } finally {
      setSubmitting(false);
    }
  };


  if (loading) return <div className="loading">Loading...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="activities-page">
      <h2>Sustainable Activities</h2>

      <div className="frequency-filters">
        {['all', 'daily', 'weekly'].map(freq => (
          <button
            key={freq}
            className={`frequency-btn ${activeFrequency === freq ? 'active' : ''}`}
            onClick={() => setActiveFrequency(freq)}
          >
            {freq.charAt(0).toUpperCase() + freq.slice(1)}
          </button>
        ))}
      </div>

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

      <div className="activities-grid">
        {filteredActivities.length > 0 ? (
          filteredActivities.map(activity => (
            <ActivityCard
              key={activity.id || activity.Id}
              activity={activity}
              onCompleteClick={handleCompleteClick}
              isPending={pendingActivities.includes(activity.id)}
            />
          ))
        ) : (
          <p className="no-activities">No activities found for the selected filters.</p>
        )}
      </div>

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
                <label htmlFor="confirmation">{getConfirmationText(selectedActivity)}</label>
              </div>

              <div className="form-group">
                <label htmlFor="completedAt">Date Completed</label>
                <input
                  type="date"
                  id="completedAt"
                  value={completedAt}
                  onChange={(e) => setCompletedAt(e.target.value)}
                  required
                />
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
                <label htmlFor="quantity">Quantity (required for carbon impact)</label>
                <input
                  type="number"
                  id="quantity"
                  step="0.01"
                  min="0"
                  value={quantity}
                  onChange={(e) => setQuantity(e.target.value)}
                  placeholder="E.g., 20 for km, liters, or kWh"
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="image">Upload Image (required)</label>
                <input
                  type="file"
                  id="image"
                  accept="image/*"
                  onChange={handleImageChange}
                  required
                />
                <small className="image-guidance">
                  Please upload a clear photo as evidence of your activity. For example: a picture of your reusable bottle, your bike, a public transport ticket, or your completed action. Avoid uploading selfies or unrelated images.
                </small>
                {imagePreview && (
                  <div className="image-preview">
                    <img src={imagePreview} alt="Preview" />
                  </div>
                )}
              </div>

              <div className="modal-actions">
                <button type="button" className="cancel-btn" onClick={() => setShowCompletionModal(false)} disabled={submitting}>Cancel</button>
                <button
                  type="submit"
                  className="submit-btn"
                  disabled={
                    submitting ||
                    !confirmationChecked ||
                    !completedAt ||
                    !image ||
                    !quantity || Number(quantity) <= 0
                  }
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
