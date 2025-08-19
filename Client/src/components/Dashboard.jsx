import { useState, useEffect, useMemo } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from './context/UserContext';
import '../styling/Dashboard.css';
import StreakCounter from './StreakCounter';

import {
  LineChart, Line, BarChart, Bar, PieChart, Pie,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, Cell
} from 'recharts';

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

const avatarStyles = [
  { name: 'Sprout Seeker', img: '/avatars/bronze-sprout-seeker.png' },
  { name: 'Seedling Adventurer', img: '/avatars/bronze-seedling-adventurer.png' },
  { name: 'Eco Explorer', img: '/avatars/bronze-eco-explorer.png' },
  { name: 'Leaf Guardian', img: '/avatars/silver-leaf-guardian.png' },
  { name: 'Eco Pathfinder', img: '/avatars/silver-eco-pathfinder.png' },
  { name: 'River Protector', img: '/avatars/silver-river-protector.png' },
  { name: 'Sunlight Steward', img: '/avatars/gold-sunlight-steward.png' },
  { name: 'Forest Champion', img: '/avatars/gold-forest-champion.png' },
  { name: 'Wildlife Guardian', img: '/avatars/gold-wildlife-guardian.png' },
  { name: 'Planet Protector', img: '/avatars/platinum-planet-protector.png' },
  { name: 'Harmony Keeper', img: '/avatars/platinum-harmony-keeper.png' },
  { name: "Gaia's Guardian", img: '/avatars/platinum-gaias-guardian.png' },
];

const API_BASE_URL = (
  import.meta.env.DEV
    ? ''
    : (import.meta.env.VITE_API_URL || 'https://greenwestminster.onrender.com')
).replace(/\/$/, '');

// helpers to persist dismissed approvals
const getIdSet = (key) => {
  try { return new Set(JSON.parse(localStorage.getItem(key) || '[]')); }
  catch { return new Set(); }
};
const setIdSet = (key, set) => localStorage.setItem(key, JSON.stringify(Array.from(set)));

const formatDateTime = (d) =>
  new Date(d).toLocaleString(undefined, {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });

