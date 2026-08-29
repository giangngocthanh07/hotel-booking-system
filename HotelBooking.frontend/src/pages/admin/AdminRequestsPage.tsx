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
import "./AdminRequestsPage.css"; 

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

  // --- HTML/CSS Custom Modal ---
  function renderModal() {
    if (!selectedRequest) return null;

    const isHotel = requestType === "Hotel";
    const hotelReq = selectedRequest as HotelApprovalRequest;
    const upgradeReq = selectedRequest as UpgradeRequest;

    return (
      <div className="req-modal-overlay">
        <div className="req-modal-content">
          <div className="req-modal-header">
            <h2 className="req-modal-title sunset-gradient-text">
              {isHotel ? "Hotel Approval Details" : "Upgrade Request Details"}
            </h2>
            <button onClick={() => setSelectedRequest(null)} className="req-modal-close">✕</button>
          </div>
          
          <div className="req-modal-body">
            {/* Overview Section */}
            <div className="req-modal-overview">
              <div className="overview-item">
                <span className="overview-label">Request ID</span>
                <span className="overview-value">#{selectedRequest.requestId}</span>
              </div>
              <div className="overview-item">
                <span className="overview-label">Status</span>
                <span>{renderStatusBadge(selectedRequest.status)}</span>
              </div>
              <div className="overview-item">
                <span className="overview-label">Requested At</span>
                <span className="overview-value">{new Date(selectedRequest.requestedAt).toLocaleString()}</span>
              </div>
              {selectedRequest.processedAt && (
                <div className="overview-item">
                  <span className="overview-label">Processed At</span>
                  <span className="overview-value">{new Date(selectedRequest.processedAt).toLocaleString()}</span>
                </div>
              )}
            </div>

            <h3 className="req-section-title">Specific Information</h3>
            <div className="req-info-container">
              {isHotel ? (
                <div className="req-info-grid-2col">
                  {/* Property Details */}
                  <div className="req-info-card">
                    <h4 className="req-info-card-title">Property Details</h4>
                    <div className="req-info-list">
                      <div className="info-row">
                        <span className="info-label">Hotel Name</span>
                        <span className="info-val strong">{hotelReq.name}</span>
                      </div>
                      <div className="info-row">
                        <span className="info-label">Address</span>
                        <span className="info-val">{hotelReq.address}, {hotelReq.wardName}, {hotelReq.provinceName}, {hotelReq.countryName}</span>
                      </div>
                      <div className="info-row">
                        <span className="info-label">Property Type</span>
                        <span className="info-val">{hotelReq.propertyTypeName}</span>
                      </div>
                      <div className="info-row">
                        <span className="info-label">Tax Code</span>
                        <span className="info-val">{hotelReq.taxCode}</span>
                      </div>
                      <div className="info-row">
                        <span className="info-label">License Document</span>
                        <span className="info-val">
                          <a href={hotelReq.businessLicenseUrl} target="_blank" rel="noreferrer" className="req-link-btn">
                            View Document
                          </a>
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Owner Contact */}
                  <div className="req-info-card">
                    <h4 className="req-info-card-title">Owner Contact</h4>
                    <div className="req-info-list">
                      <div className="info-row">
                        <span className="info-label">Full Name</span>
                        <span className="info-val strong">{hotelReq.ownerFullName}</span>
                      </div>
                      <div className="info-row">
                        <span className="info-label">User ID</span>
                        <span className="info-val">#{hotelReq.ownerId}</span>
                      </div>
                      <div className="info-row">
                        <span className="info-label">Email Address</span>
                        <span className="info-val highlight">{hotelReq.ownerEmail}</span>
                      </div>
                      <div className="info-row">
                        <span className="info-label">Phone Number</span>
                        <span className="info-val">{hotelReq.ownerPhoneNumber}</span>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="req-info-card full-width">
                  <h4 className="req-info-card-title">Customer Details</h4>
                  <div className="req-info-grid-3col">
                    <div className="info-row">
                      <span className="info-label">Full Name</span>
                      <span className="info-val strong">{upgradeReq.fullName}</span>
                    </div>
                    <div className="info-row">
                      <span className="info-label">Username</span>
                      <span className="info-val">@{upgradeReq.userName}</span>
                    </div>
                    <div className="info-row">
                      <span className="info-label">Email</span>
                      <span className="info-val highlight">{upgradeReq.email}</span>
                    </div>
                    <div className="info-row">
                      <span className="info-label">Phone</span>
                      <span className="info-val">{upgradeReq.phoneNumber}</span>
                    </div>
                    <div className="info-row">
                      <span className="info-label">Address</span>
                      <span className="info-val">{upgradeReq.address}</span>
                    </div>
                    <div className="info-row">
                      <span className="info-label">Tax Code</span>
                      <span className="info-val">{upgradeReq.taxCode}</span>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
          
          <div className="req-modal-footer">
            <button onClick={() => setSelectedRequest(null)} className="req-btn-cancel">
              Close
            </button>
            <div className="req-modal-actions">
              {selectedRequest.canReject && (
                <button onClick={() => handleReject(selectedRequest.requestId)} className="req-btn-reject">
                  Reject Request
                </button>
              )}
              {selectedRequest.canApprove && (
                <button onClick={() => handleApprove(selectedRequest.requestId)} className="req-btn-approve">
                  Approve Request
                </button>
              )}
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
