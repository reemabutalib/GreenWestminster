import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';

// Import components
import Navigation from './components/common/Navigation';
import Dashboard from './components/dashboard/Dashboard';
import ActivitiesPage from './components/activities/ActivitiesPage';
import ChallengesPage from './components/challenges/ChallengesPage';
import ChallengeDetails from './components/challenges/ChallengeDetails';
import LeaderboardPage from './components/leaderboard/LeaderboardPage';
import ProfilePage from './components/profile/ProfilePage';
import Login from './components/auth/Login';
import Register from './components/auth/Register';

function App() {
  return (
    <Router>
      <div className="app">
        <Navigation />
        
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/activities" element={<ActivitiesPage />} />
            <Route path="/challenges" element={<ChallengesPage />} />
            <Route path="/challenges/:id" element={<ChallengeDetails />} />
            <Route path="/leaderboard" element={<LeaderboardPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;