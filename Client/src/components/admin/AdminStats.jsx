import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Bar, Pie, Line } from 'react-chartjs-2';
import { Chart, registerables } from 'chart.js';

// Register Chart.js components
Chart.register(...registerables);

const AdminStats = () => {
  const [userStats, setUserStats] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setIsLoading(true);
        const token = localStorage.getItem('token');
        const response = await axios.get('http://localhost:80/api/admin/user-stats', {
          headers: { Authorization: `Bearer ${token}` }
        });
        setUserStats(response.data);
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to fetch statistics');
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchStats();
  }, []);

  if (isLoading) return <div className="loading-container">Loading statistics...</div>;
  if (error) return <div className="error-message">Error: {error}</div>;
  if (!userStats) return <div>No data available</div>;

  return (
    <div className="admin-stats">
      <h2>User Engagement Dashboard</h2>
      
      <div className="stats-section">
        <h3>User Statistics</h3>
        <div className="stat-cards">
          <div className="stat-card">
            <h4>Total Users</h4>
            <p className="stat-number">{userStats.users.totalCount}</p>
          </div>
          <div className="stat-card">
            <h4>Active Users (7 Days)</h4>
            <p className="stat-number">{userStats.users.activeLast7Days}</p>
          </div>
          <div className="stat-card">
            <h4>Active Users (30 Days)</h4>
            <p className="stat-number">{userStats.users.activeLast30Days}</p>
          </div>
        </div>
      </div>
      
      <div className="stats-section">
        <h3>Activity Statistics</h3>
        <div className="stat-cards">
          <div className="stat-card">
            <h4>Total Activities</h4>
            <p className="stat-number">{userStats.activities.totalCount}</p>
          </div>
          <div className="stat-card">
            <h4>Total Completions</h4>
            <p className="stat-number">{userStats.activities.totalCompletions}</p>
          </div>
          <div className="stat-card">
            <h4>Total Events</h4>
            <p className="stat-number">{userStats.events.totalCount}</p>
          </div>
        </div>
        
        {/* Popular Activities Chart */}
        {userStats.activities.popularActivities && userStats.activities.popularActivities.length > 0 && (
          <div className="chart">
            <h4>Most Popular Activities</h4>
            <Bar 
              data={{
                labels: userStats.activities.popularActivities.map(a => a.title),
                datasets: [{
                  label: 'Completions',
                  data: userStats.activities.popularActivities.map(a => a.completionCount),
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
        
        {/* Daily Completions Chart */}
        {userStats.activities.dailyCompletions && userStats.activities.dailyCompletions.length > 0 && (
          <div className="chart">
            <h4>Activity Completions (Last 30 Days)</h4>
            <Line 
              data={{
                labels: userStats.activities.dailyCompletions.map(d => 
                  new Date(d.date).toLocaleDateString()
                ),
                datasets: [{
                  label: 'Completions',
                  data: userStats.activities.dailyCompletions.map(d => d.count),
                  fill: false,
                  borderColor: 'rgb(75, 192, 92)',
                  tension: 0.1
                }]
              }}
              options={{ 
                responsive: true,
                plugins: {
                  legend: { position: 'top' },
                  title: { display: true, text: 'Daily Activity Completions' }
                }
              }}
            />
          </div>
        )}
      </div>
      
      <div className="stats-section">
        <h3>Points Distribution</h3>
        {userStats.users.pointsDistribution && userStats.users.pointsDistribution.length > 0 && (
          <div className="chart">
            <Pie 
              data={{
                labels: userStats.users.pointsDistribution.map(d => d.pointRange),
                datasets: [{
                  label: 'Users',
                  data: userStats.users.pointsDistribution.map(d => d.userCount),
                  backgroundColor: [
                    'rgba(75, 192, 92, 0.6)',
                    'rgba(54, 162, 235, 0.6)',
                    'rgba(255, 206, 86, 0.6)',
                    'rgba(153, 102, 255, 0.6)',
                    'rgba(255, 159, 64, 0.6)',
                    'rgba(255, 99, 132, 0.6)'
                  ]
                }]
              }}
              options={{ 
                responsive: true,
                plugins: {
                  legend: { position: 'top' },
                  title: { display: true, text: 'User Points Distribution' }
                }
              }}
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
            {userStats.users.topByPoints && userStats.users.topByPoints.map((user, index) => (
              <tr key={user.id || index}>
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