// Kotoba Odyssey — Astral Journey Client Logic

// 1. STATE & DATA
const state = {
  currentTab: 'dashboard',
  currentNovel: 'ginga',
  fontSize: 19,
  isBookmarked: false,
  readerTheme: 'night',
  user: {
    name: '旅人 (Traveler)',
    title: 'Starlight Scholar',
    level: 3,
    xp: 210,
    requiredXp: 390,
    streak: 5,
    savedWordsCount: 48,
    dueReviewCount: 3
  },
  currentDictWord: null,
  flashcards: [
    {
      kanji: '銀河',
      reading: 'ぎんが (Ginga)',
      meaning: 'Ngân hà, dải thiên hà (Milky Way)',
      sentence: '「上から下へ白くけむった<span class="term-hl">銀河</span>の帯のようなところを指しながら」',
      level: 'JLPT N3',
      source: '銀河鉄道の夜',
      interval: 1,
      ease: 2.50
    },
    {
      kanji: '星座',
      reading: 'せいざ (Seiza)',
      meaning: 'Chòm sao, tinh tú trên bầu trời đêm',
      sentence: '「黒板に吊した大きな黒い<span class="term-hl">星座</span>の図」',
      level: 'JLPT N3',
      source: '銀河鉄道の夜',
      interval: 1,
      ease: 2.40
    },
    {
      kanji: '無鉄砲',
      reading: 'むてっぽう (Muteppou)',
      meaning: 'Hấp tấp, liều lĩnh, không suy nghĩ tính toán',
      sentence: '「親譲りの<span class="term-hl">無鉄砲</span>で小供の時から損ばかりしている」',
      level: 'JLPT N1',
      source: '坊っちゃん',
      interval: 3,
      ease: 2.30
    }
  ],
  currentCardIndex: 0,
  dictionaryDB: {
    '銀河': { reading: 'ぎんが', jlpt: 'JLPT N3', pos: 'Danh từ (Noun)', meaning: 'Ngân hà, dải thiên hà (Milky Way)', example: '上から下へ白くけむった銀河の帯のようなところを指しながら。' },
    '星座': { reading: 'せいざ', jlpt: 'JLPT N3', pos: 'Danh từ (Noun)', meaning: 'Chòm sao, tinh tú', example: '黒板に吊した大きな黒い星座の図。' },
    '黒板': { reading: 'こくばん', jlpt: 'JLPT N5', pos: 'Danh từ (Noun)', meaning: 'Bảng đen (lớp học)', example: '先生は黒板に大きな図を描きました。' },
    '先生': { reading: 'せんせい', jlpt: 'JLPT N5', pos: 'Danh từ (Noun)', meaning: 'Thầy cô giáo, tiên sinh', example: '先生は早くもそれを見附けたのでした。' },
    '川': { reading: 'かわ', jlpt: 'JLPT N5', pos: 'Danh từ (Noun)', meaning: 'Dòng sông, con suối', example: '川だと云われたりしていたこの白いもの。' },
    '乳': { reading: 'ちち', jlpt: 'JLPT N3', pos: 'Danh từ (Noun)', meaning: 'Sữa (dải Ngân hà / Milky Way)', example: '乳の流れたあとだと云われていた。' },
    '白い': { reading: 'しろい', jlpt: 'JLPT N5', pos: 'Tính từ đuôi い', meaning: 'Màu trắng, sáng trắng', example: 'ぼんやりと白いものがほんとうは何か。' },
    '図': { reading: 'ず', jlpt: 'JLPT N3', pos: 'Danh từ (Noun)', meaning: 'Bản đồ, sơ đồ, hình vẽ', example: '大きな黒い星座の図。' },
    '手': { reading: 'て', jlpt: 'JLPT N5', pos: 'Danh từ (Noun)', meaning: 'Bàn tay, tay', example: 'カムパネルラが手をあげました。' },
    '星': { reading: 'ほし', jlpt: 'JLPT N5', pos: 'Danh từ (Noun)', meaning: 'Ngôi sao, thiên thể', example: 'たしかにあれがみんな星だと。' },
    '教室': { reading: 'きょうしつ', jlpt: 'JLPT N5', pos: 'Danh từ (Noun)', meaning: 'Phòng học, lớp học', example: 'このごろはジョバンニはまるで毎日教室でもねむく。' },
    '本': { reading: 'ほん', jlpt: 'JLPT N5', pos: 'Danh từ (Noun)', meaning: 'Sách, cuốn sách', example: '本を読むひまも読む本もないので。' },
    '読む': { reading: 'よむ', jlpt: 'JLPT N5', pos: 'Động từ nhóm 1', meaning: 'Đọc (sách, báo)', example: 'いつか雑誌で読んだのでしたが。' },
    '答える': { reading: 'こたえる', jlpt: 'JLPT N4', pos: 'Động từ nhóm 2', meaning: 'Trả lời, giải đáp', example: '立って見るともうはっきりとそれを答えることができませんでした。' },
    '銀河鉄道': { reading: 'ぎんがてつどう', jlpt: 'JLPT N3', pos: 'Danh từ ghép', meaning: 'Tàu hỏa chạy trên dải ngân hà', example: 'この銀河鉄道に乗って一緒に行こうね。' }
  },
  vnLines: [
    { speaker: 'カムパネルラ', role: '• Bạn đồng hành', text: '「ジョバンニ、僕たちどこまでもどこまでも、この銀河鉄道に乗って一緒に行こうね」', trans: '"Giovanni à, hai chúng ta hãy cứ cùng nhau ngồi trên chuyến tàu hỏa ngân hà này, đi đến mãi tận cùng nhé..."' },
    { speaker: 'ジョバンニ', role: '• Nhân vật chính', text: '「ああ、僕たちどこまでも一緒に行こう。僕、もうあのさそりのように、ほんとうにみんなの幸のためならば僕のからだなんか百ぺん灼いてもかまわない」', trans: '"Ừ, chúng mình sẽ cùng đi tới bất cứ đâu. Mình đã không còn sợ nữa, nếu vì hạnh phúc của mọi người thì dù thân xác này có bị thiêu cháy trăm lần mình cũng cam lòng."' },
    { speaker: 'カムパネルラ', role: '• Bạn đồng hành', text: '「あ、あそこにきれいな野原があるよ。ほら、りんどうの花が咲いている」', trans: '"Kìa, đằng kia có một cánh đồng thật đẹp! Nhìn xem, hoa gentian đang nở rộ kìa!"' }
  ],
  vnLineIndex: 0
};

