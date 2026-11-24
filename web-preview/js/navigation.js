// ===================================================================
// KOTOBA ODYSSEY - NAVIGATION & NOVEL CATALOG CONTROLLER
// ===================================================================

let currentInspectedId = 'ginga';
let currentNovelDbId = 1;
let currentChapterNum = 1;
let currentTotalChapters = 1;
const cachedNovelChapters = {};

let currentReaderFontSize = 19;
let currentReaderFontType = 'mincho'; // 'mincho' or 'gothic'
let currentExploreViewMode = 'grid'; // 'grid' or 'list'
let selectedExploreJlpt = 'all';

// ================= 1. CORE PANE SWITCHER =================
function switchPane(paneId) {
  // Alias mappings for backward compatibility
  if (paneId === 'srs') paneId = 'learning';
  if (paneId === 'library') paneId = 'explore';

  // Update navbar links
  document.querySelectorAll('.ln-nav-link, .mobile-nav-item, .nav-item').forEach(el => {
    if (el.dataset.pane === paneId) {
      el.classList.add('active');
    } else {
      el.classList.remove('active');
    }
  });

  // Hide all panes & activate target
  document.querySelectorAll('.pane').forEach(el => {
    el.classList.remove('active');
  });

  const targetPane = document.getElementById(`pane-${paneId}`);
  if (targetPane) {
    targetPane.classList.add('active');
  }

  // Update Breadcrumb & sub-bar
  const crumbCurrent = document.getElementById('crumbCurrent');
  const titles = {
    home: 'Trang chủ',
    explore: 'Khám phá thư viện Light Novel',
    detail: 'Chi tiết tác phẩm',
    reader: 'Không gian đọc Light Novel',
    learning: 'Khu vực học tập & Flashcards SRS',
    vn: 'Visual Novel Theatre & AI Character Chat',
    profile: 'Tủ sách cá nhân & Cài đặt'
  };

  if (crumbCurrent) {
    crumbCurrent.textContent = titles[paneId] || 'Trang chủ';
  }

  // Handle specific pane hooks
  if (paneId === 'explore') {
    renderExploreCatalog();
  } else if (paneId === 'vn') {
    if (typeof updateVnStage === 'function') updateVnStage();
  }

  // Scroll viewport to top
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function openLibraryOrReader() {
  switchPane('explore');
}

function showBookshelfView() {
  switchPane('explore');
}

function showReaderView() {
  switchPane('reader');
}

// ================= 2. BOOK DETAIL VIEW CONTROLLER =================
function openBookDetail(bookId) {
  const book = booksData[bookId] || booksData.ginga;
  currentInspectedId = bookId;
  currentNovelDbId = book.dbId || 1;

  const coverEl = document.getElementById('detailCoverImg');
  const backdropEl = document.getElementById('detailBackdrop');
  const titleJp = document.getElementById('detailJpTitle');
  const titleRomaji = document.getElementById('detailRomajiTitle');
  const authorEl = document.getElementById('detailAuthor');
  const jlptEl = document.getElementById('detailJlpt');
  const genreEl = document.getElementById('detailGenre');
  const chaptersCountEl = document.getElementById('detailChaptersCount');
  const wordCountEl = document.getElementById('detailWordCount');
  const totalChaptersEl = document.getElementById('detailTotalChapters');
  const synopsisEl = document.getElementById('detailSynopsis');
  const progressEl = document.getElementById('detailProgress');
  const btnRead = document.getElementById('btnDetailRead');

  if (coverEl) coverEl.src = book.cover;
  if (backdropEl) backdropEl.style.backgroundImage = `url('${book.cover}')`;
  if (titleJp) titleJp.textContent = book.title;
  if (titleRomaji) titleRomaji.textContent = book.romaji ? `(${book.romaji})` : '';
  if (authorEl) authorEl.textContent = `Tác giả: ${book.author}`;
  if (jlptEl) {
    jlptEl.textContent = `JLPT ${book.level}`;
    jlptEl.className = `chip ${book.level.toLowerCase()}`;
  }
  if (genreEl) genreEl.textContent = book.genre;
  if (chaptersCountEl) chaptersCountEl.textContent = book.chapters || '1 Chương';
  if (wordCountEl) wordCountEl.textContent = (book.words || '1,000').replace(/[^0-9,]/g, '');
  if (totalChaptersEl) totalChaptersEl.textContent = (book.chapters || '1').replace(/[^0-9]/g, '');
  if (synopsisEl) synopsisEl.textContent = book.synopsis;
  if (progressEl) progressEl.textContent = book.progress || 'Chưa đọc';

  if (btnRead) {
    btnRead.onclick = () => openBook(bookId, 1);
  }

  const btnDetailVn = document.getElementById('btnDetailVn');
  if (btnDetailVn) {
    btnDetailVn.onclick = () => openVN(bookId);
  }

  // Render Chapter List
  renderDetailChapterList(book);

  // Render Vocab Preview
  renderDetailVocabPreview(book);

  // Update URL hash
  if (window.location.hash !== `#detail/${bookId}`) {
    history.replaceState(null, '', `#detail/${bookId}`);
  }

  switchPane('detail');
}

function renderDetailChapterList(book) {
  const container = document.getElementById('detailChapterList');
  if (!container) return;

  const chaps = (typeof getNovelChapters === 'function') 
    ? getNovelChapters(book.id) 
    : [];
  const total = chaps.length || (parseInt((book.chapters || '25').replace(/[^0-9]/g, '')) || 25);
  let html = '';

  for (let ch = 1; ch <= total; ch++) {
    const isCompleted = ch === 1 && book.id === 'ginga';
    const statusText = isCompleted ? '✓ Đã đọc' : (ch === 1 ? 'Đang đọc' : 'Chưa đọc');
    const statusClass = isCompleted ? 'done' : (ch === 1 ? 'reading' : 'unread');

    const chapData = chaps[ch - 1];
    const chapTitle = chapData ? chapData.title : `Chương ${ch}: Tiếp diễn câu chuyện`;

    html += `
      <div class="chapter-row-item" onclick="openBook('${book.id}', ${ch})">
        <div class="chap-left">
          <span class="chap-num-badge">Chương ${ch}</span>
          <div class="chap-info-meta">
            <span class="chap-row-title">${escapeHtml(chapTitle)}</span>
            <span class="chap-row-stats">Khoảng 550 chữ Hán • Đầy đủ Furigana &amp; Cú pháp AI</span>
          </div>
        </div>
        <div class="chap-right">
          <span class="chap-status-pill ${statusClass}">${statusText}</span>
          <button class="chap-read-action-btn">Đọc ➔</button>
        </div>
      </div>
    `;
  }

  container.innerHTML = html;
}

function renderDetailVocabPreview(book) {
  const container = document.getElementById('detailVocabPreview');
  if (!container) return;

  const sampleVocab = [
    { kanji: '銀河', reading: 'ぎんが', meaning: 'Dải Ngân Hà', level: 'N2' },
    { kanji: '星座', reading: 'せいざ', meaning: 'Chòm sao', level: 'N2' },
    { kanji: '切符', reading: 'きっぷ', meaning: 'Tấm vé', level: 'N5' },
    { kanji: '黒板', reading: 'こくばん', meaning: 'Bảng đen', level: 'N5' },
    { kanji: '承知', reading: 'しょうち', meaning: 'Hiểu rõ, nhận thức', level: 'N3' }
  ];

  container.innerHTML = sampleVocab.map(v => `
    <div class="vocab-preview-pill" onclick="speakWordText('${v.kanji}', '${v.reading}')" title="Nghe phát âm: ${v.reading}">
      <span class="vp-kanji">${v.kanji}</span>
      <span class="vp-reading">(${v.reading})</span>
      <span class="vp-meaning">${v.meaning}</span>
      <span class="chip ${v.level.toLowerCase()}">${v.level}</span>
      <span class="vp-audio">🔊</span>
    </div>
  `).join('');
}

function openCurrentInspectedBook() {
  openBook(currentInspectedId, 1);
}

function openVNFromCurrentBook() {
  openVN(currentInspectedId || currentVnBookKey || 'ginga');
}

function toggleBookmarkCurrentBook() {
  const btn = document.getElementById('btnDetailBookmark');
  if (btn) {
    if (btn.classList.contains('bookmarked')) {
      btn.classList.remove('bookmarked');
      btn.innerHTML = '<span>🔖 Thêm vào Tủ sách</span>';
      showToast('Đã bỏ lưu tác phẩm khỏi Tủ sách cá nhân');
    } else {
      btn.classList.add('bookmarked');
      btn.innerHTML = '<span>★ Đã lưu vào Tủ sách</span>';
      showToast('Đã thêm tác phẩm vào Tủ sách yêu thích của bạn! 📚');
    }
  }
}

// ================= 3. READER ENGINE & WORKSPACE =================
async function openBook(bookId, targetChapter = 1) {
  const book = booksData[bookId] || booksData.ginga;
  
  currentInspectedId = bookId;
  currentNovelDbId = book.dbId || 1;
  currentChapterNum = targetChapter || 1;

  // Header Elements
  const bTitle = document.getElementById('readerBookTitle');
  const pHeading = document.getElementById('proseHeading');
  const chapterSelect = document.getElementById('readerChapterSelect');
  const progressBottom = document.getElementById('chapterProgressBottom');
  const prevBtn = document.getElementById('btnPrevChapterBottom');
  const nextBtn = document.getElementById('btnNextChapterBottom');
  const subBarCurrentBook = document.getElementById('subBarCurrentBook');

  const romajiLabel = book.romaji ? `(${book.romaji})` : '';
  if (bTitle) {
    bTitle.innerHTML = `${escapeHtml(book.title)} <span class="reader-title-romaji">${escapeHtml(romajiLabel)}</span>`;
  }
  if (pHeading) pHeading.textContent = `第${currentChapterNum}章　${book.title}`;
  if (subBarCurrentBook) subBarCurrentBook.textContent = book.title;

  const rSub = document.getElementById('readingSub');
  if (rSub) {
    rSub.textContent = `${book.author || 'Tác giả'} • Cấp độ ${book.level || 'N3'} • Chạm vào từng từ để tra cứu hoặc bấm nút bên phải để phân tích cú pháp AI`;
  }

  // Update URL hash
  if (window.location.hash !== `#read/${currentNovelDbId}/${currentChapterNum}`) {
    history.replaceState(null, '', `#read/${currentNovelDbId}/${currentChapterNum}`);
  }

  // Load chapters for dropdown
  try {
    if (!cachedNovelChapters[currentNovelDbId]) {
      const novelData = await fetchFromBackend(`/novels/${currentNovelDbId}`);
      if (novelData && novelData.chapters) {
        cachedNovelChapters[currentNovelDbId] = novelData.chapters;
      }
    }

    const backendChaps = cachedNovelChapters[currentNovelDbId] || [];
    const localChaps = (typeof getNovelChapters === 'function') ? getNovelChapters(bookId || book.id) : [];
    const chaps = (backendChaps.length > 0) ? backendChaps : localChaps;
    currentTotalChapters = chaps.length || 25;

    if (chapterSelect) {
      if (chaps.length > 0) {
        chapterSelect.innerHTML = chaps.map(c => 
          `<option value="${c.chapterNumber}" ${c.chapterNumber === currentChapterNum ? 'selected' : ''}>Chương ${c.chapterNumber}: ${escapeHtml(c.title || 'Chương ' + c.chapterNumber)}</option>`
        ).join('');
      } else {
        chapterSelect.innerHTML = `<option value="1" selected>Chương 1: Mở đầu</option>`;
      }
      chapterSelect.value = currentChapterNum;
    }

    if (progressBottom) {
      progressBottom.textContent = `Chương ${currentChapterNum} / ${currentTotalChapters}`;
    }

    if (prevBtn) prevBtn.disabled = currentChapterNum <= 1;
    if (nextBtn) nextBtn.disabled = currentChapterNum >= currentTotalChapters;
  } catch (err) {
    console.warn('Could not load chapters list', err);
  }

  // Fetch real chapter text from SQLite backend or Canonical 25-Chapter Registry
  try {
    const chapterData = await fetchFromBackend(`/novels/${currentNovelDbId}/chapters/${currentChapterNum}`);
    if (chapterData && chapterData.content) {
      if (pHeading) pHeading.textContent = chapterData.title || `第${currentChapterNum}章`;
      if (typeof window.renderReaderProseContent === 'function') {
        window.renderReaderProseContent(chapterData.content);
      }
    } else {
      const localChaps = (typeof getNovelChapters === 'function') ? getNovelChapters(bookId || book.id) : [];
      const chapEntry = localChaps[currentChapterNum - 1];
      const crawledEntry = (typeof crawledChapterProse !== 'undefined' && currentChapterNum === 1 && (crawledChapterProse[bookId] || (book && crawledChapterProse[book.id])));
      
      const targetTitle = (crawledEntry && crawledEntry.title) || (chapEntry && chapEntry.title) || `第${currentChapterNum}章 ${book.title}`;
      const targetContent = (crawledEntry && crawledEntry.content) || (chapEntry && chapEntry.content);

      if (pHeading) pHeading.textContent = targetTitle;
      if (typeof window.renderReaderProseContent === 'function') {
        window.renderReaderProseContent(targetContent || `第${currentChapterNum}章の本文。\n物語の情景が目に浮かぶように広がっていく。\n「言葉の旅」は、確かな学びとなって読者の心に残っていく。`);
      }
    }
  } catch (err) {
    console.warn('Could not load chapter text from API, using canonical chapter data', err);
    const localChaps = (typeof getNovelChapters === 'function') ? getNovelChapters(bookId || book.id) : [];
    const chapEntry = localChaps[currentChapterNum - 1];
    const targetTitle = (chapEntry && chapEntry.title) || `第${currentChapterNum}章 ${book.title}`;
    const targetContent = (chapEntry && chapEntry.content) || `第${currentChapterNum}章の本文。\n物語の情景が目に浮かぶように広がっていく。`;
    if (pHeading) pHeading.textContent = targetTitle;
    if (typeof window.renderReaderProseContent === 'function') {
      window.renderReaderProseContent(targetContent);
    }
  }

  showReaderView();
}

function changeReaderChapter(newChapNum) {
  const num = parseInt(newChapNum) || 1;
  openBook(currentInspectedId, num);
  const canvas = document.querySelector('.reader-canvas-scroll');
  if (canvas) canvas.scrollTop = 0;
}

function goToNextChapter() {
  if (currentChapterNum < currentTotalChapters) {
    changeReaderChapter(currentChapterNum + 1);
  } else {
    showToast('Bạn đang ở chương cuối cùng của tác phẩm.');
  }
}

function goToPrevChapter() {
  if (currentChapterNum > 1) {
    changeReaderChapter(currentChapterNum - 1);
  }
}

// ================= 4. JAPANESE TYPOGRAPHY & READER CONTROLS =================
function toggleReaderFont() {
  const prose = document.getElementById('readingProse');
  const label = document.getElementById('readerFontLabel');
  if (!prose) return;

  if (currentReaderFontType === 'mincho') {
    currentReaderFontType = 'gothic';
    prose.classList.remove('font-mincho');
    prose.classList.add('font-gothic');
    if (label) label.textContent = 'Gothic';
    showToast('Đã đổi sang phông chữ Gothic (Sans)');
  } else {
    currentReaderFontType = 'mincho';
    prose.classList.remove('font-gothic');
    prose.classList.add('font-mincho');
    if (label) label.textContent = 'Mincho';
    showToast('Đã đổi sang phông chữ Mincho (Serif)');
  }
}

function adjustReaderFontSize(delta) {
  const prose = document.getElementById('readingProse');
  const display = document.getElementById('readerFontSizeDisplay');
  currentReaderFontSize = Math.min(28, Math.max(15, currentReaderFontSize + delta));

  if (prose) {
    prose.style.fontSize = `${currentReaderFontSize}px`;
    prose.style.lineHeight = '2.3';
  }
  if (display) {
    display.textContent = `${currentReaderFontSize}px`;
  }
}

function toggleReaderFocusMode() {
  const shell = document.getElementById('readerShell');
  const btn = document.getElementById('btnReaderFocus');
  if (!shell) return;

  shell.classList.toggle('focus-mode');
  const isFocus = shell.classList.contains('focus-mode');
  if (btn) {
    btn.textContent = isFocus ? '✕ Thoát tập trung' : '⛶ Tập trung';
  }
  showToast(isFocus ? 'Đã bật chế độ tập trung (Zen Mode)' : 'Đã thoát chế độ tập trung');
}

// ================= 5. HOME EXPLORATION & JLPT FILTERING =================
const featuredHeroData = {
  ginga: {
    title: '銀河鉄道の夜',
    romaji: 'Night on the Galactic Railroad • 宮沢賢治 (Miyazawa Kenji)',
    cover: 'assets/cover_ginga.jpg',
    level: 'JLPT N3',
    levelClass: 'n3',
    genre: 'Cosmic Fantasy',
    status: 'Hoàn thành',
    synopsis: 'Kiệt tác kỳ ảo kinh điển bất hủ của văn học Nhật Bản. Cùng cậu bé Giovanni bước lên chuyến tàu đêm lấp lánh xuyên qua dòng sông sao Ngân Hà, gặp gỡ những linh hồn du hành và tìm kiếm câu trả lời cho niềm hạnh phúc đích thực.',
    stats: '<span>📚 2 Chương truyện</span> • <span>✍️ 770 Chữ Hán JLPT</span> • <span>🏷️ Văn học &amp; Kỳ ảo</span>'
  },
  botchan: {
    title: '坊っちゃん',
    romaji: 'Botchan • 夏目漱石 (Natsume Soseki)',
    cover: 'assets/cover_botchan.jpg',
    level: 'JLPT N4',
    levelClass: 'n4',
    genre: 'Slice of Life',
    status: 'Hoàn thành',
    synopsis: 'Một câu chuyện hài hước, châm biếm sâu cay và tràn đầy tinh thần chính nghĩa của một chàng trai trẻ Tokyo chuyển về dạy học ở vùng quê Shikoku xa xôi. Tác phẩm kinh điển tuyệt vời để bắt đầu đọc tiếng Nhật sơ trung cấp.',
    stats: '<span>📚 1 Chương truyện</span> • <span>✍️ 450 Chữ Hán JLPT</span> • <span>🏷️ Hài hước &amp; Học đường</span>'
  },
  kokoro: {
    title: 'こころ',
    romaji: 'Kokoro (Trái Tim) • 夏目漱石 (Natsume Soseki)',
    cover: 'assets/cover_kokoro.jpg',
    level: 'JLPT N2',
    levelClass: 'n2',
    genre: 'Psychological',
    status: 'Hoàn thành',
    synopsis: 'Tiểu thuyết tâm lý đỉnh cao của Natsume Soseki khai thác sâu sắc sự cô đơn, mặc cảm tội lỗi và những biến động nội tâm của con người thời kỳ chuyển giao hiện đại hóa Minh Trị. Thích hợp cho người học N2 rèn luyện đọc hiểu tinh tế.',
    stats: '<span>📚 1 Chương truyện</span> • <span>✍️ 920 Chữ Hán JLPT</span> • <span>🏷️ Tâm lý &amp; Triết học</span>'
  },
  merosu: {
    title: '走れメロス',
    romaji: 'Run, Melos! • 太宰治 (Dazai Osamu)',
    cover: 'assets/covers/cover_anime_adventurer.jpg',
    level: 'JLPT N3',
    levelClass: 'n3',
    genre: 'Action / Adventure',
    status: 'Hoàn thành',
    synopsis: 'Khúc ca bất hủ về niềm tin, lòng trung nghĩa và tình bạn vượt lên trên nỗi sợ hãi cái chết. Melos chạy đua với mặt trời lặn trên con đường gập ghềnh trở lại pháp trường để cứu người bạn tri kỷ Selinuntius khỏi bạo chúa Dionysius.',
    stats: '<span>📚 1 Chương truyện</span> • <span>✍️ 680 Chữ Hán JLPT</span> • <span>🏷️ Hành động &amp; Tình bạn</span>'
  }
};

function setFeaturedHero(key, btn) {
  const data = featuredHeroData[key];
  if (!data) return;

  document.querySelectorAll('.hero-switch-btn').forEach(b => b.classList.remove('active'));
  if (btn) btn.classList.add('active');

  const card = document.getElementById('homeHeroCard');
  if (card) {
    card.style.opacity = '0.7';
    setTimeout(() => { card.style.opacity = '1'; }, 150);
  }

  const coverEl = document.getElementById('heroCoverImg');
  const backdropEl = document.getElementById('heroBackdrop');
  const titleEl = document.getElementById('heroTitle');
  const romajiEl = document.getElementById('heroRomaji');
  const synopsisEl = document.getElementById('heroSynopsis');
  const jlptEl = document.getElementById('heroJlptBadge');
  const genreEl = document.getElementById('heroGenreBadge');
  const statsEl = document.getElementById('heroStatsRow');
  const btnRead = document.getElementById('btnHeroRead');
  const btnDetail = document.getElementById('btnHeroDetail');
  const btnVN = document.getElementById('btnHeroVN');

  if (coverEl) coverEl.src = data.cover;
  if (backdropEl) backdropEl.style.backgroundImage = `url('${data.cover}')`;
  if (titleEl) titleEl.textContent = data.title;
  if (romajiEl) romajiEl.textContent = data.romaji;
  if (synopsisEl) synopsisEl.textContent = data.synopsis;
  if (jlptEl) {
    jlptEl.textContent = data.level;
    jlptEl.className = `chip ${data.levelClass}`;
  }
  if (genreEl) genreEl.textContent = data.genre;
  if (statsEl) statsEl.innerHTML = data.stats;

  if (btnRead) btnRead.onclick = () => openBook(key, 1);
  if (btnDetail) btnDetail.onclick = () => openBookDetail(key);
  if (btnVN) btnVN.onclick = () => openVN(key);
}

function startSrsReviewModal() {
  switchPane('learning');
  setTimeout(() => {
    const arena = document.querySelector('.flashcard-arena');
    if (arena) arena.scrollIntoView({ behavior: 'smooth' });
  }, 120);
}

function renderHomeBookshelfFallback() {
  const homeGrid = document.getElementById('homeBookshelfGrid');
  if (!homeGrid) return;
  if (homeGrid.children.length > 0) return;

  const novels = Object.values(booksData);
  novels.forEach(book => {
    const card = document.createElement('div');
    card.className = 'book-shelf-card';
    card.dataset.id = book.id;
    card.dataset.level = book.level || 'N3';
    card.innerHTML = `
      <div class="shelf-cover-wrap">
        <img src="${book.cover || 'assets/cover_ginga.jpg'}" alt="${escapeHtml(book.title)}" class="shelf-cover-img" loading="lazy">
        <span class="shelf-jlpt-tag">${book.level || 'N3'}</span>
      </div>
      <div class="shelf-info">
        <div class="shelf-title">${escapeHtml(book.title)}</div>
        ${book.romaji ? `<div class="shelf-romaji-subtitle">(${escapeHtml(book.romaji)})</div>` : ''}
        <div class="shelf-author">${escapeHtml(book.author || 'Tác giả')}</div>
        <div class="shelf-stats-line">
          <span>${book.chapters ? book.chapters.length : 1} chương</span> • <span class="shelf-read-btn">Xem chi tiết ➔</span>
        </div>
      </div>
    `;
    card.onclick = () => openBookDetail(book.id);
    homeGrid.appendChild(card);
  });
}

function filterHomeByJlpt(level, btn) {
  document.querySelectorAll('.jlpt-filter-tabs .jlpt-tab-pill').forEach(b => b.classList.remove('active'));
  if (btn) btn.classList.add('active');

  const cards = document.querySelectorAll('#homeBookshelfGrid .book-shelf-card');
  let visibleCount = 0;

  cards.forEach(card => {
    const cardLevel = (card.dataset.level || '').toUpperCase();
    if (level === 'all' || cardLevel === level.toUpperCase()) {
      card.style.display = 'flex';
      visibleCount++;
    } else {
      card.style.display = 'none';
    }
  });

  const sub = document.getElementById('jlptTabSubtitle');
  if (sub) {
    sub.textContent = `${visibleCount} tác phẩm phù hợp`;
  }
}

function scrollToRankings() {
  switchPane('home');
  setTimeout(() => {
    const el = document.getElementById('rankings');
    if (el) el.scrollIntoView({ behavior: 'smooth' });
  }, 100);
}

// ================= 6. EXPLORE CATALOG (FULL LIBRARY) =================
function renderExploreCatalog() {
  const grid = document.getElementById('exploreGrid');
  if (!grid) return;

  const novels = Object.values(booksData);
  grid.innerHTML = '';

  novels.forEach(book => {
    const card = document.createElement('div');
    card.className = `explore-novel-card ${currentExploreViewMode}`;
    card.dataset.id = book.id;
    card.dataset.level = book.level || 'N3';
    card.dataset.genre = book.genre || 'Classic';
    card.dataset.title = (book.title + ' ' + (book.romaji || '') + ' ' + (book.author || '')).toLowerCase();

    card.innerHTML = `
      <div class="card-cover-container" onclick="openBookDetail('${book.id}')">
        <img src="${book.cover}" alt="${escapeHtml(book.title)}" class="card-cover-img" loading="lazy">
        <span class="chip ${book.level.toLowerCase()} card-jlpt-tag">${book.level}</span>
      </div>
      <div class="card-info-container">
        <div class="card-title-row">
          <h3 class="card-novel-title" onclick="openBookDetail('${book.id}')">${escapeHtml(book.title)}</h3>
          <span class="card-genre-pill">${escapeHtml(book.genre || 'Văn học')}</span>
        </div>
        <div class="card-romaji-subtitle">${escapeHtml(book.romaji ? '(' + book.romaji + ')' : '')}</div>
        <div class="card-author-name">Tác giả: ${escapeHtml(book.author || 'Khuyết danh')}</div>
        <p class="card-synopsis-snippet">${escapeHtml(book.synopsis || '')}</p>
        <div class="card-footer-strip">
          <span class="card-chapters-badge">${book.chapters || '1 Chương'}</span>
          <div class="card-actions-group">
            <button class="btn btn-secondary btn-sm" onclick="openBookDetail('${book.id}')">Chi tiết</button>
            <button class="btn btn-accent btn-sm" onclick="event.stopPropagation(); openVN('${book.id}')" title="Trải nghiệm Visual Novel AI">🎭 VN</button>
            <button class="btn btn-primary btn-sm" onclick="openBook('${book.id}', 1)">Đọc ➔</button>
          </div>
        </div>
      </div>
    `;

    grid.appendChild(card);
  });

  filterExploreCatalog();
}

function filterExploreCatalog() {
  const searchInput = document.getElementById('exploreSearchInput');
  const genreSelect = document.getElementById('exploreGenreSelect');
  const sortSelect = document.getElementById('exploreSortSelect');
  const emptyState = document.getElementById('exploreEmptyState');

  const query = (searchInput ? searchInput.value : '').toLowerCase().trim();
  const selectedGenre = genreSelect ? genreSelect.value : 'all';

  const cards = document.querySelectorAll('#exploreGrid .explore-novel-card');
  let visibleCount = 0;

  cards.forEach(card => {
    const cardTitle = card.dataset.title || '';
    const cardLevel = (card.dataset.level || '').toUpperCase();
    const cardGenre = card.dataset.genre || '';

    const matchQuery = !query || cardTitle.includes(query);
    const matchJlpt = selectedExploreJlpt === 'all' || cardLevel === selectedExploreJlpt.toUpperCase();
    const matchGenre = selectedGenre === 'all' || cardGenre.toLowerCase().includes(selectedGenre.toLowerCase());

    if (matchQuery && matchJlpt && matchGenre) {
      card.style.display = currentExploreViewMode === 'grid' ? 'flex' : 'grid';
      visibleCount++;
    } else {
      card.style.display = 'none';
    }
  });

  if (emptyState) {
    emptyState.style.display = visibleCount === 0 ? 'flex' : 'none';
  }
}

function setExploreJlpt(level, btn) {
  selectedExploreJlpt = level;
  document.querySelectorAll('#exploreJlptPills .filter-pill').forEach(b => b.classList.remove('active'));
  if (btn) btn.classList.add('active');
  filterExploreCatalog();
}

function setExploreViewMode(mode) {
  currentExploreViewMode = mode;
  const btnGrid = document.getElementById('btnViewGrid');
  const btnList = document.getElementById('btnViewList');
  const grid = document.getElementById('exploreGrid');

  if (btnGrid) btnGrid.classList.toggle('active', mode === 'grid');
  if (btnList) btnList.classList.toggle('active', mode === 'list');
  if (grid) {
    grid.className = `explore-catalog-grid ${mode}-mode`;
  }
  filterExploreCatalog();
}

function resetExploreFilters() {
  const searchInput = document.getElementById('exploreSearchInput');
  const genreSelect = document.getElementById('exploreGenreSelect');
  if (searchInput) searchInput.value = '';
  if (genreSelect) genreSelect.value = 'all';

  const allPill = document.querySelector('#exploreJlptPills .filter-pill');
  setExploreJlpt('all', allPill);
}

// ================= 7. LEARNING HUB TABS CONTROLLER =================
function switchLearningTab(tabId) {
  const tabs = ['srs', 'vocab', 'quiz', 'progress'];
  tabs.forEach(t => {
    const btn = document.getElementById(`tabBtn${t.charAt(0).toUpperCase() + t.slice(1)}`);
    const content = document.getElementById(`tabContent${t.charAt(0).toUpperCase() + t.slice(1)}`);
    if (btn) btn.classList.toggle('active', t === tabId);
    if (content) content.classList.toggle('active', t === tabId);
  });
}

function handleVocabSearch(query) {
  const q = (query || '').toLowerCase().trim();
  const rows = document.querySelectorAll('#vocabBankRows .vocab-row:not(.head)');
  rows.forEach(r => {
    const text = r.textContent.toLowerCase();
    r.style.display = (!q || text.includes(q)) ? 'grid' : 'none';
  });
}

function filterVocabByNovel(novelTitle) {
  const rows = document.querySelectorAll('#vocabBankRows .vocab-row:not(.head)');
  rows.forEach(r => {
    const source = (r.dataset.source || '').toLowerCase();
    r.style.display = (novelTitle === 'all' || source.includes(novelTitle.toLowerCase())) ? 'grid' : 'none';
  });
}

// ================= 8. THEME & AUDIO MANAGEMENT =================
let globalTheme = 'obsidian'; // 'obsidian', 'paper', 'sepia'

function toggleGlobalTheme() {
  if (globalTheme === 'obsidian') {
    setGlobalTheme('paper');
  } else if (globalTheme === 'paper') {
    setGlobalTheme('sepia');
  } else {
    setGlobalTheme('obsidian');
  }
}

function setGlobalTheme(theme) {
  globalTheme = theme;
  document.body.classList.remove('theme-obsidian', 'theme-paper', 'theme-sepia');
  document.body.classList.add(`theme-${theme}`);

  const themeIcon = document.getElementById('themeBtnIcon');
  if (themeIcon) {
    themeIcon.textContent = theme === 'paper' ? '📜' : (theme === 'sepia' ? '🍂' : '🌌');
  }

  showToast(`Đã chuyển chủ đề sang: ${theme === 'paper' ? 'Giấy ngà (Paper Warm)' : (theme === 'sepia' ? 'Trầm (Sepia Vintage)' : 'Đêm (Obsidian Dark)')}`);
}

function setGlobalFont(font) {
  const prose = document.getElementById('readingProse');
  if (prose) {
    prose.classList.remove('font-mincho', 'font-gothic');
    prose.classList.add(font === 'gothic' ? 'font-gothic' : 'font-mincho');
    showToast(`Đã đổi phông đọc mặc định sang: ${font === 'gothic' ? 'Noto Sans JP (Gothic)' : 'Shippori Mincho (Serif)'}`);
  }
}

// ================= 9. GLOBAL HELPERS =================
function handleGlobalSearch(query) {
  const q = (query || '').toLowerCase().trim();
  if (!q) return;

  // If user hits enter or types length >= 2, jump to explore with query
  switchPane('explore');
  const expInput = document.getElementById('exploreSearchInput');
  if (expInput) {
    expInput.value = q;
    filterExploreCatalog();
  }
}

function speakWordText(kanji, reading) {
  if ('speechSynthesis' in window) {
    window.speechSynthesis.cancel();
    const u = new SpeechSynthesisUtterance(reading || kanji);
    u.lang = 'ja-JP';
    u.rate = 0.9;
    window.speechSynthesis.speak(u);
  }
}

function toggleMobileNav() {
  const drawer = document.getElementById('mobileNavDrawer');
  if (drawer) drawer.classList.toggle('open');
}

function closeMobileNav() {
  const drawer = document.getElementById('mobileNavDrawer');
  if (drawer) drawer.classList.remove('open');
}

// Select book in bookshelf (compatibility with existing scripts)
function selectBook(bookId) {
  openBookDetail(bookId);
}
