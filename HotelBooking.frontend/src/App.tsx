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
import AdminManagementPage from "./pages/admin/AdminManagementPage";
import BecomePartnerPage from "./pages/customer/BecomePartnerPage";
import UserProfilePage from "./pages/user/UserProfilePage";
import HotelRegistrationPage from "./pages/owner/HotelRegistrationPage";
import OwnerLayout from "./layouts/OwnerLayout";
import OwnerDashboardPage from "./pages/owner/OwnerDashboardPage";
import ProtectedRoute from "./components/auth/ProtectedRoute";
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
        
        {/* User Profile */}
        <Route path="/profile" element={<UserProfilePage />} />

        {/* Owner section */}
        <Route element={<ProtectedRoute allowedRoles={["Owner"]} />}>
          <Route path="/owner" element={<OwnerLayout />}>
            <Route path="dashboard" element={<OwnerDashboardPage />} />
            <Route path="registration" element={<HotelRegistrationPage />} />
          </Route>
        </Route>

        {/* Admin section */}
        <Route path="/admin" element={<AdminLayout />}>
          <Route path="dashboard" element={<AdminDashboardPage />} />
          <Route path="requests" element={<AdminRequestsPage />} />
          <Route path="management" element={<AdminManagementPage />} />
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
