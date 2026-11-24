
// ===================================================================
// KOTOBA ODYSSEY - SATORI READER CONTROLLER
// Prose rendering, furigana toggle, interactive lookups & sentence analysis
// ===================================================================

// currentInspectedId is defined in navigation.js
var currentChapterNum = typeof currentChapterNum !== 'undefined' ? currentChapterNum : 1;
let currentWordLookup = {
  word: '銀河鉄道',
  furigana: 'ぎんがてつどう',
  romaji: 'Ginga Tetsudou',
  meaning: 'Đường sắt Ngân Hà (Tuyến tàu vũ trụ huyền ảo xuyên qua các chòm sao)',
  level: 'N3'
};

// ================= CONDITIONAL TRANSLATION STATE & CONTROLLER =================
let currentChapterParagraphs = [];
const unlockedTranslations = new Set();
const paragraphLookups = new Set();
let pendingUnlockParagraphIdx = null;

function closeCondTransModal(e) {
  if (e && e.target !== e.currentTarget && !e.target.classList.contains('btn-secondary')) {
    return;
  }
  const modal = document.getElementById('condTransModal');
  if (modal) modal.classList.remove('open');
  pendingUnlockParagraphIdx = null;
}

function handleConditionalTranslate(paraIdx) {
  if (unlockedTranslations.has(paraIdx)) {
    toggleTransBox(paraIdx);
    return;
  }

  if (paragraphLookups.has(paraIdx)) {
    executeUnlockParagraph(paraIdx);
    playSuccessChime();
    return;
  }

  pendingUnlockParagraphIdx = paraIdx;
  const modal = document.getElementById('condTransModal');
  if (modal) modal.classList.add('open');
}

function confirmUnlockWithXp() {
  if (pendingUnlockParagraphIdx === null) return;
  const idx = pendingUnlockParagraphIdx;
  addLocalXp(-5);
  closeCondTransModal();
  executeUnlockParagraph(idx);
  playSuccessChime();
}

async function executeUnlockParagraph(paraIdx) {
  unlockedTranslations.add(paraIdx);

  const btn = document.getElementById(`btnTrans_${paraIdx}`);
  if (btn) {
    btn.className = 'btn-cond-translate unlocked';
    btn.innerHTML = '<span>🇻🇳 Xem bản dịch</span>';
  }

  const rawText = currentChapterParagraphs[paraIdx] || '';
  if (!rawText) return;

  const block = document.getElementById(`proseBlock_${paraIdx}`);
  if (!block) return;

  let existingBox = document.getElementById(`transBox_${paraIdx}`);
  if (!existingBox) {
    existingBox = document.createElement('div');
    existingBox.className = 'prose-trans-box';
    existingBox.id = `transBox_${paraIdx}`;
    existingBox.innerHTML = `
      <div class="trans-head-row">
        <span class="trans-badge">🇻🇳 BẢN DỊCH TIẾNG VIỆT • VĂN PHONG LIGHT NOVEL</span>
        <button class="trans-close-btn" onclick="toggleTransBox(${paraIdx})" title="Thu gọn bản dịch">✕</button>
      </div>
      <div class="trans-text-vn" id="transText_${paraIdx}">⏳ Đang phân tích và dịch ngữ cảnh...</div>
      <div class="trans-grammar-row" id="transGrammar_${paraIdx}"></div>
      <div class="trans-nuance-tip" id="transNuance_${paraIdx}"></div>
    `;
    block.appendChild(existingBox);

    try {
      const res = await fetchFromBackend('/translate', {
        method: 'POST',
        body: JSON.stringify({ text: rawText })
      });
      const transEl = document.getElementById(`transText_${paraIdx}`);
      const grammarEl = document.getElementById(`transGrammar_${paraIdx}`);
      const nuanceEl = document.getElementById(`transNuance_${paraIdx}`);

      if (transEl) {
        transEl.textContent = (res && res.translatedText) ? res.translatedText : rawText;
      }

      const grammarTags = [];
      if (/てしまう|ちゃった|ちまった/.test(rawText)) grammarTags.push('〜てしまう (Lỡ / Hoàn tất)');
      if (/んだが|のだが|んです/.test(rawText)) grammarTags.push('〜んだが (Giải thích bối cảnh)');
      if (/わけにはいかない/.test(rawText)) grammarTags.push('〜わけにはいかない (Không thể)');
      if (/ている|ていた/.test(rawText)) grammarTags.push('〜ている (Trạng thái đang tiếp diễn)');
      if (/そうだ|らしい/.test(rawText)) grammarTags.push('〜そうだ / らしい (Dường như / Phán đoán)');
      if (/かもしれない/.test(rawText)) grammarTags.push('〜かもしれない (Có lẽ)');
      if (/ながら/.test(rawText)) grammarTags.push('〜ながら (Song song 2 hành động)');
      if (/させられた|された/.test(rawText)) grammarTags.push('Thể bị động / Sai khiến');
      if (grammarTags.length === 0) grammarTags.push('Cấu trúc tự sự / Đối thoại học đường');

      if (grammarEl) {
        grammarEl.innerHTML = grammarTags.map(t => `<span class="trans-grammar-tag">${escapeHtml(t)}</span>`).join('');
      }

      if (nuanceEl) {
        let nuance = 'Văn phong tự nhiên của nhân vật trong tiểu thuyết, kết hợp cảm xúc và ngữ cảnh tình huống.';
        if (/俺|僕/.test(rawText)) nuance = 'Ngôi thứ nhất (Ore/Boku) thể hiện tâm lý nam chính bộc trực, gần gũi.';
        else if (/私/.test(rawText)) nuance = 'Ngôi thứ nhất (Watashi) mang sắc thái trang nhã, nữ tính hoặc lịch sự.';
        nuanceEl.innerHTML = `💡 <em>Sắc thái:</em> ${escapeHtml(nuance)}`;
      }
    } catch (err) {
      console.warn('Translate error', err);
    }
  } else {
    existingBox.style.display = 'block';
  }
}

