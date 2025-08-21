window.IntersectionObserver = class {
  constructor() {}
  observe() {}
  disconnect() {}
  unobserve() {}
};

import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import HomePage from '../components/HomePage';
import { MemoryRouter } from 'react-router-dom';

describe('HomePage', () => {
  it('renders Green Westminster title', () => {
    render(
      <MemoryRouter>
        <HomePage />
      </MemoryRouter>
    );
    expect(screen.getAllByText(/Green Westminster/i).length).toBeGreaterThan(0);
  });
});