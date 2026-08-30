import { useState, useEffect } from "react";
import { 
  getAmenityTypes, getAmenities, createAmenity, updateAmenity, deleteAmenity,
  getBedTypes, createBedType, updateBedType, deleteBedType
} from "../../services/adminManagementService";
import type { AmenityType, AmenityItem, BaseAdminItem } from "../../services/adminManagementService";
import "./AdminManagementPage.css";

type TabType = "Amenities" | "BedTypes" | "RoomViews" | "Services" | "Policies";

export default function AdminManagementPage() {
  const [activeTab, setActiveTab] = useState<TabType>("Amenities");
  
  // Data state
  const [amenityTypes, setAmenityTypes] = useState<AmenityType[]>([]);
  const [items, setItems] = useState<any[]>([]);
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

  // Load basic data
  useEffect(() => {
    async function init() {
      if (activeTab === "Amenities") {
        const typeRes = await getAmenityTypes();
        if (typeRes.statusCode === "Success" && typeRes.content) {
          setAmenityTypes(typeRes.content);
        }
      }
      loadItems();
    }
    init();
  }, [activeTab]);

  async function loadItems() {
    setLoading(true);
    setError("");
    try {
      if (activeTab === "Amenities") {
        const res = await getAmenities(1, 100);
        if (res.statusCode === "Success" && res.content) {
          setItems(res.content.items || []);
        } else {
          setError(res.message || "Failed to load amenities.");
        }
      } else if (activeTab === "BedTypes") {
        const res = await getBedTypes(1, 100);
        if (res.statusCode === "Success" && res.content) {
          setItems(res.content.items || []);
        } else {
          setError(res.message || "Failed to load bed types.");
        }
      } else {
        setItems([]); // Placeholders for other tabs
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

  function openEditModal(item: any) {
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
    if (!window.confirm("Are you sure you want to delete this item?")) return;
    try {
      let res;
      if (activeTab === "Amenities") {
        res = await deleteAmenity(id);
      } else if (activeTab === "BedTypes") {
        res = await deleteBedType(id);
      }
      
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
      if (activeTab === "Amenities") {
        if (modalMode === "Create") {
          res = await createAmenity({ name: formData.name, description: formData.description, typeId: formData.typeId });
        } else {
          res = await updateAmenity(editingId!, { name: formData.name, description: formData.description });
        }
      } else if (activeTab === "BedTypes") {
        if (modalMode === "Create") {
          res = await createBedType({ name: formData.name, description: formData.description });
        } else {
          res = await updateBedType(editingId!, { name: formData.name, description: formData.description });
        }
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
    <div className="admin-manage-page">
      <div className="manage-header">
        <div>
          <h1 className="manage-title">System Data Management</h1>
          <p className="manage-subtitle">Manage system dictionaries like amenities, bed types, policies, etc.</p>
        </div>
        <button className="btn-create" onClick={openCreateModal}>+ Add New</button>
      </div>

      <div className="manage-tabs">
        <button className={activeTab === "Amenities" ? "tab active" : "tab"} onClick={() => setActiveTab("Amenities")}>Amenities</button>
        <button className={activeTab === "BedTypes" ? "tab active" : "tab"} onClick={() => setActiveTab("BedTypes")}>Bed Types</button>
        <button className={activeTab === "RoomViews" ? "tab active" : "tab"} onClick={() => setActiveTab("RoomViews")}>Room Views (Coming soon)</button>
        <button className={activeTab === "Services" ? "tab active" : "tab"} onClick={() => setActiveTab("Services")}>Services (Coming soon)</button>
        <button className={activeTab === "Policies" ? "tab active" : "tab"} onClick={() => setActiveTab("Policies")}>Policies (Coming soon)</button>
      </div>

      <div className="manage-content">
        {error && <div className="error-alert">{error}</div>}
        
        {loading ? (
          <div className="loading-state">Loading data...</div>
        ) : (
          <div className="table-responsive">
            <table className="manage-table">
              <thead>
                <tr>
                  <th style={{ width: "80px" }}>ID</th>
                  <th>Name</th>
                  {activeTab === "Amenities" && <th>Type</th>}
                  <th>Description</th>
                  <th style={{ width: "150px", textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="empty-state">No items found for {activeTab}.</td>
                  </tr>
                ) : (
                  items.map(item => (
                    <tr key={item.id}>
                      <td>{item.id}</td>
                      <td className="fw-bold">{item.name}</td>
                      {activeTab === "Amenities" && (
                        <td>
                          <span className="type-badge">{getTypeName(item.typeId)}</span>
                        </td>
                      )}
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
        )}
      </div>

      {/* CUSTOM MODAL */}
      {isModalOpen && (
        <div className="manage-modal-overlay">
          <div className="manage-modal">
            <div className="modal-header">
              <h3>{modalMode} {activeTab === "Amenities" ? "Amenity" : "Bed Type"}</h3>
              <button className="close-btn" onClick={() => setIsModalOpen(false)}>×</button>
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

                {activeTab === "Amenities" && modalMode === "Create" && (
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