const Dashboard = () => {
  const { currentUser } = useAuth();

  // toast
  const [toast, setToast] = useState(null);
  const showToast = (msg) => {
    setToast(msg);
    window.clearTimeout(showToast._t);
    showToast._t = window.setTimeout(() => setToast(null), 3000);
  };

  // core state
  const [userData, setUserData] = useState(null);
  const [levelInfo, setLevelInfo] = useState({
    currentLevel: "Bronze",
    pointsToNextLevel: 100,
    progressPercentage: 0,
    nextLevel: "Silver",
    levelThresholds: [
      { level: "Bronze", threshold: 0 },
      { level: "Silver", threshold: 500 },
      { level: "Gold", threshold: 1000 },
      { level: "Platinum", threshold: 5000 }
    ]
  });

  const [activityStats, setActivityStats] = useState({ categoryCounts: [], weeklyActivity: [] });
  const [pointsHistory, setPointsHistory] = useState([]);
  const [carbonImpact, setCarbonImpact] = useState({ co2Reduced: 0, treesEquivalent: 0, waterSaved: 0 });

  const [lowTipCategories, setLowTipCategories] = useState([]);
  const [pendingItems, setPendingItems] = useState([]);
  const [allCompletions, setAllCompletions] = useState([]);
  const [userChallenges, setUserChallenges] = useState([]);

  const [resubmitOpen, setResubmitOpen] = useState(false);
  const [resubmitTarget, setResubmitTarget] = useState(null);
  const [resubmitNotes, setResubmitNotes] = useState('');
  const [resubmitFile, setResubmitFile] = useState(null);
  const [resubmitting, setResubmitting] = useState(false);

  const [dismissedApproved, setDismissedApproved] = useState(() => getIdSet('approvedDismissedIds'));

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const token = useMemo(() => (currentUser?.token || localStorage.getItem('token') || ''), [currentUser]);

// Reveal-on-scroll (fail-safe + handles late-mounted nodes)
useEffect(() => {
  const body = document.body;
  body.classList.add('enable-reveal');

  const prefersReduced =
    window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches;

  // Helper: reveal immediately (fallback / reduced motion)
  const revealAllNow = () => {
    document.querySelectorAll('[data-reveal]').forEach((n) => {
      n.classList.add('is-revealed');
    });
  };

  if (prefersReduced || typeof IntersectionObserver === 'undefined') {
    revealAllNow();
    return () => body.classList.remove('enable-reveal');
  }

  // IO that reveals elements as they enter viewport
  const io = new IntersectionObserver(
    (entries) => {
      entries.forEach((e) => {
        if (e.isIntersecting) {
          e.target.classList.add('is-revealed');
          io.unobserve(e.target);
        }
      });
    },
    { threshold: 0.12, rootMargin: '0px 0px -10% 0px' }
  );

  // Observe any element we find (and don't re-observe already revealed)
  const observe = (el) => {
    if (!el || !(el instanceof Element)) return;
    if (el.classList.contains('is-revealed')) return;
    io.observe(el);
  };

  // Observe what exists now…
  document.querySelectorAll('[data-reveal]').forEach(observe);

  // …and also anything that appears later (after data finishes loading)
  const mo = new MutationObserver((mutations) => {
    for (const m of mutations) {
      m.addedNodes.forEach((node) => {
        if (!(node instanceof Element)) return;
        if (node.hasAttribute('data-reveal')) observe(node);
        node.querySelectorAll?.('[data-reveal]').forEach(observe);
      });
    }
  });
  mo.observe(document.body, { childList: true, subtree: true });

  // Cleanup
  return () => {
    mo.disconnect();
    io.disconnect();
    body.classList.remove('enable-reveal');
  };
}, []);



  /* =======================
     Data fetching
     ======================= */
  useEffect(() => {
    if (!currentUser) { setLoading(false); return; }
    const userId = currentUser.userId;
    if (!userId) { setError('No user ID available'); setLoading(false); return; }

    const fetchAll = async () => {
      try {
        const authHeader = { Authorization: `Bearer ${token}` };

        // user
        const userRes = await fetch(`${API_BASE_URL}/api/users/${userId}`, { headers: authHeader });
        if (!userRes.ok) throw new Error('Failed to fetch user data');
        const user = await userRes.json();
        setUserData({ ...user, userId });

        // level
        const levelRes = await fetch(`${API_BASE_URL}/api/users/${userId}/level-info`, { headers: authHeader });
        if (levelRes.ok) setLevelInfo(await levelRes.json());

        // impact (Approved)
        const impactRes = await fetch(`${API_BASE_URL}/api/activities/users/${userId}/carbon-impact`, { headers: authHeader });
        if (impactRes.ok) {
          const impactData = await impactRes.json();
          setCarbonImpact({
            co2Reduced: impactData.co2Reduced?.toFixed(2) ?? 0,
            treesEquivalent: impactData.co2Reduced ? (impactData.co2Reduced / 21).toFixed(2) : 0,
            waterSaved: impactData.waterSaved ?? 0
          });
        }

        // pending/rejected
        const pendingRes = await fetch(`${API_BASE_URL}/api/activities/pending/${userId}`, { headers: authHeader });
        if (pendingRes.ok) setPendingItems(await pendingRes.json());

        // all completions
        const allRes = await fetch(`${API_BASE_URL}/api/activities/completed/${userId}`, { headers: authHeader });
        if (allRes.ok) setAllCompletions(await allRes.json());

        // activity stats → tips
        const statsRes = await fetch(`${API_BASE_URL}/api/users/${userId}/activity-stats`, { headers: authHeader });
        if (statsRes.ok) {
          const statsData = await statsRes.json();
          setActivityStats(statsData);

          const allCategories = ["Water", "Energy", "Transport", "Waste", "Food"];
          const categoryMap = {
            "Water Conservation": "Water",
            "Energy Saving": "Energy",
            "Transportation": "Transport",
            "Waste Reduction": "Waste",
            "Food Choices": "Food"
          };
          const mergedCategoryCounts = allCategories.map(cat => {
            const match = statsData.categoryCounts.find(
              c => categoryMap[c.name] === cat || c.name === cat
            );
            return { name: cat, value: match ? match.value : 0 };
          });
          const sorted = mergedCategoryCounts.sort((a, b) => a.value - b.value);
          setLowTipCategories(sorted.slice(0, 2).map(c => c.name));
        }

        // points history
        const pointsRes = await fetch(`${API_BASE_URL}/api/users/${userId}/points-history`, { headers: authHeader });
        if (pointsRes.ok) setPointsHistory(await pointsRes.json());

        // user challenges
        const myChallengesRes = await fetch(`${API_BASE_URL}/api/challenges/user/${userId}`, { headers: authHeader });
        if (myChallengesRes.ok) {
          const list = await myChallengesRes.json();
          setUserChallenges(Array.isArray(list) ? list : []);
        }

        setLoading(false);
      } catch (err) {
        console.error(err);
        setError(err.message || 'Failed to load dashboard');
        setLoading(false);
      }
    };

    fetchAll();
  }, [currentUser, token]);

  // Recently Approved (not dismissed)
  const approvedNotifications = useMemo(() => {
    const cutoff = Date.now() - 14 * 24 * 60 * 60 * 1000;
    return (allCompletions || [])
      .filter(c => String(c.reviewStatus).toLowerCase() === 'approved')
      .filter(c => {
        const id = c.id;
        if (dismissedApproved.has(id)) return false;
        const when = new Date(c.completedAt).getTime();
        return isFinite(when) ? when >= cutoff : true;
      })
      .sort((a, b) => new Date(b.completedAt) - new Date(a.completedAt));
  }, [allCompletions, dismissedApproved]);

  const dismissApproved = (item) => {
    const next = new Set(dismissedApproved);
    next.add(item.id);
    setDismissedApproved(next);
    setIdSet('approvedDismissedIds', next);

    const pts = Number(item.pointsEarned || 0);
    const title = item.activity?.title || 'your activity';
    showToast(pts > 0 ? `+${pts} points for “${title}” 🎉` : `Marked as read: “${title}”`);
  };

  const inProgressChallenges = useMemo(() => {
    const now = new Date();
    const byId = new Map();
    (userChallenges || []).forEach(c => {
      const start = new Date(c.startDate);
      const end = new Date(c.endDate);
      if (start <= now && end >= now) byId.set(c.id, c);
    });
    return [...byId.values()];
  }, [userChallenges]);

  const submitResubmission = async () => {
    if (!resubmitTarget) return;
    setResubmitting(true);
    try {
      const form = new FormData();
      form.append('userId', String(userData.userId));
      form.append('notes', resubmitNotes);
      form.append('image', resubmitFile);

      const resp = await fetch(`${API_BASE_URL}/api/activities/${resubmitTarget.id}/resubmit`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
        body: form
      });

      if (!resp.ok) {
        const err = await resp.json().catch(() => ({}));
        throw new Error(err.message || 'Failed to resubmit activity');
      }
      const data = await resp.json();

      setPendingItems(prev =>
        prev.map(item =>
          item.id === resubmitTarget.id
            ? {
                ...item,
                reviewStatus: 'Pending Review',
                adminNotes: null,
                imageUrl: data?.completion?.imageUrl ?? item.imageUrl
              }
            : item
        )
      );

      setResubmitOpen(false);
      setResubmitTarget(null);
      setResubmitNotes('');
      setResubmitFile(null);
      alert('Resubmitted for review ✅');
    } catch (e) {
      alert(e.message);
    } finally {
      setResubmitting(false);
    }
  };

  // early returns
  if (!currentUser && !loading) return <Navigate to="/login" />;
  if (loading) return <div className="loading-container"><div className="loading">Loading...</div></div>;
  if (error) return <div className="error-container"><div className="error">Error: {error}</div></div>;
  if (!userData) return <div className="no-user-container"><div className="no-user">No user data available</div></div>;

  const selectedAvatar = avatarStyles.find((style) => style.name === userData.avatarStyle);
  const COLORS = ['#4CAF50', '#8BC34A', '#CDDC39', '#FFC107', '#FF9800', '#009688'];

  return (
    <div className="dashboard">
      <header className="dashboard-header" data-reveal="up" data-reveal-delay="0">
        <h2>Welcome back, {userData.username}!</h2>
        <p className="dashboard-guidance">
          Need help? Hover over icons and stats for tips. Complete activities and challenges to earn more points!
        </p>
        <div className="dashboard-header-row">
          <div className="dashboard-avatar-block">
            <img
              src={selectedAvatar ? selectedAvatar.img : '/avatars/classic.png'}
              alt="Your avatar"
              className="dashboard-avatar-img"
              style={{ width: 80, height: 80, borderRadius: '50%', objectFit: 'cover', marginRight: 24 }}
              title="This is your current avatar. Click below to customize."
            />
            <Link to="/profile/avatar" className="customize-avatar-link" title="Change your avatar to personalize your profile!">
              Customize Avatar
            </Link>
          </div>

          <div className="user-stats">
            <div className="stat">
              <span className="stat-value" data-testid="user-points" title="Earn points by completing activities and challenges!">
                {userData.points}
              </span>
              <span className="stat-label">Total Points</span>
            </div>
            <StreakCounter userId={userData.userId} />
          </div>

          <div className="member-since-block" title="The date you joined Green Westminster.">
            <span className="member-since-label">Member since</span>
            <span className="member-since-date">
              {userData.joinDate && !isNaN(Date.parse(userData.joinDate))
                ? new Date(userData.joinDate).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' })
                : "Unknown"}
            </span>
          </div>
        </div>
      </header>

      {approvedNotifications.length > 0 && (
        <section className="approved-section" data-reveal="up" data-reveal-delay="80">
          <div className="section-header"><h3>Recently Approved</h3></div>
          <ul className="approved-list">
            {approvedNotifications.map(item => (
              <li key={item.id} className="approved-item">
                <div className="approved-row">
                  <div className="approved-meta">
                    <span className="approved-title">{item.activity?.title || 'Activity Approved'}</span>
                    <span className="approved-date">{formatDateTime(item.completedAt)}</span>
                  </div>
                  <button
                    className="approved-gotit"
                    onClick={() => dismissApproved(item)}
                    aria-label="Dismiss approved item"
                  >
                    Got it
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </section>
      )}

      {/* Level Progress */}
      <section className="level-section" data-reveal="up" data-reveal-delay="120">
        <div className="section-header"><h3>Your Level</h3></div>
        <div className="level-card">
          <div className="level-badge">
            <div className="badge-icon">
              {({ Bronze: "🥉", Silver: "🥈", Gold: "🥇", Platinum: "🏆" }[levelInfo.currentLevel] || "👑")}
            </div>
            <div className="badge-text">{levelInfo.currentLevel}</div>
          </div>
          <div className="level-progress">
            <div className="progress-text">
              <span className="level-title">{levelInfo.currentLevel}</span>
              {levelInfo.pointsToNextLevel > 0 && levelInfo.nextLevel
                ? <span className="points-to-next">{levelInfo.pointsToNextLevel} points to {levelInfo.nextLevel}</span>
                : <span className="max-level">Max Level Reached!</span>}
            </div>
            <div className="progress-bar">
              <div className="progress-fill" style={{ width: `${levelInfo.progressPercentage}%` }} />
            </div>
          </div>
        </div>
      </section>

      {/* Carbon Impact */}
      <section className="carbon-impact-section" data-reveal="up" data-reveal-delay="160">
        <div className="section-header"><h3>Carbon Impact Estimate</h3></div>
        <div className="carbon-impact-container">
          <div className="impact-card"><div className="impact-icon">🌍</div><div className="impact-value">{carbonImpact.co2Reduced} kg</div><div className="impact-label">CO₂ Reduced</div></div>
          <div className="impact-card"><div className="impact-icon">🌳</div><div className="impact-value">{carbonImpact.treesEquivalent}</div><div className="impact-label">Trees Equivalent</div></div>
          <div className="impact-card"><div className="impact-icon">💧</div><div className="impact-value">{carbonImpact.waterSaved} L</div><div className="impact-label">Water Saved</div></div>
        </div>
      </section>

      {/* Pending / Rejected */}
      <section className="pending-activities-section" data-reveal="up" data-reveal-delay="200">
        <div className="section-header">
          <h3>Pending & Rejected</h3>
          <Link to="/activities" className="cta-btn">Go to Activities</Link>
        </div>

        {pendingItems.length === 0 ? (
          <p>Nothing requiring your attention.<br /><span className="dashboard-guidance">Tip: Try logging a new activity to earn more points!</span></p>
        ) : (
          <ul className="pending-activities-list">
            {pendingItems.map(activity => (
              <li key={activity.id}>
                <div className="pending-row">
                  <span className="pending-title">{activity.title}</span>
                  <span className={`status ${activity.reviewStatus?.toLowerCase()}`}>{activity.reviewStatus}</span>
                </div>

                {activity.reviewStatus === 'Rejected' && (
                  <>
                    <div className="admin-notes">
                      <strong>Reason:</strong> {activity.adminNotes?.trim() ? activity.adminNotes : 'No reason provided.'}
                    </div>
                    <div className="pending-actions">
                      <button
                        className="resubmit-btn"
                        onClick={() => {
                          setResubmitTarget(activity);
                          setResubmitNotes('');
                          setResubmitFile(null);
                          setResubmitOpen(true);
                        }}
                      >
                        Resubmit
                      </button>
                    </div>
                  </>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>

      {/* Your Challenges (only in-progress) */}
      <section className="user-challenges-section" data-reveal="up" data-reveal-delay="240">
        <div className="section-header">
          <h3>Your Challenges</h3>
          <Link to="/challenges" className="cta-btn">Go to Challenges</Link>
        </div>

        {inProgressChallenges.length === 0 ? (
          <p>You haven’t joined any in-progress challenges yet.</p>
        ) : (
          <ul className="user-challenges-list">
            {inProgressChallenges.map(c => {
              const start = new Date(c.startDate);
              const end = new Date(c.endDate);
              return (
                <li key={c.id} className="challenge-item in-progress">
                  <div className="challenge-info">
                    <h4>{c.title}</h4>
                    <span className="dates">{start.toLocaleDateString()} – {end.toLocaleDateString()}</span>
                  </div>
                  <span className="status in-progress">In Progress</span>
                </li>
              );
            })}
          </ul>
        )}
      </section>

      {/* Tips */}
      {lowTipCategories.length > 0 && (
        <section className="tips-section" data-reveal="up" data-reveal-delay="280">
          <div className="section-header"><h3>Personalised Sustainability Tips</h3></div>
          <div className="tips-container">
            {lowTipCategories.map((category, idx) => (
              <div key={idx} className="tip-card">
                <h4>{category} Tips</h4>
                <ul>
                  {(tipsByCategory[category] || []).slice(0, 2).map((tip, i) => (<li key={i}>🌱 {tip}</li>))}
                </ul>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* Progress Charts */}
      <section className="progress-section" data-reveal="up" data-reveal-delay="320">
        <div className="section-header"><h3>Your Progress</h3></div>
        <div className="charts-container">
          <div className="chart-row">
            <div className="chart-card">
              <h4>Activities by Category</h4>
              <div className="chart-split">
                <div className="chart-plot">
                  <ResponsiveContainer width="100%" height={260}>
                    <PieChart margin={{ top: 4, right: 4, bottom: 4, left: 4 }}>
                      <Pie
                        data={activityStats.categoryCounts}
                        cx="50%" cy="50%"
                        innerRadius={45} outerRadius={95}
                        labelLine={false}
                        dataKey="value" nameKey="name"
                        isAnimationActive={true}
                        animationDuration={700}
                        animationBegin={120}
                        label={({ percent }) => (percent > 0.08 ? `${(percent * 100).toFixed(0)}%` : '')}
                      >
                        {activityStats.categoryCounts.map((_, i) => (
                          <Cell key={i} fill={COLORS[i % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip formatter={(v, n) => [`${v} activities`, n]} />
                    </PieChart>
                  </ResponsiveContainer>
                </div>

                <ul className="chart-legend">
                  {activityStats.categoryCounts.map((d, i) => (
                    <li key={d.name}>
                      <span className="swatch" style={{ background: COLORS[i % COLORS.length] }} />
                      {d.name}
                    </li>
                  ))}
                </ul>
              </div>
            </div>

            <div className="chart-card">
              <h4>Weekly Activity</h4>
              <ResponsiveContainer width="100%" height={200}>
                <BarChart data={activityStats.weeklyActivity} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" opacity={0.5} />
                  <XAxis dataKey="day" />
                  <YAxis />
                  <Tooltip formatter={(value) => [`${value} activities`]} />
                  <Bar dataKey="count" fill="#4CAF50" isAnimationActive animationDuration={700} animationBegin={180} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          <div className="chart-card full-width">
            <h4>Points Progress</h4>
            <ResponsiveContainer width="100%" height={200}>
              <LineChart data={pointsHistory} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis />
                <Tooltip formatter={(value) => [`${value} points`]} />
                <Legend />
                <Line type="monotone" dataKey="points" stroke="#2E7D32" strokeWidth={2} dot={{ r: 4 }} isAnimationActive animationDuration={750} animationBegin={220} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </section>

      {/* Resubmit Modal */}
      {resubmitOpen && resubmitTarget && (
        <div className="modal-overlay">
          <div className="completion-modal">
            <h3>Resubmit: {resubmitTarget.title}</h3>

            <label className="form-group">
              <span>Notes</span>
              <textarea
                value={resubmitNotes}
                onChange={(e) => setResubmitNotes(e.target.value)}
                rows={3}
                placeholder="Add clarification or fixes based on the admin’s feedback"
              />
            </label>

            <label className="form-group">
              <span>Upload new image</span>
              <input
                type="file"
                accept="image/*"
                onChange={(e) => setResubmitFile(e.target.files?.[0] || null)}
              />
            </label>

            <div className="modal-actions">
              <button className="cancel-btn" onClick={() => setResubmitOpen(false)} disabled={resubmitting}>
                Cancel
              </button>
              <button
                className="submit-btn"
                onClick={submitResubmission}
                disabled={resubmitting || !resubmitNotes.trim() || !resubmitFile}
              >
                {resubmitting ? 'Submitting...' : 'Send for Review'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Toast */}
      {toast && (
        <div className="toast toast-success">
          {toast}
        </div>
      )}
    </div>
  );
};

export default Dashboard;
