// ===================================================================
// KOTOBA ODYSSEY - CHAPTER COMPREHENSION QUIZ CONTROLLER
// ===================================================================

let activeQuizData = null;
let activeQuizQuestionIdx = 0;
let activeQuizAnswers = {};

async function openChapterQuiz(novelId, chapterNum) {
  const targetNovelId = novelId || currentNovelDbId || 1;
  const targetChapterNum = chapterNum || currentChapterNum || 1;

  const modal = document.getElementById('chapterQuizModal');
  if (!modal) return;

  modal.classList.add('open');
  modal.style.display = 'flex';
  document.getElementById('quizQuestionBody').style.display = 'block';
  document.getElementById('quizResultsBody').style.display = 'none';
  document.getElementById('btnNextQuizQuestion').style.display = 'none';

  try {
    const res = await fetch(`${API_BASE}/novels/${targetNovelId}/chapters/${targetChapterNum}/quiz`);
    if (res.ok) {
      activeQuizData = await res.json();
      activeQuizQuestionIdx = 0;
      activeQuizAnswers = {};

      const titleEl = document.getElementById('quizModalTitle');
      if (titleEl) {
        titleEl.textContent = `${activeQuizData.novelTitle} • ${activeQuizData.chapterTitle}`;
      }

      renderCurrentQuizQuestion();
    } else {
      alert('Không thể tải bài tập trắc nghiệm cho chương này.');
      closeChapterQuiz();
    }
  } catch (err) {
    console.error('Failed to load chapter quiz', err);
    closeChapterQuiz();
  }
}

function closeChapterQuiz(e) {
  if (e && e.target !== e.currentTarget && !e.target.classList.contains('modal-close-btn') && e.target.tagName !== 'BUTTON') {
    return;
  }
  const modal = document.getElementById('chapterQuizModal');
  if (modal) {
    modal.classList.remove('open');
    modal.style.display = 'none';
  }
}

function renderCurrentQuizQuestion() {
  if (!activeQuizData || !activeQuizData.questions || activeQuizData.questions.length === 0) return;

  const q = activeQuizData.questions[activeQuizQuestionIdx];
  const total = activeQuizData.questions.length;

  const badge = document.getElementById('quizProgressBadge');
  const catLabel = q.category === 'Vocabulary' ? '🔤 Từ vựng' : (q.category === 'Grammar' ? '📐 Ngữ pháp' : '📖 Đọc hiểu');
  if (badge) badge.innerHTML = `Câu ${activeQuizQuestionIdx + 1} / ${total} <span class="chip" style="margin-left: 6px; font-size: 11px;">${catLabel}</span>`;

  const quoteEl = document.getElementById('quizContextQuote');
  if (quoteEl) {
    quoteEl.textContent = q.contextSentence ? `"${q.contextSentence}"` : '';
    quoteEl.style.display = q.contextSentence ? 'block' : 'none';
  }

  const jpEl = document.getElementById('quizQuestionJp');
  const vnEl = document.getElementById('quizQuestionVn');
  if (jpEl) jpEl.textContent = q.questionJp;
  if (vnEl) vnEl.textContent = q.questionVn;

  const expBox = document.getElementById('quizExplanationBox');
  const alreadyChosen = activeQuizAnswers[q.questionId];

  if (expBox) {
    if (alreadyChosen !== undefined) {
      const expContent = document.getElementById('quizExpContent');
      if (expContent) expContent.textContent = q.explanation || 'Hãy ghi nhớ cấu trúc ngữ pháp và từ vựng trong câu hỏi này.';
      expBox.style.display = 'block';
    } else {
      expBox.style.display = 'none';
    }
  }

  const grid = document.getElementById('quizOptionsGrid');
  if (!grid) return;
  grid.innerHTML = '';

  const labels = ['A', 'B', 'C', 'D'];
  q.options.forEach((opt, idx) => {
    const btn = document.createElement('button');
    btn.className = 'quiz-option-btn';
    btn.innerHTML = `
      <span class="opt-badge">${labels[idx] || (idx + 1)}</span>
      <span>${opt}</span>
    `;

    if (alreadyChosen !== undefined) {
      btn.classList.add('disabled');
      if (idx === q.correctIndex) {
        btn.classList.add('correct');
      } else if (idx === alreadyChosen) {
        btn.classList.add('wrong');
      }
    } else {
      btn.onclick = () => selectQuizOption(idx);
    }
    grid.appendChild(btn);
  });

  const nextBtn = document.getElementById('btnNextQuizQuestion');
  if (nextBtn) {
    if (alreadyChosen !== undefined) {
      const isLast = activeQuizQuestionIdx === activeQuizData.questions.length - 1;
      nextBtn.textContent = isLast ? 'Hoàn thành bài tập ➔' : 'Câu tiếp theo ➔';
      nextBtn.style.display = 'inline-block';
    } else {
      nextBtn.style.display = 'none';
    }
  }
}

