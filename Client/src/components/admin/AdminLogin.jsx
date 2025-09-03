import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/UserContext';
import '../../styling/AdminLogin.css';

const AdminLogin = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { login, logout } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isLoading) return;
    setError('');
    setIsLoading(true);

    try {
      const result = await login(email, password);

      // Normalize role from whatever your login returns
      const role =
        result?.userRole ||
        result?.user?.role ||
        result?.role ||
        '';

      if (result?.success && String(role).toLowerCase() === 'admin') {
        navigate('/admin/dashboard');
        return; // success
      }

      if (result?.success) {
        logout(); // clear any non-admin login
        setError("You don't have access to this page as you are not an admin.");
      return;
      }

      setError(result?.error || 'Invalid admin credentials');
    } catch (err) {
      setError(err?.message || 'Something went wrong while logging in.');
    } finally {
      setIsLoading(false);
    }
  };
  
  return (
    <div className="login-container">
      <h2>Admin Login</h2>

      <form onSubmit={handleSubmit} aria-busy={isLoading}>
        <input
          type="email"
          placeholder="Admin Email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          autoComplete="username"
          required
          disabled={isLoading}
        />

        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          autoComplete="current-password"
          required
          disabled={isLoading}
        />

        <button
          type="submit"
          className={isLoading ? 'is-loading' : ''}
          disabled={isLoading || !email || !password}
        >
          {isLoading ? 'Logging in…' : 'Log In'}
        </button>

        {error && <div className="error">{error}</div>}
      </form>
    </div>
  );
};

export default AdminLogin;
