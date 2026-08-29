// LoginPage.tsx
// CONTAINER component: quan ly state va goi authService
// Sau do truyen du lieu xuong cho LoginForm de hien thi

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import LoginForm from "../components/auth/LoginForm";
import { loginUser } from "../services/authService";
import type { LoginRequest } from "../types/auth.types";

function LoginPage() {
  // State cho cac o input
  const [usernameOrEmail, setUsernameOrEmail] = useState("");
  const [password, setPassword] = useState("");

  // State cho trang thai loading va thong bao loi
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  // Hook de chuyen trang sau khi dang nhap thanh cong
  const navigate = useNavigate();

  // Kiem tra o nhap truoc khi gui len server
  function validateForm(): boolean {
    if (usernameOrEmail.trim() === "") {
      setErrorMessage("Vui long nhap ten dang nhap hoac email.");
      return false;
    }
    if (password.trim() === "") {
      setErrorMessage("Vui long nhap mat khau.");
      return false;
    }
    return true;
  }

  // Ham xu ly khi nguoi dung nhan nut Dang nhap
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

      if (response.isSuccess) {
        // Luu token vao localStorage
        localStorage.setItem("accessToken", response.data.accessToken);
        localStorage.setItem("fullName", response.data.fullName);

        // Luu danh sach roles vao localStorage (dung for loop thay vi map/join)
        let rolesString = "";
        for (let i = 0; i < response.data.roles.length; i++) {
          if (i > 0) {
            rolesString = rolesString + ",";
          }
          rolesString = rolesString + response.data.roles[i];
        }
        localStorage.setItem("roles", rolesString);

        // Chuyen ve trang chu
        navigate("/");
      } else {
        setErrorMessage(response.message || "Dang nhap that bai. Vui long thu lai.");
      }
    } catch {
      setErrorMessage("Loi ket noi. Vui long kiem tra lai.");
    } finally {
      setIsLoading(false);
    }
  }

  // Truyen state va ham xuong LoginForm de hien thi
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
