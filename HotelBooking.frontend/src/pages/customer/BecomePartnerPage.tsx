import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createUpgradeRequest } from "../../services/customerService";
import "./BecomePartnerPage.css";

export default function BecomePartnerPage() {
  const [address, setAddress] = useState("");
  const [taxCode, setTaxCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);
  const navigate = useNavigate();

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
        setSuccess(true);
      } else {
        setError(res.message || "Failed to submit request. You may already have a pending request.");
      }
    } catch (err: unknown) {
      setError("A network error occurred. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="partner-page-wrapper">
      <div className="partner-page-container">
        <div className="partner-form-card">
          <div className="partner-form-header">
            <h2>Become a Partner</h2>
            <p>Join our platform to list your properties and reach millions of guests.</p>
          </div>

          {success ? (
            <div className="partner-success-state">
              <div className="success-icon">✓</div>
              <h3>Request Submitted!</h3>
              <p>Your request to become a partner has been received and is currently pending admin approval. We will notify you once it has been reviewed.</p>
              <button className="partner-btn-primary" onClick={() => navigate("/")}>
                Return to Home
              </button>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="partner-form">
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
