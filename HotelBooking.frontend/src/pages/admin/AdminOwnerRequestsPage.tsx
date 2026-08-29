import { useEffect, useState } from "react";
import { getUpgradeRequests, approveUpgrade, rejectUpgrade } from "../../services/adminService";
import type { UpgradeRequest } from "../../types/admin.types";

function AdminOwnerRequestsPage() {
  const [requests, setRequests] = useState<UpgradeRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [statusFilter, setStatusFilter] = useState("Pending");

  async function fetchRequests() {
    setLoading(true);
    setError("");
    try {
      const res = await getUpgradeRequests(1, 50, statusFilter === "All" ? undefined : statusFilter);
      if (res.statusCode === "Success") {
        setRequests(res.data?.items || []);
      } else {
        setError(res.message || "Failed to load requests.");
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Network error");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchRequests();
  }, [statusFilter]);

  async function handleApprove(id: number) {
    if (!window.confirm("Are you sure you want to approve this upgrade request?")) return;
    try {
      const res = await approveUpgrade(id);
      if (res.statusCode === "Success") {
        alert("Upgrade request approved successfully!");
        fetchRequests();
      } else {
        alert("Failed: " + res.message);
      }
    } catch (e) {
      alert("Error approving request.");
    }
  }

  async function handleReject(id: number) {
    if (!window.confirm("Are you sure you want to reject this upgrade request?")) return;
    try {
      const res = await rejectUpgrade(id);
      if (res.statusCode === "Success") {
        alert("Upgrade request rejected successfully!");
        fetchRequests();
      } else {
        alert("Failed: " + res.message);
      }
    } catch (e) {
      alert("Error rejecting request.");
    }
  }

  return (
    <div className="admin-page">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "24px" }}>
        <h1 style={{ fontSize: "24px", color: "#0F172A", margin: 0 }}>Owner Upgrade Requests</h1>
        <select 
          value={statusFilter} 
          onChange={(e) => setStatusFilter(e.target.value)}
          style={{ padding: "8px 12px", borderRadius: "8px", border: "1px solid #CBD5E1", fontSize: "14px" }}
        >
          <option value="All">All Statuses</option>
          <option value="Pending">Pending</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
      </div>

      {error && <div style={{ padding: "12px", backgroundColor: "#FEE2E2", color: "#B91C1C", borderRadius: "8px", marginBottom: "16px" }}>{error}</div>}

      {loading ? (
        <p>Loading requests...</p>
      ) : requests.length === 0 ? (
        <div style={{ padding: "40px", textAlign: "center", backgroundColor: "#fff", borderRadius: "12px", border: "1px solid #E2E8F0" }}>
          No owner upgrade requests found.
        </div>
      ) : (
        <div style={{ backgroundColor: "#fff", borderRadius: "12px", border: "1px solid #E2E8F0", overflow: "hidden" }}>
          <table style={{ width: "100%", borderCollapse: "collapse", textAlign: "left" }}>
            <thead style={{ backgroundColor: "#F8FAFC", borderBottom: "1px solid #E2E8F0" }}>
              <tr>
                <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>ID</th>
                <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Customer</th>
                <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Contact</th>
                <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Tax Code</th>
                <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Status</th>
                <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase", textAlign: "right" }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {requests.map((req) => (
                <tr key={req.requestId} style={{ borderBottom: "1px solid #F1F5F9" }}>
                  <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A" }}>#{req.requestId}</td>
                  <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A", fontWeight: 500 }}>
                    {req.fullName}
                    <div style={{ fontSize: "12px", color: "#64748B", fontWeight: 400, marginTop: "4px" }}>@{req.userName}</div>
                  </td>
                  <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A" }}>
                    {req.email}
                    <div style={{ fontSize: "12px", color: "#64748B", marginTop: "4px" }}>{req.phoneNumber}</div>
                  </td>
                  <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A" }}>{req.taxCode}</td>
                  <td style={{ padding: "16px", fontSize: "14px" }}>
                    <span style={{ 
                      padding: "4px 8px", 
                      borderRadius: "9999px", 
                      fontSize: "12px", 
                      fontWeight: 600,
                      backgroundColor: req.status === "Approved" ? "#D1FAE5" : req.status === "Rejected" ? "#FEE2E2" : "#FEF3C7",
                      color: req.status === "Approved" ? "#065F46" : req.status === "Rejected" ? "#991B1B" : "#92400E"
                    }}>
                      {req.status}
                    </span>
                  </td>
                  <td style={{ padding: "16px", textAlign: "right" }}>
                    {req.canApprove && (
                      <button onClick={() => handleApprove(req.requestId)} style={{ padding: "6px 12px", backgroundColor: "#10B981", color: "white", border: "none", borderRadius: "6px", cursor: "pointer", fontSize: "13px", fontWeight: 600, marginRight: "8px" }}>
                        Approve
                      </button>
                    )}
                    {req.canReject && (
                      <button onClick={() => handleReject(req.requestId)} style={{ padding: "6px 12px", backgroundColor: "#EF4444", color: "white", border: "none", borderRadius: "6px", cursor: "pointer", fontSize: "13px", fontWeight: 600 }}>
                        Reject
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default AdminOwnerRequestsPage;