function toggleTransBox(paraIdx) {
  const box = document.getElementById(`transBox_${paraIdx}`);
  if (box) {
    box.style.display = (box.style.display === 'none') ? 'block' : 'none';
  }
}

function extractJapaneseSentence(paragraph, targetWord) {
  if (!paragraph) return '';
  const sentences = paragraph.split(/(?<=[。！？\n])/);
  for (const s of sentences) {
    if (s.includes(targetWord)) {
      return s.trim();
    }
  }
  return paragraph.trim();
}

async function handleWordLookupInParagraph(element, word, paraIdx) {
  paragraphLookups.add(paraIdx);
  
  if (!unlockedTranslations.has(paraIdx)) {
    const btn = document.getElementById(`btnTrans_${paraIdx}`);
    if (btn) {
      btn.className = 'btn-cond-translate';
      btn.style.borderColor = 'rgba(16, 185, 129, 0.6)';
      btn.style.color = '#10B981';
      btn.innerHTML = '<span>🔓 Đủ điều kiện! Bấm để xem dịch</span>';
    }
  }

  let reading = word;
  let meaning = 'Đang tra cứu từ điển...';
  let level = 'N3';
  let sentence = extractJapaneseSentence(currentChapterParagraphs[paraIdx] || '', word);

  try {
    const dictRes = await fetchFromBackend(`/dictionary/lookup?query=${encodeURIComponent(word)}`);
    if (dictRes && (dictRes.term || dictRes.word)) {
      word = dictRes.term || dictRes.word;
      reading = dictRes.reading || word;
      let meaningText = dictRes.meaning || 'Từ vựng tiếng Nhật';
      if (dictRes.inflectionNote) {
        meaningText = `[${dictRes.inflectionNote}] ${meaningText}`;
      }
      meaning = meaningText;
      level = dictRes.jlptLevel || 'N3';
    }
  } catch (e) {
    // fallback
  }

  lookup(element, word, reading, meaning, level, sentence);
}

