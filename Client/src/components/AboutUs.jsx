import React from 'react';

const AboutUs = () => (
  <div className="about-us-container" style={{ maxWidth: 900, margin: '2rem auto', padding: '2rem', background: '#f6fff6', borderRadius: 16, boxShadow: '0 2px 12px rgba(46,125,50,0.07)' }}>
    <h2 style={{ color: '#2e7d32', fontSize: '2.2rem', marginBottom: '1rem' }}>About Us</h2>
    <img
      src="/images/AboutUs.jpg"
      alt="Sustainability at Westminster"
      style={{ width: '100%', height: 220, objectFit: 'cover', borderRadius: 8, boxShadow: '0 2px 8px rgba(46,125,50,0.12)', marginBottom: '2rem' }}
    />
    <div>
      <p style={{ fontSize: '1.15rem', marginBottom: '1.2rem' }}>
        At the University of Westminster, our commitment to <strong>sustainable development</strong> is at the heart of our vision, mission, and values. We strive to create a positive impact on society and the environment through responsible practices, innovative research, and inclusive education.
      </p>
      <p style={{ fontSize: '1.15rem', marginBottom: '1.2rem' }}>
        Our approach to sustainability includes reducing our carbon footprint, promoting ethical and responsible resource use, and empowering our community to make a difference. We believe in fostering a culture of environmental awareness and social responsibility across all aspects of university life.
      </p>
      <p style={{ fontSize: '1.15rem' }}>
        To learn more about our sustainability vision, mission, and values, please visit the official page:<br />
        <a
          href="https://www.westminster.ac.uk/about-us/our-university/vision-mission-and-values/sustainable-development"
          target="_blank"
          rel="noopener noreferrer"
          style={{ color: '#388e3c', fontWeight: 500 }}
        >
          Sustainable Development at Westminster
        </a>
      </p>
    </div>
  </div>
);

export default AboutUs;