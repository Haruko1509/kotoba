const https = require('https');

https.get('https://ncode.syosetu.com/n2267be/1/', {
  headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36' }
}, res => {
  let data = '';
  res.on('data', chunk => data += chunk);
  res.on('end', () => {
    console.log('Total HTML length:', data.length);
    const m = data.match(/<div[^>]*id="novel_honbun"[^>]*>([\s\S]*?)<\/div>/i);
    if (m) {
      console.log('Found novel_honbun:', m[1].substring(0, 300));
    } else {
      console.log('Not matched id=novel_honbun. Searching for novel_view or js-novel-text:');
      const tags = data.match(/<div[^>]*class="[^"]*novel[^"]*"[^>]*>/gi);
      console.log('Matching div tags:', tags);
      const pL1 = data.indexOf('id="L1"');
      console.log('Index of L1:', pL1);
      if (pL1 !== -1) {
        console.log('Snippet around L1:', data.substring(pL1 - 100, pL1 + 200));
      }
    }
  });
});

