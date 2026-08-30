const fs = require('fs');
const file = 'src/components/admin/management/ServicesTab.tsx';
let content = fs.readFileSync(file, 'utf8');

// 1. Update openCreateModal
content = content.replace(
  /hasNightFee: false,\s*additionalFee: 0,/,
  'hasNightFee: false,\n      isNightFeePaid: false,\n      additionalFee: 0,'
);

// 2. Update openEditModal
content = content.replace(
  /hasNightFee: item\.hasNightFee \|\| false,\s*additionalFee: item\.additionalFee \|\| 0,/,
  'hasNightFee: item.hasNightFee || false,\n      isNightFeePaid: (item.additionalFee && item.additionalFee > 0) ? true : false,\n      additionalFee: item.additionalFee || 0,'
);

// 3. Update handleFormSubmit payload
content = content.replace(
  /additionalFee: formData\.hasNightFee \? formData\.additionalFee : 0,/,
  'additionalFee: formData.hasNightFee && formData.isNightFeePaid ? formData.additionalFee : 0,'
);

// 4. Update the Night Fee UI
const oldUI = `          <div style={{ padding: "12px", border: "1px solid #E2E8F0", borderRadius: "8px", backgroundColor: "#F8FAFC" }}>
            <div className="form-group" style={{ marginBottom: formData.hasNightFee ? "12px" : "0" }}>
              <label>Has Night Fee?</label>
              <select value={formData.hasNightFee ? "true" : "false"} onChange={e => setFormData({...formData, hasNightFee: e.target.value === "true"})} disabled={formLoading}>
                <option value="false">No</option>
                <option value="true">Yes</option>
              </select>
            </div>
            {formData.hasNightFee && (
              <div className="profile-grid" style={{ gap: "12px" }}>
                <div className="form-group">
                  <label>Additional Fee (VNĐ)</label>
                  <input type="number" value={formData.additionalFee} onChange={e => setFormData({...formData, additionalFee: parseFloat(e.target.value)})} disabled={formLoading} />
                </div>
                <div className="form-group">
                  <label>Start Time</label>
                  <input type="time" step="1" value={formData.additionalFeeStartTime} onChange={e => setFormData({...formData, additionalFeeStartTime: e.target.value})} disabled={formLoading} />
                </div>
                <div className="form-group">
                  <label>End Time</label>
                  <input type="time" step="1" value={formData.additionalFeeEndTime} onChange={e => setFormData({...formData, additionalFeeEndTime: e.target.value})} disabled={formLoading} />
                </div>
              </div>
            )}
          </div>`;

const newUI = `          <div style={{ padding: "12px", border: "1px solid #E2E8F0", borderRadius: "8px", backgroundColor: "#F8FAFC" }}>
            <div className="form-group" style={{ marginBottom: formData.hasNightFee ? "12px" : "0" }}>
              <label>Has Night Fee?</label>
              <select value={formData.hasNightFee ? "true" : "false"} onChange={e => setFormData({...formData, hasNightFee: e.target.value === "true"})} disabled={formLoading}>
                <option value="false">No</option>
                <option value="true">Yes</option>
              </select>
            </div>
            {formData.hasNightFee && (
              <>
                <div className="profile-grid" style={{ gap: "12px", marginBottom: "12px" }}>
                  <div className="form-group" style={{ gridColumn: formData.isNightFeePaid ? "auto" : "1 / -1" }}>
                    <label>Is Night Fee Paid?</label>
                    <select 
                      value={formData.isNightFeePaid ? "true" : "false"} 
                      onChange={e => setFormData({...formData, isNightFeePaid: e.target.value === "true", additionalFee: e.target.value === "true" ? formData.additionalFee : 0})} 
                      disabled={formLoading}
                    >
                      <option value="false">Free</option>
                      <option value="true">Paid</option>
                    </select>
                  </div>
                  {formData.isNightFeePaid && (
                    <div className="form-group">
                      <label>Additional Fee (VNĐ)</label>
                      <input type="number" value={formData.additionalFee} onChange={e => setFormData({...formData, additionalFee: parseFloat(e.target.value)})} disabled={formLoading} />
                    </div>
                  )}
                </div>
                <div className="profile-grid" style={{ gap: "12px" }}>
                  <div className="form-group">
                    <label>Start Time</label>
                    <input type="time" step="1" value={formData.additionalFeeStartTime} onChange={e => setFormData({...formData, additionalFeeStartTime: e.target.value})} disabled={formLoading} />
                  </div>
                  <div className="form-group">
                    <label>End Time</label>
                    <input type="time" step="1" value={formData.additionalFeeEndTime} onChange={e => setFormData({...formData, additionalFeeEndTime: e.target.value})} disabled={formLoading} />
                  </div>
                </div>
              </>
            )}
          </div>`;

content = content.replace(oldUI, newUI);

// Check if Additional Fee label has VND instead of VNĐ and fix it for replacement matching
if (content.indexOf(oldUI) === -1) {
  // Try with VND
  const oldUIVND = oldUI.replace('VNĐ', 'VND');
  content = content.replace(oldUIVND, newUI);
}

fs.writeFileSync(file, content, 'utf8');
