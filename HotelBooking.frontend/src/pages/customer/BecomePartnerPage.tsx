import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { createUpgradeRequest, getMyUpgradeInfo } from "../../services/customerService";
import { isLoggedIn, getStoredRoles } from "../../services/authService";
import "./BecomePartnerPage.css";

export default function BecomePartnerPage() {
  const [address, setAddress] = useState("");
  const [taxCode, setTaxCode] = useState("");
  
  const [loading, setLoading] = useState(false);
  const [initLoading, setInitLoading] = useState(true);
  const [error, setError] = useState("");
  const [status, setStatus] = useState<string>("None");
  
  const navigate = useNavigate();

  useEffect(() => {
    if (!isLoggedIn()) {
      navigate("/login", { replace: true });
      return;
    }
    
    const roles = getStoredRoles();
    if (roles.includes("Owner") || roles.includes("Admin")) {
      navigate("/404", { replace: true });
      return;
    }

    async function loadInfo() {
      try {
        const res = await getMyUpgradeInfo();
        if (res.statusCode === "Success" && res.content) {
          setStatus(res.content.requestStatus || "None");
        } else {
          setError("Could not load your current status.");
        }
      } catch (err) {
        setError("Network error while checking status.");
      } finally {
        setInitLoading(false);
      }
    }
    loadInfo();
  }, []);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!address.trim() || !taxCode.trim()) {
      setError("Please fill in all required fields.");
      return;
    }

    setLoading(true);
    setError("");

    try {
      const res = await createUpgradeRequest({ address, taxCode });
      if (res.statusCode === "Success") {
        setStatus("Pending");
      } else {
        setError(res.message || "Failed to submit request.");
      }
    } catch (err: unknown) {
      setError("A network error occurred. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  if (initLoading) {
    return (
      <div className="partner-page-wrapper">
        <div className="partner-page-container">
          <div className="partner-form-card" style={{ padding: "40px", textAlign: "center", color: "#64748B" }}>
            Loading your status...
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="partner-page-wrapper">
      <div className="partner-page-container">
        <div className="partner-form-card">
          <div className="partner-form-header">
            <h2>Become a Partner</h2>
            <p>Join our platform to list your properties and reach millions of guests.</p>
          </div>

          {status === "Pending" ? (
            <div className="partner-success-state">
              <div className="success-icon" style={{ background: "linear-gradient(135deg, #F59E0B, #FBBF24)" }}>⏳</div>
              <h3>Request is Pending</h3>
              <p>Your request to become a partner is currently under review by our administration team. Please wait for an email notification regarding the outcome.</p>
              <button className="partner-btn-secondary" onClick={() => navigate("/")} style={{ width: "100%", marginTop: "12px" }}>
                Return to Home
              </button>
            </div>
          ) : status === "Approved" ? (
            <div className="partner-success-state">
              <div className="success-icon">✓</div>
              <h3>Already a Partner!</h3>
              <p>Your account is already upgraded to an Owner account.</p>
              <button className="partner-btn-primary" onClick={() => navigate("/owner/dashboard")} style={{ width: "100%" }}>
                Go to Owner Dashboard
              </button>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="partner-form">
              {status === "Rejected" && (
                <div className="partner-error-alert" style={{ backgroundColor: "#FEF2F2", color: "#DC2626", borderColor: "#FCA5A5" }}>
                  Your previous request was rejected. You can submit a new request below.
                </div>
              )}
              {error && <div className="partner-error-alert">{error}</div>}
              
              <div className="partner-form-group">
                <label htmlFor="address">Business Address <span className="required">*</span></label>
                <input
                  id="address"
                  type="text"
                  placeholder="Enter your registered business address"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  disabled={loading}
                />
              </div>

              <div className="partner-form-group">
                <label htmlFor="taxCode">Tax Code (Mã số thuế) <span className="required">*</span></label>
                <input
                  id="taxCode"
                  type="text"
                  placeholder="Enter your tax code"
                  value={taxCode}
                  onChange={(e) => setTaxCode(e.target.value)}
                  disabled={loading}
                />
              </div>

              <div className="partner-form-actions">
                <button 
                  type="button" 
                  className="partner-btn-secondary"
                  onClick={() => navigate("/")}
                  disabled={loading}
                >
                  Cancel
                </button>
                <button 
                  type="submit" 
                  className="partner-btn-primary"
                  disabled={loading}
                >
                  {loading ? "Submitting..." : "Submit Request"}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
