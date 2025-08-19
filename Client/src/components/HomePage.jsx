import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import '../styling/HomePage.css';

const campusList = [
  { name: 'Cavendish', img: '/maps/cavendish-map.png' },
  { name: 'Marylebone', img: '/maps/marylebone-map.png' },
  { name: 'Regent Street', img: '/maps/regent-map.png' },
  { name: 'Harrow', img: '/maps/harrow-map.png' },
  { name: 'Little Titchfield Street', img: '/maps/lts-map.png' },
  { name: 'Wells Street', img: '/maps/wells-map.png' }
];

const recyclingBinLocations = {
  Cavendish: [
    { name: 'Ground Floor Lobby', x: 60, y: 120 },
    { name: 'Canteen', x: 180, y: 80 }
  ],
  Marylebone: [
    { name: 'Main Entrance', x: 100, y: 100 },
    { name: 'Library', x: 200, y: 150 }
  ],
  'Regent Street': [
    { name: 'Reception', x: 80, y: 90 },
    { name: 'Student Lounge', x: 160, y: 130 }
  ],
  Harrow: [
    { name: 'Art Block', x: 120, y: 110 },
    { name: 'Café', x: 210, y: 70 }
  ],
  'Little Titchfield Street': [{ name: 'Main Hall', x: 90, y: 100 }],
  'Wells Street': [{ name: 'Entrance', x: 100, y: 120 }]
};

const HomePage = () => {
  const [selectedCampus, setSelectedCampus] = useState('Cavendish');

  useEffect(() => {
    const timeline = document.querySelector('.steps-timeline');
    if (!timeline) return;

    const steps = Array.from(timeline.querySelectorAll('.step'));
    timeline.classList.add('is-init');
    steps.forEach((s, i) => s.style.setProperty('--i', i));

    const io = new IntersectionObserver(
      entries => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            timeline.classList.add('in-view');
            steps.forEach((s, i) => {
              setTimeout(() => s.classList.add('is-visible'), i * 120);
            });
            io.disconnect();
          }
        });
      },
      { threshold: 0.25 }
    );

    io.observe(timeline);
    return () => io.disconnect();
  }, []);

  return (
    <div className="site-shell home-page">
      <main className="site-main">
        {/* HERO */}
        <section className="hero-section container py-5">
          <div className="hero-grid">
            {/* Floating chips */}
            <figure className="hero-chip hero-chip--top-left shadow-sm">
              <div className="chip-icon">🌳</div>
              <figcaption>
                <span className="chip-title">+685</span>
                <small className="text-muted d-block">Trees planted</small>
              </figcaption>
            </figure>

            <figure className="hero-chip hero-chip--mid-left shadow-sm">
              <div className="chip-icon">💧</div>
              <figcaption>
                <span className="chip-title">12,000L</span>
                <small className="text-muted d-block">Water saved</small>
              </figcaption>
            </figure>

            <figure className="hero-chip hero-chip--mid-right shadow-sm">
              <div className="chip-icon">🌍</div>
              <figcaption>
                <span className="chip-title">4,500kg</span>
                <small className="text-muted d-block">CO₂ saved</small>
              </figcaption>
            </figure>

            <div className="hero-copy text-center mx-auto">
              <h1 className="display-5 fw-bold lh-tight mb-3">
                Join <span className="highlight-green">Green Westminster</span>
              </h1>
              <p className="hero-subtitle mb-4">
                A community that is making a positive environmental impact on campus
              </p>
              <div className="d-flex justify-content-center gap-3">
                <Link to="/register" className="btn btn-success btn-lg rounded-pill">
                  Get Started
                </Link>
                <Link to="/login" className="btn btn-outline-success btn-lg rounded-pill">
                  Sign In
                </Link>
              </div>
            </div>
          </div>

          {/* Tagline / Ticker */}
          <div className="ticker">
            <div className="ticker__shadow"></div>
            <ul className="ticker__track">
              <li>Small Actions, Big Impact</li>
              <li>Challenge Yourself</li>
              <li>Celebrate Every Action</li>
              <li>Make Sustainability Fun</li>
              <li>Shape a Greener Campus</li>
              <li>Earn Rewards for Change</li>
              <li>Turn Habits Into Points</li>
              <li>Inspire Your Peers</li>
              <li>Compete With Friends</li>
              <li>Small Actions, Big Impact</li>
              <li>Challenge Yourself</li>
              <li>Celebrate Every Action</li>
            </ul>
          </div>
        </section>

        {/* Features */}
        <section className="features-section py-5">
          <div className="container">
            <h2 className="mb-4 text-center">Why Join Green Westminster?</h2>
            <div className="features-grid">
              <div>
                <div className="feature-card card-home rounded-4 shadow-sm p-4 text-center">
                  <div className="feature-icon fs-2 mb-3">🌱</div>
                  <h3 className="h5 fw-bold mb-2">Track Your Impact</h3>
                  <p>
                    Log your sustainable actions and see your positive environmental impact grow over
                    time
                  </p>
                </div>
              </div>
              <div>
                <div className="feature-card card-home rounded-4 shadow-sm p-4 text-center">
                  <div className="feature-icon fs-2 mb-3">🏆</div>
                  <h3 className="h5 fw-bold mb-2">Earn Rewards</h3>
                  <p>Complete challenges to earn points and badges while making a real difference</p>
                </div>
              </div>
              <div>
                <div className="feature-card card-home rounded-4 shadow-sm p-4 text-center">
                  <div className="feature-icon fs-2 mb-3">👥</div>
                  <h3 className="h5 fw-bold mb-2">Join the Community</h3>
                  <p>
                    Connect with like-minded students and participate in group sustainability
                    initiatives
                  </p>
                </div>
              </div>
              <div>
                <div className="feature-card card-home rounded-4 shadow-sm p-4 text-center">
                  <div className="feature-icon fs-2 mb-3">🎓</div>
                  <h3 className="h5 fw-bold mb-2">Boost Your Employability</h3>
                  <p>
                    Earn points for your sustainable actions and use them towards your Westminster
                    Employability Award.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* How It Works */}
