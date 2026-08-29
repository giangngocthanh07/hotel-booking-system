// Header.tsx
// Top navigation bar shown on every page — Hotels.com style.
// Shows "Sign in" when logged out, or a circular avatar (first letter of the user's name) when logged in.

import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  AUTH_CHANGED_EVENT,
  getStoredFullName,
  getStoredRoles,
  isLoggedIn,
  logout,
} from "../../services/authService";
import "./Header.css";

function Header() {
  const [fullName, setFullName] = useState<string | null>(getStoredFullName());
  const [roles, setRoles] = useState<string[]>(getStoredRoles());
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  // Re-read the session whenever it changes (login/logout), even within the same tab
  useEffect(() => {
    function refreshSession() {
      setFullName(getStoredFullName());
      setRoles(getStoredRoles());
    }
    window.addEventListener(AUTH_CHANGED_EVENT, refreshSession);
    window.addEventListener("storage", refreshSession);
    return () => {
      window.removeEventListener(AUTH_CHANGED_EVENT, refreshSession);
      window.removeEventListener("storage", refreshSession);
    };
  }, []);

  // Close the avatar dropdown when clicking outside of it
  useEffect(() => {
    function handleOutsideClick(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsMenuOpen(false);
      }
    }
    document.addEventListener("mousedown", handleOutsideClick);
    return () => document.removeEventListener("mousedown", handleOutsideClick);
  }, []);

  function handleSignOut() {
    logout();
    setIsMenuOpen(false);
    navigate("/");
  }

  const initial = fullName ? fullName.trim().charAt(0).toUpperCase() : "";

  return (
    <header className="site-header">
      <a href="/" className="site-header__logo">
        <span className="site-header__logo-badge">H</span>
        HotelBooking
      </a>

      <nav className="site-header__nav">
        <span className="site-header__currency">VND • 🇻🇳</span>
        {!roles.includes("Owner") && !roles.includes("Admin") && (
          <a href="#" className="site-header__link site-header__link--accent">
            List your property
          </a>
        )}
        <a href="#" className="site-header__link">
          Support
        </a>
        <a href="#" className="site-header__link">
          Trips
        </a>
        <a href="#" className="site-header__icon-link" aria-label="Messages">
          💬
        </a>

        {isLoggedIn() && fullName ? (
          <div className="site-header__avatar-wrapper" ref={menuRef}>
            <button
              type="button"
              className="site-header__avatar"
              onClick={() => setIsMenuOpen((open) => !open)}
              aria-label={`Account menu for ${fullName}`}
            >
              {initial}
            </button>
            {isMenuOpen && (
              <div className="site-header__menu">
                <div className="site-header__menu-name">{fullName}</div>
                <div className="site-header__menu-divider"></div>
                
                <a href="/profile" className="site-header__menu-item">
                  User profile
                </a>
                
                {roles.includes("Owner") && (
                  <a href="/owner/dashboard" className="site-header__menu-item">
                    Owner dashboard
                  </a>
                )}

                {roles.includes("Admin") && (
                  <a href="/admin/dashboard" className="site-header__menu-item">
                    Admin dashboard
                  </a>
                )}
                
                <button
                  type="button"
                  className="site-header__menu-item"
                  onClick={handleSignOut}
                >
                  Sign out
                </button>
              </div>
            )}
          </div>
        ) : (
          <a
            href="/login"
            className="site-header__link site-header__link--signin"
          >
            Sign in
          </a>
        )}
      </nav>
    </header>
  );
}

export default Header;