// 2. INITIALIZATION
document.addEventListener('DOMContentLoaded', () => {
  setupNavigation();
  renderFlashcard();
  updateUserStatsUI();
});

// 3. NAVIGATION
function setupNavigation() {
  document.querySelectorAll('.nav-link').forEach(btn => {
    btn.addEventListener('click', () => {
      const target = btn.getAttribute('data-tab');
      switchTab(target);
    });
  });
}

function switchTab(tabId) {
  state.currentTab = tabId;

  // Update navigation items
  document.querySelectorAll('.nav-link').forEach(b => {
    b.classList.toggle('active', b.getAttribute('data-tab') === tabId);
  });

  // Breadcrumb text
  const titles = {
    dashboard: 'Trang chủ (Home)',
    library: 'Thư viện tác phẩm (Library)',
    reader: 'Không gian đọc (Reader)',
    vocabulary: 'Sổ từ vựng & SRS (Vocabulary)',
    visualnovel: 'Visual Novel Mode (ビジュアルノベル)'
  };
  const crumb = document.getElementById('crumbCurrent');
  if (crumb) {
    crumb.textContent = titles[tabId] || 'Trang chủ';
  }

  // Update panes
  document.querySelectorAll('.content-pane').forEach(p => {
    p.classList.remove('active');
  });
  const targetPane = document.getElementById(`pane-${tabId}`);
  if (targetPane) {
    targetPane.classList.add('active');
  }
}

// 4. READER WORKSPACE & DICTIONARY
function inspectWord(word) {
  const data = state.dictionaryDB[word] || {
    reading: word,
    jlpt: 'JLPT N3',
    pos: 'Từ ngữ',
    meaning: 'Nghĩa từ điển đang được đồng bộ...',
    example: `Văn cảnh: ...${word}...`
  };

  state.currentDictWord = { word, ...data };

  document.getElementById('inspectEmpty').style.display = 'none';
  const content = document.getElementById('inspectContent');
  content.style.display = 'block';

  document.getElementById('insWord').textContent = word;
  document.getElementById('insReading').textContent = `${data.reading}`;
  document.getElementById('insJlpt').textContent = data.jlpt;
  document.getElementById('insPos').textContent = data.pos;
  document.getElementById('insMeaning').textContent = data.meaning;
  document.getElementById('insExample').textContent = data.example;

  showAstralToast(`🔍 Đã tra cứu từ: 「${word}」 (${data.reading})`);
}

