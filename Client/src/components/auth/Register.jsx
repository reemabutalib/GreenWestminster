import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/UserContext';
import '../../styling/Auth.css';

const Register = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    course: '',
    yearOfStudy: 1,
    accommodationType: ''
  });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [serverError, setServerError] = useState('');

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
    
    // Clear error when user starts typing in a field
    if (errors[name]) {
      setErrors({ ...errors, [name]: '' });
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.username.trim()) {
      newErrors.username = 'Username is required';
    } else if (formData.username.length < 3) {
      newErrors.username = 'Username must be at least 3 characters';
    }
    
    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Email is invalid';
    }
    
    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters';
    }
    
    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password';
    } else if (formData.confirmPassword !== formData.password) {
      newErrors.confirmPassword = 'Passwords do not match';
    }
    
    // Validate course
    if (!formData.course.trim()) {
      newErrors.course = 'Course is required';
    }
    
    // Accommodation type validation
    if (!formData.accommodationType) {
      newErrors.accommodationType = 'Accommodation type is required';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (validateForm()) {
      setIsSubmitting(true);
      setServerError('');
      
      try {
        console.log('Submitting registration form with data:', {
          username: formData.username,
          email: formData.email,
          password: '********', // Don't log actual password
          confirmPassword: '********', // Don't log actual confirmPassword
          course: formData.course,
          yearOfStudy: formData.yearOfStudy,
          accommodationType: formData.accommodationType
        });
        
        const result = await register(
          formData.username,
          formData.email,
          formData.password,
          formData.confirmPassword, // Added confirmPassword parameter
          formData.course,
          formData.yearOfStudy,
          formData.accommodationType
        );
        
        if (result.success) {
          // Redirect to login page on successful registration
          navigate('/login', { state: { registered: true } });
        } else {
          setServerError(result.error || 'Registration failed');
        }
      } catch (error) {
        console.error('Registration error in component:', error);
        setServerError(error.message || 'An error occurred during registration');
      } finally {
        setIsSubmitting(false);
      }
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <h2>Create Account</h2>
          <p>Join Green Westminster and start your sustainability journey</p>
        </div>
        
        {serverError && <div className="auth-error">{serverError}</div>}
        
        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Choose a username"
            />
            {errors.username && <span className="error-message">{errors.username}</span>}
          </div>
          
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="Enter your email"
            />
            {errors.email && <span className="error-message">{errors.email}</span>}
          </div>
          
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Create a password"
            />
            {errors.password && <span className="error-message">{errors.password}</span>}
          </div>
          
          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Confirm your password"
            />
            {errors.confirmPassword && <span className="error-message">{errors.confirmPassword}</span>}
          </div>
          
          {/* Student-specific fields */}
          <div className="form-group">
            <label htmlFor="course">Course</label>
            <input
              type="text"
              id="course"
              name="course"
              value={formData.course}
              onChange={handleChange}
              placeholder="e.g. Computer Science"
            />
            {errors.course && <span className="error-message">{errors.course}</span>}
          </div>
          
          <div className="form-group">
            <label htmlFor="yearOfStudy">Year of Study</label>
            <select
              id="yearOfStudy"
              name="yearOfStudy"
              value={formData.yearOfStudy}
              onChange={handleChange}
            >
              <option value="1">1st Year</option>
              <option value="2">2nd Year</option>
              <option value="3">3rd Year</option>
              <option value="4">4th Year</option>
              <option value="5">5th Year</option>
              <option value="6">Postgraduate</option>
            </select>
            {errors.yearOfStudy && <span className="error-message">{errors.yearOfStudy}</span>}
          </div>
          
          <div className="form-group">
            <label htmlFor="accommodationType">Accommodation Type</label>
            <select
              id="accommodationType"
              name="accommodationType"
              value={formData.accommodationType}
              onChange={handleChange}
            >
              <option value="">Select accommodation type</option>
              <option value="University Halls">University Halls</option>
              <option value="Private Accommodation">Private Accommodation</option>
              <option value="Family Home">Family Home</option>
              <option value="Other">Other</option>
            </select>
            {errors.accommodationType && <span className="error-message">{errors.accommodationType}</span>}
          </div>
          
          <button 
            type="submit"
            className="auth-button"
            disabled={isSubmitting}
          >
            {isSubmitting ? 'Creating Account...' : 'Create Account'}
          </button>
        </form>
        
        <div className="auth-footer">
          <p>Already have an account? <Link to="/login">Log In</Link></p>
        </div>
      </div>
      
      <div className="auth-benefits">
        <h3>Join Green Westminster</h3>
        <div className="benefits-list">
          <div className="benefit-item">
            <div className="benefit-icon">🌱</div>
            <div className="benefit-text">Track your sustainability progress</div>
          </div>
          
          <div className="benefit-item">
            <div className="benefit-icon">🏆</div>
            <div className="benefit-text">Earn points and climb the leaderboard</div>
          </div>
          
          <div className="benefit-item">
            <div className="benefit-icon">🌍</div>
            <div className="benefit-text">Join challenges with other students</div>
          </div>
          
          <div className="benefit-item">
            <div className="benefit-icon">📊</div>
            <div className="benefit-text">Measure your environmental impact</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;