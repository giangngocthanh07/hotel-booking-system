import { useState, useEffect } from "react";
import { 
  getRoomViews, createRoomView, updateRoomView, deleteRoomView 
} from "../../../services/adminManagementService";
import type { RoomViewItem } from "../../../services/adminManagementService";

export default function RoomViewsTab() {
  const [items, setItems] = useState<RoomViewItem[]>([]);
  
  // States
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<"Create" | "Edit">("Create");
  const [editingId, setEditingId] = useState<number | null>(null);
  
  // Form State
  const [formData, setFormData] = useState({ name: "", description: "" });
  const [formLoading, setFormLoading] = useState(false);
  const [formError, setFormError] = useState("");

  useEffect(() => {
    loadItems();
  }, []);

  async function loadItems() {
    setLoading(true);
    setError("");
    try {
      const res = await getRoomViews();
      if (res.statusCode === "Success" && res.content) {
        setItems(res.content);
      } else {
        setError(res.message || "Failed to load room views.");
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
    setFormData({ name: "", description: "" });
    setFormError("");
    setIsModalOpen(true);
  }

  function openEditModal(item: RoomViewItem) {
    setModalMode("Edit");
    setEditingId(item.id);
    setFormData({ name: item.name, description: item.description || "" });
    setFormError("");
    setIsModalOpen(true);
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Are you sure you want to delete this room view?")) return;
    try {
      const res = await deleteRoomView(id);
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

    try {
      let res;
      if (modalMode === "Create") {
        res = await createRoomView(formData);
      } else {
        res = await updateRoomView(editingId!, formData);
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
        <button className="btn-create" onClick={openCreateModal}>+ Add Room View</button>
      </div>

      {error && <div className="error-alert">{error}</div>}
      
      {loading ? (
        <div className="loading-state">Loading room views...</div>
      ) : (
        <div className="table-responsive">
          <table className="manage-table">
            <thead>
              <tr>
                <th style={{ width: "80px" }}>ID</th>
                <th style={{ width: "30%" }}>Name</th>
                <th>Description</th>
                <th style={{ width: "120px", textAlign: "right" }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 ? (
                <tr>
                  <td colSpan={4} className="empty-state">No room views found.</td>
                </tr>
              ) : (
                items.map(item => (
                  <tr key={item.id}>
                    <td>{item.id}</td>
                    <td className="fw-bold">{item.name}</td>
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
          <div className="manage-modal">
            <div className="modal-header">
              <h3>{modalMode} Room View</h3>
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
                    placeholder="E.g., Ocean View, City View"
                  />
                </div>

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
