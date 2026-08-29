// LoginForm.tsx
// PRESENTATIONAL component — no API calls, no state management.
// Receives data and callbacks from LoginPage via props and renders the UI only.

import React from "react";

// Props that LoginForm expects to receive from LoginPage
interface LoginFormProps {
  usernameOrEmail: string;
  password: string;
  isLoading: boolean;
  errorMessage: string;
  onUsernameOrEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: () => void;
}

function LoginForm(props: LoginFormProps) {
  // Prevent the browser from reloading the page on form submit
  function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    props.onSubmit();
  }

  return (
    <div className="auth-card">
      {/* App logo and tagline */}
      <div className="auth-logo">
        <h1>HotelBooking</h1>
        <p>Find your perfect stay</p>
      </div>

      <h2 className="auth-title">Sign In</h2>

      {/* Show error alert only when there is an error */}
      {props.errorMessage !== "" && (
        <div className="alert alert-error">
          {props.errorMessage}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Username or Email input */}
        <div className="form-group">
          <label htmlFor="usernameOrEmail">Username or Email</label>
          <input
            id="usernameOrEmail"
            type="text"
            value={props.usernameOrEmail}
            placeholder="Enter your username or email..."
            onChange={(e) => props.onUsernameOrEmailChange(e.target.value)}
          />
        </div>

        {/* Password input */}
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={props.password}
            placeholder="Enter your password..."
            onChange={(e) => props.onPasswordChange(e.target.value)}
          />
        </div>

        {/* Submit button — disabled while loading */}
        <button
          type="submit"
          className="btn-primary"
          disabled={props.isLoading}
        >
          {props.isLoading ? "Signing in..." : "Sign In"}
        </button>
      </form>

      {/* Link to the register page */}
      <div className="auth-footer">
        Don&apos;t have an account?{" "}
        <a href="/register">Sign up</a>
      </div>
    </div>
  );
}

export default LoginForm;
