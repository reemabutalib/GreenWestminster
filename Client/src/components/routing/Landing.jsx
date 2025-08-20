// src/components/routing/Landing.jsx
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/UserContext";
import HomePage from "../HomePage";

export default function Landing() {
  const { currentUser, loading } = useAuth();
  
  if (loading) return <div className="loading-auth">Loading…</div>;
  if (currentUser) {
    const to = currentUser.role === "Admin" ? "/admin/dashboard" : "/dashboard";
    return <Navigate to={to} replace />;
  }
  return <HomePage />;
}
