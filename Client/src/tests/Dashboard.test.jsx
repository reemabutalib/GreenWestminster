import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import Dashboard from '../components/Dashboard';
import { MemoryRouter } from 'react-router-dom';

// Mock ResizeObserver for recharts
if (typeof window !== 'undefined' && !window.ResizeObserver) {
  window.ResizeObserver = class {
    observe() {}
    unobserve() {}
    disconnect() {}
  };
}

// Mock useAuth
vi.mock('../components/context/UserContext', () => ({
  useAuth: () => ({
    currentUser: {
      userId: 'test-user-id',
      username: 'testuser',
      points: 100,
      avatarStyle: 'classic',
      avatarItems: [],
    },
  }),
}));

// Mock fetch for all API calls
beforeEach(() => {
  globalThis.fetch = vi.fn((url) => {
    // Mock user data
    if (
      url.includes('/api/users/test-user-id') &&
      !url.includes('carbon-impact') &&
      !url.includes('activity-stats') &&
      !url.includes('points-history') &&
      !url.includes('activities')
    ) {
      return Promise.resolve({
        ok: true,
        json: () =>
          Promise.resolve({
            userId: 'test-user-id',
            username: 'testuser',
            points: 100,
            avatarStyle: 'classic',
            avatarItems: [],
          }),
      });
    }
    // Mock activities endpoint
    if (url.includes('/api/users/test-user-id/activities')) {
      return Promise.resolve({
        ok: true,
        json: () => Promise.resolve([]), // Return an empty array or mock activities
      });
    }
    // Mock level info
    if (url.includes('/api/activities/level/')) {
      return Promise.resolve({
        ok: true,
        json: () =>
          Promise.resolve({
            currentLevel: 1,
            pointsToNextLevel: 100,
            progressPercentage: 0,
            levelThresholds: [
              { level: 1, threshold: 0 },
              { level: 2, threshold: 100 },
              { level: 3, threshold: 250 },
              { level: 4, threshold: 500 },
              { level: 5, threshold: 1000 },
            ],
          }),
      });
    }
    // Mock carbon impact
    if (url.includes('carbon-impact')) {
      return Promise.resolve({
        ok: true,
        json: () =>
          Promise.resolve({
            co2Reduced: 50,
            treesEquivalent: 2,
            waterSaved: 100,
          }),
      });
    }
    // Mock activity stats
    if (url.includes('activity-stats')) {
      return Promise.resolve({
        ok: true,
        json: () =>
          Promise.resolve({
            categoryCounts: [],
            weeklyActivity: [],
          }),
      });
    }
    // Mock points history
    if (url.includes('points-history')) {
      return Promise.resolve({
        ok: true,
        json: () => Promise.resolve([]),
      });
    }
    // Default fallback
    return Promise.resolve({
      ok: true,
      json: () => Promise.resolve({}),
    });
  });
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('Dashboard', () => {
  it('renders dashboard welcome message', async () => {
    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );
    const welcome = await screen.findByText(/welcome/i);
    expect(welcome).to.exist;
  });

  it('shows user points', async () => {
    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );
    const points = await screen.findByTestId('user-points');
    expect(points.textContent).to.equal('100');
  });
});