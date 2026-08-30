const fs = require('fs');
const file = 'src/components/admin/management/ServicesTab.tsx';
let content = fs.readFileSync(file, 'utf8');

const oldGrid = `            <div className="profile-grid" style={{ gap: "12px" }}>
              <div className="form-group">
                <label>One-Way Base Price (VND)</label>
                <input type="number" value={formData.price} onChange={e => setFormData({...formData, price: parseFloat(e.target.value)})} disabled={formLoading} />
              </div>
              <div className="form-group">
                <label>Is One-Way Paid?</label>
                <select value={formData.isOneWayPaid ? "true" : "false"} onChange={e => setFormData({...formData, isOneWayPaid: e.target.value === "true"})} disabled={formLoading}>
                  <option value="false">Free</option>
                  <option value="true">Paid</option>
                </select>
              </div>
            </div>`;

const newGrid = `            <div className="profile-grid" style={{ gap: "12px" }}>
              <div className="form-group" style={{ gridColumn: formData.isOneWayPaid ? "auto" : "1 / -1" }}>
                <label>Is One-Way Paid?</label>
                <select 
                  value={formData.isOneWayPaid ? "true" : "false"} 
                  onChange={e => setFormData({...formData, isOneWayPaid: e.target.value === "true", price: e.target.value === "true" ? formData.price : 0})} 
                  disabled={formLoading}
                >
                  <option value="false">Free</option>
                  <option value="true">Paid</option>
                </select>
              </div>
              {formData.isOneWayPaid && (
                <div className="form-group">
                  <label>One-Way Base Price (VNĐ)</label>
                  <input type="number" value={formData.price} onChange={e => setFormData({...formData, price: parseFloat(e.target.value)})} disabled={formLoading} />
                </div>
              )}
            </div>`;

content = content.replace(oldGrid, newGrid);

// Ensure other 'Price (VND)' labels are also changed to 'Price (VNĐ)'
content = content.replace(/Price \(VND\)/g, 'Price (VNĐ)');

fs.writeFileSync(file, content, 'utf8');
