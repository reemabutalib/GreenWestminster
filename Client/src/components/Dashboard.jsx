import { useState, useEffect } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from './context/UserContext';
import '../styling/Dashboard.css';
import StreakCounter from './StreakCounter';
import ActiveChallenges from './ActiveChallenges';
import ActivitiesPage from './ActivitiesPage';
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, Cell
} from 'recharts';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';

const Dashboard = () => {
  const { currentUser } = useAuth();
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [carbonImpact, setCarbonImpact] = useState({
    co2Reduced: 0,
    treesEquivalent: 0,
    waterSaved: 0
  });
  const [activityStats, setActivityStats] = useState({
    categoryCounts: [],
    weeklyActivity: []
  });
  const [pointsHistory, setPointsHistory] = useState([]);
  const [levelInfo, setLevelInfo] = useState({
    currentLevel: 1,
    pointsToNextLevel: 100,
    progressPercentage: 0,
    levelThresholds: [
      { level: 1, threshold: 0 },
      { level: 2, threshold: 100 },
      { level: 3, threshold: 250 },
      { level: 4, threshold: 500 },
      { level: 5, threshold: 1000 }
    ]
  });

  const COLORS = ['#4CAF50', '#8BC34A', '#CDDC39', '#FFC107', '#FF9800', '#009688'];

  // Debug: log currentUser on every render
  console.log('[Dashboard] currentUser:', currentUser);

  useEffect(() => {
    const fetchUserData = async () => {
      console.log('[Dashboard] fetchUserData called');
      if (!currentUser) {
        setLoading(false);
        return;
      }

      const userId = currentUser.userId;
      console.log('[Dashboard] Using userId:', userId);

      if (!userId) {
        setError('No user ID available');
        setLoading(false);
        return;
      }

      try {
        const token = currentUser.token || localStorage.getItem('token');
        console.log('[Dashboard] Fetching user data for userId:', userId);
        const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!response.ok) {
          let errorMessage = 'Failed to fetch user data';
          try {
            const errorData = await response.json();
            errorMessage = errorData.message || errorMessage;
          } catch {
            errorMessage = `${errorMessage} (${response.status}: ${response.statusText})`;
          }
          throw new Error(errorMessage);
        }

        const userData = await response.json();
        console.log('[Dashboard] userData from API:', userData);
        setUserData({ ...userData, userId }); // Ensure userId is present

        // Fetch level information
        try {
          const levelResponse = await fetch(`${API_BASE_URL}/api/activities/level/${userId}`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });

          if (levelResponse.ok) {
            const levelData = await levelResponse.json();
            setLevelInfo(levelData);
          } else {
            console.warn('Level info not available, calculating locally');
            const points = userData.points || 0;
            const level = calculateLevel(points);
            setLevelInfo({
              currentLevel: level,
              pointsToNextLevel: calculatePointsToNextLevel(points, level),
              progressPercentage: calculateProgressPercentage(points, level),
              levelThresholds: [
                { level: 1, threshold: 0 },
                { level: 2, threshold: 100 },
                { level: 3, threshold: 250 },
                { level: 4, threshold: 500 },
                { level: 5, threshold: 1000 }
              ]
            });
          }
        } catch (levelErr) {
          console.error('Error fetching level info:', levelErr);
          const points = userData.points || 0;
          const level = calculateLevel(points);
          setLevelInfo({
            currentLevel: level,
            pointsToNextLevel: calculatePointsToNextLevel(points, level),
            progressPercentage: calculateProgressPercentage(points, level),
            levelThresholds: [
              { level: 1, threshold: 0 },
              { level: 2, threshold: 100 },
              { level: 3, threshold: 250 },
              { level: 4, threshold: 500 },
              { level: 5, threshold: 1000 }
            ]
          });
        }

        // Fetch carbon impact data
        try {
          const carbonResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/carbon-impact`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });

          if (carbonResponse.ok) {
            const carbonData = await carbonResponse.json();
            setCarbonImpact(carbonData);
          } else {
            console.warn('Carbon impact data not available, using estimates based on points');
            setCarbonImpact({
              co2Reduced: Math.round(userData.points * 0.5),
              treesEquivalent: Math.round(userData.points * 0.02),
              waterSaved: Math.round(userData.points * 2)
            });
          }
        } catch (carbonErr) {
          console.error('Error fetching carbon impact:', carbonErr);
          setCarbonImpact({
            co2Reduced: Math.round(userData.points * 0.5),
            treesEquivalent: Math.round(userData.points * 0.02),
            waterSaved: Math.round(userData.points * 2)
          });
        }

        // Fetch activity statistics for charts
        try {
          const statsResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/activity-stats`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });

          if (statsResponse.ok) {
            const statsData = await statsResponse.json();
            setActivityStats(statsData);
          } else {
            console.warn('Activity stats not available, using sample data');
            setActivityStats({
              categoryCounts: [
                { name: 'Waste Reduction', value: 5 },
                { name: 'Transportation', value: 3 },
                { name: 'Energy', value: 4 },
                { name: 'Water Conservation', value: 2 },
                { name: 'Food', value: 1 }
              ],
              weeklyActivity: [
                { day: 'Mon', count: 2 },
                { day: 'Tue', count: 3 },
                { day: 'Wed', count: 1 },
                { day: 'Thu', count: 4 },
                { day: 'Fri', count: 2 },
                { day: 'Sat', count: 0 },
                { day: 'Sun', count: 1 }
              ]
            });
          }

          // Fetch points history
          const pointsResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/points-history`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });

          if (pointsResponse.ok) {
            const pointsData = await pointsResponse.json();
            setPointsHistory(pointsData);
          } else {
            console.warn('Points history not available, using sample data');
            const samplePoints = [
              { date: 'Week 1', points: Math.floor(Math.random() * 50) + 10 },
              { date: 'Week 2', points: Math.floor(Math.random() * 50) + 20 },
              { date: 'Week 3', points: Math.floor(Math.random() * 50) + 30 },
              { date: 'Week 4', points: Math.floor(Math.random() * 50) + 40 }
            ];
            setPointsHistory(samplePoints);
          }
        } catch (statsErr) {
          console.error('Error fetching statistics:', statsErr);
          setActivityStats({
            categoryCounts: [
              { name: 'Waste Reduction', value: 5 },
              { name: 'Transportation', value: 3 },
              { name: 'Energy', value: 4 },
              { name: 'Water Conservation', value: 2 },
              { name: 'Food', value: 1 }
            ],
            weeklyActivity: [
              { day: 'Mon', count: 2 },
              { day: 'Tue', count: 3 },
              { day: 'Wed', count: 1 },
              { day: 'Thu', count: 4 },
              { day: 'Fri', count: 2 },
              { day: 'Sat', count: 0 },
              { day: 'Sun', count: 1 }
            ]
          });
          setPointsHistory([
            { date: 'Week 1', points: 25 },
            { date: 'Week 2', points: 40 },
            { date: 'Week 3', points: 30 },
            { date: 'Week 4', points: 65 }
          ]);
        }

        setLoading(false);
      } catch (err) {
        console.error('Error fetching user data:', err);
        setError(err.message);
        setLoading(false);
      }
    };

    fetchUserData();
    // eslint-disable-next-line
  }, [currentUser]);

  // Helper functions for level calculations
  const calculateLevel = (points) => {
    if (points >= 1000) return 5;
    if (points >= 500) return 4;
    if (points >= 250) return 3;
    if (points >= 100) return 2;
    return 1;
  };

  const calculatePointsToNextLevel = (points, level) => {
    if (level === 5) return 0;
    const nextLevelThresholds = [100, 250, 500, 1000];
    return nextLevelThresholds[level - 1] - points;
  };

  const calculateProgressPercentage = (points, level) => {
    if (level === 5) return 100;
    const levelThresholds = [0, 100, 250, 500, 1000];
    const currentLevelThreshold = levelThresholds[level - 1];
    const nextLevelThreshold = levelThresholds[level];
    const pointsInCurrentLevel = points - currentLevelThreshold;
    const pointsRequiredForNextLevel = nextLevelThreshold - currentLevelThreshold;
    return Math.round((pointsInCurrentLevel / pointsRequiredForNextLevel) * 100);
  };

  if (!currentUser && !loading) {
    return <Navigate to="/login" />;
  }

  if (loading) return <div className="loading-container"><div className="loading">Loading...</div></div>;
  if (error) return <div className="error-container"><div className="error">Error: {error}</div></div>;
  if (!userData) return <div className="no-user-container"><div className="no-user">No user data available</div></div>;

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h2>Welcome back, {userData.username}!</h2>
        <div className="dashboard-avatar-block">
    <img
      src={
        userData.avatarStyle
          ? `/avatars/${userData.avatarStyle}.png`
          : '/avatars/classic.png'
      }
      alt="Your avatar"
      className="dashboard-avatar-img"
      style={{ width: 80, height: 80, borderRadius: '50%', objectFit: 'cover', marginRight: 24 }}
    />
    <Link to="/profile/avatar" className="customize-avatar-link">
      Customize Avatar
    </Link>
  </div>
        <div className="user-stats">
          <div className="stat">
            <span className="stat-value" data-testid="user-points">{userData.points}</span>
            <span className="stat-label">Points</span>
          </div>
          <StreakCounter userId={userData.userId} />
        </div>
      </header>

      {/* Level Progress Section */}
      <section className="level-section">
        <div className="section-header">
          <h3>Your Level</h3>
        </div>
        <div className="level-card">
          <div className="level-badge">
            <div className="badge-icon">👑</div>
            <div className="badge-text">Level {levelInfo.currentLevel}</div>
          </div>
          <div className="level-progress">
            <div className="progress-text">
              <span className="level-title">Level {levelInfo.currentLevel}</span>
              {levelInfo.pointsToNextLevel > 0 ? (
                <span className="points-to-next">{levelInfo.pointsToNextLevel} points to Level {levelInfo.currentLevel + 1}</span>
              ) : (
                <span className="max-level">Max Level Reached!</span>
              )}
            </div>
            <div className="progress-bar">
              <div 
                className="progress-fill" 
                style={{ width: `${levelInfo.progressPercentage}%` }}
              ></div>
            </div>
          </div>
        </div>
      </section>

      {/* Carbon Impact Section */}
      <section className="carbon-impact-section">
        <div className="section-header">
          <h3>Carbon Impact Estimate</h3>
        </div>
        <div className="carbon-impact-container">
          <div className="impact-card">
            <div className="impact-icon">🌍</div>
            <div className="impact-value">{carbonImpact.co2Reduced} kg</div>
            <div className="impact-label">CO₂ Reduced</div>
          </div>
          <div className="impact-card">
            <div className="impact-icon">🌳</div>
            <div className="impact-value">{carbonImpact.treesEquivalent}</div>
            <div className="impact-label">Trees Equivalent</div>
          </div>
          <div className="impact-card">
            <div className="impact-icon">💧</div>
            <div className="impact-value">{carbonImpact.waterSaved} L</div>
            <div className="impact-label">Water Saved</div>
          </div>
        </div>
      </section>

      {/* Progress Charts Section */}
      <section className="progress-section">
        <div className="section-header">
          <h3>Your Progress</h3>
        </div>
        <div className="charts-container">
          <div className="chart-row">
            {/* Category Distribution Chart */}
            <div className="chart-card">
              <h4>Activities by Category</h4>
              <ResponsiveContainer width="100%" height={200}>
                <PieChart>
                  <Pie
                    data={activityStats.categoryCounts}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                    nameKey="name"
                    label={({ name, percent }) => {
                      const percentValue = (percent * 100).toFixed(0);
                      return name?.length > 10 ? `${percentValue}%` : `${name}: ${percentValue}%`;
                    }}
                  >
                    {activityStats.categoryCounts.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value, name) => [`${value} activities`, name]} />
                  <Legend layout="vertical" verticalAlign="middle" align="right" />
                </PieChart>
              </ResponsiveContainer>
            </div>

            {/* Weekly Activity Chart */}
            <div className="chart-card">
              <h4>Weekly Activity</h4>
              <ResponsiveContainer width="100%" height={200}>
                <BarChart
                  data={activityStats.weeklyActivity}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="day" />
                  <YAxis />
                  <Tooltip formatter={(value) => [`${value} activities`]} />
                  <Bar dataKey="count" fill="#4CAF50" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Points Progress Chart */}
          <div className="chart-card full-width">
            <h4>Points Progress</h4>
            <ResponsiveContainer width="100%" height={200}>
              <LineChart
                data={pointsHistory}
                margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis />
                <Tooltip formatter={(value) => [`${value} points`]} />
                <Legend />
                <Line type="monotone" dataKey="points" stroke="#2E7D32" strokeWidth={2} dot={{ r: 4 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </section>

      <section className="dashboard-content">
        <div className="section-header">
          <h3>Log Your Actions</h3>
          <Link to="/activities" className="view-all">View All</Link>
        </div>
        <ActivitiesPage userId={userData.userId} />

        <div className="section-header">
          <h3>Active Challenges</h3>
          <Link to="/challenges" className="view-all">View All</Link>
        </div>
        <ActiveChallenges userId={userData.userId} />
      </section>
    </div>
  );
};

export default Dashboard;