function formatParagraphInteractive(para, paraIdx) {
  const regex = /([\u4e00-\u9faf\u3400-\u4dbf]+|[\u30a0-\u30ff]+|[\u3040-\u309f]{1,4}|[a-zA-Z0-9]+|[^\s\w\u3040-\u30ff\u4e00-\u9faf]+)/gu;
  const parts = para.match(regex) || [para];
  
  return parts.map(tok => {
    if (/^[\s、。！？「」『』（）…—\.,!?\(\)"]+$/.test(tok)) {
      return escapeHtml(tok);
    }
    return `<span class="jp-w" onclick="handleWordLookupInParagraph(this, '${escapeHtml(tok)}', ${paraIdx})">${escapeHtml(tok)}</span>`;
  }).join('');
}

function renderReaderProseContent(rawText) {
  const container = document.getElementById('readingProse');
  if (!container) return;

  unlockedTranslations.clear();
  paragraphLookups.clear();

  currentChapterParagraphs = (rawText || '').split(/\r?\n/).filter(p => p.trim().length > 0);
  container.innerHTML = '';

  currentChapterParagraphs.forEach((para, idx) => {
    const block = document.createElement('div');
    block.className = 'prose-block';
    block.id = `proseBlock_${idx}`;
    block.innerHTML = `
      <p>${formatParagraphInteractive(para, idx)}</p>
      <div class="prose-action-bar">
        <button class="btn-sentence-syntax" onclick="explainCustomParagraphByIndex(${idx})">
          <span>🔍 Phân tích ngữ pháp đoạn ${idx + 1}</span>
        </button>
        <button class="btn-cond-translate locked" id="btnTrans_${idx}" onclick="handleConditionalTranslate(${idx})">
          <span>🔒 Dịch đoạn (Có điều kiện)</span>
        </button>
      </div>
    `;
    container.appendChild(block);
  });
}
window.renderReaderProseContent = renderReaderProseContent;

function explainCustomParagraphByIndex(idx) {
  const text = currentChapterParagraphs[idx] || '';
  if (!text) return;
  explainCustomParagraph(text);
}

async function explainCustomParagraph(text, reqType = 'explain') {
  const modal = document.getElementById('sentenceAnalysisModal');
  const sentJp = document.getElementById('sentModalJp');
  const sentVn = document.getElementById('sentModalVn');
  const tokensList = document.getElementById('sentTokensList');
  const grammarNote = document.getElementById('sentGrammarNote');
  const providerBadge = document.getElementById('sentModalProviderBadge');
  const followUpHistory = document.getElementById('aiFollowUpHistory');

  if (!modal) return;
  modal.classList.add('open');
  modal.style.display = 'flex';

  if (followUpHistory && reqType === 'explain') {
    followUpHistory.innerHTML = '';
  }

  currentSentenceAnalysis = {
    id: 'custom',
    level: booksData[currentInspectedId]?.level || 'N3',
    jp: text,
    vn: '',
    grammarNote: '',
    tokens: []
  };

  if (sentJp) sentJp.textContent = text;
  if (sentVn) sentVn.textContent = '⏳ Đang phân tích cấu trúc cú pháp & dịch nghĩa...';
  if (tokensList && reqType === 'explain') tokensList.innerHTML = '<div style="color: var(--text-dim); padding: 12px;">Đang phân tích từ loại cú pháp...</div>';
  if (grammarNote) grammarNote.innerHTML = '<strong>⏳ Đang kết nối AI Tutor Engine...</strong>';
  if (providerBadge) {
    providerBadge.textContent = '⏳ Đang phân tích...';
    providerBadge.style.color = '#F59E0B';
  }

  const book = booksData[currentInspectedId] || booksData.ginga;

  try {
    const aiRes = await fetchFromBackend('/ai/explain', {
      method: 'POST',
      body: JSON.stringify({
        sentence: text,
        context: currentChapterParagraphs.slice(Math.max(0, (currentInspectedId || 0) - 1), (currentInspectedId || 0) + 2).join(' ') || book.title || 'Novel context',
        novelTitle: book.title || 'Tiểu thuyết',
        chapterTitle: `Chương ${currentChapterNum || 1}`,
        chapterNumber: currentChapterNum || 1,
        jlptLevel: book.level || 'N3',
        requestType: reqType,
        includeExercise: true
      })
    });

    if (aiRes && aiRes.success) {
      if (providerBadge) {
        if (aiRes.isAiGenerated) {
          providerBadge.textContent = '🤖 ' + (aiRes.provider || 'Gemini AI');
          providerBadge.style.color = '#10B981';
          providerBadge.style.borderColor = 'rgba(16, 185, 129, 0.4)';
        } else {
          providerBadge.textContent = '⚡ ' + (aiRes.provider || 'Local NLP');
          providerBadge.style.color = '#818CF8';
          providerBadge.style.borderColor = 'rgba(99, 102, 241, 0.4)';
        }
      }

      if (sentVn) sentVn.textContent = aiRes.translationVn || 'Bản dịch ngữ cảnh';
      currentSentenceAnalysis.vn = aiRes.translationVn || '';
      currentSentenceAnalysis.grammarNote = aiRes.grammarInsight || '';

      // 1. Render Structure & Grammar Note Box
      if (grammarNote) {
        let structureHtml = '';
        if (aiRes.sentenceStructure) {
          const s = aiRes.sentenceStructure;
          structureHtml = `
            <div style="background: rgba(15, 23, 42, 0.6); border: 1px solid rgba(59, 130, 246, 0.3); border-radius: 8px; padding: 12px; margin-bottom: 12px;">
              <div style="font-weight: 700; color: #38BDF8; font-size: 13px; margin-bottom: 8px; display: flex; align-items: center; gap: 6px;">
                <span>📐</span> <span>PHÂN TÍCH CẤU TRÚC CÂU (SENTENCE STRUCTURE)</span>
              </div>
              <div style="display: grid; grid-template-columns: 100px 1fr; row-gap: 6px; font-size: 12.5px;">
                <span style="color: #94A3B8;">Chủ ngữ:</span>
                <span style="color: #F8FAFC; font-weight: 600;">${escapeHtml(s.subject || '(Ẩn trong mạch văn)')}</span>
                <span style="color: #94A3B8;">Vị ngữ:</span>
                <span style="color: #10B981; font-weight: 600;">${escapeHtml(s.predicate || '')}</span>
                ${s.object ? `<span style="color: #94A3B8;">Tân ngữ:</span><span style="color: #F59E0B;">${escapeHtml(s.object)}</span>` : ''}
                ${s.clauseRelations ? `<span style="color: #94A3B8;">Mệnh đề:</span><span style="color: #CBD5E1;">${escapeHtml(s.clauseRelations)}</span>` : ''}
              </div>
              ${s.modifiers && s.modifiers.length > 0 ? `
                <div style="margin-top: 8px; font-size: 12px; color: #94A3B8;">
                  <span style="font-weight: 600;">Thành phần bổ nghĩa:</span> ${s.modifiers.map(m => `<span class="chip" style="margin: 2px;">${escapeHtml(m)}</span>`).join('')}
                </div>` : ''}
            </div>
          `;
        }

        let detailedGrammarHtml = '';
        if (aiRes.detailedGrammar && aiRes.detailedGrammar.length > 0) {
          detailedGrammarHtml = `
            <div style="margin-bottom: 12px;">
              <div style="font-weight: 700; color: #F59E0B; font-size: 13px; margin-bottom: 8px;">📖 ĐIỂM NGỮ PHÁP THEN CHỐT</div>
              ${aiRes.detailedGrammar.map(g => `
                <div style="background: rgba(245, 158, 11, 0.08); border-left: 3px solid #F59E0B; padding: 8px 12px; margin-bottom: 6px; border-radius: 4px; font-size: 12.5px;">
                  <div style="font-weight: 700; color: #FCD34D;">${escapeHtml(g.pattern)}: <span style="font-weight: 400; color: #F1F5F9;">${escapeHtml(g.meaning)}</span></div>
                  <div style="color: #94A3B8; font-size: 11.5px; margin-top: 2px;">Công thức: ${escapeHtml(g.structure)}</div>
                  <div style="color: #CBD5E1; font-size: 12px; margin-top: 4px;">${escapeHtml(g.usageAndNuance)}</div>
                  ${g.exampleSentence ? `<div style="margin-top: 4px; font-size: 11.5px; color: #6EE7B7;">Ví dụ: ${escapeHtml(g.exampleSentence)} ${g.exampleTranslation ? `(${escapeHtml(g.exampleTranslation)})` : ''}</div>` : ''}
                </div>
              `).join('')}
            </div>
          `;
        }

        let learningNotesHtml = '';
        if (aiRes.learningNotes) {
          const ln = aiRes.learningNotes;
          learningNotesHtml = `
            <div style="background: rgba(99, 102, 241, 0.08); border: 1px solid rgba(99, 102, 241, 0.2); border-radius: 6px; padding: 10px 12px; margin-bottom: 12px; font-size: 12px;">
              <div style="font-weight: 700; color: #818CF8; margin-bottom: 4px;">💡 LỜI KHUYÊN HỌC TẬP (${escapeHtml(ln.jlptLevelAdvice || 'Trình độ ' + (aiRes.jlptLevel || 'N3'))})</div>
              ${ln.keyTakeaways ? ln.keyTakeaways.map(t => `<div style="color: #E2E8F0; margin-bottom: 2px;">• ${escapeHtml(t)}</div>`).join('') : ''}
              ${ln.commonMistakes && ln.commonMistakes.length > 0 ? `<div style="color: #F87171; margin-top: 4px; font-size: 11.5px;">⚠️ <strong>Lỗi thường gặp:</strong> ${ln.commonMistakes.map(m => escapeHtml(m)).join('; ')}</div>` : ''}
            </div>
          `;
        }

        let exerciseHtml = '';
        if (aiRes.exercise && aiRes.exercise.question) {
          const ex = aiRes.exercise;
          exerciseHtml = `
            <div id="aiExerciseWidget" style="background: rgba(30, 41, 59, 0.9); border: 1px solid #10B981; border-radius: 8px; padding: 12px; margin-bottom: 12px;">
              <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
                <span style="font-weight: 700; color: #10B981; font-size: 12.5px;">🎯 BÀI TẬP NHANH CỦNG CỐ KIẾN THỨC</span>
                <span class="chip n3" style="font-size: 11px;">+10 XP</span>
              </div>
              <div style="font-size: 13px; font-weight: 600; color: #F8FAFC; margin-bottom: 6px;">${escapeHtml(ex.question)}</div>
              ${ex.promptJapanese ? `<div style="font-size: 13px; color: #FCD34D; font-family: 'Noto Serif JP', serif; margin-bottom: 10px; background: rgba(0,0,0,0.3); padding: 6px 10px; border-radius: 4px;">${escapeHtml(ex.promptJapanese)}</div>` : ''}
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-bottom: 10px;">
                ${(ex.options || []).map((opt, oIdx) => `
                  <button class="btn btn-secondary btn-sm" id="btnExOpt_${oIdx}" style="text-align: left; padding: 6px 10px; font-size: 12px;" onclick="checkAiExerciseOption(${oIdx}, ${ex.correctOptionIndex}, '${escapeHtml(ex.explanation).replace(/'/g, "\\'")}')">
                    ${String.fromCharCode(65 + oIdx)}. ${escapeHtml(opt)}
                  </button>
                `).join('')}
              </div>
              <div id="aiExerciseFeedback" style="display: none; padding: 8px 10px; border-radius: 4px; font-size: 12px; line-height: 1.4;"></div>
            </div>
          `;
        }

        let uncertaintyHtml = aiRes.uncertainties 
          ? `<div style="margin-top: 8px; padding: 6px 10px; background: rgba(245, 158, 11, 0.1); border-left: 3px solid #F59E0B; font-size: 11.5px; color: #F59E0B;">⚠️ ${escapeHtml(aiRes.uncertainties)}</div>`
          : '';

        grammarNote.innerHTML = `
          ${structureHtml}
          ${detailedGrammarHtml}
          ${exerciseHtml}
          ${learningNotesHtml}
          <div style="margin-top: 8px; font-style: italic; color: #94A3B8; font-size: 12px;">📖 Sắc thái văn học: ${escapeHtml(aiRes.culturalOrLiteraryNuance || 'Phong cách văn học Nhật Bản.')}</div>
          ${uncertaintyHtml}
        `;
      }

      // 2. Render Vocabulary Tokens
      if (tokensList) {
        tokensList.innerHTML = '';
        const listToRender = (aiRes.detailedVocabulary && aiRes.detailedVocabulary.length > 0)
          ? aiRes.detailedVocabulary
          : (aiRes.tokens || []);

        listToRender.slice(0, 16).forEach(tok => {
          const card = document.createElement('div');
          card.className = 'grammar-token-card';
          card.innerHTML = `
            <div class="token-head-row">
              <span class="token-word">${escapeHtml(tok.word || tok.surface)}</span>
              <span class="token-pos-badge">${escapeHtml(tok.partOfSpeech || '')}</span>
            </div>
            <div style="font-size: 11.5px; color: var(--text-dim);">${escapeHtml(tok.reading || '')}</div>
            <div class="token-role" style="color: #6EE7B7;">${escapeHtml(tok.meaningInContext || tok.meaning || '')}</div>
            <div class="token-meaning" style="font-size: 11px; color: #94A3B8;">Gốc: ${escapeHtml(tok.baseForm || tok.word || '')}</div>
            <button class="token-srs-btn" onclick="saveTokenToSrs('${escapeHtml(tok.word || tok.surface)}', '${escapeHtml(tok.reading || '')}', '${escapeHtml(tok.meaningInContext || tok.meaning || '')}', 'N3', this)">
              + Thêm thẻ SRS
            </button>
          `;
          tokensList.appendChild(card);
        });
      }
    } else {
      if (sentVn) sentVn.textContent = 'Không thể tải phân tích. Vui lòng thử lại.';
      if (grammarNote) grammarNote.innerHTML = '<span style="color: #EF4444;">Lỗi kết nối máy chủ AI Tutor.</span>';
    }
  } catch (err) {
    console.warn('AI explain error', err);
    if (sentVn) sentVn.textContent = 'Lỗi kết nối dịch vụ.';
    if (grammarNote) grammarNote.innerHTML = '<span style="color: #EF4444;">Lỗi kết nối AI Tutor.</span>';
  }
}

