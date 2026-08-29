// RegisterPage.tsx
// CONTAINER component: manages state and calls authService.
// Passes state and handlers down to RegisterForm for rendering.

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import RegisterForm from "../components/auth/RegisterForm";
import { registerUser, isApiSuccess } from "../services/authService";
import type { RegisterRequest } from "../types/auth.types";

function RegisterPage() {
  // State for each input field
  const [username, setUsername] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  // State for loading status, error messages, and success message
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const navigate = useNavigate();

  // Client-side validation before sending data to the server
  function validateForm(): boolean {
    if (username.trim() === "") {
      setErrorMessage("Please enter a username.");
      return false;
    }
    if (fullName.trim() === "") {
      setErrorMessage("Please enter your full name.");
      return false;
    }
    if (email.trim() === "") {
      setErrorMessage("Please enter your email address.");
      return false;
    }
    if (phoneNumber.trim() === "") {
      setErrorMessage("Please enter your phone number.");
      return false;
    }
    if (password.trim() === "") {
      setErrorMessage("Please enter a password.");
      return false;
    }
    if (confirmPassword !== password) {
      setErrorMessage("Passwords do not match.");
      return false;
    }
    return true;
  }

  // Called when the user clicks the Create Account button
  async function handleSubmit() {
    setErrorMessage("");
    setSuccessMessage("");

    const isValid = validateForm();
    if (!isValid) {
      return;
    }

    setIsLoading(true);

    const request: RegisterRequest = {
      username: username,
      fullName: fullName,
      email: email,
      phoneNumber: phoneNumber,
      password: password,
      confirmPassword: confirmPassword,
    };

    try {
      const response = await registerUser(request);

      if (isApiSuccess(response)) {
        setSuccessMessage(
          "Account created successfully! Redirecting to sign in...",
        );
        // Wait 2 seconds then redirect to login page
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else {
        setErrorMessage(
          response.message || "Registration failed. Please try again.",
        );
      }
    } catch {
      setErrorMessage(
        "Connection error. Please check your network and try again.",
      );
    } finally {
      setIsLoading(false);
    }
  }

  // Pass state and handlers down to the RegisterForm component
  return (
    <div className="auth-page">
      <RegisterForm
        username={username}
        fullName={fullName}
        email={email}
        phoneNumber={phoneNumber}
        password={password}
        confirmPassword={confirmPassword}
        isLoading={isLoading}
        errorMessage={errorMessage}
        successMessage={successMessage}
        onUsernameChange={setUsername}
        onFullNameChange={setFullName}
        onEmailChange={setEmail}
        onPhoneNumberChange={setPhoneNumber}
        onPasswordChange={setPassword}
        onConfirmPasswordChange={setConfirmPassword}
        onSubmit={handleSubmit}
      />
    </div>
  );
}

export default RegisterPage;
