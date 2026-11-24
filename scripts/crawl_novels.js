const https = require('https');
const fs = require('fs');

function fetchUrl(url) {
  return new Promise((resolve, reject) => {
    https.get(url, {
      headers: {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
        'Accept-Language': 'ja,en-US;q=0.9,en;q=0.8'
      }
    }, (res) => {
      if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
        return fetchUrl(res.headers.location).then(resolve).catch(reject);
      }
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => resolve(data));
    }).on('error', reject);
  });
}

function parseSyosetu(html, url) {
  // Title
  let title = '';
  // 1. New Syosetu layout:
  const titleA = html.match(/<a[^>]*class="[^"]*p-novel__series-link[^"]*"[^>]*>([\s\S]*?)<\/a>/i) ||
                 html.match(/<a[^>]*class="[^"]*p-novel__title-link[^"]*"[^>]*>([\s\S]*?)<\/a>/i) ||
                 html.match(/<div class="contents1">[\s\S]*?<a[^>]*href="[^"]*"[^>]*>([\s\S]*?)<\/a>/i);
  if (titleA) {
    title = titleA[1].replace(/<[^>]+>/g, '').trim();
  }
  if (!title) {
    const headTitle = html.match(/<title>([\s\S]*?)<\/title>/i);
    if (headTitle) {
      title = headTitle[1].replace(/<[^>]+>/g, '').split(' - ')[0].trim();
    }
  }

  // Author
  let author = 'Syosetu Author';
  const authorMatch = html.match(/class="[^"]*p-novel__author[^"]*"[^>]*>[\s\S]*?(?:作者：|<a[^>]*>)([\s\S]*?)(?:<\/a>|<\/div>)/i) ||
                      html.match(/<div class="novel_writername">[\s\S]*?(?:作者：|<a[^>]*>)([\s\S]*?)(?:<\/a>|<\/div>)/i);
  if (authorMatch) {
    author = authorMatch[1].replace(/<[^>]+>/g, '').trim();
  }

  // Chapter subtitle
  let chapTitle = 'Chương 1';
  const subMatch = html.match(/<h1[^>]*class="[^"]*p-novel__title[^"]*"[^>]*>([\s\S]*?)<\/h1>/i) ||
                   html.match(/<p class="novel_subtitle">([\s\S]*?)<\/p>/i);
  if (subMatch) {
    chapTitle = subMatch[1].replace(/<[^>]+>/g, '').trim();
  }

  // Content body
  let body = '';
  const bodyMatch = html.match(/<div[^>]*class="[^"]*(?:p-novel__text|novel_view)[^"]*"[^>]*>([\s\S]*?)<\/div>/i) ||
                    html.match(/<div id="novel_honbun"[^>]*>([\s\S]*?)<\/div>/i);
  if (bodyMatch) {
    let raw = bodyMatch[1];
    raw = raw.replace(/<p id="L\d+">([\s\S]*?)<\/p>/gi, '$1\n');
    raw = raw.replace(/<ruby>(.*?)<rt>(.*?)<\/rt><\/ruby>/gi, '$1($2)');
    raw = raw.replace(/<br\s*\/?>/gi, '\n');
    raw = raw.replace(/<[^>]+>/g, '');
    raw = raw.replace(/&nbsp;/g, ' ');
    raw = raw.replace(/&lt;/g, '<');
    raw = raw.replace(/&gt;/g, '>');
    raw = raw.replace(/&amp;/g, '&');
    body = raw.split('\n').map(l => l.trim()).filter(l => l.length > 0).join('\n\n');
  }

  return { title, author, chapTitle, body };
}

async function run() {
  console.log('Testing crawl from Syosetu with updated modern parser...');
  const testUrls = [
    { key: 'rezero', url: 'https://ncode.syosetu.com/n2267be/1/', author: '長月達平 (Nagatsuki Tappei)', genre: 'Isekai & Dark Fantasy', level: 'N2', romaji: 'Re:Zero kara Hajimeru Isekai Seikatsu', cover: 'assets/covers/cover_re_zero.jpg' },
    { key: 'slime', url: 'https://ncode.syosetu.com/n6316bn/1/', author: '伏瀬 (Fuse)', genre: 'Isekai & Fantasy', level: 'N3', romaji: 'Tensei shitara Slime Datta Ken', cover: 'assets/covers/cover_slime.jpg' },
    { key: 'honzuki', url: 'https://ncode.syosetu.com/n4830bu/1/', author: '香月美夜 (Kazuki Miya)', genre: 'Slice of Life & Fantasy', level: 'N3', romaji: 'Honzuki no Gekokujou', cover: 'assets/covers/cover_anime_cafe.jpg' },
    { key: 'kumabear', url: 'https://ncode.syosetu.com/n4185ci/1/', author: 'くまなの (Kumanano)', genre: 'Isekai & Slice of Life', level: 'N4', romaji: 'Kuma Kuma Kuma Bear', cover: 'assets/covers/cover_anime_forest.jpg' }
  ];

  const results = [];

  for (const item of testUrls) {
    try {
      console.log(`Fetching ${item.url}...`);
      const html = await fetchUrl(item.url);
      const parsed = parseSyosetu(html, item.url);
      console.log('-> Title:', parsed.title);
      console.log('-> Chapter:', parsed.chapTitle);
      console.log('-> Body length:', parsed.body.length, 'chars');
      console.log('-> First 100 chars:', parsed.body.substring(0, 100));
      console.log('---');
      results.push({
        ...item,
        title: parsed.title,
        chapterTitle: parsed.chapTitle,
        content: parsed.body,
        author: parsed.author !== 'Syosetu Author' ? parsed.author : item.author
      });
    } catch (e) {
      console.error('Error fetching', item.url, e.message);
    }
  }

  fs.writeFileSync('scripts/crawled_novels.json', JSON.stringify(results, null, 2), 'utf8');
  console.log('Saved', results.length, 'crawled novels to scripts/crawled_novels.json');
}

run();
