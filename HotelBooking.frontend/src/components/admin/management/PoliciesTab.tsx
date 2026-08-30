import { useState, useEffect } from "react";
import { 
  getPolicyTypes, getPolicies, createPolicy, updatePolicy, deletePolicy 
} from "../../../services/adminManagementService";
import type { PolicyType, PolicyItem } from "../../../services/adminManagementService";

function formatFee(fee?: number) {
  if (!fee || fee === 0) return "Free";
  return `${fee.toLocaleString("vi-VN")} VNĐ`;
}

export default function PoliciesTab() {
  const [policyTypes, setPolicyTypes] = useState<PolicyType[]>([]);
  const [items, setItems] = useState<PolicyItem[]>([]);
  
  // Filtering & Pagination
  const [selectedTypeId, setSelectedTypeId] = useState<number | "All">("All");
  const [pageIndex, setPageIndex] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // States
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<"Create" | "Edit">("Create");
  const [editingId, setEditingId] = useState<number | null>(null);
  
  // Form State
  const [formData, setFormData] = useState<any>({});
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState("");

  useEffect(() => {
    async function loadTypes() {
      const typeRes = await getPolicyTypes();
      if (typeRes.statusCode === "Success" && typeRes.content) {
        setPolicyTypes(typeRes.content);
      }
    }
    loadTypes();
  }, []);

  useEffect(() => {
    loadItems();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedTypeId, pageIndex]);

  async function loadItems() {
    setLoading(true);
    setError("");
    try {
      const typeFilter = selectedTypeId === "All" ? undefined : selectedTypeId;
      const res = await getPolicies(pageIndex, 10, typeFilter);
      if (res.statusCode === "Success" && res.content) {
        setItems(res.content.items || []);
        setTotalPages(res.content.totalPages || 1);
      } else {
        setError(res.message || "Failed to load policies.");
      }
    } catch (err) {
      setError("Network error while loading data.");
    } finally {
      setLoading(false);
    }
  }

  function openCreateModal() {
    setModalMode("Create");
    setEditingId(null);
    const initialTypeId = policyTypes.length > 0 ? policyTypes[0].id : 1002;
    setFormData({ 
      name: "", 
      description: "", 
      typeId: initialTypeId,
      // Init all possible fields just in case
      checkInTime: "14:00:00", checkOutTime: "12:00:00", earlyCheckInFee: 0, lateCheckOutFee: 0,
      daysBeforeCheckIn: 0, refundPercent: 0, isRefundable: false,
      minAge: 0, maxAge: 12, extraBedFee: 0,
      petFee: 0, isPetAllowed: false
    });
    setFormError("");
    setIsModalOpen(true);
  }

  function openEditModal(item: PolicyItem) {
    setModalMode("Edit");
    setEditingId(item.id);
    setFormData({ 
      name: item.name, 
      description: item.description || "", 
      typeId: item.typeId,
      checkInTime: item.checkInTime || "14:00:00",
      checkOutTime: item.checkOutTime || "12:00:00",
      earlyCheckInFee: item.earlyCheckInFee || 0,
      lateCheckOutFee: item.lateCheckOutFee || 0,
      daysBeforeCheckIn: item.daysBeforeCheckIn || 0,
      refundPercent: item.refundPercent || 0,
      isRefundable: item.isRefundable || false,
      minAge: item.minAge || 0,
      maxAge: item.maxAge || 12,
      extraBedFee: item.extraBedFee || 0,
      petFee: item.petFee || 0,
      isPetAllowed: item.isPetAllowed || false
    });
    setFormError("");
    setIsModalOpen(true);
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Are you sure you want to delete this policy?")) return;
    try {
      const res = await deletePolicy(id);
      if (res && res.statusCode === "Success") {
        if (items.length === 1 && pageIndex > 1) {
          setPageIndex(pageIndex - 1);
        } else {
          loadItems();
        }
      } else {
        alert("Delete failed: " + (res?.message || "Unknown error"));
      }
    } catch (e) {
      alert("Network error.");
    }
  }

  function getApiEndpointType(typeId: number): "check-in-out" | "cancellation" | "pet" | "children" {
    if (typeId === 1002) return "check-in-out";
    if (typeId === 1003) return "cancellation";
    if (typeId === 1004) return "children";
    if (typeId === 2002) return "pet";
    return "check-in-out"; // default fallback
  }

  function getDiscriminator(typeId: number): string {
    if (typeId === 1002) return "checkInOut";
    if (typeId === 1003) return "cancellation";
    if (typeId === 1004) return "children";
    if (typeId === 2002) return "pets";
    return "checkInOut";
  }

  async function handleFormSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!formData.name.trim()) {
      setFormError("Name is required.");
      return;
    }
    setFormLoading(true);
    setFormError("");

    const endpointType = getApiEndpointType(formData.typeId);
    
    // Prepare payload cleanly based on type
    let payload: any = { 
      name: formData.name, 
      description: formData.description,
      discriminator: getDiscriminator(formData.typeId)
    };
    
    if (modalMode === "Create") {
      payload.typeId = formData.typeId;
    }

    if (endpointType === "check-in-out") {
      payload = { ...payload, checkInTime: formData.checkInTime, checkOutTime: formData.checkOutTime, earlyCheckInFee: formData.earlyCheckInFee, lateCheckOutFee: formData.lateCheckOutFee };
    } else if (endpointType === "cancellation") {
      payload = { ...payload, daysBeforeCheckIn: formData.daysBeforeCheckIn, refundPercent: formData.refundPercent, isRefundable: formData.isRefundable };
    } else if (endpointType === "children") {
      payload = { ...payload, minAge: formData.minAge, maxAge: formData.maxAge, extraBedFee: formData.extraBedFee };
    } else if (endpointType === "pet") {
      payload = { ...payload, petFee: formData.petFee, isPetAllowed: formData.isPetAllowed };
    }

    try {
      let res;
      if (modalMode === "Create") {
        res = await createPolicy(endpointType, payload);
      } else {
        res = await updatePolicy(editingId!, endpointType, payload);
      }

      if (res && res.statusCode === "Success") {
        setIsModalOpen(false);
        loadItems();
      } else {
        setFormError(res?.message || "Operation failed.");
      }
    } catch (err) {
      setFormError("Network error.");
    } finally {
      setFormLoading(false);
    }
  }

  function getTypeName(typeId: number) {
    const t = policyTypes.find(x => x.id === typeId);
    return t ? t.name : `Type ${typeId}`;
  }

  // Render specific details based on discriminator
  function renderPolicyDetails(item: PolicyItem) {
    switch(item.discriminator) {
      case "checkInOut":
        return (
          <div style={{ fontSize: "12px", color: "#64748B" }}>
            <div>In: <strong>{item.checkInTime}</strong> | Out: <strong>{item.checkOutTime}</strong></div>
            <div>Early Fee: <strong>{formatFee(item.earlyCheckInFee)}</strong> | Late Fee: <strong>{formatFee(item.lateCheckOutFee)}</strong></div>
          </div>
        );
      case "cancellation":
        return (
          <div style={{ fontSize: "12px", color: "#64748B" }}>
            <div>Refundable: <strong>{item.isRefundable ? "Yes" : "No"}</strong></div>
            {item.isRefundable && <div>{item.refundPercent}% refund if {item.daysBeforeCheckIn} days before.</div>}
          </div>
        );
      case "children":
        return (
          <div style={{ fontSize: "12px", color: "#64748B" }}>
            <div>Age: <strong>{item.minAge} - {item.maxAge}</strong></div>
            <div>Extra Bed Fee: <strong>{formatFee(item.extraBedFee)}</strong></div>
          </div>
        );
      case "pets":
        return (
          <div style={{ fontSize: "12px", color: "#64748B" }}>
            <div>Allowed: <strong>{item.isPetAllowed ? "Yes" : "No"}</strong></div>
            {item.isPetAllowed && <div>Fee: <strong>{formatFee(item.petFee)}</strong></div>}
          </div>
        );
      default:
        return <em>No details</em>;
    }
  }

  // Handle dynamic fields rendering for the form
  function renderDynamicFormFields() {
    const type = getApiEndpointType(formData.typeId);
    
    if (type === "check-in-out") {
      return (
        <div className="profile-grid" style={{ gap: "12px" }}>
          <div className="form-group">
            <label>Check-In Time</label>
            <input type="time" step="1" value={formData.checkInTime} onChange={e => setFormData({...formData, checkInTime: e.target.value})} disabled={formLoading} />
          </div>
          <div className="form-group">
            <label>Check-Out Time</label>
            <input type="time" step="1" value={formData.checkOutTime} onChange={e => setFormData({...formData, checkOutTime: e.target.value})} disabled={formLoading} />
          </div>
          <div className="form-group">
            <label>Early Check-In Fee (VNĐ)</label>
            <input type="number" value={formData.earlyCheckInFee} onChange={e => setFormData({...formData, earlyCheckInFee: parseFloat(e.target.value)})} disabled={formLoading} />
          </div>
          <div className="form-group">
            <label>Late Check-Out Fee (VNĐ)</label>
            <input type="number" value={formData.lateCheckOutFee} onChange={e => setFormData({...formData, lateCheckOutFee: parseFloat(e.target.value)})} disabled={formLoading} />
          </div>
        </div>
      );
    }
    
    if (type === "cancellation") {
      return (
        <div className="profile-grid" style={{ gap: "12px" }}>
          <div className="form-group" style={{ gridColumn: "1 / -1" }}>
            <label>Is Refundable?</label>
            <select value={formData.isRefundable ? "true" : "false"} onChange={e => setFormData({...formData, isRefundable: e.target.value === "true"})} disabled={formLoading}>
              <option value="false">No</option>
              <option value="true">Yes</option>
            </select>
          </div>
          {formData.isRefundable && (
            <>
              <div className="form-group">
                <label>Days Before Check-In</label>
                <input type="number" value={formData.daysBeforeCheckIn} onChange={e => setFormData({...formData, daysBeforeCheckIn: parseInt(e.target.value)})} disabled={formLoading} />
              </div>
              <div className="form-group">
                <label>Refund Percent (%)</label>
                <input type="number" max="100" value={formData.refundPercent} onChange={e => setFormData({...formData, refundPercent: parseFloat(e.target.value)})} disabled={formLoading} />
              </div>
            </>
          )}
        </div>
      );
    }
    
    if (type === "children") {
      return (
        <div className="profile-grid" style={{ gap: "12px" }}>
          <div className="form-group">
            <label>Min Age</label>
            <input type="number" value={formData.minAge} onChange={e => setFormData({...formData, minAge: parseInt(e.target.value)})} disabled={formLoading} />
          </div>
          <div className="form-group">
            <label>Max Age</label>
            <input type="number" value={formData.maxAge} onChange={e => setFormData({...formData, maxAge: parseInt(e.target.value)})} disabled={formLoading} />
          </div>
          <div className="form-group">
            <label>Extra Bed Fee (VNĐ)</label>
            <input type="number" value={formData.extraBedFee} onChange={e => setFormData({...formData, extraBedFee: parseFloat(e.target.value)})} disabled={formLoading} />
          </div>
        </div>
      );
    }
    
    if (type === "pet") {
      return (
        <div className="profile-grid" style={{ gap: "12px" }}>
          <div className="form-group" style={{ gridColumn: "1 / -1" }}>
            <label>Is Pet Allowed?</label>
            <select value={formData.isPetAllowed ? "true" : "false"} onChange={e => setFormData({...formData, isPetAllowed: e.target.value === "true"})} disabled={formLoading}>
              <option value="false">No</option>
              <option value="true">Yes</option>
            </select>
          </div>
          {formData.isPetAllowed && (
            <div className="form-group">
              <label>Pet Fee (VNĐ)</label>
              <input type="number" value={formData.petFee} onChange={e => setFormData({...formData, petFee: parseFloat(e.target.value)})} disabled={formLoading} />
            </div>
          )}
        </div>
      );
    }

    return null;
  }

  return (
    <div className="amenities-tab-container">
      {/* TOOLBAR */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          <label style={{ fontSize: "14px", fontWeight: 600, color: "#475569" }}>Filter by Type:</label>
          <select 
            value={selectedTypeId} 
            onChange={(e) => {
              setSelectedTypeId(e.target.value === "All" ? "All" : parseInt(e.target.value));
              setPageIndex(1);
            }}
            style={{ padding: "8px 12px", borderRadius: "6px", border: "1px solid #CBD5E1", outline: "none" }}
          >
            <option value="All">All Types</option>
            {policyTypes.map(t => (
              <option key={t.id} value={t.id}>{t.name}</option>
            ))}
          </select>
        </div>
        <button className="btn-create" onClick={openCreateModal}>+ Add Policy</button>
      </div>

      {error && <div className="error-alert">{error}</div>}
      
      {loading ? (
        <div className="loading-state">Loading policies...</div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="manage-table">
              <thead>
                <tr>
                  <th style={{ width: "80px" }}>ID</th>
                  <th style={{ width: "20%" }}>Name</th>
                  <th style={{ width: "15%" }}>Type</th>
                  <th>Configuration Details</th>
                  <th style={{ width: "120px", textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="empty-state">No policies found.</td>
                  </tr>
                ) : (
                  items.map(item => (
                    <tr key={item.id}>
                      <td>{item.id}</td>
                      <td className="fw-bold">{item.name}</td>
                      <td>
                        <span className="type-badge">{getTypeName(item.typeId)}</span>
                      </td>
                      <td>{renderPolicyDetails(item)}</td>
                      <td className="actions-cell">
                        <button className="btn-icon edit" onClick={() => openEditModal(item)} title="Edit">✎</button>
                        <button className="btn-icon delete" onClick={() => handleDelete(item.id)} title="Delete">🗑</button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {/* PAGINATION */}
          {totalPages > 1 && (
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: "20px" }}>
              <span style={{ fontSize: "14px", color: "#64748B" }}>
                Page {pageIndex} of {totalPages}
              </span>
              <div style={{ display: "flex", gap: "8px" }}>
                <button 
                  onClick={() => setPageIndex(p => Math.max(1, p - 1))} 
                  disabled={pageIndex === 1}
                  style={{ padding: "6px 12px", borderRadius: "6px", border: "1px solid #CBD5E1", background: pageIndex === 1 ? "#F1F5F9" : "#fff", cursor: pageIndex === 1 ? "not-allowed" : "pointer" }}
                >
                  Previous
                </button>
                <button 
                  onClick={() => setPageIndex(p => Math.min(totalPages, p + 1))} 
                  disabled={pageIndex === totalPages}
                  style={{ padding: "6px 12px", borderRadius: "6px", border: "1px solid #CBD5E1", background: pageIndex === totalPages ? "#F1F5F9" : "#fff", cursor: pageIndex === totalPages ? "not-allowed" : "pointer" }}
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </>
      )}

      {/* CUSTOM MODAL */}
      {isModalOpen && (
        <div className="manage-modal-overlay">
          <div className="manage-modal" style={{ maxWidth: "600px" }}>
            <div className="modal-header">
              <h3>{modalMode} Policy</h3>
              <button type="button" className="close-btn" onClick={() => setIsModalOpen(false)}>Ã—</button>
            </div>
            <form onSubmit={handleFormSubmit}>
              <div className="modal-body" style={{ maxHeight: "65vh", overflowY: "auto" }}>
                {formError && <div className="error-alert">{formError}</div>}
                
                <div className="form-group">
                  <label>Name <span className="req">*</span></label>
                  <input 
                    type="text" 
                    value={formData.name} 
                    onChange={e => setFormData({...formData, name: e.target.value})} 
                    disabled={formLoading}
                    placeholder="E.g., Standard Cancellation"
                  />
                </div>

                {modalMode === "Create" && (
                  <div className="form-group">
                    <label>Policy Type</label>
                    <select 
                      value={formData.typeId} 
                      onChange={e => setFormData({...formData, typeId: parseInt(e.target.value)})}
                      disabled={formLoading}
                    >
                      {policyTypes.map(t => (
                        <option key={t.id} value={t.id}>{t.name}</option>
                      ))}
                    </select>
                  </div>
                )}
                {modalMode === "Edit" && (
                  <div className="form-group">
                    <label>Policy Type (Read-only)</label>
                    <input 
                      type="text" 
                      value={getTypeName(formData.typeId)} 
                      disabled
                      style={{ backgroundColor: "#F1F5F9", color: "#64748B" }}
                    />
                  </div>
                )}

                <div className="form-group">
                  <label>Description</label>
                  <textarea 
                    value={formData.description} 
                    onChange={e => setFormData({...formData, description: e.target.value})}
                    disabled={formLoading}
                    rows={2}
                    placeholder="Optional description..."
                  />
                </div>

                <div style={{ marginTop: "24px", marginBottom: "16px", borderBottom: "1px solid #E2E8F0", paddingBottom: "8px" }}>
                  <strong style={{ color: "#0F172A", fontSize: "14px" }}>Configuration Details</strong>
                </div>

                {renderDynamicFormFields()}

              </div>
              <div className="modal-footer">
                <button type="button" className="btn-secondary" onClick={() => setIsModalOpen(false)} disabled={formLoading}>Cancel</button>
                <button type="submit" className="btn-primary" disabled={formLoading}>
                  {formLoading ? "Saving..." : "Save"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}





