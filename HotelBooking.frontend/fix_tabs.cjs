const fs = require('fs');
const file = 'src/pages/admin/AdminManagementPage.tsx';
let content = fs.readFileSync(file, 'utf8');

// Update imports
if (!content.includes('import UnitTypesTab')) {
  content = content.replace(
    'import BedTypesTab from "../../components/admin/management/BedTypesTab";',
    'import BedTypesTab from "../../components/admin/management/BedTypesTab";\nimport UnitTypesTab from "../../components/admin/management/UnitTypesTab";\nimport RoomQualitiesTab from "../../components/admin/management/RoomQualitiesTab";'
  );
}

// Update TabType
content = content.replace(
  'type TabType = "Amenities" | "BedTypes" | "RoomViews" | "Services" | "Policies";',
  'type TabType = "Amenities" | "UnitTypes" | "BedTypes" | "RoomViews" | "RoomQualities" | "Services" | "Policies";'
);

// Update nav buttons
const oldNav = `<button className={activeTab === "Amenities" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("Amenities")}>Amenities</button>
        <button className={activeTab === "BedTypes" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("BedTypes")}>Bed Types</button>
        <button className={activeTab === "RoomViews" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("RoomViews")}>Room Views</button>
        <button className={activeTab === "Services" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("Services")}>Services</button>
        <button className={activeTab === "Policies" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("Policies")}>Policies</button>`;

const newNav = `<button className={activeTab === "Amenities" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("Amenities")}>Amenities</button>
        <button className={activeTab === "UnitTypes" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("UnitTypes")}>Unit Types</button>
        <button className={activeTab === "BedTypes" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("BedTypes")}>Bed Types</button>
        <button className={activeTab === "RoomViews" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("RoomViews")}>Room Views</button>
        <button className={activeTab === "RoomQualities" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("RoomQualities")}>Room Qualities</button>
        <button className={activeTab === "Services" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("Services")}>Services</button>
        <button className={activeTab === "Policies" ? "manage-tab active" : "manage-tab"} onClick={() => setActiveTab("Policies")}>Policies</button>`;

content = content.replace(oldNav, newNav);

// Update active components
const oldComponents = `{activeTab === "Amenities" && <AmenitiesTab />}
        {activeTab === "BedTypes" && <BedTypesTab />}
        {activeTab === "RoomViews" && <RoomViewsTab />}
        {activeTab === "Services" && <ServicesTab />}
        {activeTab === "Policies" && <PoliciesTab />}`;

const newComponents = `{activeTab === "Amenities" && <AmenitiesTab />}
        {activeTab === "UnitTypes" && <UnitTypesTab />}
        {activeTab === "BedTypes" && <BedTypesTab />}
        {activeTab === "RoomViews" && <RoomViewsTab />}
        {activeTab === "RoomQualities" && <RoomQualitiesTab />}
        {activeTab === "Services" && <ServicesTab />}
        {activeTab === "Policies" && <PoliciesTab />}`;

content = content.replace(oldComponents, newComponents);

fs.writeFileSync(file, content, 'utf8');
