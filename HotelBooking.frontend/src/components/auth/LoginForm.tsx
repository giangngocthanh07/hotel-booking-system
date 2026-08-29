// LoginForm.tsx
// Component THUAN UI (Presentational) - khong goi API, khong co state
// Nhan du lieu va ham tu LoginPage thong qua props, chi hien thi form

import React from "react";

// Dinh nghia cac props ma LoginForm can nhan tu LoginPage
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
        <p>Tim cho nghi hoan hao cho ban</p>
      </div>

      <h2 className="auth-title">Dang nhap</h2>

      {/* Chi hien thi thong bao loi neu co loi */}
      {props.errorMessage !== "" && (
        <div className="alert alert-error">
          {props.errorMessage}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* O nhap ten dang nhap hoac email */}
        <div className="form-group">
          <label htmlFor="usernameOrEmail">Ten dang nhap hoac Email</label>
          <input
            id="usernameOrEmail"
            type="text"
            value={props.usernameOrEmail}
            placeholder="Nhap ten dang nhap hoac email..."
            onChange={(e) => props.onUsernameOrEmailChange(e.target.value)}
          />
        </div>

        {/* O nhap mat khau */}
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

        {/* Nut dang nhap - bi disable khi dang loading */}
        <button
          type="submit"
          className="btn-primary"
          disabled={props.isLoading}
        >
          {props.isLoading ? "Dang dang nhap..." : "Dang nhap"}
        </button>
      </form>

      {/* Link chuyen sang trang dang ky */}
      <div className="auth-footer">
        Chua co tai khoan?{" "}
        <a href="/register">Dang ky ngay</a>
      </div>
    </div>
  );
}

export default LoginForm;
