import { useState } from "react";
import AmenitiesTab from "../../components/admin/management/AmenitiesTab";
import PoliciesTab from "../../components/admin/management/PoliciesTab";
import ServicesTab from "../../components/admin/management/ServicesTab";
import "./AdminManagementPage.css";

type TabType = "Amenities" | "BedTypes" | "RoomViews" | "Services" | "Policies";

export default function AdminManagementPage() {
  const [activeTab, setActiveTab] = useState<TabType>("Amenities");
  
  return (
    <div className="admin-manage-page">
      <div className="manage-header">
        <div>
          <h1 className="manage-title">System Data Management</h1>
          <p className="manage-subtitle">Manage system dictionaries like amenities, bed types, policies, etc.</p>
        </div>
      </div>

      <div className="manage-tabs">
        <button className={activeTab === "Amenities" ? "tab active" : "tab"} onClick={() => setActiveTab("Amenities")}>Amenities</button>
        <button className={activeTab === "BedTypes" ? "tab active" : "tab"} onClick={() => setActiveTab("BedTypes")}>Bed Types</button>
        <button className={activeTab === "RoomViews" ? "tab active" : "tab"} onClick={() => setActiveTab("RoomViews")}>Room Views</button>
        <button className={activeTab === "Services" ? "tab active" : "tab"} onClick={() => setActiveTab("Services")}>Services</button>
        <button className={activeTab === "Policies" ? "tab active" : "tab"} onClick={() => setActiveTab("Policies")}>Policies</button>
      </div>

      <div className="manage-content">
        {activeTab === "Amenities" && <AmenitiesTab />}
        {activeTab === "BedTypes" && <div className="empty-state">Bed Types management (Coming soon)</div>}
        {activeTab === "RoomViews" && <div className="empty-state">Room Views management (Coming soon)</div>}
        {activeTab === "Services" && <ServicesTab />}
        {activeTab === "Policies" && <PoliciesTab />}
      </div>
    </div>
  );
}
