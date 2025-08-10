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

  const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';
  const token = localStorage.getItem('token');

  const fetchSubmissions = async () => {
    setLoading(true);
    setError(null);
    try {
      const query = new URLSearchParams();
      query.append('reviewStatus', 'Pending Review');
      if (startDate) query.append('startDate', startDate);
      if (endDate) query.append('endDate', endDate);

      const response = await axios.get(`${API_URL}/api/admin/activity-completions?${query.toString()}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      setSubmissions(response.data);
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
        `${API_URL}/api/activities/review/${id}`,
        { status, adminNotes },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      setSubmissions((prev) => prev.filter((s) => s.id !== id));
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
        {submissions.map((submission) => (
          <div key={submission.id} className="submission-card">
            <h4>{submission.activityTitle}</h4>
            <p><strong>User:</strong> {submission.username}</p>
            <p><strong>Date:</strong> {new Date(submission.completedAt).toLocaleString()}</p>
            {submission.quantity && <p><strong>Quantity:</strong> {submission.quantity}</p>}
            <p><strong>Notes:</strong> {submission.notes || 'N/A'}</p>

            {submission.imageUrl && (
              <div className="submission-image">
                <img src={submission.imageUrl} alt="Evidence" />
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
        ))}
      </div>
    </div>
  );
};

export default ReviewSubmissions;
