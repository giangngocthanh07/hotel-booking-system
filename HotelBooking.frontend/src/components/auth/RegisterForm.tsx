// RegisterForm.tsx
// PRESENTATIONAL component — no API calls, no state management.
// Receives data and callbacks from RegisterPage via props and renders the UI only.

import React from "react";

// Props that RegisterForm expects to receive from RegisterPage
interface RegisterFormProps {
  username: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  isLoading: boolean;
  errorMessage: string;
  successMessage: string;
  onUsernameChange: (value: string) => void;
  onFullNameChange: (value: string) => void;
  onEmailChange: (value: string) => void;
  onPhoneNumberChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onConfirmPasswordChange: (value: string) => void;
  onSubmit: () => void;
}

function RegisterForm(props: RegisterFormProps) {
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
        <p className="auth-tagline">🎉 It&apos;s free — join us today!</p>
      </div>

      <div className="auth-title-block">
        <h2 className="auth-title">Create your account</h2>
        <p className="auth-subtitle">Fill in the details below to get started</p>
      </div>

      {/* Show error alert */}
      {props.errorMessage !== "" && (
        <div className="alert alert-error">
          {props.errorMessage}
        </div>
      )}

      {/* Show success alert */}
      {props.successMessage !== "" && (
        <div className="alert alert-success">
          {props.successMessage}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="username">Username</label>
          <input
            id="username"
            type="text"
            value={props.username}
            placeholder="Enter your username..."
            onChange={(e) => props.onUsernameChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="fullName">Full Name</label>
          <input
            id="fullName"
            type="text"
            value={props.fullName}
            placeholder="Enter your full name..."
            onChange={(e) => props.onFullNameChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={props.email}
            placeholder="Enter your email address..."
            onChange={(e) => props.onEmailChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="phoneNumber">Phone Number</label>
          <input
            id="phoneNumber"
            type="tel"
            value={props.phoneNumber}
            placeholder="Enter your phone number..."
            onChange={(e) => props.onPhoneNumberChange(e.target.value)}
          />
        </div>

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

        <div className="form-group">
          <label htmlFor="confirmPassword">Confirm Password</label>
          <input
            id="confirmPassword"
            type="password"
            value={props.confirmPassword}
            placeholder="Re-enter your password..."
            onChange={(e) => props.onConfirmPasswordChange(e.target.value)}
          />
        </div>

        {/* Submit button — disabled while loading */}
        <button
          type="submit"
          className="btn-primary"
          disabled={props.isLoading}
        >
          {props.isLoading ? "Creating account..." : "Create Account"}
        </button>
      </form>

      {/* Link to the login page */}
      <div className="auth-footer">
        Already have an account?{" "}
        <a href="/login" className="auth-footer-link">Sign in →</a>
      </div>
    </div>
  );
}

export default RegisterForm;
