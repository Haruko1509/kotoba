const fs = require('fs');

const html = fs.readFileSync('web-preview/index.html', 'utf8');
const lines = html.split('\n');

console.log('=== READER SECTION IN INDEX.HTML ===');
let inReader = false;
let readerLines = [];
lines.forEach((line, idx) => {
  if (line.includes('id="pane-reader"') || line.includes('class="pane pane-reader"') || line.includes('id="readerShell"')) {
    inReader = true;
  }
  if (inReader) {
    readerLines.push({ lineNum: idx + 1, text: line });
    if (line.includes('id="pane-srs"') || line.includes('id="pane-learning"') || line.includes('id="pane-profile"')) {
      inReader = false;
    }
  }
});

console.log(`Found ${readerLines.length} lines in Reader Pane.`);
// Print key sections
readerLines.forEach(l => {
  if (l.text.includes('<header') || l.text.includes('<button') || l.text.includes('<select') || l.text.includes('id=') || l.text.includes('class="reader-')) {
    console.log(`${l.lineNum}: ${l.text.trim()}`);
  }
});

