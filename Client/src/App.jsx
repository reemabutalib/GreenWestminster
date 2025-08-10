import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';

// Import components
import HomePage from './components/HomePage';
import Navigation from './components/Navigation';
import Dashboard from './components/Dashboard';
import ActivitiesPage from './components/ActivitiesPage';
import ChallengesPage from './components/ChallengesPage';
import ChallengeDetails from './components/ChallengeDetails';
import LeaderboardPage from './components/LeaderboardPage';
import ProfilePage from './components/ProfilePage';
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import EventsPage from './components/EventsPage';
import AboutUs from './components/AboutUs';
import AvatarCustomizer from './components/AvatarCustomizer';

// Import admin components
import AdminDashboard from './components/admin/AdminDashboard';
import AdminStats from './components/admin/AdminStats';
import ManageActivities from './components/admin/ManageActivities';
import ManageEvents from './components/admin/ManageEvents';
import ManageChallenges from './components/admin/ManageChallenges';
import AdminLogin from './components/admin/AdminLogin';
import ReviewSubmissions from './components/admin/ReviewSubmissions';

// Import auth context
import { AuthProvider, useAuth } from './components/context/UserContext';

// Protected route component
const ProtectedRoute = ({ children }) => {
  const { currentUser, loading } = useAuth();
  
  if (loading) {
    return <div className="loading-auth">Authenticating...</div>;
  }
  
  if (!currentUser) {
    return <Navigate to="/login" />;
  }
  
  return children;
};

// Admin route component
const AdminRoute = ({ children }) => {
  const { currentUser, loading } = useAuth();
  console.log('AdminRoute:', { currentUser, loading });

  if (loading) {
    return <div className="loading-auth">Authenticating...</div>;
  }
  if (!currentUser) {
    return <Navigate to="/admin/login" />;
  }
  if (currentUser.role !== 'Admin') {
    return <Navigate to="/dashboard" />;
  }
  return children;
};

function AppContent() {
  return (
    <Router>
      <div className="app">
        <Navigation />
        
        <main className="main-content">
          <Routes>
            {/* Public routes */}
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/leaderboard" element={<LeaderboardPage />} />
            <Route path="/events" element={<EventsPage />} />
            <Route path="/admin/login" element={<AdminLogin />} />
            <Route path="/about-us" element={<AboutUs />} />
            
            {/* Protected routes */}
            <Route path="/dashboard" element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            } />
            <Route path="/activities" element={
              <ProtectedRoute>
                <ActivitiesPage />
              </ProtectedRoute>
            } />
            <Route path="/challenges" element={
              <ProtectedRoute>
                <ChallengesPage />
              </ProtectedRoute>
            } />
            <Route path="/challenges/:id" element={
              <ProtectedRoute>
                <ChallengeDetails />
              </ProtectedRoute>
            } />
            <Route path="/profile" element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            } />

            <Route path="/profile/avatar" element={
            <ProtectedRoute>
            <AvatarCustomizer />
            </ProtectedRoute>
            } />
            
            {/* Admin routes */}
            <Route path="/admin/dashboard/*" element={
              <AdminRoute>
                <AdminDashboard />
              </AdminRoute>
            }>
              <Route index element={<AdminStats />} />
              <Route path="activities" element={<ManageActivities />} />
              <Route path="events" element={<ManageEvents />} />
              <Route path="challenges" element={<ManageChallenges />} />
              <Route path="review" element={<ReviewSubmissions />} />
            </Route>
          </Routes>
        </main>
      </div>
    </Router>
  );
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;