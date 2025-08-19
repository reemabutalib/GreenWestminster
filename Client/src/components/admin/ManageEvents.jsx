import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_BASE_URL = (
    import.meta.env.DEV
      ? ''  // dev -> use Vite proxy
      : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
  ).replace(/\/$/, '');

const ManageEvents = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editingEvent, setEditingEvent] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    location: '',
    startDate: '',          // datetime-local string
    endDate: '',            // datetime-local string (optional)
    registrationLink: '',
    organizer: '',
    maxAttendees: '',       // keep as string in state; convert on submit
    category: '',
    isVirtual: false,
    image: null
  });

  const token = localStorage.getItem('token');

  const fetchEvents = async () => {
    try {
      setLoading(true);
      const res = await axios.get(`${API_BASE_URL}/api/events`);
      setEvents(res.data);
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
      setFormData(f => ({ ...f, [name]: files?.[0] ?? null }));
    } else {
      setFormData(f => ({ ...f, [name]: type === 'checkbox' ? checked : value }));
    }
  };

  // Build FormData with correct types and omitting empty strings
  const buildEventFormData = (data, { isUpdate = false } = {}) => {
    const fd = new FormData();

    const appendIfValue = (key, val) => {
      if (val === null || val === undefined) return;
      if (typeof val === 'string' && val.trim() === '') return;
      fd.append(key, val);
    };

    // required
    appendIfValue('title', data.title?.trim());
    appendIfValue('description', data.description?.trim());

    // optional text
    appendIfValue('location', data.location?.trim());
    appendIfValue('registrationLink', data.registrationLink?.trim());
    appendIfValue('organizer', data.organizer?.trim());
    appendIfValue('category', data.category?.trim());

    // numbers
    if (data.maxAttendees !== '' && data.maxAttendees !== null && data.maxAttendees !== undefined) {
      const n = Number(data.maxAttendees);
      if (!Number.isNaN(n)) appendIfValue('maxAttendees', String(n));
    }

    // booleans (as "true"/"false")
    appendIfValue('isVirtual', data.isVirtual ? 'true' : 'false');

    // dates (ISO)
    if (data.startDate) {
      // datetime-local returns "YYYY-MM-DDTHH:mm"
      const startIso = new Date(data.startDate).toISOString();
      appendIfValue('startDate', startIso);
    }
    if (data.endDate) {
      const endIso = new Date(data.endDate).toISOString();
      appendIfValue('endDate', endIso);
    }

    // image (file)
    if (data.image instanceof File) {
      fd.append('image', data.image);
    } else if (!isUpdate) {
      // on create, if no file, do nothing (assuming image is optional)
    }
    return fd;
  };

  const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};

  const handleAddEvent = async (e) => {
    e.preventDefault();
    try {
      // quick client validation
      if (!formData.title.trim()) throw new Error('Title is required');
      if (!formData.startDate) throw new Error('Start date/time is required');
      if (formData.endDate && new Date(formData.endDate) < new Date(formData.startDate)) {
        throw new Error('End date must be after start date');
      }

      const eventFormData = buildEventFormData(formData);
      const res = await axios.post(`${API_BASE_URL}/api/events`, eventFormData, { headers: authHeaders });
      console.log('Event created:', res.data);

      resetForm();
      fetchEvents();
    } catch (err) {
      console.error('Create event failed:', {
        status: err?.response?.status,
        data: err?.response?.data,
        message: err?.message
      });
      setError(
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        err.message ||
        'Failed to add event'
      );
    }
  };

  const handleEditEvent = (event) => {
    setEditingEvent(event.id);
    const startDate = event.startDate ? new Date(event.startDate).toISOString().slice(0, 16) : '';
    const endDate = event.endDate ? new Date(event.endDate).toISOString().slice(0, 16) : '';
    setFormData({
      id: event.id,
      title: event.title ?? '',
      description: event.description ?? '',
      location: event.location ?? '',
      startDate,
      endDate,
      registrationLink: event.registrationLink ?? '',
      organizer: event.organizer ?? '',
      maxAttendees: event.maxAttendees ?? '',
      category: event.category ?? '',
      isVirtual: !!event.isVirtual,
      image: null
    });
  };

  const handleUpdateEvent = async (e) => {
    e.preventDefault();
    try {
      if (!editingEvent) return;

      if (!formData.title.trim()) throw new Error('Title is required');
      if (!formData.startDate) throw new Error('Start date/time is required');
      if (formData.endDate && new Date(formData.endDate) < new Date(formData.startDate)) {
        throw new Error('End date must be after start date');
      }

      const eventFormData = buildEventFormData(formData, { isUpdate: true });
      const res = await axios.put(`${API_BASE_URL}/api/events/${editingEvent}`, eventFormData, { headers: authHeaders });
      console.log('Event updated:', res.data);

      resetForm();
      setEditingEvent(null);
      fetchEvents();
    } catch (err) {
      console.error('Update event failed:', {
        status: err?.response?.status,
        data: err?.response?.data,
        message: err?.message
      });
      setError(
        err?.response?.data?.message ||
        err?.response?.data?.error ||
        err.message ||
        'Failed to update event'
      );
    }
  };

  const handleDeleteEvent = async (id) => {
    if (!window.confirm('Are you sure you want to delete this event?')) return;
    try {
      await axios.delete(`${API_BASE_URL}/api/events/${id}`, { headers: authHeaders });
      fetchEvents();
    } catch (err) {
      console.error('Delete event failed:', err?.response?.data || err);
      setError('Failed to delete event');
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
          <label htmlFor="registrationLink">Registration Link</label>
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
          <label htmlFor="organizer">Organiser</label>
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