function selectQuizOption(choiceIdx) {
  const q = activeQuizData.questions[activeQuizQuestionIdx];
  activeQuizAnswers[q.questionId] = choiceIdx;

  const buttons = document.querySelectorAll('.quiz-option-btn');
  buttons.forEach((btn, idx) => {
    btn.classList.add('disabled');
    btn.onclick = null;
    if (idx === q.correctIndex) {
      btn.classList.add('correct');
    } else if (idx === choiceIdx) {
      btn.classList.add('wrong');
    }
  });

  const expBox = document.getElementById('quizExplanationBox');
  const expContent = document.getElementById('quizExpContent');
  if (expBox && expContent) {
    expContent.textContent = q.explanation || 'Hãy ghi nhớ cấu trúc ngữ pháp và từ vựng trong câu hỏi này.';
    expBox.style.display = 'block';
  }

  if (choiceIdx === q.correctIndex) {
    playSuccessChime();
  } else {
    playKeyTick();
  }

  const nextBtn = document.getElementById('btnNextQuizQuestion');
  if (nextBtn) {
    const isLast = activeQuizQuestionIdx === activeQuizData.questions.length - 1;
    nextBtn.textContent = isLast ? 'Hoàn thành bài tập ➔' : 'Câu tiếp theo ➔';
    nextBtn.style.display = 'inline-block';
  }
}

async function advanceQuizQuestion() {
  if (activeQuizQuestionIdx < activeQuizData.questions.length - 1) {
    activeQuizQuestionIdx++;
    renderCurrentQuizQuestion();
  } else {
    await submitQuizResults();
  }
}

async function submitQuizResults() {
  try {
    const res = await fetch(`${API_BASE}/novels/${activeQuizData.novelId}/chapters/${activeQuizData.chapterNumber}/quiz/submit`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        novelId: activeQuizData.novelId,
        chapterNumber: activeQuizData.chapterNumber,
        answers: activeQuizAnswers
      })
    });

    if (res.ok) {
      const result = await res.json();
      displayQuizResults(result);
      playSuccessChime();
      fetchUserProgress();
    }
  } catch (err) {
    console.error('Failed to submit quiz results', err);
  }
}