function checkAiExerciseOption(selectedIdx, correctIdx, explanation) {
  const fb = document.getElementById('aiExerciseFeedback');
  if (!fb) return;

  for (let i = 0; i < 4; i++) {
    const btn = document.getElementById(`btnExOpt_${i}`);
    if (btn) {
      btn.disabled = true;
      if (i === correctIdx) {
        btn.style.backgroundColor = '#10B981';
        btn.style.borderColor = '#10B981';
        btn.style.color = '#FFF';
      } else if (i === selectedIdx) {
        btn.style.backgroundColor = '#EF4444';
        btn.style.borderColor = '#EF4444';
        btn.style.color = '#FFF';
      }
    }
  }

  fb.style.display = 'block';
  if (selectedIdx === correctIdx) {
    fb.style.backgroundColor = 'rgba(16, 185, 129, 0.15)';
    fb.style.border = '1px solid #10B981';
    fb.style.color = '#6EE7B7';
    fb.innerHTML = `🎉 <strong>Chính xác! (+10 XP)</strong> ${escapeHtml(explanation)}`;
    playSuccessChime();
    addLocalXp(10);
  } else {
    fb.style.backgroundColor = 'rgba(239, 68, 68, 0.15)';
    fb.style.border = '1px solid #EF4444';
    fb.style.color = '#FCA5A5';
    fb.innerHTML = `❌ <strong>Chưa chính xác.</strong> ${escapeHtml(explanation)}`;
  }
}

