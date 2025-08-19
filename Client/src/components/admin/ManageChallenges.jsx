import React, { useState, useEffect } from 'react';
import axios from 'axios';

const ManageChallenges = () => {
  const [challenges, setChallenges] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editingChallenge, setEditingChallenge] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    pointsReward: 0
  });

  const API_BASE_URL = (
    import.meta.env.DEV
      ? ''  // dev -> use Vite proxy
      : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
  ).replace(/\/$/, '');

  const fetchData = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_BASE_URL}/api/challenges`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setChallenges(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch challenges');
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
    setFormData({
      ...formData,
      [name]: type === 'number' ? parseInt(value) : value
    });
  };

  const handleAddChallenge = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      await axios.post(`${API_BASE_URL}/api/challenges`, formData, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      });
      setFormData({
        title: '',
        description: '',
        startDate: '',
        endDate: '',
        pointsReward: 0
      });
      fetchData();
    } catch (err) {
      setError('Failed to add challenge: ' + (err.response?.data?.message || err.message));
      console.error(err);
    }
  };

  const handleEditChallenge = (challenge) => {
    setEditingChallenge(challenge.id);
    const startDate = challenge.startDate ? new Date(challenge.startDate).toISOString().slice(0, 10) : '';
    const endDate = challenge.endDate ? new Date(challenge.endDate).toISOString().slice(0, 10) : '';
    setFormData({
      id: challenge.id,
      title: challenge.title,
      description: challenge.description,
      startDate: startDate,
      endDate: endDate,
      pointsTarget: challenge.pointsReward
    });
  };

  const handleUpdateChallenge = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      await axios.put(`${API_BASE_URL}/api/challenges/${editingChallenge}`, formData, {
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      });
      setEditingChallenge(null);
      setFormData({
        title: '',
        description: '',
        startDate: '',
        endDate: '',
        pointsReward: 0
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
      await axios.delete(`${API_BASE_URL}/api/challenges/${id}`, {
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
      pointsReward: 0
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
          <label htmlFor="pointsReward">Points Target</label>
          <input
            type="number"
            id="pointsReward"
            name="pointsReward"
            value={formData.pointsReward}
            onChange={handleInputChange}
            className="form-control"
            min="0"
            required
          />
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
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {challenges.map(challenge => (
              <tr key={challenge.id}>
                <td>{challenge.title}</td>
                <td>{new Date(challenge.startDate).toLocaleDateString()} - {new Date(challenge.endDate).toLocaleDateString()}</td>
                <td>{challenge.pointsReward}</td>
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
