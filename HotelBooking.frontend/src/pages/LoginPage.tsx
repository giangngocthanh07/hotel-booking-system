// LoginPage.tsx
// CONTAINER component: manages state and calls authService.
// Passes state and handlers down to LoginForm for rendering.

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import LoginForm from "../components/auth/LoginForm";
import {
  loginUser,
  isApiSuccess,
  AUTH_CHANGED_EVENT,
} from "../services/authService";
import type { LoginRequest } from "../types/auth.types";

function LoginPage() {
  // State for each input field
  const [usernameOrEmail, setUsernameOrEmail] = useState("");
  const [password, setPassword] = useState("");

  // State for loading status and error messages
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  // Hook to navigate to another page after a successful login
  const navigate = useNavigate();

  // Client-side validation before sending data to the server
  function validateForm(): boolean {
    if (usernameOrEmail.trim() === "") {
      setErrorMessage("Please enter your username or email.");
      return false;
    }
    if (password.trim() === "") {
      setErrorMessage("Please enter your password.");
      return false;
    }
    return true;
  }

  // Called when the user clicks the Sign In button
  async function handleSubmit() {
    setErrorMessage("");

    const isValid = validateForm();
    if (!isValid) {
      return;
    }

    setIsLoading(true);

    const request: LoginRequest = {
      usernameOrEmail: usernameOrEmail,
      password: password,
    };

    try {
      const response = await loginUser(request);

      if (isApiSuccess(response)) {
        // Save token and user info to localStorage
        localStorage.setItem("accessToken", response.content.accessToken);
        localStorage.setItem("fullName", response.content.fullName);

        // Save roles list using a for loop (no map/join)
        let rolesString = "";
        for (let i = 0; i < response.content.roles.length; i++) {
          if (i > 0) {
            rolesString = rolesString + ",";
          }
          rolesString = rolesString + response.content.roles[i];
        }
        localStorage.setItem("roles", rolesString);

        // Let the Header (and any other listener) know the session changed
        window.dispatchEvent(new Event(AUTH_CHANGED_EVENT));

        // Redirect to home page
        navigate("/");
      } else {
        setErrorMessage(response.message || "Login failed. Please try again.");
      }
    } catch {
      setErrorMessage(
        "Connection error. Please check your network and try again.",
      );
    } finally {
      setIsLoading(false);
    }
  }

  // Pass state and handlers down to the LoginForm component
  return (
    <div className="auth-page">
      <LoginForm
        usernameOrEmail={usernameOrEmail}
        password={password}
        isLoading={isLoading}
        errorMessage={errorMessage}
        onUsernameOrEmailChange={setUsernameOrEmail}
        onPasswordChange={setPassword}
        onSubmit={handleSubmit}
      />
    </div>
  );
}

export default LoginPage;
