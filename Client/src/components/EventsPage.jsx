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

  const IS_DEV = import.meta.env.DEV;

const BACKEND_ORIGIN =
  (import.meta.env.VITE_API_URL || '').replace(/\/$/, '') ||
  (import.meta.env.DEV ? 'http://localhost:5138' : '');

  function normalizeEventImageUrl(raw) {
  if (!raw) return '';

  // helper: extract "/uploads/..." from any string
  const toUploadsPath = (s) => {
    if (!s) return '';
    if (s.includes('/uploads/')) return s.slice(s.indexOf('/uploads/'));
    // treat bare filename as an event image
    const filename = s.replace(/^\/+/, '');
    return `/uploads/events/${filename}`;
  };

  try {
    // Use window.origin to resolve relative paths too
    const u = new URL(raw, window.location.origin);

    // If the host is localhost (with or without port) and we have an uploads path,
    // convert to a relative path so Vite proxy kicks in during dev.
    if (u.hostname === 'localhost' && u.pathname.includes('/uploads/')) {
      const path = toUploadsPath(u.pathname);
      return IS_DEV ? path : `${API_BASE_URL}${path}`;
    }

    // If this is already an absolute URL to your API domain, just upgrade http if needed
    if (location.protocol === 'https:' && u.protocol === 'http:') {
      u.protocol = 'https:';
    }
    return u.toString();
  } catch {
    // If constructing URL fails, treat it as relative/filename
    const path = toUploadsPath(raw);
    return IS_DEV ? path : `${API_BASE_URL}${path}`;
  }
}

// Turn whatever the server sent into a safe, loadable URL.
function normalizeImageUrl(url) {
  if (!url) return '';

  // If absolute: upgrade http->https when the page is https
  if (/^https?:\/\//i.test(url)) {
    if (window.location.protocol === 'https:' && url.startsWith('http://')) {
      return url.replace(/^http:\/\//i, 'https://');
    }
    return url;
  }

  // If relative: ensure we hit the backend origin (works in dev & prod)
  return `${BACKEND_ORIGIN}${url.startsWith('/') ? '' : '/'}${url}`;
}

/**
 * Make any backend-provided image URL safe & absolute.
 * - Accepts absolute (http/https), relative (/uploads/...), or bare filename.
 * - Rewrites http->https when page is https.
 * - Rewrites any "localhost" host to your production API host in prod.
 */
function normalizeEventImageUrl(raw) {
  if (!raw) return '';

  // If absolute
  if (/^https?:\/\//i.test(raw)) {
    // In prod, if data contains localhost, replace with your API host
    if (!import.meta.env.DEV && /\/\/localhost(:\d+)?\//i.test(raw)) {
      const path = raw.substring(raw.indexOf('/uploads/')); // keep path from /uploads
      return `${API_BASE_URL}${path}`;
    }
    // Avoid mixed content when the page is https
    if (location.protocol === 'https:' && raw.startsWith('http://')) {
      return raw.replace(/^http:\/\//i, 'https://');
    }
    return raw;
  }

  // If relative and already has /uploads/
  if (raw.includes('/uploads/')) {
    const pathFromUploads = raw.slice(raw.indexOf('/uploads/')); // ensure it starts at /uploads
    return API_BASE_URL ? `${API_BASE_URL}${pathFromUploads}` : pathFromUploads;
  }

  // Bare filename → assume it lives in /uploads/events/<file>
  const filename = raw.replace(/^\/+/, '');
  return API_BASE_URL
    ? `${API_BASE_URL}/uploads/events/${filename}`
    : `/uploads/events/${filename}`;
}
  
  useEffect(() => {
  console.log("Events sample", events.slice(0,2).map(e => e.imageUrl));

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
    <img
      src={normalizeEventImageUrl(event.imageUrl)}
      alt={event.title}
      onError={(e) => {
        console.warn('Image failed', { raw: event.imageUrl, src: e.currentTarget.src });
        // last-resort: if http and page is https, try upgrading
        if (e.currentTarget.src.startsWith('http://') && location.protocol === 'https:') {
          e.currentTarget.src = e.currentTarget.src.replace(/^http:\/\//, 'https://');
        }
      }}
    />
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
