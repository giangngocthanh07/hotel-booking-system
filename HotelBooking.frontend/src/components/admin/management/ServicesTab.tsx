import { useState, useEffect } from "react";
import { 
  getServiceTypes, getServices, createService, updateService, deleteService 
} from "../../../services/adminManagementService";
import type { ServiceType, ServiceItem } from "../../../services/adminManagementService";

function formatFee(fee?: number) {
  if (fee === undefined || fee === null || fee === 0) return "Free";
  return `${fee.toLocaleString("vi-VN")} VNĐ`;
}

export default function ServicesTab() {
  const [serviceTypes, setServiceTypes] = useState<ServiceType[]>([]);
  const [items, setItems] = useState<ServiceItem[]>([]);
  
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
      const typeRes = await getServiceTypes();
      if (typeRes.statusCode === "Success" && typeRes.content) {
        setServiceTypes(typeRes.content);
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
      const res = await getServices(pageIndex, 10, typeFilter);
      if (res.statusCode === "Success" && res.content) {
        setItems(res.content.items || []);
        setTotalPages(res.content.totalPages || 1);
      } else {
        setError(res.message || "Failed to load services.");
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
    const initialTypeId = serviceTypes.length > 0 ? serviceTypes[0].id : 1;
    setFormData({ 
      name: "", 
      description: "", 
      typeId: initialTypeId,
      price: 0,
      
      // Standard
      unit: "Person",
      
      // Airport Transfer
      isOneWayPaid: false,
      hasRoundTrip: false,
      isRoundTripPaid: false,
      roundTripPrice: 0,
      hasNightFee: false,
      additionalFee: 0,
      additionalFeeStartTime: "22:00:00",
      additionalFeeEndTime: "06:00:00",
      maxPassengers: 4,
      maxLuggage: 2
    });
    setFormError("");
    setIsModalOpen(true);
  }

  function openEditModal(item: ServiceItem) {
    setModalMode("Edit");
    setEditingId(item.id);
    setFormData({ 
      name: item.name, 
      description: item.description || "", 
      typeId: item.typeId,
      price: item.price || 0,
      
      unit: item.unit || "Person",
      
      isOneWayPaid: item.isOneWayPaid || false,
      hasRoundTrip: item.hasRoundTrip || false,
      isRoundTripPaid: item.isRoundTripPaid || false,
      roundTripPrice: item.roundTripPrice || 0,
      hasNightFee: item.hasNightFee || false,
      additionalFee: item.additionalFee || 0,
      additionalFeeStartTime: item.additionalFeeStartTime || "22:00:00",
      additionalFeeEndTime: item.additionalFeeEndTime || "06:00:00",
      maxPassengers: item.maxPassengers || 4,
      maxLuggage: item.maxLuggage || 2
    });
    setFormError("");
    setIsModalOpen(true);
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Are you sure you want to delete this service?")) return;
    try {
      const res = await deleteService(id);
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

  function getApiEndpointType(typeId: number): "standard" | "airport-transfer" {
    return typeId === 2 ? "airport-transfer" : "standard";
  }

  function getDiscriminator(typeId: number): string {
    return typeId === 2 ? "airport" : "standard";
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
    
    // Base payload
    let payload: any = { 
      name: formData.name, 
      description: formData.description,
      price: formData.price,
      discriminator: getDiscriminator(formData.typeId)
    };

    if (endpointType === "standard") {
      payload = { ...payload, unit: formData.unit };
    } else {
      payload = { 
        ...payload, 
        isOneWayPaid: formData.isOneWayPaid,
        hasRoundTrip: formData.hasRoundTrip,
        isRoundTripPaid: formData.hasRoundTrip ? formData.isRoundTripPaid : false,
        roundTripPrice: formData.hasRoundTrip && formData.isRoundTripPaid ? formData.roundTripPrice : 0,
        hasNightFee: formData.hasNightFee,
        additionalFee: formData.hasNightFee ? formData.additionalFee : 0,
        additionalFeeStartTime: formData.hasNightFee ? formData.additionalFeeStartTime : null,
        additionalFeeEndTime: formData.hasNightFee ? formData.additionalFeeEndTime : null,
        maxPassengers: formData.maxPassengers,
        maxLuggage: formData.maxLuggage
      };
    }

    try {
      let res;
      if (modalMode === "Create") {
        res = await createService(endpointType, payload);
      } else {
        res = await updateService(editingId!, endpointType, payload);
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
    const t = serviceTypes.find(x => x.id === typeId);
    return t ? t.name : `Type ${typeId}`;
  }

  function renderServiceDetails(item: ServiceItem) {
    if (item.typeId === 1 || item.discriminator === "standard") {
      return (
        <div style={{ fontSize: "12px", color: "#64748B" }}>
          <div>Price: <strong>{formatFee(item.price)}</strong> / {item.unit || "unit"}</div>
        </div>
      );
    }
    
    if (item.typeId === 2 || item.discriminator === "airportTransfer") {
      return (
        <div style={{ fontSize: "12px", color: "#64748B" }}>
          <div>Base Price: <strong>{formatFee(item.price)}</strong> ({item.isOneWayPaid ? "Paid" : "Free"})</div>
          <div>Capacity: <strong>{item.maxPassengers}</strong> pax | <strong>{item.maxLuggage}</strong> luggage</div>
          {item.hasRoundTrip && (
            <div>Round Trip: <strong>{item.isRoundTripPaid ? formatFee(item.roundTripPrice) : "Free"}</strong></div>
          )}
          {item.hasNightFee && (
            <div>Night Fee ({item.additionalFeeStartTime} - {item.additionalFeeEndTime}): <strong>{formatFee(item.additionalFee)}</strong></div>
          )}
        </div>
      );
    }
    
    return <em>No details</em>;
  }

  function renderDynamicFormFields() {
    const type = getApiEndpointType(formData.typeId);
    
    if (type === "standard") {
      return (
        <div className="profile-grid" style={{ gap: "12px" }}>
          <div className="form-group">
            <label>Price (VNĐ)</label>
            <input type="number" value={formData.price} onChange={e => setFormData({...formData, price: parseFloat(e.target.value)})} disabled={formLoading} />
          </div>
          <div className="form-group">
            <label>Unit (e.g. Person, Room)</label>
            <input type="text" value={formData.unit} onChange={e => setFormData({...formData, unit: e.target.value})} disabled={formLoading} />
          </div>
        </div>
      );
    }
    
    if (type === "airport-transfer") {
      return (
        <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
          <div className="profile-grid" style={{ gap: "12px" }}>
            <div className="form-group">
              <label>One-Way Base Price (VNĐ)</label>
              <input type="number" value={formData.price} onChange={e => setFormData({...formData, price: parseFloat(e.target.value)})} disabled={formLoading} />
            </div>
            <div className="form-group">
              <label>Is One-Way Paid?</label>
              <select value={formData.isOneWayPaid ? "true" : "false"} onChange={e => setFormData({...formData, isOneWayPaid: e.target.value === "true"})} disabled={formLoading}>
                <option value="false">Free</option>
                <option value="true">Paid</option>
              </select>
            </div>
          </div>
          
          <div className="profile-grid" style={{ gap: "12px" }}>
            <div className="form-group">
              <label>Max Passengers</label>
              <input type="number" value={formData.maxPassengers} onChange={e => setFormData({...formData, maxPassengers: parseInt(e.target.value)})} disabled={formLoading} />
            </div>
            <div className="form-group">
              <label>Max Luggage</label>
              <input type="number" value={formData.maxLuggage} onChange={e => setFormData({...formData, maxLuggage: parseInt(e.target.value)})} disabled={formLoading} />
            </div>
          </div>

          <div style={{ padding: "12px", border: "1px solid #E2E8F0", borderRadius: "8px", backgroundColor: "#F8FAFC" }}>
            <div className="form-group" style={{ marginBottom: formData.hasRoundTrip ? "12px" : "0" }}>
              <label>Has Round Trip?</label>
              <select value={formData.hasRoundTrip ? "true" : "false"} onChange={e => setFormData({...formData, hasRoundTrip: e.target.value === "true"})} disabled={formLoading}>
                <option value="false">No</option>
                <option value="true">Yes</option>
              </select>
            </div>
            {formData.hasRoundTrip && (
              <div className="profile-grid" style={{ gap: "12px" }}>
                <div className="form-group">
                  <label>Is Round Trip Paid?</label>
                  <select value={formData.isRoundTripPaid ? "true" : "false"} onChange={e => setFormData({...formData, isRoundTripPaid: e.target.value === "true"})} disabled={formLoading}>
                    <option value="false">Free</option>
                    <option value="true">Paid</option>
                  </select>
                </div>
                {formData.isRoundTripPaid && (
                  <div className="form-group">
                    <label>Round Trip Price (VNĐ)</label>
                    <input type="number" value={formData.roundTripPrice} onChange={e => setFormData({...formData, roundTripPrice: parseFloat(e.target.value)})} disabled={formLoading} />
                  </div>
                )}
              </div>
            )}
          </div>

          <div style={{ padding: "12px", border: "1px solid #E2E8F0", borderRadius: "8px", backgroundColor: "#F8FAFC" }}>
            <div className="form-group" style={{ marginBottom: formData.hasNightFee ? "12px" : "0" }}>
              <label>Has Night Fee?</label>
              <select value={formData.hasNightFee ? "true" : "false"} onChange={e => setFormData({...formData, hasNightFee: e.target.value === "true"})} disabled={formLoading}>
                <option value="false">No</option>
                <option value="true">Yes</option>
              </select>
            </div>
            {formData.hasNightFee && (
              <div className="profile-grid" style={{ gap: "12px" }}>
                <div className="form-group">
                  <label>Additional Fee (VNĐ)</label>
                  <input type="number" value={formData.additionalFee} onChange={e => setFormData({...formData, additionalFee: parseFloat(e.target.value)})} disabled={formLoading} />
                </div>
                <div className="form-group">
                  <label>Start Time</label>
                  <input type="time" step="1" value={formData.additionalFeeStartTime} onChange={e => setFormData({...formData, additionalFeeStartTime: e.target.value})} disabled={formLoading} />
                </div>
                <div className="form-group">
                  <label>End Time</label>
                  <input type="time" step="1" value={formData.additionalFeeEndTime} onChange={e => setFormData({...formData, additionalFeeEndTime: e.target.value})} disabled={formLoading} />
                </div>
              </div>
            )}
          </div>
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
            {serviceTypes.map(t => (
              <option key={t.id} value={t.id}>{t.name}</option>
            ))}
          </select>
        </div>
        <button className="btn-create" onClick={openCreateModal}>+ Add Service</button>
      </div>

      {error && <div className="error-alert">{error}</div>}
      
      {loading ? (
        <div className="loading-state">Loading services...</div>
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
                    <td colSpan={5} className="empty-state">No services found.</td>
                  </tr>
                ) : (
                  items.map(item => (
                    <tr key={item.id}>
                      <td>{item.id}</td>
                      <td className="fw-bold">{item.name}</td>
                      <td>
                        <span className="type-badge">{getTypeName(item.typeId)}</span>
                      </td>
                      <td>{renderServiceDetails(item)}</td>
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
          <div className="manage-modal" style={{ maxWidth: "650px" }}>
            <div className="modal-header">
              <h3>{modalMode} Service</h3>
              <button type="button" className="close-btn" onClick={() => setIsModalOpen(false)}>×</button>
            </div>
            <form onSubmit={handleFormSubmit}>
              <div className="modal-body" style={{ maxHeight: "70vh", overflowY: "auto" }}>
                {formError && <div className="error-alert">{formError}</div>}
                
                <div className="form-group">
                  <label>Name <span className="req">*</span></label>
                  <input 
                    type="text" 
                    value={formData.name} 
                    onChange={e => setFormData({...formData, name: e.target.value})} 
                    disabled={formLoading}
                    placeholder="E.g., Airport Transfer, Free Breakfast"
                  />
                </div>

                {modalMode === "Create" && (
                  <div className="form-group">
                    <label>Service Type</label>
                    <select 
                      value={formData.typeId} 
                      onChange={e => setFormData({...formData, typeId: parseInt(e.target.value)})}
                      disabled={formLoading}
                    >
                      {serviceTypes.map(t => (
                        <option key={t.id} value={t.id}>{t.name}</option>
                      ))}
                    </select>
                  </div>
                )}
                {modalMode === "Edit" && (
                  <div className="form-group">
                    <label>Service Type (Read-only)</label>
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
