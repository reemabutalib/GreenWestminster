import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import Register from '../components/auth/Register';
import { MemoryRouter } from 'react-router-dom';

vi.mock('../components/context/UserContext', () => ({
  useAuth: () => ({
    register: vi.fn(),
    currentUser: null,
  }),
}));

describe('Register', () => {
 it('renders registration form fields', () => {
  render(
    <MemoryRouter>
      <Register />
    </MemoryRouter>
  );
  expect(screen.getByLabelText(/Username/i)).to.exist;
  expect(screen.getByLabelText('Email', { selector: 'input[type="email"]' })).to.exist;
  expect(screen.getByLabelText('Password')).to.exist;
expect(screen.getByLabelText('Confirm Password')).to.exist;
});

  it('shows error if form submitted empty', () => {
    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>
    );
    fireEvent.click(screen.getAllByRole('button', { name: /create account/i })[0]);
    // Adjust this line based on your actual error message
    expect(screen.getAllByText(/required|username|email|password/i).length).toBeGreaterThan(0);
  });
});