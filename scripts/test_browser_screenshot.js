const http = require('http');
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const baseDir = path.resolve('web-preview');
const server = http.createServer((req, res) => {
  let reqPath = req.url.split('?')[0];
  if (reqPath === '/') reqPath = '/index.html';
  const filePath = path.join(baseDir, reqPath);
  if (fs.existsSync(filePath) && fs.statSync(filePath).isFile()) {
    let contentType = 'text/plain';
    if (filePath.endsWith('.html')) contentType = 'text/html; charset=utf-8';
    if (filePath.endsWith('.js')) contentType = 'application/javascript; charset=utf-8';
    if (filePath.endsWith('.css')) contentType = 'text/css; charset=utf-8';
    if (filePath.endsWith('.png')) contentType = 'image/png';
    if (filePath.endsWith('.jpg') || filePath.endsWith('.jpeg')) contentType = 'image/jpeg';
    if (filePath.endsWith('.svg')) contentType = 'image/svg+xml';
    res.writeHead(200, { 'Content-Type': contentType });
    fs.createReadStream(filePath).pipe(res);
  } else {
    res.writeHead(404);
    res.end('Not found: ' + reqPath);
  }
});

server.listen(8099, () => {
  console.log('[PROBE] Local HTTP server serving web-preview on port 8099');
  const edgePath = 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe';
  const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
  const outPng = path.resolve('web-preview/test_browser_capture.png');
  const tmpProfile = path.resolve('web-preview/temp_browser_profile');

  try {
    const cmd = `"${edgePath}" --headless --disable-gpu --no-sandbox --no-first-run --disable-background-networking --disable-component-update --user-data-dir="${tmpProfile}" --window-size=1280,800 --screenshot="${outPng}" http://127.0.0.1:8099/#vn/tebukuro`;
    console.log('[BROWSER] Executing:', cmd);
    execSync(cmd, { timeout: 12000, stdio: 'inherit' });
    if (fs.existsSync(outPng)) {
      console.log('[BROWSER] SUCCESS: Real Browser screenshot created! Path:', outPng, 'Size:', fs.statSync(outPng).size, 'bytes');
    } else {
      console.log('[BROWSER] FAIL: Screenshot file not found');
    }
  } catch (err) {
    console.error('[BROWSER] Error:', err.message);
  } finally {
    server.close();
  }
});
