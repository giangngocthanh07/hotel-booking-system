import { useState, useEffect } from "react";
import { 
  getAmenityTypes, 
  getAmenities, 
  createAmenity, 
  updateAmenity, 
  deleteAmenity 
} from "../../../services/adminManagementService";
import type { AmenityType, AmenityItem } from "../../../services/adminManagementService";

export default function AmenitiesTab() {
  const [amenityTypes, setAmenityTypes] = useState<AmenityType[]>([]);
  const [items, setItems] = useState<AmenityItem[]>([]);
  
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
  const [formData, setFormData] = useState({ name: "", description: "", typeId: 1 });
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState("");

  useEffect(() => {
    async function loadTypes() {
      const typeRes = await getAmenityTypes();
      if (typeRes.statusCode === "Success" && typeRes.content) {
        setAmenityTypes(typeRes.content);
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
      const res = await getAmenities(pageIndex, 10, typeFilter);
      if (res.statusCode === "Success" && res.content) {
        setItems(res.content.items || []);
        setTotalPages(res.content.totalPages || 1);
      } else {
        setError(res.message || "Failed to load amenities.");
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
    setFormData({ name: "", description: "", typeId: amenityTypes.length > 0 ? amenityTypes[0].id : 1 });
    setFormError("");
    setIsModalOpen(true);
  }

  function openEditModal(item: AmenityItem) {
    setModalMode("Edit");
    setEditingId(item.id);
    setFormData({ 
      name: item.name, 
      description: item.description || "", 
      typeId: item.typeId || (amenityTypes.length > 0 ? amenityTypes[0].id : 1)
    });
    setFormError("");
    setIsModalOpen(true);
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Are you sure you want to delete this amenity?")) return;
    try {
      const res = await deleteAmenity(id);
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

  async function handleFormSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!formData.name.trim()) {
      setFormError("Name is required.");
      return;
    }
    setFormLoading(true);
    setFormError("");

    try {
      let res;
      if (modalMode === "Create") {
        res = await createAmenity({ name: formData.name, description: formData.description, typeId: formData.typeId });
      } else {
        // Edit DTO only requires name and description according to backend rules
        res = await updateAmenity(editingId!, { name: formData.name, description: formData.description });
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
    const t = amenityTypes.find(x => x.id === typeId);
    return t ? t.name : `Type ${typeId}`;
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
            {amenityTypes.map(t => (
              <option key={t.id} value={t.id}>{t.name}</option>
            ))}
          </select>
        </div>
        <button className="btn-create" onClick={openCreateModal}>+ Add Amenity</button>
      </div>

      {error && <div className="error-alert">{error}</div>}
      
      {loading ? (
        <div className="loading-state">Loading amenities...</div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="manage-table">
              <thead>
                <tr>
                  <th style={{ width: "80px" }}>ID</th>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Description</th>
                  <th style={{ width: "120px", textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="empty-state">No amenities found.</td>
                  </tr>
                ) : (
                  items.map(item => (
                    <tr key={item.id}>
                      <td>{item.id}</td>
                      <td className="fw-bold">{item.name}</td>
                      <td>
                        <span className="type-badge">{getTypeName(item.typeId)}</span>
                      </td>
                      <td className="text-muted">{item.description || "-"}</td>
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
          <div className="manage-modal">
            <div className="modal-header">
              <h3>{modalMode} Amenity</h3>
              <button type="button" className="close-btn" onClick={() => setIsModalOpen(false)}>×</button>
            </div>
            <form onSubmit={handleFormSubmit}>
              <div className="modal-body">
                {formError && <div className="error-alert">{formError}</div>}
                
                <div className="form-group">
                  <label>Name <span className="req">*</span></label>
                  <input 
                    type="text" 
                    value={formData.name} 
                    onChange={e => setFormData({...formData, name: e.target.value})} 
                    disabled={formLoading}
                    placeholder="E.g., Free WiFi"
                  />
                </div>

                {modalMode === "Create" && (
                  <div className="form-group">
                    <label>Amenity Type</label>
                    <select 
                      value={formData.typeId} 
                      onChange={e => setFormData({...formData, typeId: parseInt(e.target.value)})}
                      disabled={formLoading}
                    >
                      {amenityTypes.map(t => (
                        <option key={t.id} value={t.id}>{t.name}</option>
                      ))}
                    </select>
                  </div>
                )}
                {modalMode === "Edit" && (
                  <div className="form-group">
                    <label>Amenity Type (Read-only)</label>
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
                    rows={3}
                    placeholder="Short description..."
                  />
                </div>
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
