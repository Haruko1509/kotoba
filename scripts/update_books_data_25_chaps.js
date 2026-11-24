const fs = require('fs');

let content = fs.readFileSync('web-preview/js/data.js', 'utf8');

// Replace all chapters: 'X Chương' with chapters: '25 Chương'
content = content.replace(/chapters:\s*'[0-9]+\s*Chương[^']*'/g, "chapters: '25 Chương'");
content = content.replace(/chapters:\s*'[0-9]+\s*Chương'/g, "chapters: '25 Chương'");

// Replace small word counts with realistic numbers for 25 chapters
content = content.replace(/words:\s*'[0-9,]+\s*Chữ Hán'/g, "words: '28,500 Chữ Hán'");
content = content.replace(/vocab:\s*'[0-9,]+\s*Từ JLPT'/g, "vocab: '420 Từ JLPT'");

fs.writeFileSync('web-preview/js/data.js', content, 'utf8');
console.log('✓ Updated booksData in web-preview/js/data.js to 25 chapters for each novel!');

