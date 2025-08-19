import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Bar, Pie, Line } from 'react-chartjs-2';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

const AdminStats = () => {
  const [stats, setStats] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  const API_BASE_URL = (
    import.meta.env.DEV
      ? ''  // dev -> use Vite proxy
      : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
  ).replace(/\/$/, '');

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const token = localStorage.getItem('token');
        const response = await axios.get(`${API_BASE_URL}/api/admin/user-stats`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        console.log("[AdminStats] API response:", response.data);
        setStats(response.data);
      } catch (err) {
        setError(err.message || 'Failed to fetch statistics');
      } finally {
        setIsLoading(false);
      }
    };

    fetchStats();
  }, []);

  if (isLoading) return <div>Loading stats...</div>;
  if (error) return <div>Error: {error}</div>;
  if (!stats) return <div>No data available.</div>;

  return (
    <div className="admin-stats">
      <h2>User Engagement Dashboard</h2>

      <div className="stats-section">
        <h3>User Statistics</h3>
        <div className="stat-cards">
          <div className="stat-card">
            <h4>Total Users</h4>
            <p className="stat-number">{stats.totalUsers}</p>
          </div>
          <div className="stat-card">
            <h4>Active Users (7 Days)</h4>
            <p className="stat-number">{stats.activeUsersLast7Days}</p>
          </div>
          <div className="stat-card">
            <h4>Active Users (30 Days)</h4>
            <p className="stat-number">{stats.activeUsersLast30Days}</p>
          </div>
        </div>
      </div>

      <div className="stats-section">
        <h3>Activity Statistics</h3>
        <div className="stat-cards">
          <div className="stat-card">
            <h4>Total Activities</h4>
            <p className="stat-number">{stats.totalActivities}</p>
          </div>
          <div className="stat-card">
            <h4>Total Completions</h4>
            <p className="stat-number">{stats.totalCompletions}</p>
          </div>
          <div className="stat-card">
            <h4>Total Events</h4>
            <p className="stat-number">{stats.totalEvents}</p>
          </div>
        </div>

        {stats.popularActivitiesWithDetails?.length > 0 && (
          <div className="chart">
            <h4>Most Popular Activities</h4>
            <Bar
              data={{
                labels: stats.popularActivitiesWithDetails.map(a => a.title),
                datasets: [{
                  label: 'Completions',
                  data: stats.popularActivitiesWithDetails.map(a => a.completionCount),
                  backgroundColor: 'rgba(75, 192, 92, 0.6)'
                }]
              }}
              options={{
                responsive: true,
                plugins: {
                  legend: { position: 'top' },
                  title: { display: true, text: 'Activity Popularity' }
                }
              }}
            />
          </div>
        )}
      </div>

      <div className="stats-section">
        <h3>Points Distribution</h3>
        {stats.pointsDistribution?.length > 0 && (
          <div style={{ maxWidth: 320, margin: '0 auto' }}>
            <Pie
              data={{
                labels: stats.pointsDistribution.map(d => d.pointRange),
                datasets: [{
                  label: 'Users',
                  data: stats.pointsDistribution.map(d => d.userCount),
                  backgroundColor: [
                    'rgba(75, 192, 192, 0.6)',
                    'rgba(255, 206, 86, 0.6)',
                    'rgba(153, 102, 255, 0.6)'
                  ]
                }]
              }}
              options={{
                maintainAspectRatio: false
              }}
              height={220}
              width={320}
            />
          </div>
        )}
      </div>

      <div className="stats-section">
        <h3>Top Users</h3>
        <table className="stats-table">
          <thead>
            <tr>
              <th>Username</th>
              <th>Points</th>
              <th>Current Streak</th>
              <th>Max Streak</th>
              <th>Join Date</th>
            </tr>
          </thead>
          <tbody>
            {stats.topUsers.map((user, index) => (
              <tr key={index}>
                <td>{user.username}</td>
                <td>{user.points}</td>
                <td>{user.currentStreak}</td>
                <td>{user.maxStreak}</td>
                <td>{new Date(user.joinDate).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminStats;
