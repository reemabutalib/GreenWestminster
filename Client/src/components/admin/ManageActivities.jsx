import React, { useState, useEffect } from 'react';
import axios from 'axios';

const ManageActivities = () => {
  const [activities, setActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editingActivity, setEditingActivity] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    pointsValue: 0,
    category: '',
    isDaily: false,
    isWeekly: false,
    isOneTime: false
  });

  const fetchActivities = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      const response = await axios.get('http://localhost:80/api/activities', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setActivities(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch activities');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchActivities();
  }, []);

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });
  };

  const handleAddActivity = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      await axios.post('http://localhost:80/api/activities', formData, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      });
      
      // Reset form and refresh activities
      setFormData({
        title: '',
        description: '',
        pointsValue: 0,
        category: '',
        isDaily: false,
        isWeekly: false,
        isOneTime: false
      });
      fetchActivities();
    } catch (err) {
      setError('Failed to add activity');
      console.error(err);
    }
  };

  const handleEditActivity = (activity) => {
    setEditingActivity(activity.id);
    setFormData({
      id: activity.id,
      title: activity.title,
      description: activity.description,
      pointsValue: activity.pointsValue,
      category: activity.category,
      isDaily: activity.isDaily,
      isWeekly: activity.isWeekly,
      isOneTime: activity.isOneTime
    });
  };

  const handleUpdateActivity = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      await axios.put('http://localhost:80/api/activities', formData, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      });
      
      // Reset editing state and refresh activities
      setEditingActivity(null);
      setFormData({
        title: '',
        description: '',
        pointsValue: 0,
        category: '',
        isDaily: false,
        isWeekly: false,
        isOneTime: false
      });
      fetchActivities();
    } catch (err) {
      setError('Failed to update activity');
      console.error(err);
    }
  };

  const handleDeleteActivity = async (id) => {
    if (!window.confirm('Are you sure you want to delete this activity?')) {
      return;
    }
    
    try {
      const token = localStorage.getItem('token');
      await axios.delete(`http://localhost:80/api/activities/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      fetchActivities();
    } catch (err) {
      setError('Failed to delete activity');
      console.error(err);
    }
  };

  const handleCancelEdit = () => {
    setEditingActivity(null);
    setFormData({
      title: '',
      description: '',
      pointsValue: 0,
      category: '',
      isDaily: false,
      isWeekly: false,
      isOneTime: false
    });
  };

  return (
    <div className="manage-activities">
      <h2>{editingActivity ? 'Edit Activity' : 'Add New Activity'}</h2>
      
      {error && <div className="error-message">{error}</div>}
      
      <form onSubmit={editingActivity ? handleUpdateActivity : handleAddActivity} className="form-section">
        <div className="form-group">
          <label htmlFor="title">Title</label>
          <input
            type="text"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleInputChange}
            className="form-control"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleInputChange}
            className="form-control"
            rows="4"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="pointsValue">Points</label>
          <input
            type="number"
            id="pointsValue"
            name="pointsValue"
            value={formData.pointsValue}
            onChange={handleInputChange}
            className="form-control"
            min="0"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="category">Category</label>
          <select
            id="category"
            name="category"
            value={formData.category}
            onChange={handleInputChange}
            className="form-control"
            required
          >
            <option value="">Select a Category</option>
            <option value="Energy">Energy</option>
            <option value="Water">Water</option>
            <option value="Waste Reduction">Waste Reduction</option>
            <option value="Transportation">Transportation</option>
            <option value="Food">Food</option>
            <option value="Education">Education</option>
          </select>
        </div>
        
        <div className="form-group checkboxes">
          <div>
            <input
              type="checkbox"
              id="isDaily"
              name="isDaily"
              checked={formData.isDaily}
              onChange={handleInputChange}
            />
            <label htmlFor="isDaily">Daily Activity</label>
          </div>
          
          <div>
            <input
              type="checkbox"
              id="isWeekly"
              name="isWeekly"
              checked={formData.isWeekly}
              onChange={handleInputChange}
            />
            <label htmlFor="isWeekly">Weekly Activity</label>
          </div>
          
          <div>
            <input
              type="checkbox"
              id="isOneTime"
              name="isOneTime"
              checked={formData.isOneTime}
              onChange={handleInputChange}
            />
            <label htmlFor="isOneTime">One-Time Activity</label>
          </div>
        </div>
        
        <div className="form-buttons">
          {editingActivity ? (
            <>
              <button type="submit" className="btn btn-primary">Update Activity</button>
              <button type="button" className="btn btn-secondary" onClick={handleCancelEdit}>
                Cancel
              </button>
            </>
          ) : (
            <button type="submit" className="btn btn-primary">Add Activity</button>
          )}
        </div>
      </form>
      
      <h2>All Activities</h2>
      {loading ? (
        <div className="loading-container">Loading activities...</div>
      ) : (
        <table className="stats-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Title</th>
              <th>Category</th>
              <th>Points</th>
              <th>Frequency</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {activities.map(activity => (
              <tr key={activity.id}>
                <td>{activity.id}</td>
                <td>{activity.title}</td>
                <td>{activity.category}</td>
                <td>{activity.pointsValue}</td>
                <td>
                  {activity.isDaily ? 'Daily' : ''}
                  {activity.isWeekly ? (activity.isDaily ? ', Weekly' : 'Weekly') : ''}
                  {activity.isOneTime ? (activity.isDaily || activity.isWeekly ? ', One-Time' : 'One-Time') : ''}
                </td>
                <td className="table-actions">
                  <button 
                    className="btn btn-secondary"
                    onClick={() => handleEditActivity(activity)}
                  >
                    Edit
                  </button>
                  <button 
                    className="btn btn-danger"
                    onClick={() => handleDeleteActivity(activity.id)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default ManageActivities;