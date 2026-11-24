// ===================================================================
// KOTOBA ODYSSEY - BACKEND REST API BRIDGE (SQLITE CONNECTED)
// ===================================================================

const API_BASE = 'http://localhost:5000/api';

async function fetchFromBackend(endpoint, options = {}) {
  try {
    const res = await fetch(`${API_BASE}${endpoint}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(options.headers || {})
      }
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return await res.json();
  } catch (err) {
    console.warn(`[Backend Offline / Fallback] ${endpoint}:`, err.message);
    return null;
  }
}

async function syncWithBackend() {
  console.log('[Kotoba Odyssey] Connecting to SQLite Backend at', API_BASE);

  // 1. Progress & Gamification
  await fetchUserProgress();
  await fetchLearningAnalytics();

  // 2. Real Vocab & SRS Deck
  await fetchSrsDeck();

  // 4. Novels from SQLite
  const novels = await fetchFromBackend('/novels');
  if (novels && novels.length > 0) {
    novels.forEach(n => {
      const key = `db_novel_${n.id}`;
      const coverImg = getCoverForNovel(n);
      const totalWords = n.chapters?.reduce((acc, c) => acc + (c.wordCount || 0), 0) || (n.totalWords || 0);

      booksData[key] = {
        id: key,
        dbId: n.id,
        title: n.title,
        romaji: n.romajiTitle || n.japaneseTitle,
        author: n.author,
        level: n.jlptLevel || 'N3',
        genre: n.genre,
        cover: coverImg,
        synopsis: n.description,
        chapters: `${n.totalChapters || (n.chapters ? n.chapters.length : 1)} Chương`,
        words: `${totalWords > 0 ? totalWords.toLocaleString() + ' Từ' : '100,000+ Từ'}`,
        vocab: 'Đã index',
        progress: n.lastReadAt ? 'Đang đọc' : 'Chưa đọc'
      };
    });

    renderBookshelfFromNovels(novels);
  }

  // 5. Daily Practice Recommendations
  try {
    const dailyRec = await fetchFromBackend('/practice/daily-recommendations');
    if (dailyRec) {
      const srsTitle = document.getElementById('dailyRecSrsTitle');
      const srsDesc = document.getElementById('dailyRecSrsDesc');
      const readTitle = document.getElementById('dailyRecReadTitle');
      const readDesc = document.getElementById('dailyRecReadDesc');
      const dueCardsStat = document.getElementById('statDueCards');
      const btnRead = document.getElementById('btnDailyRecRead');

      if (srsTitle) srsTitle.textContent = `${dailyRec.dueSrsCardsCount} Thẻ từ vựng cần ôn tập SRS`;
      if (dueCardsStat) dueCardsStat.textContent = `${dailyRec.dueSrsCardsCount} từ đến hạn`;
      if (readTitle) readTitle.textContent = `Đề xuất đọc: ${dailyRec.suggestedNovelTitle}`;
      if (readDesc) readDesc.textContent = `${dailyRec.suggestedChapterTitle} • +25 XP khi làm bài tập đọc hiểu`;
      if (btnRead) {
        btnRead.onclick = () => {
          openBook(`db_novel_${dailyRec.suggestedNovelId}`);
        };
      }
    }
  } catch (err) {
    console.warn('Could not load daily recommendations', err);
  }
}

async function fetchUserProgress() {
  try {
    const progress = await fetchFromBackend('/progress');
    if (progress) {
      updateProgressUI(progress);
      try { localStorage.setItem('kotoba_user_progress', JSON.stringify(progress)); } catch (e) {}
    }
  } catch (err) {
    console.warn('Could not fetch user progress:', err);
  }
}

async function fetchSrsDeck() {
  try {
    const dueItems = await fetchFromBackend('/vocab/due');
    if (dueItems && Array.isArray(dueItems) && dueItems.length > 0) {
      srsDeck.length = 0;
      dueItems.forEach(item => {
        srsDeck.push({
          id: item.id,
          kanji: item.word,
          word: item.word,
          reading: item.reading || item.word,
          romaji: item.reading || '',
          meaning: item.meaning,
          level: item.jlptLevel || 'N5',
          sentence: item.contextSentence || '',
          sentenceVn: item.sourceNovelTitle ? `Nguồn: ${item.sourceNovelTitle}` : ''
        });
      });
      srsIndex = 0;
      updateCardUI(0);
      const navBadge = document.getElementById('navSrsCount');
      if (navBadge) navBadge.textContent = srsDeck.length;
      const homeDueBadge = document.getElementById('homeDueSrsCount');
      if (homeDueBadge) homeDueBadge.textContent = srsDeck.length;
    } else {
      const allVocab = await fetchFromBackend('/vocab');
      if (allVocab && Array.isArray(allVocab) && allVocab.length > 0) {
        renderVocabBank(allVocab);
        srsDeck.length = 0;
        allVocab.forEach(item => {
          srsDeck.push({
            id: item.id,
            kanji: item.word,
            word: item.word,
            reading: item.reading || item.word,
            romaji: item.reading || '',
            meaning: item.meaning,
            level: item.jlptLevel || 'N5',
            sentence: item.contextSentence || '',
            sentenceVn: item.sourceNovelTitle ? `Nguồn: ${item.sourceNovelTitle}` : ''
          });
        });
        srsIndex = 0;
        updateCardUI(0);
      }
    }
  } catch (err) {
    console.warn('Could not fetch SRS deck from backend:', err);
  }

  // Update badge in sidebar
  const srsBadge = document.querySelector('[data-pane="srs"] .nav-badge');
  if (srsBadge) srsBadge.textContent = srsDeck.length;
}

function saveVocabToLocalStorage(vocabItem) {
  try {
    const local = JSON.parse(localStorage.getItem('kotoba_local_vocab') || '[]');
    const existingIdx = local.findIndex(v => v.word.trim().toLowerCase() === vocabItem.word.trim().toLowerCase());
    if (existingIdx >= 0) {
      if (vocabItem.contextSentence && !local[existingIdx].contextSentence?.includes(vocabItem.contextSentence)) {
        local[existingIdx].contextSentence = (local[existingIdx].contextSentence ? local[existingIdx].contextSentence + '\n\n' : '') + vocabItem.contextSentence;
        localStorage.setItem('kotoba_local_vocab', JSON.stringify(local));
      }
      return { isDuplicate: true, isEnriched: true, item: local[existingIdx] };
    }
    const existsInDeck = srsDeck.some(v => (v.kanji || v.word).trim().toLowerCase() === vocabItem.word.trim().toLowerCase());
    if (existsInDeck) {
      return { isDuplicate: true, isEnriched: false };
    }
    const item = {
      id: Date.now(),
      ...vocabItem,
      createdAt: new Date().toISOString()
    };
    local.push(item);
    localStorage.setItem('kotoba_local_vocab', JSON.stringify(local));

    srsDeck.push({
      id: item.id,
      kanji: item.word,
      word: item.word,
      reading: item.reading || item.word,
      romaji: item.reading || '',
      meaning: item.meaning,
      level: item.jlptLevel || 'N3',
      sentence: item.contextSentence || '',
      sentenceVn: item.sourceNovelTitle ? `Nguồn: ${item.sourceNovelTitle}` : ''
    });

    const srsBadge = document.querySelector('[data-pane="srs"] .nav-badge');
    if (srsBadge) srsBadge.textContent = srsDeck.length;

    return { isDuplicate: false, item };
  } catch (e) {
    return { isDuplicate: false };
  }
}

function refreshSrsDeckFromLocal() {
  const srsBadge = document.querySelector('[data-pane="srs"] .nav-badge');
  if (srsBadge) srsBadge.textContent = srsDeck.length;
  if (srsDeck.length > 0 && typeof updateCardUI === 'function') {
    updateCardUI(srsIndex);
  }
}

async function fetchLearningAnalytics() {
  try {
    const analytics = await fetchFromBackend('/progress/analytics');
    if (analytics) {
      updateAnalyticsUI(analytics);
    }
  } catch (err) {
    console.warn('Could not fetch learning analytics:', err);
  }
}

function updateAnalyticsUI(a) {
  // 1. Stats Strip
  const timeStat = document.getElementById('statTodayTime');
  const dueStat = document.getElementById('statDueCards');
  const streakStat = document.getElementById('statStreak');
  const novelStat = document.getElementById('statNovelCount');

  if (timeStat) timeStat.textContent = `${a.todayStudyMinutes} / 30 phút`;
  if (dueStat) dueStat.textContent = `${a.dueReviewsCount} từ đến hạn`;
  if (streakStat) streakStat.textContent = `${a.streakDays} ngày liên tục`;
  if (novelStat) novelStat.textContent = `${a.totalNovels} tác phẩm`;

  // 2. Activity / Analytics Card in Home Pane
  const activityContainer = document.querySelector('.dual-section-grid');
  if (activityContainer && !document.getElementById('learningAnalyticsCard')) {
    const card = document.createElement('div');
    card.id = 'learningAnalyticsCard';
    card.className = 'clean-card';
    card.style.cssText = 'grid-column: 1 / -1; margin-top: 14px; background: rgba(15, 23, 42, 0.7); border: 1px solid rgba(59, 130, 246, 0.3);';
    card.innerHTML = `
      <div class="section-header-row" style="margin-bottom: 12px;">
        <h3 class="section-title" style="font-size: 15px; color: #38BDF8; display: flex; align-items: center; gap: 6px;">
          <span>📊</span> <span>PHÂN TÍCH NĂNG LỰC &amp; TIẾN ĐỘ HỌC TẬP (LEARNING ANALYTICS)</span>
        </h3>
        <span class="chip" style="background: rgba(16, 185, 129, 0.2); color: #34D399; font-size: 11px;">Dữ liệu thực tế</span>
      </div>

      <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin-bottom: 14px;">
        <div style="background: rgba(255,255,255,0.03); padding: 10px 14px; border-radius: 6px; border-left: 3px solid #3B82F6;">
          <div style="font-size: 11.5px; color: var(--text-dim);">Chương hoàn thành</div>
          <div style="font-size: 20px; font-weight: 700; color: #60A5FA;">${a.completedChapters} chương</div>
          <div style="font-size: 11px; color: var(--text-secondary);">Tiến độ: ${a.overallReadingProgressPercent}% tủ sách</div>
        </div>
        <div style="background: rgba(255,255,255,0.03); padding: 10px 14px; border-radius: 6px; border-left: 3px solid #10B981;">
          <div style="font-size: 11.5px; color: var(--text-dim);">Vốn từ làm chủ (SRS)</div>
          <div style="font-size: 20px; font-weight: 700; color: #34D399;">${a.masteredWords} / ${a.totalSavedWords} từ</div>
          <div style="font-size: 11px; color: var(--text-secondary);">Tỷ lệ ghi nhớ: ${a.vocabularyRetentionRate}%</div>
        </div>
        <div style="background: rgba(255,255,255,0.03); padding: 10px 14px; border-radius: 6px; border-left: 3px solid #F59E0B;">
          <div style="font-size: 11.5px; color: var(--text-dim);">Điểm Quiz đọc hiểu</div>
          <div style="font-size: 20px; font-weight: 700; color: #FBBF24;">${a.averageQuizScorePercent}% TB</div>
          <div style="font-size: 11px; color: var(--text-secondary);">Đã làm: ${a.totalQuizzesAttempted} bài (${a.quizzesPassed} bài đạt)</div>
        </div>
      </div>

      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px; font-size: 12.5px;">
        <div style="background: rgba(16, 185, 129, 0.08); border-left: 3px solid #10B981; padding: 10px; border-radius: 4px;">
          <div style="font-weight: 700; color: #34D399; margin-bottom: 4px;">💪 ĐIỂM MẠNH:</div>
          ${(a.strengths || []).map(s => `<div style="color: #E2E8F0; margin-bottom: 2px;">• ${escapeHtml(s)}</div>`).join('')}
        </div>
        <div style="background: rgba(239, 68, 68, 0.08); border-left: 3px solid #EF4444; padding: 10px; border-radius: 4px;">
          <div style="font-weight: 700; color: #F87171; margin-bottom: 4px;">🎯 ĐIỂM CẦN CẢI THIỆN:</div>
          ${(a.weaknesses || []).map(w => `<div style="color: #E2E8F0; margin-bottom: 2px;">• ${escapeHtml(w)}</div>`).join('')}
        </div>
      </div>
    `;
    activityContainer.appendChild(card);
  }
}

function getCoverForNovel(n) {
  if (n.coverImagePath && n.coverImagePath.startsWith('assets/covers/')) {
    return n.coverImagePath;
  }
  const t = n.title || '';
  if (t.includes('銀河鉄道')) return 'assets/cover_ginga.jpg';
  if (t.includes('坊っちゃん')) return 'assets/cover_botchan.jpg';
  if (t.includes('ごんぎつね')) return 'assets/covers/cover_slow_life.jpg';
  if (t.includes('雪国')) return 'assets/covers/cover_anime_forest.jpg';
  if (t.includes('メロス')) return 'assets/covers/cover_anime_adventurer.jpg';
  if (t.includes('羅生門')) return 'assets/covers/cover_anime_castle.jpg';
  if (t.includes('蜘蛛の糸')) return 'assets/covers/cover_anime_sunset.jpg';
  if (t.includes('こころ')) return 'assets/covers/cover_anime_cafe.jpg';
  if (t.includes('料理店') || t.includes('山猫')) return 'assets/covers/cover_isekai_gourmet.jpg';
  if (t.includes('家庭教師')) return 'assets/covers/cover_school_romcom.jpg';
  if (t.includes('杜子春')) return 'assets/covers/cover_anime_night_shrine.jpg';
  if (t.includes('舞姫')) return 'assets/covers/cover_anime_sunset.jpg';
  return 'assets/covers/cover_anime_forest.jpg';
}

function filterByLevel(level, btn) {
  document.querySelectorAll('.filter-pill').forEach(b => b.classList.remove('active'));
  if (btn) btn.classList.add('active');

  const items = document.querySelectorAll('.library-book-item');
  items.forEach(item => {
    const itemLevel = (item.dataset.level || '').toUpperCase();
    if (level === 'all' || itemLevel === level.toUpperCase()) {
      item.style.display = 'flex';
    } else {
      item.style.display = 'none';
    }
  });
  updateLibraryEmptyState();
}

function handleSearch(query) {
  const q = (query || '').toLowerCase().trim();
  const items = document.querySelectorAll('.library-book-item');
  items.forEach(item => {
    const text = item.textContent.toLowerCase();
    if (!q || text.includes(q)) {
      item.style.display = 'flex';
    } else {
      item.style.display = 'none';
    }
  });
  updateLibraryEmptyState();
}

function updateLibraryEmptyState() {
  const grid = document.getElementById('libraryGrid');
  if (!grid) return;
  const items = grid.querySelectorAll('.library-book-item');
  let visibleCount = 0;
  items.forEach(item => {
    if (item.style.display !== 'none') visibleCount++;
  });
  let emptyState = document.getElementById('libraryEmptyState');
  if (visibleCount === 0) {
    if (!emptyState) {
      emptyState = document.createElement('div');
      emptyState.id = 'libraryEmptyState';
      emptyState.className = 'empty-state-card';
      emptyState.innerHTML = `
        <div class="empty-state-icon">📖</div>
        <div class="empty-state-title">Không tìm thấy tác phẩm phù hợp</div>
        <div class="empty-state-desc">Không có cuốn tiểu thuyết nào khớp với bộ lọc hoặc từ khóa hiện tại. Thử chọn lại cấp độ hoặc tìm kiếm khác nhé!</div>
        <button class="empty-state-btn" onclick="resetLibraryFilters()">Xem tất cả truyện</button>
      `;
      grid.appendChild(emptyState);
    } else {
      emptyState.style.display = 'flex';
    }
  } else if (emptyState) {
    emptyState.style.display = 'none';
  }
}

function resetLibraryFilters() {
  const allBtn = document.querySelector('.filter-pills .filter-pill');
  if (allBtn) filterByLevel('all', allBtn);
  const searchInput = document.querySelector('.search-input');
  if (searchInput) {
    searchInput.value = '';
    handleSearch('');
  }
}

function renderBookshelfFromNovels(novels) {
  const grid = document.getElementById('libraryGrid');
  const homeGrid = document.getElementById('homeBookshelfGrid');
  if (!novels || novels.length === 0) return;

  // 1. Render Library Grid
  if (grid) {
    grid.innerHTML = '';
    novels.forEach((n, idx) => {
      const key = `db_novel_${n.id}`;
      const coverImg = getCoverForNovel(n);
      const romajiSub = n.romajiTitle || n.romaji || '';

      const item = document.createElement('div');
      const isSelected = (currentInspectedId === key) || (!currentInspectedId && idx === 0);
      item.className = `library-book-item ${isSelected ? 'selected' : ''}`;
      item.dataset.id = key;
      item.dataset.level = n.jlptLevel || 'N3';
      item.innerHTML = `
        <div class="lib-cover-frame">
          <img src="${coverImg}" alt="${escapeHtml(n.title)}" class="lib-cover-img">
          <span class="shelf-jlpt-tag">${n.jlptLevel || 'N3'}</span>
        </div>
        <div class="lib-meta-info">
          <div class="lib-title-row">
            <div class="lib-title" title="${escapeHtml(n.title)}">${escapeHtml(n.title)}</div>
            <span class="chip ${String(n.jlptLevel || 'N3').toLowerCase()}">${n.jlptLevel || 'N3'}</span>
          </div>
          ${romajiSub ? `<div class="lib-romaji-subtitle" title="${escapeHtml(romajiSub)}">(${escapeHtml(romajiSub)})</div>` : ''}
          <div class="lib-author">${escapeHtml(n.author || 'Tác giả')}</div>
          <div style="font-size: 11px; color: var(--text-dim); margin-top: 2px;">
            ${n.totalChapters || 1} chương • ${n.genre || 'Văn học'}
          </div>
        </div>
      `;
      item.onclick = () => selectBook(key);
      grid.appendChild(item);
    });
  }

  // 2. Render Curated Bookshelf on Home Pane
  if (homeGrid) {
    homeGrid.innerHTML = '';
    novels.forEach(n => {
      const key = `db_novel_${n.id}`;
      const coverImg = getCoverForNovel(n);
      const romajiSub = n.romajiTitle || n.romaji || '';

      const card = document.createElement('div');
      card.className = 'book-shelf-card';
      card.dataset.id = key;
      card.dataset.level = n.jlptLevel || 'N3';
      card.innerHTML = `
        <div class="shelf-cover-wrap">
          <img src="${coverImg}" alt="${escapeHtml(n.title)}" class="shelf-cover-img" loading="lazy">
          <span class="shelf-jlpt-tag">${n.jlptLevel || 'N3'}</span>
        </div>
        <div class="shelf-info">
          <div class="shelf-title">${escapeHtml(n.title)}</div>
          ${romajiSub ? `<div class="shelf-romaji-subtitle">(${escapeHtml(romajiSub)})</div>` : ''}
          <div class="shelf-author">${escapeHtml(n.author || 'Tác giả')}</div>
          <div class="shelf-stats-line">
            <span>${n.totalChapters || 1} chương</span> • <span class="shelf-read-btn">Xem chi tiết ➔</span>
          </div>
        </div>
      `;
      card.onclick = () => openBookDetail(key);
      homeGrid.appendChild(card);
    });
  }

  // 3. Re-render explore catalog
  if (typeof renderExploreCatalog === 'function') {
    renderExploreCatalog();
  }

  // 4. Update stats & pills with dynamic counts
  const statCount = document.getElementById('statNovelCount');
  if (statCount) statCount.textContent = `${novels.length} tác phẩm`;

  const navLib = document.getElementById('navLibraryCount');
  if (navLib) navLib.textContent = novels.length;

  const badge = document.querySelector('.nav-item[data-pane="library"] .nav-badge');
  if (badge) badge.textContent = novels.length;

  // Count by JLPT
  const n5Count = novels.filter(n => (n.jlptLevel || '').toUpperCase() === 'N5').length;
  const n4Count = novels.filter(n => (n.jlptLevel || '').toUpperCase() === 'N4').length;
  const n3Count = novels.filter(n => (n.jlptLevel || '').toUpperCase() === 'N3').length;
  const n2Count = novels.filter(n => (n.jlptLevel || '').toUpperCase() === 'N2').length;

  const pills = document.querySelectorAll('.filter-pills .filter-pill');
  pills.forEach(p => {
    const txt = p.textContent;
    if (txt.includes('Tất cả')) p.textContent = `Tất cả (${novels.length})`;
    else if (txt.includes('N5')) p.textContent = `JLPT N5 (${n5Count})`;
    else if (txt.includes('N4')) p.textContent = `JLPT N4 (${n4Count})`;
    else if (txt.includes('N3')) p.textContent = `JLPT N3 (${n3Count})`;
    else if (txt.includes('N2')) p.textContent = `JLPT N2 (${n2Count})`;
  });

  if (novels.length > 0 && !currentInspectedId) {
    selectBook(`db_novel_${novels[0].id}`);
  }
}

function updateProgressUI(p) {
  const nameEl = document.querySelector('.user-name');
  const streakEl = document.querySelector('.user-streak');
  const xpSpan = document.querySelector('.xp-indicator span:last-child');
  const xpLevel = document.querySelector('.xp-indicator span:first-child');
  const xpFill = document.querySelector('.xp-fill');

  if (nameEl) nameEl.textContent = `${p.userName} Lv.${p.level}`;
  if (streakEl) streakEl.textContent = `🔥 ${p.streakDays} ngày liên tiếp`;
  if (xpLevel) xpLevel.textContent = `Cấp ${p.level}`;
  if (xpSpan) xpSpan.textContent = `${p.currentXp.toLocaleString()} / ${p.requiredXp.toLocaleString()} XP`;
  if (xpFill) {
    const pct = Math.min(100, Math.round((p.currentXp / p.requiredXp) * 100));
    xpFill.style.width = `${pct}%`;
  }
}

function addLocalXp(amount) {
  const xpSpan = document.querySelector('.xp-indicator span:last-child');
  if (xpSpan) {
    const parts = xpSpan.textContent.split('/');
    if (parts.length === 2) {
      let cur = parseInt(parts[0].replace(/[^0-9]/g, '')) || 0;
      let req = parseInt(parts[1].replace(/[^0-9]/g, '')) || 2000;
      cur += amount;
      xpSpan.textContent = `${cur.toLocaleString()} / ${req.toLocaleString()} XP`;
      const fill = document.querySelector('.xp-fill');
      if (fill) fill.style.width = `${Math.min(100, Math.round((cur / req) * 100))}%`;
    }
  }
}

function renderVocabBank(vocabList) {
  const rowsContainer = document.getElementById('vocabBankRows');
  const countEl = document.getElementById('vocabBankCount');

  if (countEl) {
    countEl.textContent = `${vocabList.length} từ trong cơ sở dữ liệu`;
  }
  const homeSavedVocabCount = document.getElementById('homeSavedVocabCount');
  if (homeSavedVocabCount) {
    homeSavedVocabCount.textContent = vocabList.length;
  }

  if (rowsContainer) {
    rowsContainer.innerHTML = '';
    vocabList.forEach(v => {
      const row = document.createElement('div');
      row.className = 'vocab-row';
      const level = v.jlptLevel || 'N5';
      const dueText = v.isDue ? '<span style="color: #F59E0B; font-weight: 600;">Đến hạn ôn</span>' : `${v.intervalDays || 1} ngày`;

      row.innerHTML = `
        <span class="v-kanji">${v.word}</span>
        <span class="v-reading">${v.reading}</span>
        <span class="v-meaning">${v.meaning}</span>
        <span><span class="chip ${level.toLowerCase()}">${level}</span></span>
        <span class="v-due">${dueText}</span>
      `;
      rowsContainer.appendChild(row);
    });
  }
}

function switchImportModalTab(tab) {
  const tabUrl = document.getElementById('importTabUrl');
  const tabManual = document.getElementById('importTabManual');
  const btnUrl = document.getElementById('tabImportUrlBtn');
  const btnManual = document.getElementById('tabImportManualBtn');

  if (tab === 'url') {
    if (tabUrl) tabUrl.style.display = 'block';
    if (tabManual) tabManual.style.display = 'none';
    if (btnUrl) btnUrl.classList.add('active');
    if (btnManual) btnManual.classList.remove('active');
  } else {
    if (tabUrl) tabUrl.style.display = 'none';
    if (tabManual) tabManual.style.display = 'block';
    if (btnUrl) btnUrl.classList.remove('active');
    if (btnManual) btnManual.classList.add('active');
  }
}

function fillSampleUrl(url, genre, level) {
  const urlInput = document.getElementById('importUrlInput');
  const genreSelect = document.getElementById('importUrlGenre');
  const levelSelect = document.getElementById('importUrlLevel');

  if (urlInput) urlInput.value = url;
  if (genreSelect) genreSelect.value = genre;
  if (levelSelect) levelSelect.value = level;
  playClickSound();
}

async function submitImportUrl() {
  const url = document.getElementById('importUrlInput')?.value?.trim();
  const genre = document.getElementById('importUrlGenre')?.value || 'Isekai & Fantasy';
  const level = document.getElementById('importUrlLevel')?.value || 'N3';
  const btn = document.getElementById('btnSubmitImportUrl');

  if (!url) {
    alert('Vui lòng nhập đường link tiểu thuyết (Syosetu / Kakuyomu hoặc trang web tiếng Nhật).');
    return;
  }

  if (btn) {
    btn.disabled = true;
    btn.textContent = '⏳ Đang kết nối máy chủ & cào nội dung...';
  }

  const payload = {
    url: url,
    genre: genre,
    jlptLevel: level
  };

  const res = await fetchFromBackend('/novels/import-url', {
    method: 'POST',
    body: JSON.stringify(payload)
  });

  if (res && res.novelId) {
    playSuccessChime();
    alert(`🎉 Cào và lưu tác phẩm thành công!\nTiêu đề: ${res.novelTitle || 'Tiểu thuyết mới'}\nThể loại: ${res.genre || genre}`);
    closeImportModal();
    // Refresh library and open the newly imported novel
    await syncWithBackend();
    if (typeof openBookDetail === 'function') {
      openBookDetail(`db_novel_${res.novelId}`);
    } else {
      switchPane('explore');
    }
  } else {
    alert('Không thể cào dữ liệu từ đường link này. Vui lòng kiểm tra lại liên kết hoặc chuyển sang tab Nhập thủ công.');
  }

  if (btn) {
    btn.disabled = false;
    btn.textContent = '🚀 Cào truyện & Lưu vào Thư viện ➔';
  }
}

// Novel Import Modal Handlers
function openImportModal() {
  const modal = document.getElementById('importModal');
  if (modal) modal.classList.add('open');
}

function closeImportModal(e) {
  if (e && e.target !== e.currentTarget && !e.target.classList.contains('modal-close-btn')) {
    return;
  }
  const modal = document.getElementById('importModal');
  if (modal) modal.classList.remove('open');
}

async function submitImportNovel() {
  const title = document.getElementById('importTitle')?.value?.trim();
  const level = document.getElementById('importLevel')?.value || 'N3';
  const author = document.getElementById('importAuthor')?.value?.trim() || 'Khuyết danh';
  const content = document.getElementById('importContent')?.value?.trim();
  const btn = document.getElementById('btnSubmitImport');

  if (!content) {
    alert('Vui lòng dán nội dung văn bản tiểu thuyết hoặc file JSON.');
    return;
  }

  if (btn) {
    btn.disabled = true;
    btn.textContent = '⏳ Đang phân tích & lưu...';
  }

  const isJson = content.startsWith('{');
  const payload = {
    content: content,
    format: isJson ? 'JSON' : 'TXT',
    title: title || 'Tiểu thuyết mới',
    author: author,
    jlptLevel: level,
    genre: 'General'
  };

  const res = await fetchFromBackend('/novels/import', {
    method: 'POST',
    body: JSON.stringify(payload)
  });

  if (res && (res.novelId || res.message)) {
    playSuccessChime();
    alert('🎉 Nhập tiểu thuyết thành công vào SQLite Database!');
    closeImportModal();
    // Refresh library from backend
    await syncWithBackend();
    showBookshelfView();
  } else {
    alert('Không thể lưu tiểu thuyết. Vui lòng kiểm tra lại định dạng file hoặc tiêu đề.');
  }

  if (btn) {
    btn.disabled = false;
    btn.textContent = '📥 Lưu vào Thư viện SQLite ➔';
  }
}

