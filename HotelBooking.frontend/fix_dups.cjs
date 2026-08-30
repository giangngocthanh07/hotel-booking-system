const fs = require('fs');
const file = 'src/services/adminManagementService.ts';
const lines = fs.readFileSync(file, 'utf8').split('\n');

const newLines = [];
let skip = false;
for (let i = 0; i < lines.length; i++) {
  if (lines[i].includes('// ROOM ATTRIBUTES (Bed Types for instance)')) {
    skip = true;
  }
  if (skip && lines[i].includes('// POLICIES')) {
    skip = false;
  }
  if (!skip) {
    newLines.push(lines[i]);
  }
}
fs.writeFileSync(file, newLines.join('\n'), 'utf8');
