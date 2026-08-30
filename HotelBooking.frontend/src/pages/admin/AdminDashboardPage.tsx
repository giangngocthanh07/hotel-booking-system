import { useEffect, useState } from "react";
import { getDashboardOverview } from "../../services/adminService";
import type { AdminDashboardStats } from "../../types/adminDashboard.types";
import "./AdminDashboardPage.css";

export default function AdminDashboardPage() {
  const [stats, setStats] = useState<AdminDashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadStats() {
      try {
        const res = await getDashboardOverview();
        if (res.statusCode === "Success" && res.content) {
          setStats(res.content);
        } else {
          setError(res.message || "Failed to load dashboard data");
        }
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : "Network error");
      } finally {
        setLoading(false);
      }
    }
    loadStats();
  }, []);

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', minHeight: '300px' }}>
        <div style={{ color: '#EC4899', fontSize: '18px', fontWeight: 600 }}>Loading dashboard...</div>
      </div>
    );
  }

  if (error || !stats) {
    return (
      <div style={{ padding: '24px', color: '#EF4444' }}>
        {error || "No data available."}
      </div>
    );
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
  };

  const statCards = [
    { title: "Total Users", value: stats.totalUsers, icon: "👥", color: '#3B82F6', gradient: "linear-gradient(135deg, rgba(59,130,246,0.1), rgba(59,130,246,0.05))" },
    { title: "Total Hotels", value: stats.totalHotels, icon: "🏨", color: '#F59E0B', gradient: "linear-gradient(135deg, rgba(245,158,11,0.1), rgba(245,158,11,0.05))" },
    { title: "Total Bookings", value: stats.totalBookings, icon: "📅", color: '#10B981', gradient: "linear-gradient(135deg, rgba(16,185,129,0.1), rgba(16,185,129,0.05))" },
    { title: "Revenue", value: formatCurrency(stats.totalRevenue), icon: "💰", color: '#EC4899', gradient: "linear-gradient(135deg, rgba(236,72,153,0.1), rgba(236,72,153,0.05))" },
  ];

  return (
    <div className="dashboard-container">
      <h1 className="dashboard-title">
        Admin Dashboard
      </h1>

      {/* KPI Cards */}
      <div className="kpi-grid">
        {statCards.map((card, idx) => (
          <div className="kpi-card" key={idx} style={{ background: card.gradient }}>
            <div className="kpi-card-content">
              <div className="kpi-card-title">{card.title}</div>
              <div className="kpi-card-value">{card.value}</div>
            </div>
            <div className="kpi-card-icon" style={{ color: card.color }}>
              {card.icon}
            </div>
          </div>
        ))}
      </div>

      {/* Pending Requests Lists */}
      <div className="lists-grid">
        <div className="list-panel">
          <div className="list-panel-header">
            🏢 Pending Hotel Approvals
          </div>
          <div className="list-content">
            {stats.pendingHotelRequests.length === 0 ? (
              <div className="empty-list">No pending hotel requests</div>
            ) : (
              stats.pendingHotelRequests.map((req) => (
                <div className="list-item" key={req.id}>
                  <div className="list-item-avatar" style={{ backgroundColor: 'rgba(59,130,246,0.1)', color: '#3B82F6' }}>
                    📝
                  </div>
                  <div className="list-item-text">
                    <div className="list-item-primary">{req.title}</div>
                    <div className="list-item-secondary">
                      Owner: {req.requesterName} • {new Date(req.createdAt).toLocaleDateString()}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        <div className="list-panel">
          <div className="list-panel-header">
            👑 Pending Owner Upgrades
          </div>
          <div className="list-content">
            {stats.pendingUpgradeRequests.length === 0 ? (
              <div className="empty-list">No pending owner upgrades</div>
            ) : (
              stats.pendingUpgradeRequests.map((req) => (
                <div className="list-item" key={req.id}>
                  <div className="list-item-avatar" style={{ backgroundColor: 'rgba(236,72,153,0.1)', color: '#EC4899' }}>
                    👤
                  </div>
                  <div className="list-item-text">
                    <div className="list-item-primary">{req.title}</div>
                    <div className="list-item-secondary">
                      User: {req.requesterName} • {new Date(req.createdAt).toLocaleDateString()}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
      
      {/* Monthly Revenue Trend */}
      {stats.monthlyRevenueTrend.length > 0 && (
        <div className="chart-panel">
          <div className="chart-title">
            📈 Revenue Trend (Last 6 Months)
          </div>
          <div className="chart-bars">
            {stats.monthlyRevenueTrend.map((trend, idx) => {
              const maxAmount = Math.max(...stats.monthlyRevenueTrend.map(t => t.amount), 1);
              const heightPercent = `${(trend.amount / maxAmount) * 100}%`;
              return (
                <div className="chart-bar-container" key={idx}>
                  <div className="chart-bar" style={{ height: heightPercent }}></div>
                  <div className="chart-label">{trend.month}</div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
