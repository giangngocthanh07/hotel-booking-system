// RegisterPage.tsx
// CONTAINER component: quan ly state va goi authService
// Sau do truyen du lieu xuong cho RegisterForm de hien thi

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import RegisterForm from "../components/auth/RegisterForm";
import { registerUser } from "../services/authService";
import type { RegisterRequest } from "../types/auth.types";

function RegisterPage() {
  // State cho cac o nhap trong form
  const [username, setUsername] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  // State cho loading, loi va thanh cong
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const navigate = useNavigate();

  // Kiem tra cac o nhap truoc khi gui len server
  function validateForm(): boolean {
    if (username.trim() === "") {
      setErrorMessage("Vui long nhap ten dang nhap.");
      return false;
    }
    if (fullName.trim() === "") {
      setErrorMessage("Vui long nhap ho va ten.");
      return false;
    }
    if (email.trim() === "") {
      setErrorMessage("Vui long nhap email.");
      return false;
    }
    if (phoneNumber.trim() === "") {
      setErrorMessage("Vui long nhap so dien thoai.");
      return false;
    }
    if (password.trim() === "") {
      setErrorMessage("Vui long nhap mat khau.");
      return false;
    }
    if (confirmPassword !== password) {
      setErrorMessage("Mat khau xac nhan khong khop.");
      return false;
    }
    return true;
  }

  // Ham xu ly khi nguoi dung nhan nut Tao tai khoan
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

      if (response.isSuccess) {
        setSuccessMessage("Tao tai khoan thanh cong! Chuyen sang trang dang nhap...");
        // Doi 2 giay roi chuyen sang trang dang nhap
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else {
        setErrorMessage(response.message || "Dang ky that bai. Vui long thu lai.");
      }
    } catch {
      setErrorMessage("Loi ket noi. Vui long kiem tra lai.");
    } finally {
      setIsLoading(false);
    }
  }

  // Truyen state va ham xuong RegisterForm de hien thi
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
