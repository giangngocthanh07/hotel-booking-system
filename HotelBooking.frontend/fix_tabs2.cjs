const fs = require('fs');
const file = 'src/pages/admin/AdminManagementPage.tsx';
let content = fs.readFileSync(file, 'utf8');

const oldNav = `<button className={activeTab === "Amenities" ? "tab active" : "tab"} onClick={() => setActiveTab("Amenities")}>Amenities</button>
        <button className={activeTab === "BedTypes" ? "tab active" : "tab"} onClick={() => setActiveTab("BedTypes")}>Bed Types</button>
        <button className={activeTab === "RoomViews" ? "tab active" : "tab"} onClick={() => setActiveTab("RoomViews")}>Room Views</button>
        <button className={activeTab === "Services" ? "tab active" : "tab"} onClick={() => setActiveTab("Services")}>Services</button>
        <button className={activeTab === "Policies" ? "tab active" : "tab"} onClick={() => setActiveTab("Policies")}>Policies</button>`;

const newNav = `<button className={activeTab === "Amenities" ? "tab active" : "tab"} onClick={() => setActiveTab("Amenities")}>Amenities</button>
        <button className={activeTab === "UnitTypes" ? "tab active" : "tab"} onClick={() => setActiveTab("UnitTypes")}>Unit Types</button>
        <button className={activeTab === "BedTypes" ? "tab active" : "tab"} onClick={() => setActiveTab("BedTypes")}>Bed Types</button>
        <button className={activeTab === "RoomViews" ? "tab active" : "tab"} onClick={() => setActiveTab("RoomViews")}>Room Views</button>
        <button className={activeTab === "RoomQualities" ? "tab active" : "tab"} onClick={() => setActiveTab("RoomQualities")}>Room Qualities</button>
        <button className={activeTab === "Services" ? "tab active" : "tab"} onClick={() => setActiveTab("Services")}>Services</button>
        <button className={activeTab === "Policies" ? "tab active" : "tab"} onClick={() => setActiveTab("Policies")}>Policies</button>`;

if (content.includes(oldNav)) {
  content = content.replace(oldNav, newNav);
  fs.writeFileSync(file, content, 'utf8');
  console.log("Success");
} else {
  console.log("Failed to match oldNav");
}