async function sendAiFollowUpQuestion(preCannedType = null) {
  const input = document.getElementById('aiFollowUpInput');
  const history = document.getElementById('aiFollowUpHistory');
  const btn = document.getElementById('btnSendAiFollowUp');
  let question = input ? input.value.trim() : '';
  let reqType = 'follow_up';

  if (preCannedType) {
    reqType = preCannedType;
    if (preCannedType === 'simpler') question = 'Hãy giải thích đơn giản và trực quan hơn';
    else if (preCannedType === 'more_examples') question = 'Cho tôi thêm các ví dụ tương tự';
    else if (preCannedType === 'why_translated_this_way') question = 'Tại sao câu này lại được dịch theo cách đó?';
    else if (preCannedType === 'easy_japanese') question = 'やさしい日本語で説明してください';
  }

  if (!question || !currentSentenceAnalysis || !currentSentenceAnalysis.jp) return;

  if (input) input.value = '';
  if (btn) {
    btn.disabled = true;
    btn.textContent = 'Đang giải đáp...';
  }

  // Display user question
  if (history) {
    const userMsg = document.createElement('div');
    userMsg.style.cssText = 'background: rgba(59, 130, 246, 0.15); border-left: 3px solid #3B82F6; padding: 6px 10px; border-radius: 4px; font-size: 12.5px; color: #E2E8F0;';
    userMsg.innerHTML = `<strong>Bạn:</strong> ${escapeHtml(question)}`;
    history.appendChild(userMsg);
  }

  try {
    const book = booksData[currentInspectedId] || booksData.ginga;
    const aiRes = await fetchFromBackend('/ai/chat', {
      method: 'POST',
      body: JSON.stringify({
        sentence: currentSentenceAnalysis.jp,
        context: currentSentenceAnalysis.vn || '',
        novelTitle: book.title || 'Tiểu thuyết',
        chapterTitle: `Chương ${currentChapterNum || 1}`,
        chapterNumber: currentChapterNum || 1,
        jlptLevel: book.level || 'N3',
        requestType: reqType,
        followUpQuestion: question,
        previousExplanation: currentSentenceAnalysis.grammarNote || ''
      })
    });

    if (history) {
      const aiReply = document.createElement('div');
      aiReply.style.cssText = 'background: rgba(16, 185, 129, 0.12); border-left: 3px solid #10B981; padding: 6px 10px; border-radius: 4px; font-size: 12.5px; color: #E2E8F0; line-height: 1.5;';
      const replyText = aiRes ? (aiRes.grammarInsight || aiRes.translationVn || 'Đã phân tích xong câu hỏi của bạn.') : 'Không thể kết nối AI Tutor để giải đáp câu hỏi này.';
      aiReply.innerHTML = `<strong>AI Sensei:</strong> ${replyText}`;
      history.appendChild(aiReply);
      history.scrollTop = history.scrollHeight;
    }
  } catch (err) {
    if (history) {
      const errorMsg = document.createElement('div');
      errorMsg.style.cssText = 'color: #EF4444; font-size: 12px;';
      errorMsg.textContent = 'Lỗi kết nối khi gửi câu hỏi. Vui lòng thử lại.';
      history.appendChild(errorMsg);
    }
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = 'Gửi câu hỏi';
    }
  }
}


