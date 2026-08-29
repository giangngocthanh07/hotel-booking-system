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

// --- MUI Imports ---
import { 
  Dialog, DialogTitle, DialogContent, DialogActions, 
  Button, Typography, Box, Grid, Divider, Chip, IconButton, Paper
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import BusinessIcon from "@mui/icons-material/Business";
import PersonIcon from "@mui/icons-material/Person";
import AssignmentIndIcon from "@mui/icons-material/AssignmentInd";

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
  
  function getMuiStatusColor(status: string): "default" | "success" | "error" | "warning" {
    if (status === "Approved") return "success";
    if (status === "Rejected") return "error";
    if (status === "Pending") return "warning";
    return "default";
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

  // --- MUI Dialog Modal ---
  function renderMuiModal() {
    if (!selectedRequest) return null;

    const isHotel = requestType === "Hotel";
    const hotelReq = selectedRequest as HotelApprovalRequest;
    const upgradeReq = selectedRequest as UpgradeRequest;

    return (
      <Dialog 
        open={!!selectedRequest} 
        onClose={() => setSelectedRequest(null)}
        maxWidth="md"
        fullWidth
        PaperProps={{
          sx: { borderRadius: 3, boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.25)' }
        }}
      >
        <DialogTitle sx={{ 
          m: 0, p: 3, pb: 2, 
          background: 'linear-gradient(135deg, rgba(59,130,246,0.05), rgba(236,72,153,0.05))',
          borderBottom: '1px solid #F1F5F9'
        }}>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Typography variant="h5" fontWeight="800" sx={{ 
              background: 'linear-gradient(135deg, #3B82F6, #EC4899)', 
              WebkitBackgroundClip: 'text', 
              WebkitTextFillColor: 'transparent' 
            }}>
              {isHotel ? "🏨 Hotel Registration Approval" : "👤 Owner Upgrade Request"}
            </Typography>
            <IconButton onClick={() => setSelectedRequest(null)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main', bgcolor: 'error.lighter' } }}>
              <CloseIcon />
            </IconButton>
          </Box>
        </DialogTitle>

        <DialogContent sx={{ p: 0 }}>
          {/* Overview Section */}
          <Box sx={{ p: 3, bgcolor: '#F8FAFC' }}>
            <Grid container spacing={3}>
              <Grid item xs={6} sm={3}>
                <Typography variant="overline" color="text.secondary" fontWeight="700">Request ID</Typography>
                <Typography variant="h6" fontWeight="700" color="text.primary">#{selectedRequest.requestId}</Typography>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Typography variant="overline" color="text.secondary" fontWeight="700">Status</Typography>
                <Box mt={0.5}>
                  <Chip 
                    label={selectedRequest.status} 
                    color={getMuiStatusColor(selectedRequest.status)} 
                    size="small" 
                    sx={{ fontWeight: 'bold', px: 1 }} 
                  />
                </Box>
              </Grid>
              <Grid item xs={6} sm={3}>
                <Typography variant="overline" color="text.secondary" fontWeight="700">Requested At</Typography>
                <Typography variant="body1" fontWeight="500">{new Date(selectedRequest.requestedAt).toLocaleString()}</Typography>
              </Grid>
              {selectedRequest.processedAt && (
                <Grid item xs={6} sm={3}>
                  <Typography variant="overline" color="text.secondary" fontWeight="700">Processed At</Typography>
                  <Typography variant="body1" fontWeight="500">{new Date(selectedRequest.processedAt).toLocaleString()}</Typography>
                </Grid>
              )}
            </Grid>
          </Box>

          <Divider />

          {/* Details Section */}
          <Box sx={{ p: 3 }}>
            {isHotel ? (
              <Grid container spacing={4}>
                <Grid item xs={12} md={6}>
                  <Box display="flex" alignItems="center" mb={2}>
                    <BusinessIcon sx={{ color: '#3B82F6', mr: 1 }} />
                    <Typography variant="h6" fontWeight="700">Property Details</Typography>
                  </Box>
                  <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, bgcolor: '#fff' }}>
                    <Grid container spacing={2}>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Hotel Name</Typography>
                        <Typography variant="body1" fontWeight="600">{hotelReq.name}</Typography>
                      </Grid>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Address</Typography>
                        <Typography variant="body2">{hotelReq.address}, {hotelReq.wardName}, {hotelReq.provinceName}, {hotelReq.countryName}</Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="caption" color="text.secondary" display="block">Type</Typography>
                        <Typography variant="body2">{hotelReq.propertyTypeName}</Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="caption" color="text.secondary" display="block">Tax Code</Typography>
                        <Typography variant="body2" fontWeight="500">{hotelReq.taxCode}</Typography>
                      </Grid>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Business License</Typography>
                        <Button 
                          variant="outlined" 
                          size="small" 
                          href={hotelReq.businessLicenseUrl} 
                          target="_blank" 
                          sx={{ mt: 0.5, borderColor: '#EC4899', color: '#EC4899', '&:hover': { borderColor: '#DB2777', bgcolor: 'rgba(236,72,153,0.04)' } }}
                        >
                          View Document
                        </Button>
                      </Grid>
                    </Grid>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Box display="flex" alignItems="center" mb={2}>
                    <PersonIcon sx={{ color: '#F59E0B', mr: 1 }} />
                    <Typography variant="h6" fontWeight="700">Owner Contact</Typography>
                  </Box>
                  <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, bgcolor: '#fff' }}>
                    <Grid container spacing={2}>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Full Name</Typography>
                        <Typography variant="body1" fontWeight="600">{hotelReq.ownerFullName}</Typography>
                      </Grid>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">System User ID</Typography>
                        <Typography variant="body2">#{hotelReq.ownerId}</Typography>
                      </Grid>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Email Address</Typography>
                        <Typography variant="body2" color="primary.main">{hotelReq.ownerEmail}</Typography>
                      </Grid>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Phone Number</Typography>
                        <Typography variant="body2">{hotelReq.ownerPhoneNumber}</Typography>
                      </Grid>
                    </Grid>
                  </Paper>
                </Grid>
              </Grid>
            ) : (
              <Grid container spacing={4} justifyContent="center">
                <Grid item xs={12} md={8}>
                  <Box display="flex" alignItems="center" mb={2}>
                    <AssignmentIndIcon sx={{ color: '#8B5CF6', mr: 1 }} />
                    <Typography variant="h6" fontWeight="700">Customer Details</Typography>
                  </Box>
                  <Paper variant="outlined" sx={{ p: 3, borderRadius: 2, bgcolor: '#fff' }}>
                    <Grid container spacing={3}>
                      <Grid item xs={12} sm={6}>
                        <Typography variant="caption" color="text.secondary" display="block">Full Name</Typography>
                        <Typography variant="body1" fontWeight="600">{upgradeReq.fullName}</Typography>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Typography variant="caption" color="text.secondary" display="block">Username</Typography>
                        <Typography variant="body2">@{upgradeReq.userName}</Typography>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Typography variant="caption" color="text.secondary" display="block">Email</Typography>
                        <Typography variant="body2" color="primary.main">{upgradeReq.email}</Typography>
                      </Grid>
                      <Grid item xs={12} sm={6}>
                        <Typography variant="caption" color="text.secondary" display="block">Phone</Typography>
                        <Typography variant="body2">{upgradeReq.phoneNumber}</Typography>
                      </Grid>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Address</Typography>
                        <Typography variant="body2">{upgradeReq.address}</Typography>
                      </Grid>
                      <Grid item xs={12}>
                        <Typography variant="caption" color="text.secondary" display="block">Tax Code</Typography>
                        <Typography variant="body2" fontWeight="500">{upgradeReq.taxCode}</Typography>
                      </Grid>
                    </Grid>
                  </Paper>
                </Grid>
              </Grid>
            )}
          </Box>
        </DialogContent>

        <DialogActions sx={{ p: 3, pt: 2, borderTop: '1px solid #F1F5F9', bgcolor: '#F8FAFC' }}>
          <Box display="flex" justifyContent="space-between" width="100%">
            <Button 
              variant="text" 
              color="inherit" 
              onClick={() => setSelectedRequest(null)}
              sx={{ fontWeight: 600, color: 'text.secondary' }}
            >
              Cancel
            </Button>
            <Box display="flex" gap={2}>
              {selectedRequest.canReject && (
                <Button 
                  variant="outlined" 
                  color="error"
                  startIcon={<CancelIcon />}
                  onClick={() => handleReject(selectedRequest.requestId)}
                  sx={{ borderRadius: 2, fontWeight: 600 }}
                >
                  Reject Request
                </Button>
              )}
              {selectedRequest.canApprove && (
                <Button 
                  variant="contained" 
                  color="success"
                  startIcon={<CheckCircleIcon />}
                  onClick={() => handleApprove(selectedRequest.requestId)}
                  sx={{ borderRadius: 2, fontWeight: 600, boxShadow: '0 4px 14px rgba(16, 185, 129, 0.4)' }}
                >
                  Approve Request
                </Button>
              )}
            </Box>
          </Box>
        </DialogActions>
      </Dialog>
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

      {/* Render the new MUI Modal */}
      {renderMuiModal()}
    </div>
  );
}

export default AdminRequestsPage;
