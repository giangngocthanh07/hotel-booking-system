import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getCurrentUser, updateUserProfile, isLoggedIn, AUTH_CHANGED_EVENT } from "../../services/authService";
import type { UserDetail } from "../../services/authService";
import Header from "../../components/layout/Header";
import "./UserProfilePage.css";

export default function UserProfilePage() {
  const navigate = useNavigate();
  const [user, setUser] = useState<UserDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [successMsg, setSuccessMsg] = useState("");

  // Editable fields
  const [fullName, setFullName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");

  useEffect(() => {
    if (!isLoggedIn()) {
      navigate("/login", { replace: true });
      return;
    }

    async function loadUser() {
      try {
        const res = await getCurrentUser();
        if (res.statusCode === "Success" && res.content) {
          setUser(res.content);
          setFullName(res.content.fullName || "");
          setPhoneNumber(res.content.phoneNumber || "");
          if (res.content.dateOfBirth) {
            // Format to YYYY-MM-DD for date input
            const d = new Date(res.content.dateOfBirth);
            if (!isNaN(d.getTime())) {
              setDateOfBirth(d.toISOString().split("T")[0]);
            }
          }
        } else {
          setError(res.message || "Failed to load profile.");
        }
      } catch (err) {
        setError("Network error while loading profile.");
      } finally {
        setLoading(false);
      }
    }

    loadUser();
  }, [navigate]);

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    if (!fullName.trim()) {
      setError("Full Name is required.");
      return;
    }

    setSaving(true);
    setError("");
    setSuccessMsg("");

    try {
      const res = await updateUserProfile({
        fullName,
        phoneNumber,
        dateOfBirth: dateOfBirth ? new Date(dateOfBirth).toISOString() : undefined
      });

      if (res.statusCode === "Success") {
        setSuccessMsg("Profile updated successfully!");
        // Update local storage full name if it changed
        localStorage.setItem("fullName", fullName);
        window.dispatchEvent(new Event(AUTH_CHANGED_EVENT));
      } else {
        setError(res.message || "Failed to update profile.");
      }
    } catch (err) {
      setError("Network error. Please try again.");
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <div className="profile-wrapper">
        <Header />
        <div className="profile-container" style={{ textAlign: "center", paddingTop: "100px", color: "#64748B" }}>
          Loading profile...
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="profile-wrapper">
        <Header />
        <div className="profile-container" style={{ textAlign: "center", paddingTop: "100px", color: "#EF4444" }}>
          {error || "Could not load user data."}
        </div>
      </div>
    );
  }

  // Get first letter of full name for default avatar
  const initial = user.fullName ? user.fullName.charAt(0).toUpperCase() : "?";

  return (
    <div className="profile-wrapper">
      <Header />
      <div className="profile-container">
        
        <div className="profile-card">
          <div className="profile-header-bg"></div>
          
          <div className="profile-avatar-section">
            <div className="profile-avatar">
              {user.avatarUrl ? (
                <img src={user.avatarUrl} alt={user.fullName} className="profile-avatar-img" />
              ) : (
                <span className="profile-avatar-initial">{initial}</span>
              )}
              {/* Note: Avatar upload is disabled per user request */}
              <div className="profile-avatar-overlay">Read-only</div>
            </div>
            <div className="profile-title">
              <h2>{user.fullName}</h2>
              <span className="profile-role-badge">{user.roles.join(", ") || "User"}</span>
            </div>
          </div>

          <form className="profile-form" onSubmit={handleSave}>
            {error && <div className="profile-alert error">{error}</div>}
            {successMsg && <div className="profile-alert success">{successMsg}</div>}
            
            <div className="profile-section-title">Account Information (Read-only)</div>
            
            <div className="profile-grid">
              <div className="profile-group">
                <label>Username</label>
                <input type="text" value={user.userName} disabled className="input-disabled" title="Username cannot be changed" />
              </div>
              <div className="profile-group">
                <label>Email Address</label>
                <input type="email" value={user.email} disabled className="input-disabled" title="Email cannot be changed" />
              </div>
            </div>

            <div className="profile-section-title" style={{ marginTop: "32px" }}>Personal Details</div>
            
            <div className="profile-grid">
              <div className="profile-group">
                <label htmlFor="fullName">Full Name <span className="req">*</span></label>
                <input 
                  id="fullName"
                  type="text" 
                  value={fullName} 
                  onChange={(e) => setFullName(e.target.value)}
                  disabled={saving}
                />
              </div>
              
              <div className="profile-group">
                <label htmlFor="phone">Phone Number</label>
                <input 
                  id="phone"
                  type="tel" 
                  value={phoneNumber} 
                  onChange={(e) => setPhoneNumber(e.target.value)}
                  disabled={saving}
                  placeholder="Enter phone number"
                />
              </div>

              <div className="profile-group">
                <label htmlFor="dob">Date of Birth</label>
                <input 
                  id="dob"
                  type="date" 
                  value={dateOfBirth} 
                  onChange={(e) => setDateOfBirth(e.target.value)}
                  disabled={saving}
                />
              </div>
            </div>

            <div className="profile-actions">
              <button type="submit" className="profile-btn-save" disabled={saving}>
                {saving ? "Saving Changes..." : "Save Changes"}
              </button>
            </div>
          </form>

        </div>
      </div>
    </div>
  );
}
