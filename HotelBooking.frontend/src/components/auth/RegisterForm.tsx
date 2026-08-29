// RegisterForm.tsx
// Component THUAN UI (Presentational) - khong goi API, khong co state
// Nhan du lieu va ham tu RegisterPage thong qua props, chi hien thi form

import React from "react";

// Dinh nghia cac props ma RegisterForm can nhan tu RegisterPage
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
  // Ngan chan form reload trang (hanh vi mac dinh cua browser)
  function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    props.onSubmit();
  }

  return (
    <div className="auth-card">
      {/* Logo va tieu de app */}
      <div className="auth-logo">
        <h1>HotelBooking</h1>
        <p>Tao tai khoan mien phi</p>
      </div>

      <h2 className="auth-title">Dang ky</h2>

      {/* Hien thi thong bao loi */}
      {props.errorMessage !== "" && (
        <div className="alert alert-error">
          {props.errorMessage}
        </div>
      )}

      {/* Hien thi thong bao thanh cong */}
      {props.successMessage !== "" && (
        <div className="alert alert-success">
          {props.successMessage}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="username">Ten dang nhap</label>
          <input
            id="username"
            type="text"
            value={props.username}
            placeholder="Nhap ten dang nhap..."
            onChange={(e) => props.onUsernameChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="fullName">Ho va ten</label>
          <input
            id="fullName"
            type="text"
            value={props.fullName}
            placeholder="Nhap ho va ten..."
            onChange={(e) => props.onFullNameChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={props.email}
            placeholder="Nhap dia chi email..."
            onChange={(e) => props.onEmailChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="phoneNumber">So dien thoai</label>
          <input
            id="phoneNumber"
            type="tel"
            value={props.phoneNumber}
            placeholder="Nhap so dien thoai..."
            onChange={(e) => props.onPhoneNumberChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="password">Mat khau</label>
          <input
            id="password"
            type="password"
            value={props.password}
            placeholder="Nhap mat khau..."
            onChange={(e) => props.onPasswordChange(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="confirmPassword">Xac nhan mat khau</label>
          <input
            id="confirmPassword"
            type="password"
            value={props.confirmPassword}
            placeholder="Nhap lai mat khau..."
            onChange={(e) => props.onConfirmPasswordChange(e.target.value)}
          />
        </div>

        {/* Nut tao tai khoan - bi disable khi dang loading */}
        <button
          type="submit"
          className="btn-primary"
          disabled={props.isLoading}
        >
          {props.isLoading ? "Dang tao tai khoan..." : "Tao tai khoan"}
        </button>
      </form>

      {/* Link chuyen sang trang dang nhap */}
      <div className="auth-footer">
        Da co tai khoan?{" "}
        <a href="/login">Dang nhap</a>
      </div>
    </div>
  );
}

export default RegisterForm;
