import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';

// Core layout
import Navigation from './components/Navigation';

// Public pages
import Landing from './components/routing/Landing';          // decides: guest sees HomePage, members redirected
import LeaderboardPage from './components/LeaderboardPage';
import EventsPage from './components/EventsPage';
import AboutUs from './components/AboutUs';
import TermsAndConditions from './components/TermsAndConditions';

// Auth pages
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import AdminLogin from './components/admin/AdminLogin';

// Member pages
import Dashboard from './components/Dashboard';
import ActivitiesPage from './components/ActivitiesPage';
import ChallengesPage from './components/ChallengesPage';
import ProfilePage from './components/ProfilePage';
import AvatarCustomizer from './components/AvatarCustomizer';

// Admin pages
import AdminDashboard from './components/admin/AdminDashboard';
import AdminStats from './components/admin/AdminStats';
import ManageActivities from './components/admin/ManageActivities';
import ManageEvents from './components/admin/ManageEvents';
import ManageChallenges from './components/admin/ManageChallenges';
import ReviewSubmissions from './components/admin/ReviewSubmissions';

// Auth context
import { AuthProvider, useAuth } from './components/context/UserContext';

// ---------- Route guards ----------
const ProtectedRoute = ({ children }) => {
  const { currentUser, loading } = useAuth();
  if (loading) return <div className="loading-auth">Authenticating...</div>;
  if (!currentUser) return <Navigate to="/" replace />;
  return children;
};

const GuestRoute = ({ children }) => {
  const { currentUser, loading } = useAuth();
  if (loading) return <div className="loading-auth">Loading…</div>;
  if (currentUser) {
    const to = currentUser.role === 'Admin' ? '/admin/dashboard' : '/dashboard';
    return <Navigate to={to} replace />;
  }
  return children;
};

const AdminRoute = ({ children }) => {
  const { currentUser, loading } = useAuth();
  if (loading) return <div className="loading-auth">Authenticating...</div>;
  if (!currentUser) return <Navigate to="/admin/login" replace />;
  if (currentUser.role !== 'Admin') return <Navigate to="/dashboard" replace />;
  return children;
};

// ---------- App ----------
function AppContent() {
  return (
    <Router>
      <div className="app">
        <Navigation />

        <main className="main-content">
          <Routes>
            {/* Landing: guests see marketing; members auto-redirect */}
            <Route path="/" element={<Landing />} />

            {/* Guest-only auth routes */}
            <Route path="/login" element={<GuestRoute><Login /></GuestRoute>} />
            <Route path="/register" element={<GuestRoute><Register /></GuestRoute>} />
            <Route path="/admin/login" element={<GuestRoute><AdminLogin /></GuestRoute>} />

            {/* Public info routes */}
            <Route path="/leaderboard" element={<LeaderboardPage />} />
            <Route path="/events" element={<EventsPage />} />
            <Route path="/about-us" element={<AboutUs />} />
            <Route path="/terms" element={<TermsAndConditions />} />

            {/* Member protected routes */}
            <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
            <Route path="/activities" element={<ProtectedRoute><ActivitiesPage /></ProtectedRoute>} />
            <Route path="/challenges" element={<ProtectedRoute><ChallengesPage /></ProtectedRoute>} />
            <Route path="/profile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
            <Route path="/profile/avatar" element={<ProtectedRoute><AvatarCustomizer /></ProtectedRoute>} />

            {/* Admin protected routes (with nested pages) */}
            <Route
              path="/admin/dashboard/*"
              element={
                <AdminRoute>
                  <AdminDashboard />
                </AdminRoute>
              }
            >
              <Route index element={<AdminStats />} />
              <Route path="activities" element={<ManageActivities />} />
              <Route path="events" element={<ManageEvents />} />
              <Route path="challenges" element={<ManageChallenges />} />
              <Route path="review" element={<ReviewSubmissions />} />
            </Route>

            {/* 404 fallback */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}
