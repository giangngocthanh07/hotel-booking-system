// AdminDashboardPage.tsx
import React from "react";

function AdminDashboardPage() {
  return (
    <div className="admin-dashboard">
      <h1 style={{ fontSize: "28px", color: "#0F172A", marginBottom: "8px" }}>
        Admin Dashboard
      </h1>
      <p style={{ color: "#64748B", marginBottom: "32px" }}>
        Welcome to the admin control panel. Here is the system overview.
      </p>

      {/* Basic Stats Cards for demonstration */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))", gap: "24px" }}>
        <div style={{ backgroundColor: "#fff", padding: "24px", borderRadius: "12px", border: "1px solid #E2E8F0", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
          <h3 style={{ fontSize: "14px", color: "#64748B", textTransform: "uppercase", marginBottom: "12px" }}>Total Users</h3>
          <p style={{ fontSize: "32px", fontWeight: "700", color: "#3B82F6", margin: 0 }}>1,248</p>
        </div>
        
        <div style={{ backgroundColor: "#fff", padding: "24px", borderRadius: "12px", border: "1px solid #E2E8F0", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
          <h3 style={{ fontSize: "14px", color: "#64748B", textTransform: "uppercase", marginBottom: "12px" }}>Pending Hotels</h3>
          <p style={{ fontSize: "32px", fontWeight: "700", color: "#F59E0B", margin: 0 }}>14</p>
        </div>

        <div style={{ backgroundColor: "#fff", padding: "24px", borderRadius: "12px", border: "1px solid #E2E8F0", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
          <h3 style={{ fontSize: "14px", color: "#64748B", textTransform: "uppercase", marginBottom: "12px" }}>Total Bookings</h3>
          <p style={{ fontSize: "32px", fontWeight: "700", color: "#10B981", margin: 0 }}>3,892</p>
        </div>

        <div style={{ backgroundColor: "#fff", padding: "24px", borderRadius: "12px", border: "1px solid #E2E8F0", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
          <h3 style={{ fontSize: "14px", color: "#64748B", textTransform: "uppercase", marginBottom: "12px" }}>Revenue (VND)</h3>
          <p style={{ fontSize: "28px", fontWeight: "700", color: "#EC4899", margin: 0 }}>4.2B</p>
        </div>
      </div>
    </div>
  );
}

export default AdminDashboardPage;
