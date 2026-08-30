const fs = require('fs');
const file = 'src/pages/owner/HotelRegistrationPage.tsx';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(
    'if (uploadRes.statusCode !== "Success" || !uploadRes.content?.url) {',
    'if (uploadRes.statusCode !== "Success" || !uploadRes.content?.storedFileName) {'
);

content = content.replace(
    'businessLicenseUrl: uploadRes.content.url',
    'businessLicenseUrl: uploadRes.content.storedFileName'
);

fs.writeFileSync(file, content, 'utf8');
console.log('Fixed upload url reading');
