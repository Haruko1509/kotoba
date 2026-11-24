const fs = require('fs');
let content = fs.readFileSync('web-preview/js/data.js', 'utf8');
content = content.replace(/const booksData/g, 'var booksData');
content = content.replace(/const crawledChapterProse/g, 'var crawledChapterProse');

const vm = require('vm');
const sandbox = {};
vm.createContext(sandbox);
vm.runInContext(content, sandbox);

const keys = Object.keys(sandbox.booksData);
console.log('Total books in booksData:', keys.length);
keys.forEach((k, i) => {
  const b = sandbox.booksData[k];
  console.log(`${i+1}. [${b.level}] ${b.title} (${b.romaji || ''}) - ${b.genre}`);
});
console.log('\nCrawled prose entries:', Object.keys(sandbox.crawledChapterProse));
