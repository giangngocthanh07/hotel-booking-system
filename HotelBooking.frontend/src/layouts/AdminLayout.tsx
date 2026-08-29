import { Outlet, NavLink } from "react-router-dom";
import "./AdminLayout.css";

function AdminLayout() {
  return (
    <div className="admin-layout">
      {/* Sidebar Navigation */}
      <aside className="admin-sidebar">
        <h3 className="admin-sidebar__title">Admin Panel</h3>
        <nav className="admin-sidebar__nav">
          <NavLink 
            to="/admin/dashboard" 
            end
            className={({ isActive }) => isActive ? "admin-sidebar__link active" : "admin-sidebar__link"}
          >
            <span className="admin-sidebar__icon">📊</span>
            Dashboard
          </NavLink>
          
          <NavLink 
            to="/admin/users" 
            className={({ isActive }) => isActive ? "admin-sidebar__link active" : "admin-sidebar__link"}
          >
            <span className="admin-sidebar__icon">👥</span>
            Users Management
          </NavLink>
          
          <NavLink 
            to="/admin/requests" 
            className={({ isActive }) => isActive ? "admin-sidebar__link active" : "admin-sidebar__link"}
          >
            <span className="admin-sidebar__icon">📥</span>
            Requests
          </NavLink>
          
          <NavLink 
            to="/admin/reports" 
            className={({ isActive }) => isActive ? "admin-sidebar__link active" : "admin-sidebar__link"}
          >
            <span className="admin-sidebar__icon">📈</span>
            System Reports
          </NavLink>
        </nav>
      </aside>

      {/* Main Content Area */}
      <main className="admin-main">
        <Outlet />
      </main>
    </div>
  );
}

export default AdminLayout;
