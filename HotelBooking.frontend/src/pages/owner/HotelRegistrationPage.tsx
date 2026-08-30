import { useState, useEffect } from "react";
import { 
  getCountries, getProvinces, getWards, getPropertyTypes, 
  uploadBusinessLicense, submitRegistration, getMyRegistrations
} from "../../services/ownerRegistrationService";
import type { LocationItem, PropertyTypeItem, HotelRegistrationDTO } from "../../services/ownerRegistrationService";
import "./HotelRegistrationPage.css";

export default function HotelRegistrationPage() {
  
  // Lookups
  const [countries, setCountries] = useState<LocationItem[]>([]);
  const [provinces, setProvinces] = useState<LocationItem[]>([]);
  const [wards, setWards] = useState<LocationItem[]>([]);
  const [propertyTypes, setPropertyTypes] = useState<PropertyTypeItem[]>([]);
  
  // History
  const [history, setHistory] = useState<any[]>([]);
  const [loadingHistory, setLoadingHistory] = useState(true);

  // Form State
  const [formData, setFormData] = useState<Partial<HotelRegistrationDTO>>({
    name: "",
    description: "",
    address: "",
    publicPhone: "",
    publicEmail: "",
    taxCode: "",
    starRating: undefined
  });
  
  const [file, setFile] = useState<File | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    loadLookups();
    loadHistory();
  }, []);

  async function loadLookups() {
    try {
      const [cRes, pRes] = await Promise.all([
        getCountries(),
        getPropertyTypes()
      ]);
      if (cRes.statusCode === "Success" && cRes.content) {
        setCountries(cRes.content);
        if (cRes.content.length > 0) {
          handleCountryChange(cRes.content[0].id);
        }
      }
      if (pRes.statusCode === "Success" && pRes.content) {
        setPropertyTypes(pRes.content);
        if (pRes.content.length > 0) {
          setFormData(prev => ({ ...prev, propertyTypeId: pRes.content![0].id }));
        }
      }
    } catch (err) {
      console.error("Failed to load lookups", err);
    }
  }

  async function loadHistory() {
    try {
      const res = await getMyRegistrations();
      if (res.statusCode === "Success" && res.content) {
        setHistory(res.content);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingHistory(false);
    }
  }

  async function handleCountryChange(countryId: number) {
    setFormData(prev => ({ ...prev, countryId, provinceId: undefined, wardId: undefined }));
    const res = await getProvinces(countryId);
    if (res.statusCode === "Success" && res.content) {
      setProvinces(res.content);
      setWards([]);
      if (res.content.length > 0) {
        handleProvinceChange(res.content[0].id);
      }
    }
  }

  async function handleProvinceChange(provinceId: number) {
    setFormData(prev => ({ ...prev, provinceId, wardId: undefined }));
    const res = await getWards(provinceId);
    if (res.statusCode === "Success" && res.content) {
      setWards(res.content);
      if (res.content.length > 0) {
        setFormData(prev => ({ ...prev, wardId: res.content![0].id }));
      }
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess(false);

    if (!file) {
      setError("Please upload a Business License document (PDF/JPG/PNG).");
      return;
    }

    // Latitude and Longitude co-dependence check
    if ((formData.latitude && !formData.longitude) || (!formData.latitude && formData.longitude)) {
      setError("If you provide Latitude, you must also provide Longitude, and vice versa.");
      return;
    }

    setSubmitting(true);
    try {
      // 1. Upload File
      const uploadRes = await uploadBusinessLicense(file);
      if (uploadRes.statusCode !== "Success" || !uploadRes.content?.storedFileName) {
        setError("Failed to upload business license: " + (uploadRes.message || "Unknown error"));
        setSubmitting(false);
        return;
      }

      // 2. Build Submission DTO
      const pTypeName = propertyTypes.find(x => x.id === formData.propertyTypeId)?.name || "";
      const cName = countries.find(x => x.id === formData.countryId)?.name || "";
      const prName = provinces.find(x => x.id === formData.provinceId)?.name || "";
      const wName = wards.find(x => x.id === formData.wardId)?.name || "";

      const payload: HotelRegistrationDTO = {
        name: formData.name!,
        description: formData.description || "",
        address: formData.address!,
        propertyTypeId: formData.propertyTypeId!,
        propertyTypeName: pTypeName,
        starRating: formData.starRating,
        publicPhone: formData.publicPhone!,
        publicEmail: formData.publicEmail!,
        countryId: formData.countryId!,
        countryName: cName,
        provinceId: formData.provinceId!,
        provinceName: prName,
        wardId: formData.wardId!,
        wardName: wName,
        taxCode: formData.taxCode!,
        businessLicenseUrl: uploadRes.content.storedFileName
      };

      if (formData.latitude && formData.longitude) {
        payload.latitude = formData.latitude;
        payload.longitude = formData.longitude;
      }

      // 3. Submit
      const res = await submitRegistration(payload);
      if (res.statusCode === "Success") {
        setSuccess(true);
        setFormData({
          name: "", description: "", address: "", publicPhone: "", publicEmail: "", taxCode: "", starRating: undefined,
          countryId: countries[0]?.id, provinceId: provinces[0]?.id, wardId: wards[0]?.id, propertyTypeId: propertyTypes[0]?.id
        });
        setFile(null);
        // Reset file input element if needed
        const fileInput = document.getElementById("file-upload") as HTMLInputElement;
        if (fileInput) fileInput.value = "";
        loadHistory();
      } else {
        setError(res.message || "Failed to submit request.");
      }
    } catch (err: any) {
      setError(err?.response?.data?.message || "Network error. Please try again.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="registration-page">
      <div className="registration-header">
        <h1>Register New Property</h1>
        <p>Submit your property details to become a partner.</p>
      </div>

      <div className="registration-content">
        {/* FORM SECTION */}
        <div className="form-card">
          <h2>Property Information</h2>
          {error && <div className="error-alert">{error}</div>}
          {success && <div className="success-alert">Your registration request has been submitted and is Pending approval.</div>}
          
          <form onSubmit={handleSubmit} className="reg-form">
            <div className="form-row">
              <div className="form-group">
                <label>Property Name <span className="req">*</span></label>
                <input required minLength={6} maxLength={50} value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} disabled={submitting} />
              </div>
              <div className="form-group">
                <label>Property Type <span className="req">*</span></label>
                <select required value={formData.propertyTypeId} onChange={e => setFormData({...formData, propertyTypeId: parseInt(e.target.value)})} disabled={submitting}>
                  {propertyTypes.map(pt => <option key={pt.id} value={pt.id}>{pt.name}</option>)}
                </select>
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Public Phone <span className="req">*</span></label>
                <input required pattern="\d{10}" title="Must be exactly 10 digits" value={formData.publicPhone} onChange={e => setFormData({...formData, publicPhone: e.target.value})} disabled={submitting} />
              </div>
              <div className="form-group">
                <label>Public Email <span className="req">*</span></label>
                <input required type="email" value={formData.publicEmail} onChange={e => setFormData({...formData, publicEmail: e.target.value})} disabled={submitting} />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Tax Code <span className="req">*</span></label>
                <input required pattern="\d{10}(\d{3})?" title="10 or 13 digits" value={formData.taxCode} onChange={e => setFormData({...formData, taxCode: e.target.value})} disabled={submitting} />
              </div>
              <div className="form-group">
                <label>Star Rating (Optional)</label>
                <select value={formData.starRating || ""} onChange={e => setFormData({...formData, starRating: e.target.value ? parseInt(e.target.value) : undefined})} disabled={submitting}>
                  <option value="">None</option>
                  {[1,2,3,4,5].map(s => <option key={s} value={s}>{s} Stars</option>)}
                </select>
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Country</label>
                <select value={formData.countryId} onChange={e => handleCountryChange(parseInt(e.target.value))} disabled={submitting}>
                  {countries.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Province / City</label>
                <select value={formData.provinceId} onChange={e => handleProvinceChange(parseInt(e.target.value))} disabled={submitting || provinces.length === 0}>
                  {provinces.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Ward / District</label>
                <select value={formData.wardId} onChange={e => setFormData({...formData, wardId: parseInt(e.target.value)})} disabled={submitting || wards.length === 0}>
                  {wards.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
                </select>
              </div>
            </div>

            <div className="form-group full">
              <label>Full Address <span className="req">*</span></label>
              <input required minLength={10} maxLength={500} value={formData.address} onChange={e => setFormData({...formData, address: e.target.value})} disabled={submitting} placeholder="Street, House number..." />
            </div>
            
            <div className="form-row">
              <div className="form-group">
                <label>Latitude (Optional)</label>
                <input type="number" step="any" min="-90" max="90" value={formData.latitude || ""} onChange={e => setFormData({...formData, latitude: e.target.value ? parseFloat(e.target.value) : undefined})} disabled={submitting} />
              </div>
              <div className="form-group">
                <label>Longitude (Optional)</label>
                <input type="number" step="any" min="-180" max="180" value={formData.longitude || ""} onChange={e => setFormData({...formData, longitude: e.target.value ? parseFloat(e.target.value) : undefined})} disabled={submitting} />
              </div>
            </div>
            
            <div className="form-group full">
              <label>Description (Optional)</label>
              <textarea rows={3} maxLength={500} value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})} disabled={submitting} />
            </div>

            <div className="form-group full">
              <label>Business License Document <span className="req">*</span></label>
              <input id="file-upload" type="file" required accept=".pdf, .jpg, .jpeg, .png" onChange={e => setFile(e.target.files?.[0] || null)} disabled={submitting} />
              <small style={{ color: "#64748B", marginTop: "4px", display: "block" }}>PDF, JPEG, or PNG up to 10MB.</small>
            </div>

            <div className="form-actions">
              <button type="submit" className="btn-primary" disabled={submitting}>
                {submitting ? "Submitting..." : "Submit Registration"}
              </button>
            </div>
          </form>
        </div>

        {/* HISTORY SECTION */}
        <div className="history-card">
          <h2>My Registration Requests</h2>
          {loadingHistory ? (
            <p>Loading history...</p>
          ) : history.length === 0 ? (
            <div className="empty-state">No registration requests found.</div>
          ) : (
            <div className="history-list">
              {history.map(req => (
                <div key={req.requestId} className="history-item">
                  <div className="hi-header">
                    <span className="hi-name">{req.name}</span>
                    <span className={`status-badge ${req.status?.toLowerCase() || 'pending'}`}>{req.status}</span>
                  </div>
                  <div className="hi-body">
                    <p><strong>Property:</strong> {req.propertyTypeName} | <strong>Rating:</strong> {req.starRating ? `${req.starRating} Stars` : "N/A"}</p>
                    <p><strong>Address:</strong> {req.address}, {req.wardName}, {req.provinceName}</p>
                    <p><strong>Tax Code:</strong> {req.taxCode}</p>
                    <p><strong>Submitted:</strong> {new Date(req.requestedAt).toLocaleString()}</p>
                  </div>
                  <div className="hi-footer">
                    <a href={req.businessLicenseUrl} target="_blank" rel="noreferrer" className="btn-link">View License</a>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
