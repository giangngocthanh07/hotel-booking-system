import { useState, useEffect } from "react";
import { 
  getBedTypes, createBedType, updateBedType, deleteBedType 
} from "../../../services/adminManagementService";
import type { BedTypeItem } from "../../../services/adminManagementService";

export default function BedTypesTab() {
  const [items, setItems] = useState<BedTypeItem[]>([]);
  
  // States
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<"Create" | "Edit">("Create");
  const [editingId, setEditingId] = useState<number | null>(null);
  
  // Form State
  const [formData, setFormData] = useState({ 
    name: "", 
    description: "",
    defaultCapacity: 1,
    isVaryingSize: false,
    minWidth: 0,
    maxWidth: 0
  });
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState("");

  useEffect(() => {
    loadItems();
  }, []);

  async function loadItems() {
    setLoading(true);
    setError("");
    try {
      const res = await getBedTypes();
      if (res.statusCode === "Success" && res.content) {
        setItems(res.content);
      } else {
        setError(res.message || "Failed to load bed types.");
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
    setFormData({ 
      name: "", 
      description: "",
      defaultCapacity: 1,
      isVaryingSize: false,
      minWidth: 0,
      maxWidth: 0
    });
    setFormError("");
    setIsModalOpen(true);
  }

  function openEditModal(item: BedTypeItem) {
    setModalMode("Edit");
    setEditingId(item.id);
    setFormData({ 
      name: item.name, 
      description: item.description || "",
      defaultCapacity: item.defaultCapacity || 1,
      isVaryingSize: item.isVaryingSize || false,
      minWidth: item.minWidth || 0,
      maxWidth: item.maxWidth || 0
    });
    setFormError("");
    setIsModalOpen(true);
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Are you sure you want to delete this bed type?")) return;
    try {
      const res = await deleteBedType(id);
      if (res && res.statusCode === "Success") {
        loadItems();
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

    const payload = {
      name: formData.name,
      description: formData.description,
      defaultCapacity: formData.defaultCapacity,
      isVaryingSize: formData.isVaryingSize,
      minWidth: formData.isVaryingSize ? 0 : formData.minWidth,
      maxWidth: formData.isVaryingSize ? 0 : formData.maxWidth
    };

    try {
      let res;
      if (modalMode === "Create") {
        res = await createBedType(payload);
      } else {
        res = await updateBedType(editingId!, payload);
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

  return (
    <div className="amenities-tab-container">
      {/* TOOLBAR */}
      <div style={{ display: "flex", justifyContent: "flex-end", marginBottom: "16px" }}>
        <button className="btn-create" onClick={openCreateModal}>+ Add Bed Type</button>
      </div>

      {error && <div className="error-alert">{error}</div>}
      
      {loading ? (
        <div className="loading-state">Loading bed types...</div>
      ) : (
        <div className="table-responsive">
          <table className="manage-table">
            <thead>
              <tr>
                <th style={{ width: "80px" }}>ID</th>
                <th style={{ width: "20%" }}>Name</th>
                <th style={{ width: "15%" }}>Capacity</th>
                <th style={{ width: "20%" }}>Size (Width)</th>
                <th>Description</th>
                <th style={{ width: "120px", textAlign: "right" }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 ? (
                <tr>
                  <td colSpan={6} className="empty-state">No bed types found.</td>
                </tr>
              ) : (
                items.map(item => (
                  <tr key={item.id}>
                    <td>{item.id}</td>
                    <td className="fw-bold">{item.name}</td>
                    <td>{item.defaultCapacity} person(s)</td>
                    <td>
                      {item.isVaryingSize ? (
                        <span style={{ fontStyle: "italic", color: "#64748B" }}>Varying Size</span>
                      ) : (
                        <span>{item.minWidth}" - {item.maxWidth}"</span>
                      )}
                    </td>
                    <td>{item.description || <em style={{ color: "#94A3B8" }}>No description</em>}</td>
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
      )}

      {/* MODAL */}
      {isModalOpen && (
        <div className="manage-modal-overlay">
          <div className="manage-modal" style={{ maxWidth: "550px" }}>
            <div className="modal-header">
              <h3>{modalMode} Bed Type</h3>
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
                    placeholder="E.g., King Bed, Queen Bed"
                  />
                </div>

                <div className="profile-grid" style={{ gap: "12px", marginBottom: "16px" }}>
                  <div className="form-group">
                    <label>Default Capacity (1-10)</label>
                    <input 
                      type="number" 
                      min="1" 
                      max="10" 
                      value={formData.defaultCapacity} 
                      onChange={e => setFormData({...formData, defaultCapacity: parseInt(e.target.value)})} 
                      disabled={formLoading}
                    />
                  </div>
                  <div className="form-group">
                    <label>Is Size Varying?</label>
                    <select 
                      value={formData.isVaryingSize ? "true" : "false"} 
                      onChange={e => setFormData({...formData, isVaryingSize: e.target.value === "true"})} 
                      disabled={formLoading}
                    >
                      <option value="false">Fixed Size / Range</option>
                      <option value="true">Yes, Varying Size</option>
                    </select>
                  </div>
                </div>

                {!formData.isVaryingSize && (
                  <div className="profile-grid" style={{ gap: "12px", marginBottom: "16px", padding: "12px", backgroundColor: "#F8FAFC", borderRadius: "8px", border: "1px solid #E2E8F0" }}>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label>Min Width (inches)</label>
                      <input 
                        type="number" 
                        step="0.1" 
                        value={formData.minWidth} 
                        onChange={e => setFormData({...formData, minWidth: parseFloat(e.target.value)})} 
                        disabled={formLoading}
                      />
                    </div>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label>Max Width (inches)</label>
                      <input 
                        type="number" 
                        step="0.1" 
                        value={formData.maxWidth} 
                        onChange={e => setFormData({...formData, maxWidth: parseFloat(e.target.value)})} 
                        disabled={formLoading}
                      />
                    </div>
                  </div>
                )}

                <div className="form-group">
                  <label>Description</label>
                  <textarea 
                    value={formData.description} 
                    onChange={e => setFormData({...formData, description: e.target.value})}
                    disabled={formLoading}
                    rows={3}
                    placeholder="Optional description..."
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