// ================= READER INTERACTIONS (SATORI STYLE) =================
let readerTheme = 'paper'; // 'paper', 'sepia', 'dark'
function toggleReaderTheme() {
  const shell = document.getElementById('readerShell');
  const label = document.getElementById('readerThemeLabel');
  if (!shell) return;

  shell.classList.remove('dark-theme', 'sepia-theme');
  if (readerTheme === 'paper') {
    readerTheme = 'sepia';
    shell.classList.add('sepia-theme');
    if (label) label.textContent = 'Trầm (Sepia)';
  } else if (readerTheme === 'sepia') {
    readerTheme = 'dark';
    shell.classList.add('dark-theme');
    if (label) label.textContent = 'Đêm (Dark)';
  } else {
    readerTheme = 'paper';
    if (label) label.textContent = 'Giấy ngà';
  }
}

let furiganaVisible = true;
function toggleFurigana() {
  furiganaVisible = !furiganaVisible;
  const rtElements = document.querySelectorAll('rt');
  const fStatus = document.getElementById('furiStatus');

  rtElements.forEach(rt => {
    rt.style.visibility = furiganaVisible ? 'visible' : 'hidden';
  });

  if (fStatus) {
    fStatus.textContent = furiganaVisible ? 'Hiện' : 'Ẩn';
  }
}

function lookup(element, word, reading, meaning, level, sentence) {
  // Highlight clicked token
  document.querySelectorAll('.jp-w').forEach(el => el.classList.remove('active-word'));
  if (element) element.classList.add('active-word');

  currentWordLookup = { word, reading, meaning, level, sentence };

  const dock = document.getElementById('dictDock');
  const wEl = document.getElementById('dictWord');
  const rEl = document.getElementById('dictReading');
  const lEl = document.getElementById('dictLevel');
  const mEl = document.getElementById('dictMeaning');
  const sEl = document.getElementById('dictSentence');

  if (wEl) wEl.textContent = word;
  if (rEl) rEl.textContent = reading;
  if (lEl) {
    lEl.textContent = `JLPT ${level}`;
    lEl.className = `chip ${level.toLowerCase()}`;
  }
  if (mEl) mEl.textContent = meaning;
  if (sEl) sEl.textContent = `"${sentence}"`;

  const btnSave = document.getElementById('btnSaveSrs');
  if (btnSave) {
    const alreadySaved = srsDeck.some(c => (c.kanji || c.word) === word);
    if (alreadySaved) {
      btnSave.textContent = '💾 Bổ sung câu ngữ cảnh';
      btnSave.style.backgroundColor = 'rgba(59, 130, 246, 0.7)';
      btnSave.disabled = false;
    } else {
      btnSave.textContent = '+ Lưu vào SRS';
      btnSave.style.backgroundColor = '';
      btnSave.disabled = false;
    }
  }

  if (dock) {
    dock.style.display = 'flex';
  }
}

function closeDictDock() {
  const dock = document.getElementById('dictDock');
  if (dock) dock.style.display = 'none';
  document.querySelectorAll('.jp-w').forEach(el => el.classList.remove('active-word'));
}

function speakCurrentWord() {
  if ('speechSynthesis' in window && currentWordLookup.reading) {
    window.speechSynthesis.cancel();
    const u = new SpeechSynthesisUtterance(currentWordLookup.reading);
    u.lang = 'ja-JP';
    u.rate = 0.9;
    window.speechSynthesis.speak(u);
  }
}

