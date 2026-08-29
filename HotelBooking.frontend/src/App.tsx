// App.tsx
// Diem vao cua ung dung: cau hinh Router va cac Routes (duong dan trang)

import { Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

function App() {
  return (
    <Routes>
      {/* Trang chu tam thoi - se thay the sau */}
      <Route
        path="/"
        element={
          <div style={{ textAlign: "center", padding: "80px", fontFamily: "sans-serif" }}>
            <h1 style={{ fontSize: "36px", background: "linear-gradient(135deg, #3B82F6, #EC4899)", WebkitBackgroundClip: "text", WebkitTextFillColor: "transparent" }}>
              HotelBooking
            </h1>
            <p style={{ color: "#64748B", marginTop: "12px", fontSize: "16px" }}>
              Trang chu - Dang xay dung...
            </p>
            <div style={{ marginTop: "28px", display: "flex", gap: "16px", justifyContent: "center" }}>
              <a href="/login" style={{ padding: "10px 24px", background: "#3B82F6", color: "white", borderRadius: "8px", textDecoration: "none", fontWeight: 600 }}>
                Dang nhap
              </a>
              <a href="/register" style={{ padding: "10px 24px", background: "#EC4899", color: "white", borderRadius: "8px", textDecoration: "none", fontWeight: 600 }}>
                Dang ky
              </a>
            </div>
          </div>
        }
      />

      {/* Trang dang nhap - URL: /login */}
      <Route path="/login" element={<LoginPage />} />

      {/* Trang dang ky - URL: /register */}
      <Route path="/register" element={<RegisterPage />} />

      {/* Moi URL khong khop -> chuyen ve trang chu */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
