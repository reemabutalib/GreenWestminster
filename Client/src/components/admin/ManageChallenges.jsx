import React, { useState, useEffect } from 'react';
import axios from 'axios';

const ManageChallenges = () => {
  const [challenges, setChallenges] = useState([]);
  const [activities, setActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editingChallenge, setEditingChallenge] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    pointsTarget: 0,
    activityIds: []
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      
      const [challengesResponse, activitiesResponse] = await Promise.all([
        axios.get('http://localhost:80/api/challenges', {
          headers: { Authorization: `Bearer ${token}` }
        }),
        axios.get('http://localhost:80/api/activities', {
          headers: { Authorization: `Bearer ${token}` }
        })
      ]);
      
      setChallenges(challengesResponse.data);
      setActivities(activitiesResponse.data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleInputChange = (e) => {
    const { name, value, type } = e.target;
    
    if (name === 'activityIds') {
      // Handle multiple select
      const selectedOptions = Array.from(e.target.selectedOptions, option => parseInt(option.value));
      setFormData({
        ...formData,
        activityIds: selectedOptions
      });
    } else {
      setFormData({
        ...formData,
        [name]: type === 'number' ? parseInt(value) : value
      });
    }
  };

  const handleAddChallenge = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      await axios.post('http://localhost:80/api/challenges', formData, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      });
      
      // Reset form and refresh challenges
      setFormData({
        title: '',
        description: '',
        startDate: '',
        endDate: '',
        pointsTarget: 0,
        activityIds: []
      });
      fetchData();
    } catch (err) {
      setError('Failed to add challenge: ' + (err.response?.data?.message || err.message));
      console.error(err);
    }
  };

  const handleEditChallenge = (challenge) => {
    setEditingChallenge(challenge.id);
    
    // Format dates for form inputs
    const startDate = challenge.startDate ? new Date(challenge.startDate).toISOString().slice(0, 10) : '';
    const endDate = challenge.endDate ? new Date(challenge.endDate).toISOString().slice(0, 10) : '';
    
    setFormData({
      id: challenge.id,
      title: challenge.title,
      description: challenge.description,
      startDate: startDate,
      endDate: endDate,
      pointsTarget: challenge.pointsTarget,
      activityIds: challenge.activities ? challenge.activities.map(a => a.id) : []
    });
  };

  const handleUpdateChallenge = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      await axios.put(`http://localhost:80/api/challenges/${editingChallenge}`, formData, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      });
      
      // Reset editing state and refresh challenges
      setEditingChallenge(null);
      setFormData({
        title: '',
        description: '',
        startDate: '',
        endDate: '',
        pointsTarget: 0,
        activityIds: []
      });
      fetchData();
    } catch (err) {
      setError('Failed to update challenge: ' + (err.response?.data?.message || err.message));
      console.error(err);
    }
  };

  const handleDeleteChallenge = async (id) => {
    if (!window.confirm('Are you sure you want to delete this challenge?')) {
      return;
    }
    
    try {
      const token = localStorage.getItem('token');
      await axios.delete(`http://localhost:80/api/challenges/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      fetchData();
    } catch (err) {
      setError('Failed to delete challenge');
      console.error(err);
    }
  };

  const handleCancelEdit = () => {
    setEditingChallenge(null);
    setFormData({
      title: '',
      description: '',
      startDate: '',
      endDate: '',
      pointsTarget: 0,
      activityIds: []
    });
  };

  return (
    <div className="manage-challenges">
      <h2>{editingChallenge ? 'Edit Challenge' : 'Add New Challenge'}</h2>
      
      {error && <div className="error-message">{error}</div>}
      
      <form onSubmit={editingChallenge ? handleUpdateChallenge : handleAddChallenge} className="form-section">
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
          <label htmlFor="startDate">Start Date</label>
          <input
            type="date"
            id="startDate"
            name="startDate"
            value={formData.startDate}
            onChange={handleInputChange}
            className="form-control"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="endDate">End Date</label>
          <input
            type="date"
            id="endDate"
            name="endDate"
            value={formData.endDate}
            onChange={handleInputChange}
            className="form-control"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="pointsTarget">Points Target</label>
          <input
            type="number"
            id="pointsTarget"
            name="pointsTarget"
            value={formData.pointsTarget}
            onChange={handleInputChange}
            className="form-control"
            min="0"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="activityIds">Associated Activities (Hold Ctrl/Cmd to select multiple)</label>
          <select
            id="activityIds"
            name="activityIds"
            multiple
            value={formData.activityIds}
            onChange={handleInputChange}
            className="form-control"
            size="5"
          >
            {activities.map(activity => (
              <option key={activity.id} value={activity.id}>
                {activity.title} ({activity.category}, {activity.pointsValue} pts)
              </option>
            ))}
          </select>
        </div>
        
        <div className="form-buttons">
          {editingChallenge ? (
            <>
              <button type="submit" className="btn btn-primary">Update Challenge</button>
              <button type="button" className="btn btn-secondary" onClick={handleCancelEdit}>
                Cancel
              </button>
            </>
          ) : (
            <button type="submit" className="btn btn-primary">Add Challenge</button>
          )}
        </div>
      </form>
      
      <h2>All Challenges</h2>
      {loading ? (
        <div className="loading-container">Loading challenges...</div>
      ) : (
        <table className="stats-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Date Range</th>
              <th>Points Target</th>
              <th>Activities</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {challenges.map(challenge => (
              <tr key={challenge.id}>
                <td>{challenge.title}</td>
                <td>{new Date(challenge.startDate).toLocaleDateString()} - {new Date(challenge.endDate).toLocaleDateString()}</td>
                <td>{challenge.pointsTarget}</td>
                <td>{challenge.activities ? challenge.activities.length : 0}</td>
                <td className="table-actions">
                  <button 
                    className="btn btn-secondary"
                    onClick={() => handleEditChallenge(challenge)}
                  >
                    Edit
                  </button>
                  <button 
                    className="btn btn-danger"
                    onClick={() => handleDeleteChallenge(challenge.id)}
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

export default ManageChallenges;