async function saveWordToSRS(paramWord, paramReading, paramMeaning, paramJlpt, paramSentence, paramSentenceVn, paramNovelTitle) {
  const btn = document.getElementById('btnSaveSrs');
  const targetWord = paramWord || currentWordLookup?.word;
  if (!targetWord) {
    showToastNotification('Chưa chọn từ để lưu!', 'warning');
    return { success: false };
  }

  if (btn && !paramWord) {
    btn.textContent = '⏳ Đang lưu...';
    btn.disabled = true;
  }

  const payload = {
    word: targetWord,
    reading: paramReading || currentWordLookup?.reading || targetWord,
    meaning: paramMeaning || currentWordLookup?.meaning || 'Từ vựng tiếng Nhật',
    jlptLevel: paramJlpt || currentWordLookup?.level || 'N3',
    contextSentence: paramSentence || currentWordLookup?.sentence || '',
    sourceNovelTitle: paramNovelTitle || booksData[currentInspectedId]?.title || '銀河鉄道の夜',
    sourceChapterId: currentChapterNum || 1
  };

  try {
    const res = await fetch(`${API_BASE}/vocab`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.status === 201 || res.ok) {
      const created = await res.json();
      playSuccessChime();
      addLocalXp(10);
      if (created && created.alreadyExisted) {
        showToastNotification(`Đã bổ sung ngữ cảnh mới cho "${payload.word}"! (+10 XP)`);
        if (btn) {
          btn.textContent = '✓ Đã bổ sung ngữ cảnh!';
          btn.style.backgroundColor = '#10B981';
          btn.disabled = true;
        }
        return { success: true, isDuplicate: true };
      } else {
        showToastNotification(`Đã lưu "${payload.word}" vào Flashcard SRS! (+10 XP)`);
        if (btn) {
          btn.textContent = '✓ Đã lưu SRS!';
          btn.style.backgroundColor = '#10B981';
          btn.disabled = true;
        }
        await fetchSrsDeck();
        await fetchUserProgress();
        return { success: true, isDuplicate: false };
      }
    } else if (res.status === 409) {
      if (btn) {
        btn.textContent = '✓ Đã có trong SRS';
        btn.style.backgroundColor = 'rgba(59, 130, 246, 0.4)';
        btn.disabled = true;
      }
      showToastNotification(`Từ "${payload.word}" đã có trong kho từ vựng.`, 'info');
      return { success: true, isDuplicate: true };
    } else {
      throw new Error(`HTTP ${res.status}`);
    }
  } catch (err) {
    console.warn('Backend offline, lưu vào bộ nhớ cục bộ:', err.message);
    const saved = saveVocabToLocalStorage(payload);
    if (saved.isDuplicate) {
      if (saved.isEnriched) {
        showToastNotification(`Đã bổ sung ngữ cảnh cho "${payload.word}" (Offline).`, 'info');
        if (btn) {
          btn.textContent = '✓ Đã bổ sung (Offline)';
          btn.disabled = true;
        }
      } else {
        if (btn) {
          btn.textContent = '✓ Đã có trong SRS';
          btn.disabled = true;
        }
        showToastNotification(`Từ "${payload.word}" đã có trong kho từ vựng.`, 'info');
      }
      return { success: true, isDuplicate: true };
    } else {
      playSuccessChime();
      addLocalXp(10);
      showToastNotification(`Đã lưu "${payload.word}" vào SRS (Offline)! (+10 XP)`);
      if (btn) {
        btn.textContent = '✓ Đã lưu SRS (Offline)';
        btn.style.backgroundColor = '#10B981';
        btn.disabled = true;
      }
      refreshSrsDeckFromLocal();
      return { success: true, isDuplicate: false };
    }
  }
}


// Satori Reader Sentence Grammar & Syntax Analysis
let currentSentenceAnalysis = sentencesData.sent1;

function openSentenceAnalysis(sentenceId) {
  const sent = sentencesData[sentenceId] || sentencesData.sent1;
  currentSentenceAnalysis = sent;

  const modal = document.getElementById('sentenceAnalysisModal');
  const jpEl = document.getElementById('sentModalJp');
  const vnEl = document.getElementById('sentModalVn');
  const lvlEl = document.getElementById('sentModalLevel');
  const tokensEl = document.getElementById('sentTokensList');
  const noteEl = document.getElementById('sentGrammarNote');

  if (jpEl) jpEl.textContent = sent.jp;
  if (vnEl) vnEl.textContent = sent.vn;
  if (lvlEl) {
    lvlEl.textContent = `JLPT ${sent.level}`;
    lvlEl.className = `chip ${sent.level.toLowerCase()}`;
  }
  if (noteEl) noteEl.innerHTML = sent.grammarNote;

  if (tokensEl) {
    tokensEl.innerHTML = '';
    sent.tokens.forEach(tok => {
      const card = document.createElement('div');
      card.className = 'grammar-token-card';
      card.innerHTML = `
        <div class="token-head-row">
          <span class="token-word">${tok.word}</span>
          <span class="token-pos-badge">${tok.pos}</span>
        </div>
        <div style="font-size: 11.5px; color: var(--text-dim);">${tok.furigana}</div>
        <div class="token-role">${tok.role}</div>
        <div class="token-meaning">${tok.meaning}</div>
        <button class="token-srs-btn" onclick="saveTokenToSrs('${tok.word}', '${tok.furigana}', '${tok.meaning}', '${sent.level}', this)">
          + Thêm thẻ SRS
        </button>
      `;
      tokensEl.appendChild(card);
    });
  }

  if (modal) {
    modal.classList.add('open');
  }
}