function saveInsWord() {
  if (!state.currentDictWord) return;
  const { word, reading, meaning, jlpt } = state.currentDictWord;

  const tbody = document.getElementById('notebookBody');
  const tr = document.createElement('tr');
  tr.innerHTML = `
    <td><strong class="kanji-main">${word}</strong></td>
    <td class="furi-sub">${reading}</td>
    <td>${meaning}</td>
    <td><span class="tag-jlpt n3">${jlpt.replace('JLPT ', '')}</span></td>
    <td>銀河鉄道の夜</td>
    <td>1 lần</td>
    <td><span class="status-pill due">Cần ôn tập</span></td>
  `;
  tbody.insertBefore(tr, tbody.firstChild);

  state.user.savedWordsCount++;
  state.user.xp += 15;
  checkLevelUp();
  updateUserStatsUI();

  showAstralToast(`⭐ Đã lưu 「${word}」 vào Sổ từ vựng (+15 XP)!`);
}

function setReaderTheme(theme) {
  state.readerTheme = theme;
  const isWarm = theme === 'warm';
  document.body.classList.toggle('theme-warm', isWarm);
  document.getElementById('btnThemeDark').classList.toggle('active', !isWarm);
  document.getElementById('btnThemeWarm').classList.toggle('active', isWarm);
  showAstralToast(isWarm ? '📜 Đã chuyển sang chế độ Giấy ngà bảo vệ mắt.' : '🌙 Đã chuyển sang chế độ Ban đêm dịu mắt.');
}

function changeFontSize(delta) {
  state.fontSize = Math.max(14, Math.min(28, state.fontSize + delta));
  const el = document.getElementById('readingArticle');
  const lbl = document.getElementById('fontLabel');
  if (el) el.style.fontSize = `${state.fontSize}px`;
  if (lbl) lbl.textContent = `${state.fontSize}px`;
}

function toggleReaderBookmark() {
  state.isBookmarked = !state.isBookmarked;
  const star = document.getElementById('rStarIcon');
  const text = document.getElementById('rBookmarkText');
  if (state.isBookmarked) {
    star.textContent = '🌟';
    text.textContent = 'Đã lưu Bookmark';
    showAstralToast('📑 Đã đánh dấu vị trí đọc thành công!');
  } else {
    star.textContent = '⭐';
    text.textContent = 'Đánh dấu trang';
    showAstralToast('Đã xóa đánh dấu vị trí đọc.');
  }
}

function openReader(novelKey) {
  state.currentNovel = novelKey;
  const map = {
    ginga: { title: '銀河鉄道の夜', chapter: 'Chương 1: 一、午後の授業 (Giờ học buổi chiều)' },
    botchan: { title: '坊っちゃん', chapter: 'Chương 1: 第一章 (Chương 1 - Thầy giáo Botchan)' },
    gon: { title: 'ごんぎつね', chapter: 'Chương 1: 第一話 (Phần 1 - Chú cáo nhỏ)' },
    yukiguni: { title: '雪国', chapter: 'Chương 1: 第一章 (Chương 1 - Đường hầm tuyết)' }
  };
  const item = map[novelKey] || map.ginga;
  document.getElementById('rNovelTitle').textContent = item.title;
  document.getElementById('rChapterTitle').textContent = item.chapter;

  switchTab('reader');
}

function selectChapter(num) {
  document.querySelectorAll('.chapter-node').forEach((node, idx) => {
    node.classList.toggle('active', idx + 1 === num);
  });
  showAstralToast(`📖 Đang mở Chương ${num}...`);
}

// 5. FLASHCARDS & SM-2
function renderFlashcard() {
  const card = state.flashcards[state.currentCardIndex];
  if (!card) return;

  const mesh = document.getElementById('cardMesh');
  mesh.classList.remove('flipped');

  document.getElementById('fcKanji').textContent = card.kanji;
  document.getElementById('fcSentence').innerHTML = card.sentence;
  document.getElementById('fcReading').textContent = card.reading;
  document.getElementById('fcMeaning').textContent = card.meaning;
}

