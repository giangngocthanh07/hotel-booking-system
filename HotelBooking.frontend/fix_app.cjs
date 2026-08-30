const fs = require('fs');
const file = 'src/App.tsx';
let content = fs.readFileSync(file, 'utf8');

const importStr = 'import UserProfilePage from "./pages/user/UserProfilePage";\nimport HotelRegistrationPage from "./pages/owner/HotelRegistrationPage";';
content = content.replace('import UserProfilePage from "./pages/user/UserProfilePage";', importStr);

const routeStr = `{/* Owner section */}\n        <Route path="/owner/registration" element={<HotelRegistrationPage />} />\n\n        {/* Admin section */}`;
content = content.replace('{/* Admin section */}', routeStr);

fs.writeFileSync(file, content, 'utf8');
