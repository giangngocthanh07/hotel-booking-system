const fs = require('fs');
const file = 'src/App.tsx';
let content = fs.readFileSync(file, 'utf8');

// Add import
const importStr = 'import HotelRegistrationPage from "./pages/owner/HotelRegistrationPage";\nimport ProtectedRoute from "./components/auth/ProtectedRoute";';
content = content.replace('import HotelRegistrationPage from "./pages/owner/HotelRegistrationPage";', importStr);

// Wrap owner route
const oldRoute = '{/* Owner section */}\n        <Route path="/owner/registration" element={<HotelRegistrationPage />} />';
const newRoute = `{/* Owner section */}
        <Route element={<ProtectedRoute allowedRoles={["Owner"]} />}>
          <Route path="/owner/registration" element={<HotelRegistrationPage />} />
        </Route>`;
content = content.replace(oldRoute, newRoute);

fs.writeFileSync(file, content, 'utf8');
console.log("Fixed app routing");