function displayQuizResults(res) {
  document.getElementById('quizQuestionBody').style.display = 'none';
  document.getElementById('quizResultsBody').style.display = 'block';
  document.getElementById('btnNextQuizQuestion').style.display = 'none';

  const scoreBadge = document.getElementById('quizScoreBadge');
  const heading = document.getElementById('quizResultHeading');
  const desc = document.getElementById('quizResultDesc');
  const xpBadge = document.getElementById('quizXpBadge');
  const reviewList = document.getElementById('quizReviewList');

  if (scoreBadge) scoreBadge.textContent = `${res.scorePercentage}%`;
  if (heading) heading.textContent = res.passed ? '🎉 Xuất sắc! Vượt qua bài kiểm tra' : 'Cố gắng lên! Hãy ôn lại các điểm ngữ pháp';
  
  const catBreakdown = `
    <div style="display: flex; gap: 8px; justify-content: center; margin: 10px 0;">
      <span class="chip" style="background: rgba(59, 130, 246, 0.2); color: #60A5FA;">📖 Đọc hiểu: ${res.readingScorePercent || res.scorePercentage}%</span>
      <span class="chip" style="background: rgba(16, 185, 129, 0.2); color: #34D399;">🔤 Từ vựng: ${res.vocabScorePercent || res.scorePercentage}%</span>
      <span class="chip" style="background: rgba(245, 158, 11, 0.2); color: #FBBF24;">📐 Ngữ pháp: ${res.grammarScorePercent || res.scorePercentage}%</span>
    </div>
  `;

  if (desc) desc.innerHTML = `<div>${escapeHtml(res.feedbackSummary)}</div>${catBreakdown}`;
  if (xpBadge) xpBadge.textContent = res.xpEarned > 0 ? `+${res.xpEarned} XP Đã nhận!` : (res.passed ? '✓ Đã nhận XP trước đó' : 'Chưa đạt 60% (+0 XP)');

  if (reviewList) {
    reviewList.innerHTML = '';
    res.questionReviews.forEach((rev, idx) => {
      const item = document.createElement('div');
      item.className = 'quiz-review-item';
      item.style.cssText = 'padding: 10px 14px; background: rgba(255,255,255,0.03); border-radius: 6px; margin-bottom: 8px; border-left: 3px solid ' + (rev.isCorrect ? '#10B981' : '#EF4444');
      const targetPill = rev.targetKnowledge ? `<span class="chip" style="font-size: 11px; margin-left: 6px;">${escapeHtml(rev.targetKnowledge)}</span>` : '';
      item.innerHTML = `
        <div style="font-weight: 700; color: #FFF; font-size: 13.5px; margin-bottom: 4px; display: flex; align-items: center; justify-content: space-between;">
          <span>${rev.isCorrect ? '✓' : '✗'} Câu ${idx + 1}: ${escapeHtml(rev.questionJp)}</span>
          ${targetPill}
        </div>
        <div style="font-size: 12.5px; color: var(--text-secondary); line-height: 1.4;">
          ${escapeHtml(rev.explanation)}
        </div>
      `;
      reviewList.appendChild(item);
    });
  }
}

// Keyboard shortcuts (Space to flip card in SRS, Space to advance in VN, F for Focus, Escape to close modals)
document.addEventListener('keydown', (e) => {
  const activeEl = document.activeElement;
  const isInput = activeEl && (activeEl.tagName === 'INPUT' || activeEl.tagName === 'TEXTAREA');

  const srsPane = document.getElementById('pane-srs');
  if (srsPane && srsPane.classList.contains('active')) {
    if (e.code === 'Space' && !isInput) {
      e.preventDefault();
      flipCurrentCard();
      return;
    }
  }

  const vnPane = document.getElementById('pane-vn');
  if (vnPane && vnPane.classList.contains('active')) {
    if (!isInput) {
      if (e.key === 'f' || e.key === 'F') {
        e.preventDefault();
        toggleVnFocusMode();
        return;
      }
      if (e.code === 'Space' || e.key === 'Enter') {
        e.preventDefault();
        advanceVnDialogue();
        return;
      }
      if (e.key === 't' || e.key === 'T') {
        e.preventDefault();
        toggleVnTransVisibility();
        return;
      }
      if (e.key === 'a' || e.key === 'A') {
        e.preventDefault();
        toggleVnAuto();
        return;
      }
      if (e.key === 's' || e.key === 'S') {
        e.preventDefault();
        speakVnLine();
        return;
      }
    }
  }

  if (e.code === 'Escape') {
    if (isVnFocusMode) {
      toggleVnFocusMode();
      return;
    }
    closeSentenceAnalysis();
    closeImportModal();
    closeChapterQuiz();
    closeVnLearningDrawer();
    closeVnChallenge();
    const logModal = document.getElementById('vnLogModal');
    if (logModal && logModal.classList.contains('open')) {
      logModal.classList.remove('open');
    }
  }
});

