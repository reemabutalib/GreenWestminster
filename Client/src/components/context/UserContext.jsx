import { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [authError, setAuthError] = useState(null);

  useEffect(() => {
    // Check for token and get user data when app loads
    const checkAuth = async () => {
      const token = localStorage.getItem('token');
      const userId = localStorage.getItem('userId');
      
      if (token && userId) {
        try {
          const response = await fetch(`/api/users/${userId}`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });
          
          if (response.ok) {
            const userData = await response.json();
            setCurrentUser(userData);
          } else {
            // Token is invalid or expired
            localStorage.removeItem('token');
            localStorage.removeItem('userId');
            setCurrentUser(null);
          }
        } catch (error) {
          console.error('Error fetching user data:', error);
          setAuthError('Failed to authenticate user');
          setCurrentUser(null);
        }
      }
      
      setLoading(false);
    };
    
    checkAuth();
  }, []);

  const login = async (email, password) => {
    setAuthError(null);
    
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });
      
      const data = await response.json();
      
      if (!response.ok) {
        throw new Error(data.message || 'Failed to login');
      }
      
      // Store authentication data
      localStorage.setItem('token', data.token);
      localStorage.setItem('userId', data.userId);
      
      // Fetch user data
      const userResponse = await fetch(`/api/users/${data.userId}`, {
        headers: {
          'Authorization': `Bearer ${data.token}`
        }
      });
      
      if (userResponse.ok) {
        const userData = await userResponse.json();
        setCurrentUser(userData);
        return { success: true };
      } else {
        throw new Error('Failed to fetch user data');
      }
    } catch (error) {
      setAuthError(error.message);
      return { success: false, error: error.message };
    }
  };

  // Updated register function to include new student-specific fields
  const register = async (username, email, password, course = '', yearOfStudy = 1, accommodationType = '') => {
    setAuthError(null);
    
    try {
      console.log('Sending registration data:', { 
        username, 
        email, 
        course, 
        yearOfStudy, 
        accommodationType 
      });
      
      const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ 
          username, 
          email, 
          password,
          course,
          yearOfStudy: parseInt(yearOfStudy, 10), // Ensure it's a number
          accommodationType 
        }),
      });
      
      // Try to get the response text first
      const responseText = await response.text();
      console.log('Raw server response:', responseText);

      // Then try to parse it as JSON if possible
      let data;
      try {
        data = JSON.parse(responseText);
      } catch (error) {
        console.error('Failed to parse response as JSON:', responseText, error);
        throw new Error(`Server returned an invalid response format: ${error.message}`);
      }
      
      if (!response.ok) {
        throw new Error(data.message || 'Registration failed');
      }
      
      return { success: true };
    } catch (error) {
      console.error('Registration error:', error);
      setAuthError(error.message);
      return { success: false, error: error.message };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    setCurrentUser(null);
  };

  const value = {
    currentUser,
    loading,
    authError,
    login,
    register,
    logout
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};