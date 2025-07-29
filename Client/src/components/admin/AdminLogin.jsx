import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/UserContext'; // <-- import useAuth
import '../../styling/AdminLogin.css'; 

const AdminLogin = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { login } = useAuth(); // <-- get login from context

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    const result = await login(email, password); // <-- use context login
    if (result.success) {
      navigate('/admin/dashboard');
    } else {
      setError(result.error || 'Invalid admin credentials');
    }
  };

  return (
    <div className="login-container">
      <h2>Admin Login</h2>
      <form onSubmit={handleSubmit}>
        <input
          type="email"
          placeholder="Admin Email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
        />
        <button type="submit">Log In</button>
        {error && <div className="error">{error}</div>}
      </form>
    </div>
  );
};

export default AdminLogin;