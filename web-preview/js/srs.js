// ===================================================================
// KOTOBA ODYSSEY - SRS FLASHCARD ARENA (SM-2 ALGORITHM)
// ===================================================================

let srsIndex = 0;
let isFlipped = false;
let isGrading = false;

function flipCurrentCard() {
  const inner = document.getElementById('flashcardInner');
  if (inner) {
    isFlipped = !isFlipped;
    if (isFlipped) {
      inner.classList.add('flipped');
    } else {
      inner.classList.remove('flipped');
    }
    playCardFlip();
  }
}

async function gradeCard(gradeScore) {
  if (isGrading) return;
  isGrading = true;
  try {
    // Play subtle feedback chime
    playSuccessChime();

    const currentCard = srsDeck[srsIndex];
    if (currentCard && currentCard.id) {
      const res = await fetchFromBackend(`/vocab/${currentCard.id}/review`, {
        method: 'POST',
        body: JSON.stringify({ rating: gradeScore })
      });
      if (res && res.xpEarned) {
        addLocalXp(res.xpEarned);
      }
    }

    // Reset flip
    const inner = document.getElementById('flashcardInner');
    if (inner) {
      inner.classList.remove('flipped');
      isFlipped = false;
    }

    // Next card in deck
    if (srsDeck.length > 0) {
      srsIndex = (srsIndex + 1) % srsDeck.length;
      updateCardUI(srsIndex);
    }
  } finally {
    isGrading = false;
  }
}

function updateCardUI(idx) {
  const container = document.querySelector('.srs-deck-container');
  const arena = document.querySelector('.flashcard-arena');
  const ratingRow = document.querySelector('.srs-rating-row');

  if (!srsDeck || srsDeck.length === 0) {
    if (arena) arena.style.display = 'none';
    if (ratingRow) ratingRow.style.display = 'none';
    let emptyState = document.getElementById('srsEmptyState');
    if (!emptyState && container) {
      emptyState = document.createElement('div');
      emptyState.id = 'srsEmptyState';
      emptyState.className = 'empty-state-card';
      emptyState.innerHTML = `
        <div class="empty-state-icon">🎉</div>
        <div class="empty-state-title">Đã hoàn thành ôn tập SRS hôm nay!</div>
        <div class="empty-state-desc">Tuyệt vời! Bạn không còn từ vựng nào đến hạn ôn tập lúc này. Hãy tiếp tục đọc thêm các chương truyện mới để tích lũy thêm từ vựng nhé.</div>
        <button class="empty-state-btn" onclick="openLibraryOrReader()">Khám phá Thư viện truyện</button>
      `;
      container.appendChild(emptyState);
    } else if (emptyState) {
      emptyState.style.display = 'flex';
    }
    return;
  }

  const emptyState = document.getElementById('srsEmptyState');
  if (emptyState) emptyState.style.display = 'none';
  if (arena) arena.style.display = 'flex';
  if (ratingRow) ratingRow.style.display = 'flex';

  const card = srsDeck[idx];
  if (!card) return;

  setTimeout(() => {
    const kEl = document.getElementById('cardKanji');
    const romEl = document.getElementById('cardRomaji');
    const rEl = document.getElementById('cardReading');
    const mEl = document.getElementById('cardMeaning');
    const sEl = document.getElementById('cardSentence');
    const svnEl = document.getElementById('cardSentenceVn');
    const counterEl = document.getElementById('srsCounter');
    const jfEl = document.getElementById('cardJlptFront');
    const jbEl = document.getElementById('cardJlptBack');

    if (kEl) kEl.textContent = card.kanji || card.word;
    if (romEl) romEl.textContent = card.romaji || card.reading;
    if (rEl) rEl.textContent = card.reading;
    if (mEl) mEl.textContent = card.meaning;
    if (sEl) sEl.textContent = card.sentence || card.contextSentence;
    if (svnEl) svnEl.textContent = card.sentenceVn || 'Ngữ cảnh trong chương sách';
    if (counterEl) counterEl.textContent = `Thẻ ${idx + 1} / ${srsDeck.length} đến hạn`;

    if (jfEl) {
      jfEl.textContent = `JLPT ${card.level}`;
      jfEl.className = `chip ${card.level.toLowerCase()}`;
    }
    if (jbEl) {
      jbEl.textContent = `JLPT ${card.level}`;
      jbEl.className = `chip ${card.level.toLowerCase()}`;
    }
  }, 150);
}

function speakCurrentCard() {
  const card = srsDeck[srsIndex];
  if ('speechSynthesis' in window && card) {
    window.speechSynthesis.cancel();
    const u = new SpeechSynthesisUtterance(card.reading || card.kanji);
    u.lang = 'ja-JP';
    u.rate = 0.9;
    window.speechSynthesis.speak(u);
  }
}

