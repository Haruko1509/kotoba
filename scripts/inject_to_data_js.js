const fs = require('fs');
const path = require('path');

const crawled = JSON.parse(fs.readFileSync('scripts/crawled_light_novels.json', 'utf8'));
let dataJs = fs.readFileSync('web-preview/js/data.js', 'utf8');

const newBooksStr = crawled.map(b => `  ${b.id}: {
    id: '${b.id}',
    title: ${JSON.stringify(b.title)},
    romaji: ${JSON.stringify(b.romaji)},
    author: ${JSON.stringify(b.author)},
    level: '${b.level}',
    genre: '${b.genre}',
    cover: '${b.cover}',
    synopsis: ${JSON.stringify(b.synopsis)},
    chapters: '${b.chapters}',
    words: '${b.words}',
    vocab: '${b.vocab}',
    progress: 'Chưa đọc'
  }`).join(',\n');

// 1. Insert into booksData
const targetPos = dataJs.indexOf('  }\n};');
if (targetPos !== -1) {
  dataJs = dataJs.substring(0, targetPos + 3) + ',\n' + newBooksStr + '\n' + dataJs.substring(targetPos + 4);
} else {
  console.error('Could not find end of booksData');
}

// 2. Add crawledChapterProse map
const proseMap = {};
crawled.forEach(b => {
  proseMap[b.id] = {
    title: b.chapterTitle,
    content: b.chapterContent
  };
});

const proseStr = `\n// Real Authentic Japanese Chapter Texts Crawled from Syosetu\nconst crawledChapterProse = ${JSON.stringify(proseMap, null, 2)};\n`;
dataJs = dataJs + proseStr;

fs.writeFileSync('web-preview/js/data.js', dataJs, 'utf8');
console.log('Successfully injected 5 crawled Light Novels into web-preview/js/data.js!');

