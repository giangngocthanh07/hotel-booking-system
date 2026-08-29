import { useEffect, useState, useMemo } from "react";
import { 
  getHotelApprovals, 
  approveHotel, 
  rejectHotel,
  getUpgradeRequests,
  approveUpgrade,
  rejectUpgrade,
  getRequestStats
} from "../../services/adminService";
import type { HotelApprovalRequest, UpgradeRequest, BaseRequest, RequestStats } from "../../types/admin.types";
import "./AdminRequestsPage.css"; // Implemented Sunset Theme

type RequestType = "Hotel" | "Upgrade";

function AdminRequestsPage() {
  const [requestType, setRequestType] = useState<RequestType>("Hotel");
  const [statusFilter, setStatusFilter] = useState("All");
  const [searchTerm, setSearchTerm] = useState("");
  
  // Pagination State
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Data State
  const [requests, setRequests] = useState<(HotelApprovalRequest | UpgradeRequest)[]>([]);
  const [stats, setStats] = useState<RequestStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Modal State
  const [selectedRequest, setSelectedRequest] = useState<HotelApprovalRequest | UpgradeRequest | null>(null);

  // Fetch Data
  async function fetchData() {
    setLoading(true);
    setError("");
    try {
      const statsRes = await getRequestStats();
      if (statsRes.statusCode === "Success" && statsRes.content) {
        setStats(statsRes.content);
      }

      const status = statusFilter === "All" ? undefined : statusFilter;
      if (requestType === "Hotel") {
        const res = await getHotelApprovals(pageIndex, pageSize, status);
        if (res.statusCode === "Success" && res.content) {
          setRequests(res.content.items || []);
          setTotalPages(res.content.totalPages || 1);
          setTotalCount(res.content.totalCount || 0);
        } else {
          setError(res.message || "Failed to load requests.");
        }
      } else {
        const res = await getUpgradeRequests(pageIndex, pageSize, status);
        if (res.statusCode === "Success" && res.content) {
          setRequests(res.content.items || []);
          setTotalPages(res.content.totalPages || 1);
          setTotalCount(res.content.totalCount || 0);
        } else {
          setError(res.message || "Failed to load requests.");
        }
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Network error");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [requestType, statusFilter, pageIndex, pageSize]);

  function handleTypeChange(newType: RequestType) {
    setRequestType(newType);
    setPageIndex(1);
    setSearchTerm("");
  }

  function handleStatusChange(newStatus: string) {
    setStatusFilter(newStatus);
    setPageIndex(1);
  }

  const filteredRequests = useMemo(() => {
    if (!searchTerm.trim()) return requests;
    
    const lowerSearch = searchTerm.toLowerCase();
    return requests.filter(req => {
      if (req.requestId.toString().includes(lowerSearch)) return true;
      
      if (requestType === "Hotel") {
        const hReq = req as HotelApprovalRequest;
        return (
          hReq.name.toLowerCase().includes(lowerSearch) ||
          hReq.ownerFullName.toLowerCase().includes(lowerSearch) ||
          hReq.ownerEmail.toLowerCase().includes(lowerSearch) ||
          hReq.taxCode.toLowerCase().includes(lowerSearch)
        );
      } else {
        const uReq = req as UpgradeRequest;
        return (
          uReq.fullName.toLowerCase().includes(lowerSearch) ||
          uReq.userName.toLowerCase().includes(lowerSearch) ||
          uReq.email.toLowerCase().includes(lowerSearch) ||
          uReq.taxCode.toLowerCase().includes(lowerSearch)
        );
      }
    });
  }, [requests, searchTerm, requestType]);

  async function handleApprove(id: number) {
    if (!window.confirm("Are you sure you want to approve this request?")) return;
    try {
      const res = requestType === "Hotel" ? await approveHotel(id) : await approveUpgrade(id);
      if (res.statusCode === "Success") {
        alert("Request approved successfully!");
        if (selectedRequest?.requestId === id) setSelectedRequest(null);
        fetchData();
      } else {
        alert("Failed: " + res.message);
      }
    } catch (e) {
      alert("Error approving request.");
    }
  }

  async function handleReject(id: number) {
    if (!window.confirm("Are you sure you want to reject this request?")) return;
    try {
      const res = requestType === "Hotel" ? await rejectHotel(id) : await rejectUpgrade(id);
      if (res.statusCode === "Success") {
        alert("Request rejected successfully!");
        if (selectedRequest?.requestId === id) setSelectedRequest(null);
        fetchData();
      } else {
        alert("Failed: " + res.message);
      }
    } catch (e) {
      alert("Error rejecting request.");
    }
  }

  function renderStatusBadge(status: string) {
    const statusClass = status.toLowerCase();
    return (
      <span className={`req-badge ${statusClass}`}>
        {status}
      </span>
    );
  }

  function renderActionButtons(req: BaseRequest) {
    return (
      <div style={{ display: "flex", justifyContent: "flex-end", gap: "8px" }}>
        {req.canApprove && (
          <button onClick={() => handleApprove(req.requestId)} className="req-action-btn approve">
            Approve
          </button>
        )}
        {req.canReject && (
          <button onClick={() => handleReject(req.requestId)} className="req-action-btn reject">
            Reject
          </button>
        )}
      </div>
    );
  }

  function renderHotelRow(req: HotelApprovalRequest) {
    return (
      <tr key={req.requestId} style={{ cursor: "pointer" }} onClick={() => setSelectedRequest(req)}>
        <td>#{req.requestId}</td>
        <td>
          <div style={{ fontWeight: 600, color: "#0F172A", marginBottom: "4px" }}>{req.name}</div>
          <div style={{ fontSize: "12px", color: "#64748B" }}>{req.address}</div>
        </td>
        <td>
          <div style={{ color: "#0F172A", marginBottom: "4px" }}>{req.ownerFullName}</div>
          <div style={{ fontSize: "12px", color: "#64748B" }}>{req.ownerEmail}</div>
        </td>
        <td>{new Date(req.requestedAt).toLocaleDateString()}</td>
        <td>{renderStatusBadge(req.status)}</td>
        <td onClick={(e) => e.stopPropagation()}>{renderActionButtons(req)}</td>
      </tr>
    );
  }

  function renderUpgradeRow(req: UpgradeRequest) {
    return (
      <tr key={req.requestId} style={{ cursor: "pointer" }} onClick={() => setSelectedRequest(req)}>
        <td>#{req.requestId}</td>
        <td>
          <div style={{ fontWeight: 600, color: "#0F172A", marginBottom: "4px" }}>{req.fullName}</div>
          <div style={{ fontSize: "12px", color: "#64748B" }}>@{req.userName}</div>
        </td>
        <td>
          <div style={{ color: "#0F172A", marginBottom: "4px" }}>{req.email}</div>
          <div style={{ fontSize: "12px", color: "#64748B" }}>{req.phoneNumber}</div>
        </td>
        <td>{req.taxCode}</td>
        <td>{renderStatusBadge(req.status)}</td>
        <td onClick={(e) => e.stopPropagation()}>{renderActionButtons(req)}</td>
      </tr>
    );
  }

  function renderModal() {
    if (!selectedRequest) return null;

    return (
      <div className="req-modal-overlay">
        <div className="req-modal-content">
          <div className="req-modal-header">
            <h2 className="req-modal-title sunset-gradient-text">
              {requestType === "Hotel" ? "Hotel Approval Details" : "Upgrade Request Details"}
            </h2>
            <button onClick={() => setSelectedRequest(null)} className="req-modal-close">✕</button>
          </div>
          
          <div className="req-modal-body">
            <div className="req-modal-grid">
              <div>
                <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 700, marginBottom: "4px" }}>Request ID</div>
                <div style={{ fontSize: "16px", color: "#0F172A", fontWeight: 600 }}>#{selectedRequest.requestId}</div>
              </div>
              <div>
                <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 700, marginBottom: "4px" }}>Status</div>
                <div>{renderStatusBadge(selectedRequest.status)}</div>
              </div>
              <div>
                <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 700, marginBottom: "4px" }}>Requested At</div>
                <div style={{ fontSize: "15px", color: "#0F172A" }}>{new Date(selectedRequest.requestedAt).toLocaleString()}</div>
              </div>
              {selectedRequest.processedAt && (
                <div>
                  <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 700, marginBottom: "4px" }}>Processed At</div>
                  <div style={{ fontSize: "15px", color: "#0F172A" }}>{new Date(selectedRequest.processedAt).toLocaleString()}</div>
                </div>
              )}
            </div>

            <h3 style={{ fontSize: "18px", color: "#0F172A", marginBottom: "16px", fontWeight: 700 }}>Specific Information</h3>
            <div className="req-info-box">
              {requestType === "Hotel" ? (
                <div style={{ display: "grid", gap: "12px" }}>
                  <div><strong style={{ color: "#475569" }}>Hotel Name:</strong> {(selectedRequest as HotelApprovalRequest).name}</div>
                  <div><strong style={{ color: "#475569" }}>Address:</strong> {(selectedRequest as HotelApprovalRequest).address}</div>
                  <div><strong style={{ color: "#475569" }}>Location:</strong> {(selectedRequest as HotelApprovalRequest).wardName}, {(selectedRequest as HotelApprovalRequest).provinceName}, {(selectedRequest as HotelApprovalRequest).countryName}</div>
                  <div><strong style={{ color: "#475569" }}>Type:</strong> {(selectedRequest as HotelApprovalRequest).propertyTypeName}</div>
                  <div><strong style={{ color: "#475569" }}>Tax Code:</strong> {(selectedRequest as HotelApprovalRequest).taxCode}</div>
                  <div><strong style={{ color: "#475569" }}>Business License:</strong> <a href={(selectedRequest as HotelApprovalRequest).businessLicenseUrl} target="_blank" rel="noreferrer" style={{color:"#EC4899", fontWeight: 600}}>View Document</a></div>
                  <hr style={{ borderColor: "#E2E8F0", margin: "16px 0" }} />
                  <div><strong style={{ color: "#475569" }}>Owner:</strong> {(selectedRequest as HotelApprovalRequest).ownerFullName} (ID: {(selectedRequest as HotelApprovalRequest).ownerId})</div>
                  <div><strong style={{ color: "#475569" }}>Owner Email:</strong> {(selectedRequest as HotelApprovalRequest).ownerEmail}</div>
                  <div><strong style={{ color: "#475569" }}>Owner Phone:</strong> {(selectedRequest as HotelApprovalRequest).ownerPhoneNumber}</div>
                </div>
              ) : (
                <div style={{ display: "grid", gap: "12px" }}>
                  <div><strong style={{ color: "#475569" }}>Customer:</strong> {(selectedRequest as UpgradeRequest).fullName} (@{(selectedRequest as UpgradeRequest).userName})</div>
                  <div><strong style={{ color: "#475569" }}>User ID:</strong> {(selectedRequest as UpgradeRequest).userId}</div>
                  <div><strong style={{ color: "#475569" }}>Email:</strong> {(selectedRequest as UpgradeRequest).email}</div>
                  <div><strong style={{ color: "#475569" }}>Phone:</strong> {(selectedRequest as UpgradeRequest).phoneNumber}</div>
                  <div><strong style={{ color: "#475569" }}>Address:</strong> {(selectedRequest as UpgradeRequest).address}</div>
                  <div><strong style={{ color: "#475569" }}>Tax Code:</strong> {(selectedRequest as UpgradeRequest).taxCode}</div>
                </div>
              )}
            </div>

            <div style={{ marginTop: "32px", display: "flex", justifyContent: "flex-end", gap: "12px" }}>
              {renderActionButtons(selectedRequest)}
              <button onClick={() => setSelectedRequest(null)} style={{ padding: "8px 20px", backgroundColor: "#F1F5F9", color: "#475569", border: "1px solid #E2E8F0", borderRadius: "9999px", cursor: "pointer", fontSize: "14px", fontWeight: 600 }}>
                Close
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const currentStats = requestType === "Hotel" ? stats?.hotelApproval : stats?.upgradeRequest;

  return (
    <div className="admin-page">
      <div className="req-header-top">
        <div>
          <h1 className="sunset-gradient-text" style={{ fontSize: "28px", margin: "0 0 20px 0" }}>Requests Management</h1>
          <div className="req-tabs">
            <button 
              onClick={() => handleTypeChange("Hotel")}
              className={`req-tab-btn ${requestType === "Hotel" ? "active" : "inactive"}`}
            >
              🏨 Hotel Approvals
            </button>
            <button 
              onClick={() => handleTypeChange("Upgrade")}
              className={`req-tab-btn ${requestType === "Upgrade" ? "active" : "inactive"}`}
            >
              👤 Owner Upgrades
            </button>
          </div>
        </div>
      </div>

      {/* Stats Cards */}
      {currentStats && (
        <div className="req-stats-grid">
          <div className="req-stat-card total">
            <div className="req-stat-title">Total</div>
            <div className="req-stat-value sunset-gradient-text">{currentStats.total}</div>
          </div>
          <div className="req-stat-card pending">
            <div className="req-stat-title">Pending</div>
            <div className="req-stat-value" style={{ color: "#F59E0B" }}>{currentStats.pending}</div>
          </div>
          <div className="req-stat-card approved">
            <div className="req-stat-title">Approved</div>
            <div className="req-stat-value" style={{ color: "#10B981" }}>{currentStats.approved}</div>
          </div>
          <div className="req-stat-card rejected">
            <div className="req-stat-title">Rejected</div>
            <div className="req-stat-value" style={{ color: "#EF4444" }}>{currentStats.rejected}</div>
          </div>
          <div className="req-stat-card cancelled">
            <div className="req-stat-title">Cancelled</div>
            <div className="req-stat-value" style={{ color: "#9CA3AF" }}>{currentStats.cancelled}</div>
          </div>
        </div>
      )}

      {/* Filter and Search Bar */}
      <div className="req-controls">
        <div className="req-search-wrapper">
          <span className="req-search-icon">🔍</span>
          <input 
            type="text" 
            placeholder={requestType === "Hotel" ? "Search by Hotel Name, Owner, Email or Tax Code..." : "Search by Name, Username, Email or Tax Code..."}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="req-search-input"
          />
        </div>

        <div style={{ display: "flex", gap: "12px" }}>
          <select 
            value={statusFilter} 
            onChange={(e) => handleStatusChange(e.target.value)}
            className="req-filter-select"
          >
            <option value="All">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="Cancelled">Cancelled</option>
          </select>
          <select 
            value={pageSize} 
            onChange={(e) => { setPageSize(Number(e.target.value)); setPageIndex(1); }}
            className="req-filter-select"
          >
            <option value={10}>10 per page</option>
            <option value={20}>20 per page</option>
            <option value={50}>50 per page</option>
          </select>
        </div>
      </div>

      {error && <div style={{ padding: "16px", backgroundColor: "#FEF2F2", color: "#DC2626", borderRadius: "12px", marginBottom: "20px", border: "1px solid #FCA5A5" }}>{error}</div>}

      {loading ? (
        <div style={{ padding: "60px", textAlign: "center", color: "#64748B", fontSize: "16px" }}>Loading requests...</div>
      ) : filteredRequests.length === 0 ? (
        <div style={{ padding: "60px", textAlign: "center", backgroundColor: "#fff", borderRadius: "16px", border: "1px solid #F1F5F9", color: "#64748B", fontSize: "16px" }}>
          {searchTerm ? "No matching requests found for your search." : `No ${requestType.toLowerCase()} requests found.`}
        </div>
      ) : (
        <div className="req-table-container">
          <div style={{ overflowX: "auto" }}>
            <table className="req-table">
              <thead>
                <tr>
                  <th>ID</th>
                  {requestType === "Hotel" ? (
                    <>
                      <th>Hotel Name</th>
                      <th>Owner</th>
                      <th>Date</th>
                    </>
                  ) : (
                    <>
                      <th>Customer</th>
                      <th>Contact</th>
                      <th>Tax Code</th>
                    </>
                  )}
                  <th>Status</th>
                  <th style={{ textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {requestType === "Hotel" 
                  ? (filteredRequests as HotelApprovalRequest[]).map(renderHotelRow)
                  : (filteredRequests as UpgradeRequest[]).map(renderUpgradeRow)
                }
              </tbody>
            </table>
          </div>
          
          <div className="req-pagination">
            <div style={{ fontSize: "14px", color: "#64748B" }}>
              Showing <strong>{filteredRequests.length}</strong> results on this page {totalCount > 0 ? `(out of ${totalCount} total)` : ""}
            </div>
            <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
              <button 
                disabled={pageIndex === 1}
                onClick={() => setPageIndex(p => p - 1)}
                className="req-page-btn"
              >
                Previous
              </button>
              <span style={{ fontSize: "14px", color: "#0F172A", margin: "0 8px", fontWeight: 600 }}>Page {pageIndex} of {totalPages}</span>
              <button 
                disabled={pageIndex >= totalPages}
                onClick={() => setPageIndex(p => p + 1)}
                className="req-page-btn"
              >
                Next
              </button>
            </div>
          </div>
        </div>
      )}

      {renderModal()}
    </div>
  );
}

export default AdminRequestsPage;
