// App.tsx
// Application entry point — configures the Router and all routes.

import { Routes, Route, Navigate } from "react-router-dom";
import Header from "./components/layout/Header";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import HomePage from "./pages/HomePage";
import SearchResultsPage from "./pages/SearchResultsPage";

function App() {
  return (
    <>
      <Header />
      <Routes>
        {/* Home page (placeholder — will be replaced later) */}
        {/* Home page — URL: / */}
        <Route path="/" element={<HomePage />} />

        {/* Login page — URL: /login */}
        <Route path="/login" element={<LoginPage />} />

        {/* Register page — URL: /register */}
        <Route path="/register" element={<RegisterPage />} />

        <Route path="/search-results" element={<SearchResultsPage />} />

        {/* Any unmatched URL redirects to home */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  );
}

export default App;
