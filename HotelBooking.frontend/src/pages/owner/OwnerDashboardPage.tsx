import { useState, useEffect } from "react";
import { getAuthToken } from "../../services/authService";
import { API_BASE_URL } from "../../config/api";
import "./OwnerDashboardPage.css";

interface DashboardData {
  totalHotels: number;
  totalRooms: number;
  totalBookings: number;
  totalRevenue: number;
}

export default function OwnerDashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchDashboard();
  }, []);

  async function fetchDashboard() {
    try {
      const token = getAuthToken();
      const res = await fetch(`${API_BASE_URL}/hotels/owner-dashboard`, {
        headers: {
          "Authorization": `Bearer ${token}`
        }
      });
      
      const json = await res.json();
      if (res.ok && json.statusCode === "Success") {
        setData(json.content);
      } else {
        setError(json.message || "Failed to load dashboard data");
      }
    } catch (err) {
      console.error(err);
      setError("Network error fetching dashboard data");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="owner-dashboard">
      <h1>Owner Dashboard</h1>
      <p>Welcome back! Here is an overview of your properties.</p>

      {loading && <div className="loading-state">Loading dashboard data...</div>}
      
      {error && <div className="error-state">{error}</div>}

      {!loading && !error && data && (
        <>
          <div className="dashboard-stats">
            <div className="stat-card">
              <h3>Total Properties</h3>
              <div className="value">{data.totalHotels || 0}</div>
            </div>
            <div className="stat-card">
              <h3>Total Rooms</h3>
              <div className="value">{data.totalRooms || 0}</div>
            </div>
            <div className="stat-card">
              <h3>Total Bookings</h3>
              <div className="value">{data.totalBookings || 0}</div>
            </div>
            <div className="stat-card">
              <h3>Total Revenue (VND)</h3>
              <div className="value">
                {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(data.totalRevenue || 0)}
              </div>
            </div>
          </div>

          <div className="dashboard-content">
            <h2>Recent Activity</h2>
            <p style={{ color: "#64748B" }}>No recent activity to display.</p>
          </div>
        </>
      )}
    </div>
  );
}