<section className="how-it-works py-5" id="how-it-works">
  <div className="container">
    <div className="row g-5 align-items-start">
      <div className="col-lg-4">
        <h2 className="how-heading fw-bold mb-3">How It Works</h2>
        <p className="text-muted">
          Getting started is simple. Just follow these steps and start making an impact today.
        </p>
      </div>
      <div className="col-lg-8">
        {/* 👇 Added enable-anim here */}
        <div className="steps-timeline enable-anim js-steps">
          <div className="step d-flex flex-row align-items-start gap-3" data-step="1">
            <div className="step-number">1</div>
            <div>
              <h3 className="h6 fw-bold mb-1">Create an Account</h3>
              <p className="mb-0">Sign up with your university email to join the community</p>
            </div>
          </div>
          <div className="step d-flex flex-row align-items-start gap-3" data-step="2">
            <div className="step-number">2</div>
            <div>
              <h3 className="h6 fw-bold mb-1">Log Sustainable Actions</h3>
              <p className="mb-0">Record your daily eco-friendly activities on campus</p>
            </div>
          </div>
          <div className="step d-flex flex-row align-items-start gap-3" data-step="3">
            <div className="step-number">3</div>
            <div>
              <h3 className="h6 fw-bold mb-1">Complete Challenges</h3>
              <p className="mb-0">Participate in university-wide sustainability challenges</p>
            </div>
          </div>
          <div className="step d-flex flex-row align-items-start gap-3" data-step="4">
            <div className="step-number">4</div>
            <div>
              <h3 className="h6 fw-bold mb-1">Earn Recognition</h3>
              <p className="mb-0">
                Climb the leaderboard and showcase your environmental commitment
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</section>


        {/* Recycling Map */}
        <section className="recycling-map-section py-5">
          <div className="container">
            <h2 className="mb-4 text-center">Find Recycling Bins on Campus</h2>
            <div className="d-flex justify-content-center flex-wrap gap-2 campus-toggle mb-4">
              {campusList.map(campus => (
                <button
                  key={campus.name}
                  className={`btn campus-btn rounded-pill${
                    selectedCampus === campus.name ? ' btn-success' : ' btn-outline-secondary'
                  }`}
                  onClick={() => setSelectedCampus(campus.name)}
                >
                  {campus.name}
                </button>
              ))}
            </div>
            <div className="map-container mb-3">
              <div
                className="map-image-wrapper position-relative mx-auto"
                style={{ width: 500, height: 400, background: '#e8f5e9', borderRadius: 12 }}
              >
                <img
                  src={campusList.find(c => c.name === selectedCampus)?.img || ''}
                  alt={`${selectedCampus} campus map`}
                  style={{
                    width: '100%',
                    height: '100%',
                    objectFit: 'cover',
                    borderRadius: 12,
                    opacity: 0.7
                  }}
                  onError={e => {
                    e.target.style.display = 'none';
                  }}
                />
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
                      cursor: 'pointer'
                    }}
                  >
                    ♻️
                  </div>
                ))}
              </div>
              <ul className="bin-list list-unstyled mt-2 text-center">
                {recyclingBinLocations[selectedCampus].map(bin => (
                  <li key={bin.name}>♻️ {bin.name}</li>
                ))}
              </ul>
            </div>
          </div>
        </section>

        {/* CTA */}
        <section className="cta-section py-5">
          <div className="container">
            <div className="cta-content text-center">
              <h2 className="fw-bold mb-2">Ready to Make a Difference?</h2>
              <p className="mb-3">
                Join our growing community of environmentally conscious students and staff
              </p>
              <Link to="/register" className="btn btn-success btn-lg rounded-pill">
                Join Green Westminster
              </Link>
            </div>
          </div>
        </section>

        {/* Testimonials */}
        <section className="testimonials py-5">
          <div className="container">
            <h2 className="mb-4 text-center">What Our Community Says</h2>
            <div className="testimonials-grid">
              <div>
                <div className="testimonial-card shadow rounded-4 p-4 text-center">
                  <p>
                    "Green Westminster helped me become more aware of my daily habits and how they
                    impact the environment."
                  </p>
                  <p className="testimonial-author mb-0 text-muted">- Computer Science Student</p>
                </div>
              </div>
              <div>
                <div className="testimonial-card shadow rounded-4 p-4 text-center">
                  <p>
                    "I love competing with my flatmates to see who can be the most eco-friendly each
                    week!"
                  </p>
                  <p className="testimonial-author mb-0 text-muted">- Business Management Student</p>
                </div>
              </div>
              <div>
                <div className="testimonial-card shadow rounded-4 p-4 text-center">
                  <p>
                    "The challenges are creative and make sustainability fun rather than feeling like
                    a chore."
                  </p>
                  <p className="testimonial-author mb-0 text-muted">- Environmental Science Student</p>
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>

      {/* FOOTER (sticks to bottom via layout CSS) */}
      <footer className="footer">
        <div className="footer-content d-flex flex-column flex-md-row justify-content-between align-items-center py-4">
          <div className="footer-links mb-2 mb-md-0">
            <Link to="/terms" className="me-3">
              Terms &amp; Conditions
            </Link>
            <a href="mailto:sustainability@westminster.ac.uk">Contact Us</a>
          </div>
          <div>&copy; {new Date().getFullYear()} Green Westminster. All rights reserved.</div>
        </div>
      </footer>
    </div>
  );
};

export default HomePage;