function flipCard() {
  const mesh = document.getElementById('cardMesh');
  mesh.classList.toggle('flipped');
}

function submitGrade(quality) {
  const qualityNames = { 1: 'Again (Quên)', 3: 'Hard (Khó)', 4: 'Good (Nhớ)', 5: 'Easy (Rất dễ)' };
  const bonus = quality >= 3 ? 20 : 5;
  state.user.xp += bonus;
  checkLevelUp();
  updateUserStatsUI();

  showAstralToast(`🧠 Thuật toán SM-2: Đánh giá ${qualityNames[quality]} (+${bonus} XP)`);

  state.currentCardIndex = (state.currentCardIndex + 1) % state.flashcards.length;
  setTimeout(() => {
    renderFlashcard();
  }, 220);
}

// 6. QUESTS & GAMIFICATION
function claimQuest(questId, xp) {
  const btn = document.getElementById(`btnClaimQuest${questId}`);
  if (btn) {
    btn.disabled = true;
    btn.textContent = '✓ Đã nhận';
    btn.classList.remove('btn-claim');
    btn.classList.add('badge-claimed');
  }

  state.user.xp += xp;
  checkLevelUp();
  updateUserStatsUI();

  showAstralToast(`🎉 Nhận thành công +${xp} XP từ nhiệm vụ!`);
}

function checkLevelUp() {
  if (state.user.xp >= state.user.requiredXp) {
    state.user.level++;
    state.user.xp -= state.user.requiredXp;
    state.user.requiredXp = Math.round(state.user.requiredXp * 1.35);
    showAstralToast(`🌟 THĂNG CẤP! Bạn đã đạt Cấp độ Level ${state.user.level}!`, 4500);
  }
}

function updateUserStatsUI() {
  const headerXpText = document.getElementById('headerXpText');
  const headerXpFill = document.getElementById('headerXpFill');
  if (headerXpText) {
    headerXpText.textContent = `${state.user.xp} / ${state.user.requiredXp} XP`;
  }
  if (headerXpFill) {
    const pct = Math.min(100, Math.round((state.user.xp / state.user.requiredXp) * 100));
    headerXpFill.style.width = `${pct}%`;
  }
}

// 7. FILTERING & SEARCH
function filterLibrary(level, btn) {
  document.querySelectorAll('.filter-tab-pills .pill-btn').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');

  const cards = document.querySelectorAll('.ln-card');
  cards.forEach(c => {
    if (level === 'all' || c.getAttribute('data-level') === level) {
      c.style.display = 'flex';
    } else {
      c.style.display = 'none';
    }
  });
}

function searchLibrary(query) {
  const q = query.toLowerCase().trim();
  const cards = document.querySelectorAll('.ln-card');
  cards.forEach(c => {
    const text = c.textContent.toLowerCase();
    c.style.display = text.includes(q) ? 'flex' : 'none';
  });
}

function filterNotebook(query) {
  const q = query.toLowerCase().trim();
  const rows = document.querySelectorAll('#notebookBody tr');
  rows.forEach(r => {
    r.style.display = r.textContent.toLowerCase().includes(q) ? '' : 'none';
  });
}

// 8. VISUAL NOVEL
function advanceVnStory() {
  state.vnLineIndex = (state.vnLineIndex + 1) % state.vnLines.length;
  const line = state.vnLines[state.vnLineIndex];
  document.getElementById('vnSentenceJp').textContent = line.text;
  document.getElementById('vnSentenceVi').textContent = line.trans;
}

function playVoice() {
  const text = document.getElementById('vnSentenceJp').textContent;
  if ('speechSynthesis' in window) {
    const u = new SpeechSynthesisUtterance(text);
    u.lang = 'ja-JP';
    window.speechSynthesis.speak(u);
    showAstralToast('🔊 Đang phát âm câu thoại tiếng Nhật...');
  } else {
    showAstralToast('Trình duyệt không hỗ trợ Web Speech API.');
  }
}

function triggerImport() {
  showAstralToast('📥 Nạp tiểu thuyết từ file JSON hoặc TXT vào kho thư viện!');
}

// 9. TOAST
let toastTimer;
function showAstralToast(msg, duration = 3000) {
  const t = document.getElementById('astralToast');
  if (!t) return;
  t.textContent = msg;
  t.classList.add('show');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => {
    t.classList.remove('show');
  }, duration);
}
