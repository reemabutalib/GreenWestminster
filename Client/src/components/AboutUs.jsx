import React from 'react';
import '../styling/AboutUs.css';

const AboutUs = () => (
  <div className="about">
    {/* Header / Title */}
    <section className="about-hero container">
      <span className="about-kicker">Who we are</span>
      <h1 className="about-title">About Us</h1>
      <div className="about-bar" aria-hidden="true" />
    </section>

    {/* Image card */}
    <section className="about-media container">
      <figure className="about-figure">
        <img
          src="/images/AboutUs.jpg"
          alt="Sustainability at Westminster"
          className="about-img"
          loading="lazy"
          onError={(e) => { e.currentTarget.src = '/images/placeholder-wide.jpg'; }}
        />
        <figcaption className="about-figcap">
          <span className="about-chip">🌿 Community-led</span>
          <span className="about-chip">♻️ Action-focused</span>
          <span className="about-chip">🎓 On campus</span>
        </figcaption>
      </figure>
    </section>

    {/* Copy */}
    <section className="about-body container">
      <div className="about-card">
        <p>
          At the University of Westminster, our commitment to <strong>sustainable development</strong> is at the heart of our vision, mission, and values. We strive to create a positive impact on society and the environment through responsible practices, innovative research, and inclusive education.
        </p>
        <p>
          Our approach to sustainability includes reducing our carbon footprint, promoting ethical and responsible resource use, and empowering our community to make a difference. We believe in fostering a culture of environmental awareness and social responsibility across all aspects of university life.
        </p>
        <p>
          To learn more about our sustainability vision, mission, and values, please visit the official page:<br />
          <a
            href="https://www.westminster.ac.uk/about-us/our-university/vision-mission-and-values/sustainable-development"
            target="_blank"
            rel="noopener noreferrer"
            className="about-link"
          >
            Sustainable Development at Westminster
          </a>
        </p>
      </div>
    </section>
  </div>
);

export default AboutUs;
