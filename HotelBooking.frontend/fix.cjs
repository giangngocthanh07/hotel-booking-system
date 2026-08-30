const fs = require('fs');
const file = 'src/components/admin/management/PoliciesTab.tsx';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(/VND[^\)]*\)/g, 'VNĐ)');
content = content.replace(/VND[^\<]*\</g, 'VNĐ<');

// Fix buttons
content = content.replace(/title="Edit">[^<]+<\/button>/g, 'title="Edit">✎</button>');
content = content.replace(/title="Delete">[^<]+<\/button>/g, 'title="Delete">🗑</button>');

fs.writeFileSync(file, content, 'utf8');
