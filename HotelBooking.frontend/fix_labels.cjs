const fs = require('fs');
const file = 'src/components/admin/management/PoliciesTab.tsx';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(/Fee \([^\)]*\)/g, 'Fee (VNĐ)');
fs.writeFileSync(file, content, 'utf8');
