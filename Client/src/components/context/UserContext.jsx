import { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80';

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [authError, setAuthError] = useState(null);

  // On mount, check for user in localStorage and API
  useEffect(() => {
    const checkAuth = async () => {
      const token = localStorage.getItem('token');
      const userId = localStorage.getItem('userId');
      const role = localStorage.getItem('userRole');
      const email = localStorage.getItem('email');
      const username = localStorage.getItem('username');

      console.log('[UserContext] checkAuth localStorage:', { token, userId, role, email, username });

      if (token && userId && role) {
        try {
          const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
            headers: { 'Authorization': `Bearer ${token}` },
          });

          if (response.ok) {
            const userData = await response.json();
            console.log('[UserContext] checkAuth API userData:', userData);
            setCurrentUser({
              userId: userData.userId ?? userId,
              email: userData.email ?? email,
              username: userData.username ?? username,
              role,
              token,
            });
            // Optionally update localStorage with fresh data
            localStorage.setItem('email', userData.email ?? email ?? '');
            localStorage.setItem('username', userData.username ?? username ?? '');
          } else {
            // Fallback to localStorage if API fails
            console.warn('[UserContext] checkAuth API failed, using localStorage');
            setCurrentUser({ userId, email, username, role, token });
          }
        } catch (err) {
          console.error('[UserContext] checkAuth fetch error:', err);
          setCurrentUser({ userId, email, username, role, token });
        }
      } else {
        setCurrentUser(null);
      }
      setLoading(false);
    };
    checkAuth();
  }, []);

  // Login function
  const login = async (email, password) => {
  setAuthError(null);
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });

    const responseText = await response.text();
    let data;
    try {
      data = JSON.parse(responseText);
    } catch {
      throw new Error('Invalid server response format');
    }

    console.log('[UserContext] login response data:', data);

    if (!response.ok) {
      throw new Error(data.message || 'Failed to login');
    }

    // Save all user info to localStorage as strings
    localStorage.setItem('token', data.token);
    localStorage.setItem('userId', String(data.userId));
    localStorage.setItem('email', data.email || '');
    localStorage.setItem('username', data.username || '');
    if (data.roles && data.roles.length > 0) {
      localStorage.setItem('userRole', data.roles[0]);
    } else {
      localStorage.setItem('userRole', '');
    }

    const userObj = {
      userId: data.userId,
      email: data.email,
      username: data.username,
      role: (data.roles && data.roles.length > 0) ? data.roles[0] : '',
      token: data.token,
    };

    console.log('[UserContext] login setCurrentUser:', userObj);

    setCurrentUser(userObj);

    return { success: true };
  } catch (error) {
    setAuthError(error.message);
    setCurrentUser(null);
    return { success: false, error: error.message };
  }
};

  // Registration function (unchanged)
  const register = async (username, email, password, confirmPassword, course = '', yearOfStudy = 1, accommodationType = '') => {
    setAuthError(null);
    try {
      const user = {
        username,
        email,
        password,
        confirmPassword,
        course: course || '',
        yearOfStudy: parseInt(yearOfStudy, 10) || 1,
        accommodationType: accommodationType || '',
        joinDate: new Date().toISOString(),
        points: 0,
        currentStreak: 0,
        maxStreak: 0,
        lastActivityDate: new Date().toISOString()
      };

      const registerEndpoint = `${API_BASE_URL}/api/auth/register`;
      const response = await fetch(registerEndpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(user),
        credentials: 'include'
      });

      let responseText;
      try {
        responseText = await response.text();
      } catch {
        throw new Error('Failed to read server response');
      }

      let data;
      if (responseText && responseText.trim()) {
        try {
          data = JSON.parse(responseText);
        } catch {
          if (response.ok) return { success: true };
          else throw new Error(`Server returned invalid format (Status: ${response.status})`);
        }
      }

      if (!response.ok) {
        if (response.status === 500) throw new Error(data?.message || 'Internal server error occurred');
        if (response.status === 400) throw new Error(data?.message || 'Invalid registration data');
        if (response.status === 409) throw new Error(data?.message || 'User with this email already exists');
        throw new Error(data?.message || `Registration failed with status: ${response.status}`);
      }

      return { success: true, data };
    } catch (error) {
      setAuthError(error.message);
      return { success: false, error: error.message };
    }
  };

  // Logout function
  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('userRole');
    localStorage.removeItem('email');
    localStorage.removeItem('username');
    setCurrentUser(null);
    console.log('[UserContext] logout: cleared localStorage and currentUser');
  };

  const updateAvatar = async ({ userId, avatarStyle, avatarItems }) => {
  try {
    const token = localStorage.getItem('token');
    const response = await fetch(`${API_BASE_URL}/api/users/avatar`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ userId, avatarStyle, avatarItems }),
    });
    if (!response.ok) throw new Error('Failed to update avatar');
    // Optionally, refresh user data here
    return true;
  } catch (err) {
    console.error('[UserContext] updateAvatar error:', err);
    return false;
  }
};

  const value = {
    currentUser,
    loading,
    authError,
    login,
    register,
    logout,
    updateAvatar,
  };

  

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};