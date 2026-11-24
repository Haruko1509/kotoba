const https = require('https');

https.get('https://kakuyomu.jp/works/1177354054880238350', {
  headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36' }
}, res => {
  let data = '';
  res.on('data', chunk => data += chunk);
  res.on('end', () => {
    console.log('Kakuyomu work status:', res.statusCode, 'len:', data.length);
    const titleMatch = data.match(/<h1[^>]*>([\s\S]*?)<\/h1>/i);
    console.log('Kakuyomu Title:', titleMatch ? titleMatch[1].replace(/<[^>]+>/g, '').trim() : 'none');
    const firstEp = data.match(/href="(\/works\/[^\/]+\/episodes\/[^\"]+)"/i);
    console.log('First episode:', firstEp ? firstEp[1] : 'none');
  });
});

