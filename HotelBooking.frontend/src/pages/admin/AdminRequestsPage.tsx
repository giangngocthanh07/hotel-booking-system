import { useEffect, useState } from "react";
import { 
  getHotelApprovals, 
  approveHotel, 
  rejectHotel,
  getUpgradeRequests,
  approveUpgrade,
  rejectUpgrade 
} from "../../services/adminService";
import type { HotelApprovalRequest, UpgradeRequest, BaseRequest } from "../../types/admin.types";

type RequestType = "Hotel" | "Upgrade";

function AdminRequestsPage() {
  const [requestType, setRequestType] = useState<RequestType>("Hotel");
  const [statusFilter, setStatusFilter] = useState("Pending");
  
  // Pagination State
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Data State
  const [requests, setRequests] = useState<(HotelApprovalRequest | UpgradeRequest)[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Modal State
  const [selectedRequest, setSelectedRequest] = useState<HotelApprovalRequest | UpgradeRequest | null>(null);

  // Fetch Data
  async function fetchRequests() {
    setLoading(true);
    setError("");
    try {
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

  // Refetch when filters or pagination change
  useEffect(() => {
    fetchRequests();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [requestType, statusFilter, pageIndex, pageSize]);

  // Reset page number when changing type or status filter
  function handleTypeChange(newType: RequestType) {
    setRequestType(newType);
    setPageIndex(1);
  }

  function handleStatusChange(newStatus: string) {
    setStatusFilter(newStatus);
    setPageIndex(1);
  }

  // Actions
  async function handleApprove(id: number) {
    if (!window.confirm("Are you sure you want to approve this request?")) return;
    try {
      const res = requestType === "Hotel" ? await approveHotel(id) : await approveUpgrade(id);
      if (res.statusCode === "Success") {
        alert("Request approved successfully!");
        if (selectedRequest?.requestId === id) setSelectedRequest(null);
        fetchRequests();
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
        fetchRequests();
      } else {
        alert("Failed: " + res.message);
      }
    } catch (e) {
      alert("Error rejecting request.");
    }
  }

  // Render Helpers
  function renderHotelRow(req: HotelApprovalRequest) {
    return (
      <tr key={req.requestId} style={{ borderBottom: "1px solid #F1F5F9", cursor: "pointer" }} onClick={() => setSelectedRequest(req)}>
        <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A" }}>#{req.requestId}</td>
        <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A", fontWeight: 500 }}>
          {req.name}
          <div style={{ fontSize: "12px", color: "#64748B", fontWeight: 400, marginTop: "4px" }}>{req.address}</div>
        </td>
        <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A" }}>
          {req.ownerFullName}
          <div style={{ fontSize: "12px", color: "#64748B", marginTop: "4px" }}>{req.ownerEmail}</div>
        </td>
        <td style={{ padding: "16px", fontSize: "14px", color: "#0F172A" }}>{new Date(req.requestedAt).toLocaleDateString()}</td>
        <td style={{ padding: "16px", fontSize: "14px" }}>{renderStatusBadge(req.status)}</td>
        <td style={{ padding: "16px", textAlign: "right" }} onClick={(e) => e.stopPropagation()}>{renderActionButtons(req)}</td>
      </tr>
    );
  }

  function renderUpgradeRow(req: UpgradeRequest) {
    return (
      <tr key={req.requestId} style={{ borderBottom: "1px solid #F1F5F9", cursor: "pointer" }} onClick={() => setSelectedRequest(req)}>
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
        <td style={{ padding: "16px", fontSize: "14px" }}>{renderStatusBadge(req.status)}</td>
        <td style={{ padding: "16px", textAlign: "right" }} onClick={(e) => e.stopPropagation()}>{renderActionButtons(req)}</td>
      </tr>
    );
  }

  function renderStatusBadge(status: string) {
    return (
      <span style={{ 
        padding: "4px 8px", 
        borderRadius: "9999px", 
        fontSize: "12px", 
        fontWeight: 600,
        backgroundColor: status === "Approved" ? "#D1FAE5" : status === "Rejected" ? "#FEE2E2" : "#FEF3C7",
        color: status === "Approved" ? "#065F46" : status === "Rejected" ? "#991B1B" : "#92400E"
      }}>
        {status}
      </span>
    );
  }

  function renderActionButtons(req: BaseRequest) {
    return (
      <>
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
      </>
    );
  }

  function renderModal() {
    if (!selectedRequest) return null;

    return (
      <div style={{ position: "fixed", top: 0, left: 0, right: 0, bottom: 0, backgroundColor: "rgba(0,0,0,0.5)", display: "flex", justifyContent: "center", alignItems: "center", zIndex: 1000 }}>
        <div style={{ backgroundColor: "#fff", borderRadius: "12px", width: "100%", maxWidth: "600px", maxHeight: "90vh", overflowY: "auto", padding: "24px", position: "relative" }}>
          <button onClick={() => setSelectedRequest(null)} style={{ position: "absolute", top: "16px", right: "16px", background: "none", border: "none", fontSize: "20px", cursor: "pointer", color: "#64748B" }}>✕</button>
          
          <h2 style={{ fontSize: "20px", color: "#0F172A", marginBottom: "16px", paddingBottom: "12px", borderBottom: "1px solid #E2E8F0" }}>
            {requestType === "Hotel" ? "Hotel Approval Details" : "Upgrade Request Details"}
          </h2>
          
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px", marginBottom: "24px" }}>
            <div>
              <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 600, marginBottom: "4px" }}>Request ID</div>
              <div style={{ fontSize: "15px", color: "#0F172A" }}>#{selectedRequest.requestId}</div>
            </div>
            <div>
              <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 600, marginBottom: "4px" }}>Status</div>
              <div>{renderStatusBadge(selectedRequest.status)}</div>
            </div>
            <div>
              <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 600, marginBottom: "4px" }}>Requested At</div>
              <div style={{ fontSize: "15px", color: "#0F172A" }}>{new Date(selectedRequest.requestedAt).toLocaleString()}</div>
            </div>
            {selectedRequest.processedAt && (
              <div>
                <div style={{ fontSize: "12px", color: "#64748B", textTransform: "uppercase", fontWeight: 600, marginBottom: "4px" }}>Processed At</div>
                <div style={{ fontSize: "15px", color: "#0F172A" }}>{new Date(selectedRequest.processedAt).toLocaleString()}</div>
              </div>
            )}
          </div>

          <h3 style={{ fontSize: "16px", color: "#0F172A", marginBottom: "12px" }}>Specific Information</h3>
          <div style={{ backgroundColor: "#F8FAFC", padding: "16px", borderRadius: "8px", border: "1px solid #E2E8F0" }}>
            {requestType === "Hotel" ? (
              <div style={{ display: "grid", gap: "12px" }}>
                <div><strong>Hotel Name:</strong> {(selectedRequest as HotelApprovalRequest).name}</div>
                <div><strong>Address:</strong> {(selectedRequest as HotelApprovalRequest).address}</div>
                <div><strong>Location:</strong> {(selectedRequest as HotelApprovalRequest).wardName}, {(selectedRequest as HotelApprovalRequest).provinceName}, {(selectedRequest as HotelApprovalRequest).countryName}</div>
                <div><strong>Type:</strong> {(selectedRequest as HotelApprovalRequest).propertyTypeName}</div>
                <div><strong>Tax Code:</strong> {(selectedRequest as HotelApprovalRequest).taxCode}</div>
                <div><strong>Business License:</strong> <a href={(selectedRequest as HotelApprovalRequest).businessLicenseUrl} target="_blank" rel="noreferrer" style={{color:"#3B82F6"}}>View Document</a></div>
                <hr style={{ borderColor: "#E2E8F0", margin: "12px 0" }} />
                <div><strong>Owner:</strong> {(selectedRequest as HotelApprovalRequest).ownerFullName} (ID: {(selectedRequest as HotelApprovalRequest).ownerId})</div>
                <div><strong>Owner Email:</strong> {(selectedRequest as HotelApprovalRequest).ownerEmail}</div>
                <div><strong>Owner Phone:</strong> {(selectedRequest as HotelApprovalRequest).ownerPhoneNumber}</div>
              </div>
            ) : (
              <div style={{ display: "grid", gap: "12px" }}>
                <div><strong>Customer:</strong> {(selectedRequest as UpgradeRequest).fullName} (@{(selectedRequest as UpgradeRequest).userName})</div>
                <div><strong>User ID:</strong> {(selectedRequest as UpgradeRequest).userId}</div>
                <div><strong>Email:</strong> {(selectedRequest as UpgradeRequest).email}</div>
                <div><strong>Phone:</strong> {(selectedRequest as UpgradeRequest).phoneNumber}</div>
                <div><strong>Address:</strong> {(selectedRequest as UpgradeRequest).address}</div>
                <div><strong>Tax Code:</strong> {(selectedRequest as UpgradeRequest).taxCode}</div>
              </div>
            )}
          </div>

          <div style={{ marginTop: "24px", display: "flex", justifyContent: "flex-end", gap: "12px" }}>
            {renderActionButtons(selectedRequest)}
            <button onClick={() => setSelectedRequest(null)} style={{ padding: "6px 16px", backgroundColor: "#E2E8F0", color: "#475569", border: "none", borderRadius: "6px", cursor: "pointer", fontSize: "14px", fontWeight: 600 }}>
              Close
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="admin-page">
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "16px" }}>
        <div>
          <h1 style={{ fontSize: "24px", color: "#0F172A", margin: "0 0 16px 0" }}>Requests Management</h1>
          <div style={{ display: "flex", gap: "12px" }}>
            <button 
              onClick={() => handleTypeChange("Hotel")}
              style={{ 
                padding: "8px 16px", borderRadius: "8px", fontWeight: 600, cursor: "pointer", border: "none",
                backgroundColor: requestType === "Hotel" ? "#3B82F6" : "#E2E8F0",
                color: requestType === "Hotel" ? "white" : "#475569"
              }}
            >
              🏨 Hotel Approvals
            </button>
            <button 
              onClick={() => handleTypeChange("Upgrade")}
              style={{ 
                padding: "8px 16px", borderRadius: "8px", fontWeight: 600, cursor: "pointer", border: "none",
                backgroundColor: requestType === "Upgrade" ? "#3B82F6" : "#E2E8F0",
                color: requestType === "Upgrade" ? "white" : "#475569"
              }}
            >
              👤 Owner Upgrades
            </button>
          </div>
        </div>

        <div style={{ display: "flex", gap: "12px" }}>
          <select 
            value={statusFilter} 
            onChange={(e) => handleStatusChange(e.target.value)}
            style={{ padding: "8px 12px", borderRadius: "8px", border: "1px solid #CBD5E1", fontSize: "14px" }}
          >
            <option value="All">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
          <select 
            value={pageSize} 
            onChange={(e) => { setPageSize(Number(e.target.value)); setPageIndex(1); }}
            style={{ padding: "8px 12px", borderRadius: "8px", border: "1px solid #CBD5E1", fontSize: "14px" }}
          >
            <option value={10}>10 per page</option>
            <option value={20}>20 per page</option>
            <option value={50}>50 per page</option>
          </select>
        </div>
      </div>

      {error && <div style={{ padding: "12px", backgroundColor: "#FEE2E2", color: "#B91C1C", borderRadius: "8px", marginBottom: "16px" }}>{error}</div>}

      {loading ? (
        <div style={{ padding: "40px", textAlign: "center", color: "#64748B" }}>Loading requests...</div>
      ) : requests.length === 0 ? (
        <div style={{ padding: "40px", textAlign: "center", backgroundColor: "#fff", borderRadius: "12px", border: "1px solid #E2E8F0", color: "#64748B" }}>
          No {requestType.toLowerCase()} requests found.
        </div>
      ) : (
        <div style={{ backgroundColor: "#fff", borderRadius: "12px", border: "1px solid #E2E8F0", overflow: "hidden", display: "flex", flexDirection: "column" }}>
          <div style={{ overflowX: "auto" }}>
            <table style={{ width: "100%", borderCollapse: "collapse", textAlign: "left" }}>
              <thead style={{ backgroundColor: "#F8FAFC", borderBottom: "1px solid #E2E8F0" }}>
                <tr>
                  <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>ID</th>
                  {requestType === "Hotel" ? (
                    <>
                      <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Hotel Name</th>
                      <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Owner</th>
                      <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Date</th>
                    </>
                  ) : (
                    <>
                      <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Customer</th>
                      <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Contact</th>
                      <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Tax Code</th>
                    </>
                  )}
                  <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase" }}>Status</th>
                  <th style={{ padding: "16px", color: "#64748B", fontWeight: 600, fontSize: "13px", textTransform: "uppercase", textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {requestType === "Hotel" 
                  ? (requests as HotelApprovalRequest[]).map(renderHotelRow)
                  : (requests as UpgradeRequest[]).map(renderUpgradeRow)
                }
              </tbody>
            </table>
          </div>
          
          {/* Pagination Controls */}
          <div style={{ padding: "16px", borderTop: "1px solid #E2E8F0", display: "flex", justifyContent: "space-between", alignItems: "center", backgroundColor: "#F8FAFC" }}>
            <div style={{ fontSize: "14px", color: "#64748B" }}>
              Showing {requests.length} of {totalCount} results
            </div>
            <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
              <button 
                disabled={pageIndex === 1}
                onClick={() => setPageIndex(p => p - 1)}
                style={{ padding: "6px 12px", border: "1px solid #CBD5E1", borderRadius: "6px", backgroundColor: pageIndex === 1 ? "#F1F5F9" : "#fff", cursor: pageIndex === 1 ? "not-allowed" : "pointer", color: "#0F172A" }}
              >
                Previous
              </button>
              <span style={{ fontSize: "14px", color: "#0F172A", margin: "0 8px" }}>Page {pageIndex} of {totalPages}</span>
              <button 
                disabled={pageIndex >= totalPages}
                onClick={() => setPageIndex(p => p + 1)}
                style={{ padding: "6px 12px", border: "1px solid #CBD5E1", borderRadius: "6px", backgroundColor: pageIndex >= totalPages ? "#F1F5F9" : "#fff", cursor: pageIndex >= totalPages ? "not-allowed" : "pointer", color: "#0F172A" }}
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
