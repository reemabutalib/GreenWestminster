import React, { useEffect, useState } from 'react';
import axios from 'axios';
import '../../styling/ReviewSubmissions.css';

const ReviewSubmissions = () => {
  const [submissions, setSubmissions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  const API_BASE_URL = (
    import.meta.env.DEV
      ? ''  // dev -> use Vite proxy
      : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
  ).replace(/\/$/, '');
  const token = localStorage.getItem('token');

  const fetchSubmissions = async () => {
  setLoading(true);
  setError(null);
  try {
    const query = new URLSearchParams();
    // default to only pending
    query.set('reviewStatus', 'Pending Review');
    if (startDate) query.set('startDate', startDate);
    if (endDate)   query.set('endDate', endDate);

    const url = `${API_BASE_URL}/api/admin/activity-completions?reviewStatus=Pending%20Review`;
    const { data } = await axios.get(
      url,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    setSubmissions(data);
  } catch (err) {
    setError(err.message || 'Failed to load submissions');
  } finally {
    setLoading(false);
  }
};


  useEffect(() => {
    fetchSubmissions();
  }, []); // Run once on mount

  const handleReview = async (id, status) => {
  const adminNotes = prompt(`Enter notes for ${status}:`, '');
  if (adminNotes === null) return;
  if (!window.confirm(`Are you sure you want to mark this submission as ${status}?`)) return;

  setSubmitting(true);
  try {
    await axios.patch(
      `${API_BASE_URL}/api/activities/review/${id}`,
      { status, adminNotes },
      { headers: { Authorization: `Bearer ${token}` } }
    );

    // Optimistic update: drop it from the list
    setSubmissions(prev => prev.filter(s => s.id !== id));

    // Optional: keep this if you want to re-sync with server
    // await fetchSubmissions();

    alert(`Submission ${status.toLowerCase()} successfully.`);
  } catch (err) {
    alert('Failed to review submission: ' + err.message);
  } finally {
    setSubmitting(false);
  }
};


  return (
    <div className="review-submissions">
      <h2>Review Activity Completions</h2>

      {/* Optional Filters */}
      <div className="filter-controls">
        <label>
          Start Date:
          <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
        </label>
        <label>
          End Date:
          <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} />
        </label>
        <button onClick={fetchSubmissions} disabled={loading}>Apply Filters</button>
      </div>

{loading && <div>Loading submissions...</div>}
{error && <div className="error-message">Error: {error}</div>}
{!loading && submissions.length === 0 && <div>No pending submissions</div>}

<div className="submission-list">
  {submissions.map((submission) => {
    // Prefer absolute imageUrl if present. Otherwise build from imagePath + API base.
    const imgSrc =
      submission.imageUrl && /^https?:\/\//i.test(submission.imageUrl.trim())
        ? submission.imageUrl.trim()
        : submission.imagePath
        ? `${API_BASE_URL.replace(/\/$/, '')}/uploads/${String(submission.imagePath).trim()}`
        : null;

    return (
      <div key={submission.id} className="submission-card">
        <h4>{submission.activityTitle}</h4>
        <p><strong>User:</strong> {submission.username}</p>
        <p><strong>Date:</strong> {new Date(submission.completedAt).toLocaleString()}</p>
        {submission.quantity && <p><strong>Quantity:</strong> {submission.quantity}</p>}
        <p><strong>Notes:</strong> {submission.notes || 'N/A'}</p>

        {imgSrc && (
          <div className="submission-image">
            <img
              src={imgSrc}
              alt="Evidence"
              onError={(e) => {
                console.error('🧩 Image failed to load:', imgSrc);
                e.currentTarget.alt = 'Image failed to load';
                e.currentTarget.style.display = 'none'; // hide broken image icon
              }}
              style={{ maxWidth: '100%', height: 'auto', display: 'block' }}
            />
          </div>
        )}

        <div className="submission-actions">
          <button
            className="approve-btn"
            onClick={() => handleReview(submission.id, 'Approved')}
            disabled={submitting}
          >
            Approve
          </button>
          <button
            className="reject-btn"
            onClick={() => handleReview(submission.id, 'Rejected')}
            disabled={submitting}
          >
            Reject
          </button>
        </div>
      </div>
    );
  })}
</div>
</div>
  );
};

export default ReviewSubmissions;
