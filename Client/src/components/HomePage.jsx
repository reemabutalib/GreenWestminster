import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import '../styling/HomePage.css';

const campusList = [
  { name: 'Cavendish', img: '/maps/cavendish-map.png' },
  { name: 'Marylebone', img: '/maps/marylebone-map.png' },
  { name: 'Regent Street', img: '/maps/regent-map.png' },
  { name: 'Harrow', img: '/maps/harrow-map.png' },
  { name: 'Little Titchfield Street', img: '/maps/lts-map.png' },
  { name: 'Wells Street', img: '/maps/wells-map.png' },
];

const recyclingBinLocations = {
  Cavendish: [
    { name: 'Ground Floor Lobby', x: 60, y: 120 },
    { name: 'Canteen', x: 180, y: 80 },
  ],
  Marylebone: [
    { name: 'Main Entrance', x: 100, y: 100 },
    { name: 'Library', x: 200, y: 150 },
  ],
  'Regent Street': [
    { name: 'Reception', x: 80, y: 90 },
    { name: 'Student Lounge', x: 160, y: 130 },
  ],
  Harrow: [
    { name: 'Art Block', x: 120, y: 110 },
    { name: 'Café', x: 210, y: 70 },
  ],
  'Little Titchfield Street': [
    { name: 'Main Hall', x: 90, y: 100 },
  ],
  'Wells Street': [
    { name: 'Entrance', x: 100, y: 120 },
  ],
};

const HomePage = () => {
  const [selectedCampus, setSelectedCampus] = useState('Cavendish');

  return (
    <div className="home-page">
      <section className="hero-section">
        <div className="hero-content">
          <h1>Welcome to Green Westminster</h1>
          <p className="hero-subtitle">
            Join our community and make a positive environmental impact on campus
          </p>
          <div className="hero-buttons">
            <Link to="/register" className="btn btn-primary">Get Started</Link>
            <Link to="/login" className="btn btn-secondary">Sign In</Link>
          </div>
        </div>
      </section>

      <section className="features-section">
        <h2>Why Join Green Westminster?</h2>
        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">🌱</div>
            <h3>Track Your Impact</h3>
            <p>Log your sustainable actions and see your positive environmental impact grow over time</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">🏆</div>
            <h3>Earn Rewards</h3>
            <p>Complete challenges to earn points and badges while making a real difference</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">👥</div>
            <h3>Join the Community</h3>
            <p>Connect with like-minded students and participate in group sustainability initiatives</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">📊</div>
            <h3>Track Progress</h3>
            <p>View your sustainability journey with detailed dashboards and statistics</p>
          </div>
        </div>
      </section>

      <section className="how-it-works">
        <h2>How It Works</h2>
        <div className="steps-container">
          <div className="step">
            <div className="step-number">1</div>
            <h3>Create an Account</h3>
            <p>Sign up with your university email to join the community</p>
          </div>
          <div className="step">
            <div className="step-number">2</div>
            <h3>Log Sustainable Actions</h3>
            <p>Record your daily eco-friendly activities on campus</p>
          </div>
          <div className="step">
            <div className="step-number">3</div>
            <h3>Complete Challenges</h3>
            <p>Participate in university-wide sustainability challenges</p>
          </div>
          <div className="step">
            <div className="step-number">4</div>
            <h3>Earn Recognition</h3>
            <p>Climb the leaderboard and showcase your environmental commitment</p>
          </div>
        </div>
      </section>

      {/* Recycling Bins Map Section */}
      <section className="recycling-map-section">
        <h2>Find Recycling Bins on Campus</h2>
        <div className="campus-toggle">
          {campusList.map(campus => (
            <button
              key={campus.name}
              className={`campus-btn${selectedCampus === campus.name ? ' active' : ''}`}
              onClick={() => setSelectedCampus(campus.name)}
            >
              {campus.name}
            </button>
          ))}
        </div>
        <div className="map-container">
          {/* Placeholder map image or box */}
          <div className="map-image-wrapper" style={{ position: 'relative', width: 500, height: 400, margin: '0 auto', background: '#e8f5e9', borderRadius: 12 }}>
            {/* Replace src with your own placeholder images if you have them */}
            <img
              src={campusList.find(c => c.name === selectedCampus)?.img || ''}
              alt={`${selectedCampus} campus map`}
              style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: 12, opacity: 0.7 }}
              onError={e => { e.target.style.display = 'none'; }}
            />
            {/* Show bin locations as icons */}
            {recyclingBinLocations[selectedCampus].map(bin => (
              <div
                key={bin.name}
                title={`Recycling bin: ${bin.name}`}
                aria-label={`Recycling bin: ${bin.name}`}
                tabIndex={0}
                role="img"
                style={{
                  position: 'absolute',
                  left: bin.x,
                  top: bin.y,
                  transform: 'translate(-50%, -50%)',
                  background: '#fff',
                  borderRadius: '50%',
                  width: 32,
                  height: 32,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  boxShadow: '0 2px 6px rgba(0,0,0,0.1)',
                  border: '2px solid #4CAF50',
                  cursor: 'pointer',
                }}
              >
                ♻️
              </div>
            ))}
          </div>
          <ul className="bin-list">
            {recyclingBinLocations[selectedCampus].map(bin => (
              <li key={bin.name}>♻️ {bin.name}</li>
            ))}
          </ul>
        </div>
      </section>

      <section className="cta-section">
        <div className="cta-content">
          <h2>Ready to Make a Difference?</h2>
          <p>Join our growing community of environmentally conscious students and staff</p>
          <Link to="/register" className="btn btn-primary btn-large">Join Green Westminster</Link>
        </div>
      </section>

      <section className="testimonials">
        <h2>What Our Community Says</h2>
        <div className="testimonials-grid">
          <div className="testimonial-card">
            <p>"Green Westminster helped me become more aware of my daily habits and how they impact the environment."</p>
            <p className="testimonial-author">- Computer Science Student</p>
          </div>
          <div className="testimonial-card">
            <p>"I love competing with my flatmates to see who can be the most eco-friendly each week!"</p>
            <p className="testimonial-author">- Business Management Student</p>
          </div>
          <div className="testimonial-card">
            <p>"The challenges are creative and make sustainability fun rather than feeling like a chore."</p>
            <p className="testimonial-author">- Environmental Science Student</p>
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;