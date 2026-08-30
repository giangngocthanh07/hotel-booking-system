// App.tsx
// Application entry point — configures the Router and all routes.

import { Routes, Route } from "react-router-dom";
import Header from "./components/layout/Header";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import HomePage from "./pages/HomePage";
import SearchResultsPage from "./pages/SearchResultsPage";
import AdminLayout from "./layouts/AdminLayout";
import AdminDashboardPage from "./pages/admin/AdminDashboardPage";
import AdminRequestsPage from "./pages/admin/AdminRequestsPage";
import BecomePartnerPage from "./pages/customer/BecomePartnerPage";
import NotFoundPage from "./pages/NotFoundPage";

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

        {/* Customer section */}
        <Route path="/become-partner" element={<BecomePartnerPage />} />

        {/* Admin section */}
        <Route path="/admin" element={<AdminLayout />}>
          <Route path="dashboard" element={<AdminDashboardPage />} />
          <Route path="requests" element={<AdminRequestsPage />} />
          {/* Add more admin pages here later, e.g. users, hotels, etc. */}
        </Route>

        {/* Any unmatched URL shows 404 Not Found */}
        <Route path="/404" element={<NotFoundPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </>
  );
}

export default App;
