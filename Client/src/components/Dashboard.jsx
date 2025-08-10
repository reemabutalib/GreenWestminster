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

// Add this at the top, after imports
const tipsByCategory = {
  Water: [
    "Fix leaky taps to save up to 90 liters a week.",
    "Use a bowl to wash veggies instead of running water.",
    "Install a low-flow showerhead to save water and energy."
  ],
  Energy: [
    "Turn off appliances at the wall to avoid standby drain.",
    "Switch to LED bulbs—they use 80% less energy.",
    "Only boil as much water as you need in the kettle."
  ],
  Transport: [
    "Try carpooling once a week to cut emissions.",
    "Switch short car trips to walking or cycling.",
    "Use public transport for your daily commute when possible."
  ],
  Waste: [
    "Compost food scraps to reduce landfill waste.",
    "Avoid single-use plastics—carry a reusable bottle.",
    "Buy in bulk to reduce packaging waste."
  ],
  Food: [
    "Reduce meat intake—go meatless once a week.",
    "Choose seasonal, local produce.",
    "Avoid food waste by planning meals and storing properly."
  ]
};


// Avatar styles for mapping avatarStyle to image
const avatarStyles = [
  { name: 'Classic', img: '/avatars/classic.png' },
  { name: 'Eco Hero', img: '/avatars/eco-hero.png' },
  { name: 'Streak Master', img: '/avatars/streak-master.png' },
];

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';
console.log("🌐 Using API_BASE_URL:", API_BASE_URL);



const Dashboard = () => {
  const { currentUser } = useAuth();
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lowTipCategories, setLowTipCategories] = useState([]);
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

  useEffect(() => {
    const fetchUserData = async () => {
      if (!currentUser) {
        setLoading(false);
        return;
      }
      const userId = currentUser.userId;
      if (!userId) {
        setError('No user ID available');
        setLoading(false);
        return;
      }
      try {
        const token = currentUser.token || localStorage.getItem('token');
        const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!response.ok) throw new Error('Failed to fetch user data');
        const userData = await response.json();
        setUserData({ ...userData, userId });

        // Fetch level info
const levelResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/level-info`, {
  headers: { 'Authorization': `Bearer ${token}` }
});
        if (levelResponse.ok) {
          const levelData = await levelResponse.json();
          setLevelInfo(levelData);
        }

     // Fetch carbon impact from your backend
     console.log(`👀 Hitting: ${API_BASE_URL}/api/activities/users/${userId}/carbon-impact`);
const impactResponse = await fetch(`${API_BASE_URL}/api/activities/users/${userId}/carbon-impact`, {
  headers: { 'Authorization': `Bearer ${token}` }
});
if (impactResponse.ok) {
  const impactData = await impactResponse.json();
  setCarbonImpact({
    co2Reduced: impactData.co2Reduced.toFixed(2),
    treesEquivalent: (impactData.co2Reduced / 21).toFixed(2),
    waterSaved: impactData.waterSaved
  });
}


        // Fetch activity stats
        const statsResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/activity-stats`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
       if (statsResponse.ok) {
  const statsData = await statsResponse.json();
  setActivityStats(statsData);

  // Identify lowest-logged categories
  const sorted = statsData.categoryCounts
    .filter(c => c && c.name)
    .sort((a, b) => a.value - b.value);
  const lowCategories = sorted.slice(0, 2).map(c => c.name);
  setLowTipCategories(lowCategories);  // You need to define this state if not done yet
}


        // Fetch points history
        const pointsResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/points-history`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (pointsResponse.ok) {
          const pointsData = await pointsResponse.json();
          setPointsHistory(pointsData);
        }

        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };

    fetchUserData();
    // eslint-disable-next-line
  }, [currentUser]);

  if (!currentUser && !loading) {
    return <Navigate to="/login" />;
  }
  if (loading) return <div className="loading-container"><div className="loading">Loading...</div></div>;
  if (error) return <div className="error-container"><div className="error">Error: {error}</div></div>;
  if (!userData) return <div className="no-user-container"><div className="no-user">No user data available</div></div>;

  // Find the correct avatar image based on avatarStyle
  const selectedAvatar = avatarStyles.find(
    (style) => style.name === userData.avatarStyle
  );

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h2>Welcome back, {userData.username}!</h2>
        <div className="dashboard-avatar-block">
          <img
            src={selectedAvatar ? selectedAvatar.img : '/avatars/classic.png'}
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

      {lowTipCategories.length > 0 && (
  <section className="tips-section">
    <div className="section-header">
      <h3>Personalised Sustainability Tips</h3>
    </div>
    <div className="tips-container">
      {lowTipCategories.map((category, idx) => (
        <div key={idx} className="tip-card">
          <h4>{category} Tips</h4>
          <ul>
            {(tipsByCategory[category] || []).slice(0, 2).map((tip, i) => (
              <li key={i}>🌱 {tip}</li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  </section>
)}


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