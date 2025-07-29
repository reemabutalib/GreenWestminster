import React, { useState, useEffect } from 'react';
import axios from 'axios';

const ManageEvents = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editingEvent, setEditingEvent] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    location: '',
    startDate: '',
    endDate: '',
    registrationLink: '',
    organizer: '',
    maxAttendees: '',
    category: '',
    isVirtual: false,
    image: null
  });

  const fetchEvents = async () => {
    try {
      setLoading(true);
      const response = await axios.get('http://localhost:80/api/events');
      setEvents(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch events');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEvents();
  }, []);

  const handleInputChange = (e) => {
    const { name, value, type, checked, files } = e.target;
    
    if (type === 'file') {
      setFormData({
        ...formData,
        [name]: files[0]
      });
    } else {
      setFormData({
        ...formData,
        [name]: type === 'checkbox' ? checked : value
      });
    }
  };

  const handleAddEvent = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      
      // Create FormData object for file upload
      const eventFormData = new FormData();
      for (const key in formData) {
        if (formData[key] !== null && formData[key] !== undefined) {
          eventFormData.append(key, formData[key]);
        }
      }
      
      await axios.post('http://localhost:80/api/events', eventFormData, {
        headers: {
          'Content-Type': 'multipart/form-data',
          Authorization: `Bearer ${token}`
        }
      });
      
      // Reset form and refresh events
      resetForm();
      fetchEvents();
    } catch (err) {
      setError('Failed to add event: ' + (err.response?.data?.message || err.message));
      console.error(err);
    }
  };

  const handleEditEvent = (event) => {
    setEditingEvent(event.id);
    
    // Format dates for form inputs
    const startDate = event.startDate ? new Date(event.startDate).toISOString().slice(0, 16) : '';
    const endDate = event.endDate ? new Date(event.endDate).toISOString().slice(0, 16) : '';
    
    setFormData({
      id: event.id,
      title: event.title,
      description: event.description,
      location: event.location || '',
      startDate: startDate,
      endDate: endDate,
      registrationLink: event.registrationLink || '',
      organizer: event.organizer || '',
      maxAttendees: event.maxAttendees || '',
      category: event.category || '',
      isVirtual: event.isVirtual || false,
      image: null // Can't pre-populate file input
    });
  };

  const handleUpdateEvent = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      
      // Create FormData object for file upload
      const eventFormData = new FormData();
      for (const key in formData) {
        if (formData[key] !== null && formData[key] !== undefined && key !== 'id') {
          eventFormData.append(key, formData[key]);
        }
      }
      
      await axios.put(`http://localhost:80/api/events/${editingEvent}`, eventFormData, {
        headers: {
          'Content-Type': 'multipart/form-data',
          Authorization: `Bearer ${token}`
        }
      });
      
      // Reset editing state and refresh events
      resetForm();
      setEditingEvent(null);
      fetchEvents();
    } catch (err) {
      setError('Failed to update event: ' + (err.response?.data?.message || err.message));
      console.error(err);
    }
  };

  const handleDeleteEvent = async (id) => {
    if (!window.confirm('Are you sure you want to delete this event?')) {
      return;
    }
    
    try {
      const token = localStorage.getItem('token');
      await axios.delete(`http://localhost:80/api/events/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      fetchEvents();
    } catch (err) {
      setError('Failed to delete event');
      console.error(err);
    }
  };

  const resetForm = () => {
    setFormData({
      title: '',
      description: '',
      location: '',
      startDate: '',
      endDate: '',
      registrationLink: '',
      organizer: '',
      maxAttendees: '',
      category: '',
      isVirtual: false,
      image: null
    });
  };

  const handleCancelEdit = () => {
    setEditingEvent(null);
    resetForm();
  };

  return (
    <div className="manage-events">
      <h2>{editingEvent ? 'Edit Event' : 'Add New Event'}</h2>
      
      {error && <div className="error-message">{error}</div>}
      
      <form onSubmit={editingEvent ? handleUpdateEvent : handleAddEvent} className="form-section">
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
          <label htmlFor="location">Location</label>
          <input
            type="text"
            id="location"
            name="location"
            value={formData.location}
            onChange={handleInputChange}
            className="form-control"
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="startDate">Start Date & Time</label>
          <input
            type="datetime-local"
            id="startDate"
            name="startDate"
            value={formData.startDate}
            onChange={handleInputChange}
            className="form-control"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="endDate">End Date & Time (Optional)</label>
          <input
            type="datetime-local"
            id="endDate"
            name="endDate"
            value={formData.endDate}
            onChange={handleInputChange}
            className="form-control"
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="registrationLink">Registration Link (Optional)</label>
          <input
            type="url"
            id="registrationLink"
            name="registrationLink"
            value={formData.registrationLink}
            onChange={handleInputChange}
            className="form-control"
            placeholder="https://"
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="organizer">Organizer (Optional)</label>
          <input
            type="text"
            id="organizer"
            name="organizer"
            value={formData.organizer}
            onChange={handleInputChange}
            className="form-control"
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="maxAttendees">Maximum Attendees (Optional)</label>
          <input
            type="number"
            id="maxAttendees"
            name="maxAttendees"
            value={formData.maxAttendees}
            onChange={handleInputChange}
            className="form-control"
            min="0"
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
          >
            <option value="">Select a Category</option>
            <option value="Workshop">Workshop</option>
            <option value="Seminar">Seminar</option>
            <option value="Volunteer">Volunteer</option>
            <option value="Education">Education</option>
            <option value="Campus Event">Campus Event</option>
            <option value="Other">Other</option>
          </select>
        </div>
        
        <div className="form-group checkboxes">
          <div>
            <input
              type="checkbox"
              id="isVirtual"
              name="isVirtual"
              checked={formData.isVirtual}
              onChange={handleInputChange}
            />
            <label htmlFor="isVirtual">Virtual Event</label>
          </div>
        </div>
        
        <div className="form-group">
          <label htmlFor="image">Event Image {editingEvent ? '(Leave blank to keep current image)' : ''}</label>
          <input
            type="file"
            id="image"
            name="image"
            accept="image/*"
            onChange={handleInputChange}
            className="form-control"
          />
        </div>
        
        <div className="form-buttons">
          {editingEvent ? (
            <>
              <button type="submit" className="btn btn-primary">Update Event</button>
              <button type="button" className="btn btn-secondary" onClick={handleCancelEdit}>
                Cancel
              </button>
            </>
          ) : (
            <button type="submit" className="btn btn-primary">Add Event</button>
          )}
        </div>
      </form>
      
      <h2>All Events</h2>
      {loading ? (
        <div className="loading-container">Loading events...</div>
      ) : (
        <table className="stats-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Date</th>
              <th>Location</th>
              <th>Category</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {events.map(event => (
              <tr key={event.id}>
                <td>{event.title}</td>
                <td>{new Date(event.startDate).toLocaleDateString()} {event.endDate ? `- ${new Date(event.endDate).toLocaleDateString()}` : ''}</td>
                <td>{event.isVirtual ? 'Virtual' : event.location || 'N/A'}</td>
                <td>{event.category || 'N/A'}</td>
                <td className="table-actions">
                  <button 
                    className="btn btn-secondary"
                    onClick={() => handleEditEvent(event)}
                  >
                    Edit
                  </button>
                  <button 
                    className="btn btn-danger"
                    onClick={() => handleDeleteEvent(event.id)}
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

export default ManageEvents;