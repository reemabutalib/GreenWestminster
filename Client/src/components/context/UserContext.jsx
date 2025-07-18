import { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();

// API base URL - This is crucial for correct routing
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:80'; 

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
        const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          },
          // credentials: 'include'
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
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password }),
      // credentials: 'include'
    });
      
      // First get the raw response text
      const responseText = await response.text();
      console.log("Login response text:", responseText);
      
      // Try to parse as JSON if possible
      let data;
      try {
        data = JSON.parse(responseText);
      } catch (error) {
        console.error('Failed to parse login response as JSON:', responseText, error);
        throw new Error('Invalid server response format');
      }
      
      if (!response.ok) {
        throw new Error(data.message || 'Failed to login');
      }
      
      // Store authentication data
      localStorage.setItem('token', data.token);
      localStorage.setItem('userId', data.userId);
      
      // Fetch user data
      const userResponse = await fetch(`${API_BASE_URL}/api/users/${data.userId}`, {
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
      console.error('Login error:', error);
      setAuthError(error.message);
      return { success: false, error: error.message };
    }
  };

  const register = async (username, email, password, confirmPassword, course = '', yearOfStudy = 1, accommodationType = '') => {
  setAuthError(null);
  
  try {
    // Create the user object with confirmPassword field
    const user = {
      username,
      email,
      password,
      confirmPassword, // Add this field
      course: course || '',
      yearOfStudy: parseInt(yearOfStudy, 10) || 1,
      accommodationType: accommodationType || '',
      joinDate: new Date().toISOString(),
      points: 0,
      currentStreak: 0,
      maxStreak: 0,
      lastActivityDate: new Date().toISOString()
    };

    console.log('Sending registration request with payload:', JSON.stringify(user, null, 2));
    
    const registerEndpoint = `${API_BASE_URL}/api/auth/register`;
    console.log(`Making request to: ${registerEndpoint}`);

    const response = await fetch(registerEndpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(user),
      credentials: 'include'
    });
      
      // Capture detailed information about the response for debugging
      console.log(`Registration response status: ${response.status} ${response.statusText}`);
      console.log('Response headers:', Object.fromEntries([...response.headers]));
      
      // Get the raw response text first
      let responseText;
      try {
        responseText = await response.text();
        console.log('Raw response:', responseText);
      } catch (err) {
        console.error('Error reading response text:', err);
        throw new Error('Failed to read server response');
      }
      
      // Then try to parse as JSON if possible
      let data;
      if (responseText && responseText.trim()) {
        try {
          data = JSON.parse(responseText);
        } catch (err) {
          console.error('Error parsing response as JSON:', err);
          console.log('Response was:', responseText);
          
          if (response.ok) {
            // If response is OK but not JSON, still consider it a success
            return { success: true };
          } else {
            throw new Error(`Server returned invalid format (Status: ${response.status})`);
          }
        }
      }
      
      // Handle different error cases
      if (!response.ok) {
        if (response.status === 500) {
          console.error('Server error details:', data);
          throw new Error(`Server error: ${data?.message || 'Internal server error occurred'}`);
        } else if (response.status === 400) {
          throw new Error(data?.message || 'Invalid registration data');
        } else if (response.status === 409) {
          throw new Error(data?.message || 'User with this email already exists');
        } else {
          throw new Error(data?.message || `Registration failed with status: ${response.status}`);
        }
      }
      
      return { success: true, data };
      
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