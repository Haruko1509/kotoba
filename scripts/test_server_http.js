const http = require('http');
const fs = require('fs');
const path = require('path');

const baseDir = path.resolve('web-preview');
const server = http.createServer((req, res) => {
  let reqPath = req.url.split('?')[0];
  if (reqPath === '/') reqPath = '/index.html';
  const filePath = path.join(baseDir, reqPath);
  if (fs.existsSync(filePath) && fs.statSync(filePath).isFile()) {
    let contentType = 'text/plain';
    if (filePath.endsWith('.html')) contentType = 'text/html';
    if (filePath.endsWith('.js')) contentType = 'application/javascript';
    if (filePath.endsWith('.css')) contentType = 'text/css';
    res.writeHead(200, { 'Content-Type': contentType });
    fs.createReadStream(filePath).pipe(res);
  } else {
    res.writeHead(404);
    res.end('Not found: ' + reqPath);
  }
});

server.listen(8099, () => {
  console.log('[PROBE] Local HTTP server started on port 8099 (In-session)');
  
  // Test 1: Fetch index.html
  http.get('http://127.0.0.1:8099/', (res) => {
    console.log('[TEST 1] HTTP GET / -> Status:', res.statusCode, res.headers['content-type']);
    let body = '';
    res.on('data', d => body += d);
    res.on('end', () => {
      console.log('         Length:', body.length, 'bytes');
      console.log('         Contains Kotoba Odyssey:', body.includes('Kotoba Odyssey'));
      console.log('         Contains vnStage element:', body.includes('id="vnStage"'));
      console.log('         Contains vnChoiceOverlay:', body.includes('id="vnChoiceOverlay"'));
      console.log('         Contains visual-novel.js script tag:', body.includes('js/visual-novel.js'));

      // Test 2: Fetch js/visual-novel.js
      http.get('http://127.0.0.1:8099/js/visual-novel.js', (resJs) => {
        console.log('[TEST 2] HTTP GET /js/visual-novel.js -> Status:', resJs.statusCode, resJs.headers['content-type']);
        let jsBody = '';
        resJs.on('data', d => jsBody += d);
        resJs.on('end', () => {
          console.log('         Length:', jsBody.length, 'bytes');
          console.log('         Contains novelCharacterRoster:', jsBody.includes('novelCharacterRoster'));
          console.log('         Contains buildNovelBranchingChoices:', jsBody.includes('buildNovelBranchingChoices'));
          console.log('         Contains selectVnChoice:', jsBody.includes('selectVnChoice'));
          server.close();
        });
      });
    });
  }).on('error', (err) => {
    console.error('Error during HTTP request:', err.message);
    server.close();
  });
});

