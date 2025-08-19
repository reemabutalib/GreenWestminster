import React, { useState, useEffect } from 'react';
import axios from 'axios';
import '../styling/EventsPage.css';

const EventsPage = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filter, setFilter] = useState('upcoming');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [categories, setCategories] = useState([]);

  const API_BASE_URL = (
    import.meta.env.DEV
      ? ''  // dev -> use Vite proxy
      : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
  ).replace(/\/$/, '');
  
  useEffect(() => {
    const fetchEvents = async () => {
      try {
        setLoading(true);
        const endpoint = filter === 'upcoming' ? '/api/events/upcoming' : '/api/events';
        const response = await axios.get(`${API_BASE_URL}${endpoint}`);
        
        let filteredEvents = response.data;
        
        // Extract unique categories
        const uniqueCategories = [...new Set(filteredEvents
          .filter(event => event.category)
          .map(event => event.category))];
        setCategories(uniqueCategories);
        
        // Apply category filter if not 'all'
        if (categoryFilter !== 'all') {
          filteredEvents = filteredEvents.filter(event => event.category === categoryFilter);
        }
        
        setEvents(filteredEvents);
        setError(null);
      } catch (err) {
        setError('Failed to load events. Please try again later.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchEvents();
  }, [filter, categoryFilter]);
  
  const handleFilterChange = (newFilter) => {
    setFilter(newFilter);
  };
  
  const handleCategoryChange = (e) => {
    setCategoryFilter(e.target.value);
  };
  
  const formatEventDate = (startDate, endDate) => {
    const start = new Date(startDate);
    
    if (!endDate) {
      return start.toLocaleDateString(undefined, {
        weekday: 'short',
        day: 'numeric',
        month: 'short',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit'
      });
    }
    
    const end = new Date(endDate);
    
    // Same day event
    if (start.toDateString() === end.toDateString()) {
      return `${start.toLocaleDateString(undefined, {
        weekday: 'short',
        day: 'numeric',
        month: 'short',
        year: 'numeric'
      })}, ${start.toLocaleTimeString(undefined, {
        hour: 'numeric',
        minute: '2-digit'
      })} - ${end.toLocaleTimeString(undefined, {
        hour: 'numeric',
        minute: '2-digit'
      })}`;
    }
    
    // Multi-day event
    return `${start.toLocaleDateString(undefined, {
      weekday: 'short',
      day: 'numeric',
      month: 'short'
    })} - ${end.toLocaleDateString(undefined, {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
      year: 'numeric'
    })}`;
  };
  
  return (
    <div className="events-page">
      <div className="events-header">
        <h1>Sustainability Events</h1>
        <p>Join us for upcoming sustainability events and activities at Westminster.</p>
      </div>
      
      <div className="events-filters">
        <div className="filter-buttons">
          <button 
            className={filter === 'upcoming' ? 'filter-btn active' : 'filter-btn'}
            onClick={() => handleFilterChange('upcoming')}
          >
            Upcoming Events
          </button>
          <button 
            className={filter === 'all' ? 'filter-btn active' : 'filter-btn'}
            onClick={() => handleFilterChange('all')}
          >
            All Events
          </button>
        </div>
        
        {categories.length > 0 && (
          <div className="category-filter">
            <label htmlFor="category-select">Filter by Category:</label>
            <select 
              id="category-select" 
              value={categoryFilter} 
              onChange={handleCategoryChange}
            >
              <option value="all">All Categories</option>
              {categories.map(category => (
                <option key={category} value={category}>{category}</option>
              ))}
            </select>
          </div>
        )}
      </div>
      
      {loading ? (
        <div className="events-loading">Loading events...</div>
      ) : error ? (
        <div className="events-error">{error}</div>
      ) : events.length === 0 ? (
        <div className="no-events">
          <p>No events found. Check back soon for upcoming sustainability activities!</p>
        </div>
      ) : (
        <div className="events-grid">
          {events.map(event => (
            <div key={event.id} className="event-card">
              {event.imageUrl && (
                <div className="event-image">
                  <img src={event.imageUrl} alt={event.title} />
                </div>
              )}
              <div className="event-content">
                <h3 className="event-title">{event.title}</h3>
                <p className="event-date">
                  <i className="fas fa-calendar-alt"></i> 
                  {formatEventDate(event.startDate, event.endDate)}
                </p>
                <p className="event-location">
                  <i className="fas fa-map-marker-alt"></i> 
                  {event.isVirtual ? 'Virtual Event' : (event.location || 'TBD')}
                </p>
                {event.category && (
                  <span className="event-category">{event.category}</span>
                )}
                <p className="event-description">{event.description}</p>
                {event.registrationLink && (
                  <a 
                    href={event.registrationLink} 
                    className="event-register" 
                    target="_blank" 
                    rel="noopener noreferrer"
                  >
                    Register Now
                  </a>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default EventsPage;