function closeSentenceAnalysis(e) {
  if (e && e.target !== e.currentTarget && !e.target.classList.contains('modal-close-btn')) {
    return;
  }
  const modal = document.getElementById('sentenceAnalysisModal');
  if (modal) {
    modal.classList.remove('open');
    modal.style.display = 'none';
  }
}

function speakCurrentSentence() {
  if ('speechSynthesis' in window && currentSentenceAnalysis) {
    window.speechSynthesis.cancel();
    const cleanText = currentSentenceAnalysis.jp.replace(/[「」]/g, '');
    const u = new SpeechSynthesisUtterance(cleanText);
    u.lang = 'ja-JP';
    u.rate = 0.88;
    window.speechSynthesis.speak(u);
  }
}

async function askAiTutorExplanation() {
  const btn = document.getElementById('btnAskAiTutor');
  if (btn) {
    btn.disabled = true;
    btn.textContent = '⏳ AI đang phân tích ngữ cảnh...';
  }

  const payload = {
    sentence: currentSentenceAnalysis?.jp || '',
    context: booksData[currentInspectedId]?.title || 'Ginga Tetsudou no Yoru',
    jlptLevel: currentSentenceAnalysis?.level || 'N3'
  };

  const res = await fetchFromBackend('/ai/explain', {
    method: 'POST',
    body: JSON.stringify(payload)
  });

  if (res && res.success) {
    playSuccessChime();
    const noteEl = document.getElementById('sentGrammarNote');
    const badgeEl = document.querySelector('.modal-badge-chip');
    if (badgeEl) badgeEl.textContent = `✦ ${res.provider?.toUpperCase() || 'AI TUTOR EXPLANATION'}`;
    if (noteEl) {
      noteEl.innerHTML = `
        <div style="font-weight: 700; color: #F59E0B; margin-bottom: 6px;">🤖 Phân tích ngữ cảnh từ AI Sensei:</div>
        <div>${res.grammarInsight}</div>
        <div style="margin-top: 8px; font-style: italic; color: #94A3B8;">📖 Sắc thái văn học: ${res.culturalOrLiteraryNuance}</div>
      `;
    }
    if (btn) {
      btn.textContent = '✓ Đã nhận giải thích AI';
      btn.style.backgroundColor = '#10B981';
    }
  } else {
    if (btn) {
      btn.textContent = 'Hỏi lại AI Tutor';
      btn.disabled = false;
    }
  }
}

async function saveTokenToSrs(word, furigana, meaning, level, btn) {
  if (!word) return;
  if (btn) {
    btn.textContent = '⏳ Đang lưu...';
    btn.disabled = true;
  }

  const payload = {
    word: word,
    reading: furigana || word,
    meaning: meaning || 'Từ vựng tiếng Nhật',
    jlptLevel: level || 'N3',
    contextSentence: currentSentenceAnalysis?.jp || '',
    sourceNovelTitle: booksData[currentInspectedId]?.title || '銀河鉄道の夜',
    sourceChapterId: currentChapterNum || 1
  };

  try {
    const res = await fetch(`${API_BASE}/vocab`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.status === 201 || res.ok) {
      playSuccessChime();
      addLocalXp(10);
      showToastNotification(`Đã thêm "${word}" vào SRS! (+10 XP)`);
      if (btn) {
        btn.textContent = '✓ Đã lưu SRS!';
        btn.style.color = '#10B981';
        btn.disabled = true;
      }
      await fetchSrsDeck();
      await fetchUserProgress();
    } else if (res.status === 409) {
      showToastNotification(`Từ "${word}" đã có trong kho từ vựng.`, 'info');
      if (btn) {
        btn.textContent = '✓ Đã có trong SRS';
        btn.style.color = '#60A5FA';
        btn.disabled = true;
      }
    } else {
      throw new Error(`HTTP ${res.status}`);
    }
  } catch (err) {
    const saved = saveVocabToLocalStorage(payload);
    if (saved.isDuplicate) {
      showToastNotification(`Từ "${word}" đã có trong kho từ vựng.`, 'info');
      if (btn) {
        btn.textContent = '✓ Đã có trong SRS';
        btn.style.color = '#60A5FA';
        btn.disabled = true;
      }
    } else {
      playSuccessChime();
      addLocalXp(10);
      showToastNotification(`Đã thêm "${word}" vào SRS (Offline)! (+10 XP)`);
      if (btn) {
        btn.textContent = '✓ Đã lưu SRS';
        btn.style.color = '#10B981';
        btn.disabled = true;
      }
      refreshSrsDeckFromLocal();
    }
  }
}

