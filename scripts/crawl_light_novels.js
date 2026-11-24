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
  let title = '';
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

  let chapTitle = 'Chương 1';
  const subMatch = html.match(/<h1[^>]*class="[^"]*p-novel__title[^"]*"[^>]*>([\s\S]*?)<\/h1>/i) ||
                   html.match(/<p class="novel_subtitle">([\s\S]*?)<\/p>/i);
  if (subMatch) {
    chapTitle = subMatch[1].replace(/<[^>]+>/g, '').trim();
  }

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

  return { title, chapTitle, body };
}

async function run() {
  const targets = [
    {
      id: 'mushoku',
      url: 'https://ncode.syosetu.com/n9669bk/1/',
      author: '理不尽な孫の手 (Rifujin na Magonote)',
      romaji: 'Mushoku Tensei - Isekai Ittara Honki Dasu',
      level: 'N3',
      genre: 'Isekai & Fantasy',
      cover: 'assets/covers/cover_anime_adventurer.jpg',
      synopsis: 'Đỉnh cao của thể loại Isekai chuyển sinh. Một người đàn ông 34 tuổi thất nghiệp tái sinh sang thế giới kiếm và ma pháp dưới danh nghĩa Rudeus Greyrat, quyết tâm sống hết mình và không bao giờ hối tiếc.'
    },
    {
      id: 'rezero',
      url: 'https://ncode.syosetu.com/n2267be/1/',
      author: '長月達平 (Nagatsuki Tappei)',
      romaji: 'Re:Zero kara Hajimeru Isekai Seikatsu',
      level: 'N2',
      genre: 'Isekai & Dark Fantasy',
      cover: 'assets/covers/cover_re_zero.jpg',
      synopsis: 'Natsuki Subaru bất ngờ bị triệu hồi tới thế giới khác mà không hề có siêu năng lực đặc biệt, ngoại trừ năng lực duy nhất: "Trở về từ cõi chết" mỗi khi tử nạn để cứu lấy những người thân yêu.'
    },
    {
      id: 'slime',
      url: 'https://ncode.syosetu.com/n6316bn/1/',
      author: '伏瀬 (Fuse)',
      romaji: 'Tensei shitara Slime Datta Ken',
      level: 'N3',
      genre: 'Isekai & Fantasy',
      cover: 'assets/covers/cover_slime.jpg',
      synopsis: 'Mikami Satoru sau khi bị đâm chết trên đường phố Tokyo đã tái sinh thành một con Slime tại hang động thế giới khác, bắt đầu hành trình xây dựng đại liên minh ma vật Jura Tempest.'
    },
    {
      id: 'honzuki',
      url: 'https://ncode.syosetu.com/n4830bu/1/',
      author: '香月美夜 (Kazuki Miya)',
      romaji: 'Honzuki no Gekokujou (Ascendance of a Bookworm)',
      level: 'N3',
      genre: 'Slice of Life & Fantasy',
      cover: 'assets/covers/cover_anime_cafe.jpg',
      synopsis: 'Motosu Urano, một cô gái mê sách cuồng nhiệt, sau cơn động đất đã tái sinh thành cô bé Myne yếu ớt tại một thế giới trung cổ nơi sách vở chỉ dành cho giới quý tộc. Không có sách, cô quyết tâm tự mình làm ra sách!'
    },
    {
      id: 'kumabear',
      url: 'https://ncode.syosetu.com/n4185ci/2/',
      author: 'くまなの (Kumanano)',
      romaji: 'Kuma Kuma Kuma Bear',
      level: 'N4',
      genre: 'Isekai & Slice of Life',
      cover: 'assets/covers/cover_anime_forest.jpg',
      synopsis: 'Yuna - một nữ game thủ VRMMO 15 tuổi được dịch chuyển sang thế giới khác cùng bộ trang bị gấu siêu cấp dễ thương và vô địch, mở ra cuộc sống phiêu lưu thư giãn tự do.'
    }
  ];

  const novels = [];

  for (const item of targets) {
    try {
      console.log(`Crawling: ${item.romaji} (${item.url})...`);
      const html = await fetchUrl(item.url);
      const parsed = parseSyosetu(html, item.url);
      console.log(`✓ Fetched: ${parsed.title} | ${parsed.chapTitle} | ${parsed.body.length} ký tự Nhật`);

      novels.push({
        id: item.id,
        title: parsed.title || item.romaji,
        romaji: item.romaji,
        author: item.author,
        level: item.level,
        genre: item.genre,
        cover: item.cover,
        synopsis: item.synopsis,
        chapters: '1 Chương (Mở đầu)',
        words: `${Math.round(parsed.body.length * 0.4)} Chữ Hán`,
        vocab: `${Math.round(parsed.body.length * 0.03)} Từ JLPT`,
        progress: 'Chưa đọc',
        chapterTitle: parsed.chapTitle,
        chapterContent: parsed.body
      });
    } catch (e) {
      console.error(`Error crawling ${item.id}:`, e.message);
    }
  }

  fs.writeFileSync('scripts/crawled_light_novels.json', JSON.stringify(novels, null, 2), 'utf8');
  console.log(`\n🎉 Đã hoàn tất cào ${novels.length} bộ Light Novel từ Syosetu!`);
}

run();

