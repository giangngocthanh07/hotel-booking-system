import { Outlet, NavLink } from "react-router-dom";
import "./OwnerLayout.css";

export default function OwnerLayout() {
  return (
    <div className="owner-layout">
      {/* Sidebar Navigation */}
      <aside className="owner-sidebar">
        <h3 className="owner-sidebar__title">Owner Panel</h3>
        <nav className="owner-sidebar__nav">
          <NavLink 
            to="/owner/dashboard" 
            end
            className={({ isActive }) => isActive ? "owner-sidebar__link active" : "owner-sidebar__link"}
          >
            <span className="owner-sidebar__icon">📊</span>
            Dashboard
          </NavLink>
          
          <NavLink 
            to="/owner/registration" 
            className={({ isActive }) => isActive ? "owner-sidebar__link active" : "owner-sidebar__link"}
          >
            <span className="owner-sidebar__icon">🏨</span>
            Register Hotel
          </NavLink>

          <NavLink 
            to="/owner/hotels" 
            className={({ isActive }) => isActive ? "owner-sidebar__link active" : "owner-sidebar__link"}
          >
            <span className="owner-sidebar__icon">🏢</span>
            My Hotels
          </NavLink>

          <NavLink 
            to="/owner/bookings" 
            className={({ isActive }) => isActive ? "owner-sidebar__link active" : "owner-sidebar__link"}
          >
            <span className="owner-sidebar__icon">📅</span>
            Bookings
          </NavLink>
        </nav>
      </aside>

      {/* Main Content Area */}
      <main className="owner-main">
        <Outlet />
      </main>
    </div>
  );
}
