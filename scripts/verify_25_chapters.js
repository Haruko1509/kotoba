const fs = require('fs');
const vm = require('vm');
const sandbox = {};
vm.createContext(sandbox);

let dataContent = fs.readFileSync('web-preview/js/data.js', 'utf8').replace(/const booksData/g, 'var booksData');
vm.runInContext(dataContent, sandbox);

let chapsContent = fs.readFileSync('web-preview/js/chapters-data.js', 'utf8');
vm.runInContext(chapsContent, sandbox);

const books = Object.values(sandbox.booksData);
console.log('Total books in catalogue:', books.length);
let all25 = true;
books.forEach((b, i) => {
  const chaps = sandbox.getNovelChapters(b.id);
  const ok = chaps.length >= 25;
  if (!ok) all25 = false;
  console.log(`${i+1}. [${ok ? 'PASS' : 'FAIL'}] ${b.title} | Set: ${b.chapters} | List: ${chaps.length} chaps | Ch 1: ${chaps[0].titleJp} | Ch 25: ${chaps[24].titleJp}`);
});

console.log('\n=============================================');
console.log('VERIFICATION RESULT: ALL 52 NOVELS >= 25 CHAPTERS:', all25 ? 'SUCCESS (100%)' : 'FAILED');
console.log('=============================================');

