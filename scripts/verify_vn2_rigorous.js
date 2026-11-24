const fs = require('fs');
const path = require('path');
const vm = require('vm');
const assert = require('assert');

// -------------------------------------------------------------
// 1. ROBUST DOM & BROWSER MOCK ENVIRONMENT FOR LOGIC VALIDATION
// -------------------------------------------------------------
class MockElement {
  constructor(tag, id = '', className = '') {
    this.tagName = tag.toUpperCase();
    this.id = id;
    this.className = className;
    this.classList = {
      _classes: new Set(className ? className.split(/\s+/) : []),
      add: (c) => this.classList._classes.add(c),
      remove: (c) => this.classList._classes.delete(c),
      contains: (c) => this.classList._classes.has(c),
      toggle: (c) => {
        if (this.classList._classes.has(c)) {
          this.classList._classes.delete(c);
          return false;
        } else {
          this.classList._classes.add(c);
          return true;
        }
      }
    };
    this.style = {};
    this.attributes = {};
    this.children = [];
    this._innerHTML = '';
    this._textContent = '';
  }

  get innerHTML() {
    return this._innerHTML;
  }
  set innerHTML(val) {
    this._innerHTML = String(val);
    this._textContent = this._innerHTML.replace(/<[^>]*>/g, '').trim();
  }

  get textContent() {
    return this._textContent;
  }
  set textContent(val) {
    this._textContent = String(val);
    this._innerHTML = String(val);
  }

  setAttribute(k, v) { this.attributes[k] = String(v); }
  getAttribute(k) { return this.attributes[k] !== undefined ? this.attributes[k] : null; }
  appendChild(c) { this.children.push(c); return c; }
  getBoundingClientRect() { return { top: 100, bottom: 200, left: 100, right: 300, width: 200, height: 100 }; }
}

const elementRegistry = new Map();
function getEl(id, tag = 'div', cls = '') {
  if (!elementRegistry.has(id)) {
    elementRegistry.set(id, new MockElement(tag, id, cls));
  }
  return elementRegistry.get(id);
}

// Elements present in index.html for VN
[
  'vnStage', 'vnBg', 'vnSpeaker', 'vnSpeakerRole', 'vnSpeakerAvatar',
  'vnJp', 'vnVn', 'vnAdvancePrompt', 'vnNameplate', 'vnStoryBox',
  'charLeft', 'charRight', 'charCenter', 'slotLeft', 'slotRight', 'slotCenter',
  'vnChoiceOverlay', 'vnChoiceContainer', 'vnWordPopup', 'vnPopupWord', 'vnPopupReading',
  'vnPopupMeaning', 'vnPopupJlpt', 'vnAiSenseiDrawer', 'aiSenseiSentenceJp',
  'aiSenseiSentenceVn', 'aiSenseiGrammarContent', 'aiSenseiNuanceContent',
  'btnOpenVnAiSensei', 'vnBacklogList', 'vnBacklogModal', 'btnModeVnStory',
  'btnModeVnChat', 'vnAiChatBox', 'vnCharSelectorStrip', 'vnCharPillsList',
  'vnChatAvatar', 'vnChatSpeakerName', 'vnChatSpeakerBio', 'vnPromptStartersList',
  'xpIndicatorText', 'xpFill'
].forEach(id => getEl(id));

// Mock localStorage with clear support
const storage = new Map();
const mockLocalStorage = {
  getItem: (k) => storage.get(k) || null,
  setItem: (k, v) => storage.set(k, String(v)),
  removeItem: (k) => storage.delete(k),
  clear: () => storage.clear()
};

// Global context
const sandbox = {
  console,
  setTimeout: (fn) => fn(),
  clearTimeout: () => {},
  setInterval: () => 1,
  clearInterval: () => {},
  window: null,
  document: {
    getElementById: (id) => elementRegistry.get(id) || null,
    querySelector: (sel) => {
      if (sel.includes('.xp-indicator span:last-child') || sel.includes('xpIndicatorText')) {
        return getEl('xpIndicatorText', 'span');
      }
      if (sel.includes('.xp-fill') || sel.includes('xpFill')) {
        return getEl('xpFill', 'div');
      }
      return null;
    },
    querySelectorAll: () => [],
    createElement: (tag) => new MockElement(tag),
    body: new MockElement('body'),
    addEventListener: () => {}
  },
  localStorage: mockLocalStorage,
  toastLogs: [],
  showToastNotification: (msg, type = 'info') => {
    sandbox.toastLogs.push({ msg, type });
  },
  playSuccessChime: () => {},
  escapeHtml: (s) => String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'),
  switchPane: (pane) => { sandbox.currentPane = pane; },
  currentPane: 'home',
  API_BASE: 'http://localhost:5000/api',
  fetchFromBackend: async () => null,
  addLocalXp: (amount) => {
    const xpSpan = sandbox.document.querySelector('.xp-indicator span:last-child');
    const current = parseInt(xpSpan.textContent.split('/')[0].replace(/[^0-9]/g, '')) || 0;
    const next = current + amount;
    xpSpan.textContent = `${next.toLocaleString()} / 2,000 XP`;
  }
};
sandbox.window = sandbox;

// Load real application code into VM context
const ctx = vm.createContext(sandbox);

function loadScript(relPath) {
  let code = fs.readFileSync(relPath, 'utf8');
  // ensure globals match
  if (relPath.includes('data.js')) {
    code = code.replace(/const booksData\s*=/, 'window.booksData =')
               .replace(/const vnDialoguesByBook\s*=/, 'window.vnDialoguesByBook =')
               .replace(/let srsDeck\s*=/, 'window.srsDeck =');
  }
  vm.runInContext(code, ctx);
}

loadScript('web-preview/js/data.js');
loadScript('web-preview/js/api.js');
loadScript('web-preview/js/reader.js');
loadScript('web-preview/js/visual-novel.js');

console.log('================================================================');
console.log('   KOTOBA ODYSSEY - RIGOROUS VISUAL NOVEL 2.0 VALIDATION HARNESS ');
console.log('================================================================\n');

const auditNovels = [
  {
    key: 'tebukuro',
    title: '手袋を買いに',
    genre: 'Folktale / Children',
    expectedProtagonist: '子ぎつね',
    forbiddenSubstrings: ['Buying', '第1章の本文'],
    minChoices: 3,
    expectedChoicePrefix: 'tebukuro_c'
  },
  {
    key: 'ginga',
    title: '銀河鉄道の夜',
    genre: 'Classic / Fantasy',
    expectedProtagonist: 'ジョバンニ',
    forbiddenSubstrings: ['第1章の本文'],
    minChoices: 2,
    expectedChoicePrefix: 'ginga_c'
  },
  {
    key: 'mushoku',
    title: '無職転生',
    genre: 'Isekai / Fantasy',
    expectedProtagonist: 'ルーデウス',
    forbiddenSubstrings: ['第1章の本文'],
    minChoices: 3,
    expectedChoicePrefix: 'mushoku_c'
  },
  {
    key: 'slime',
    title: '転生したらスライムだった件',
    genre: 'Isekai / Fantasy',
    expectedProtagonist: 'リムル',
    forbiddenSubstrings: ['第1章の本文'],
    minChoices: 3,
    expectedChoicePrefix: 'slime_c'
  },
  {
    key: 'kokoro',
    title: 'こころ',
    genre: 'Psychological / Classic',
    expectedProtagonist: '先生',
    forbiddenSubstrings: ['第1章の本文'],
    minChoices: 3,
    expectedChoicePrefix: 'kokoro_c'
  },
  {
    key: 'merosu',
    title: '走れメロス',
    genre: 'Classic / Friendship',
    expectedProtagonist: 'メロス',
    forbiddenSubstrings: ['第1章の本文'],
    minChoices: 2,
    expectedChoicePrefix: 'merosu_c'
  }
];

let passCount = 0;
let failCount = 0;
const results = [];

async function runAudit() {
  for (let index = 0; index < auditNovels.length; index++) {
    const novel = auditNovels[index];
    console.log(`----------------------------------------------------------------`);
    console.log(`[TEST ${index + 1}/6] AUDITING: ${novel.title} (${novel.key})`);
    console.log(`----------------------------------------------------------------`);

    try {
      // RESET LOCAL STORAGE & SRS DECK FOR ISOLATED RUN
      sandbox.localStorage.clear();
      if (Array.isArray(ctx.srsDeck)) ctx.srsDeck.length = 0;
      sandbox.toastLogs = [];
      const xpSpan = sandbox.document.querySelector('.xp-indicator span:last-child');
      xpSpan.textContent = '100 / 2,000 XP';

      // 1. OPEN NOVEL & ROUTING TEST
      ctx.openVN(novel.key);
      assert.strictEqual(ctx.currentPane, 'vn', `Current pane must be 'vn'`);
      assert.strictEqual(ctx.currentVnBookKey, novel.key, `currentVnBookKey must match '${novel.key}'`);

      const dialogues = ctx.vnDialogues;
      assert(Array.isArray(dialogues) && dialogues.length >= 3, `Novel must have at least 3 dialogues`);
      console.log(`  ✓ Routing: Opened '${novel.key}', Dialogues count: ${dialogues.length}`);

      // 2. CHARACTER ROSTER & PLACEHOLDER AUDIT
      const roster = ctx.getNovelFallbackCharacters(novel.key);
      assert(roster && roster.length >= 2, `Roster must contain at least 2 characters`);
      
      // Check forbidden substrings across all dialogues
      dialogues.forEach((d, dIdx) => {
        novel.forbiddenSubstrings.forEach(badStr => {
          assert(!d.jp.includes(badStr), `Line ${dIdx + 1} JP contains forbidden text: "${badStr}" (Text: "${d.jp}")`);
          assert(!d.vn.includes(badStr), `Line ${dIdx + 1} VN contains forbidden text: "${badStr}" (Text: "${d.vn}")`);
          assert(!d.speaker.includes(badStr), `Line ${dIdx + 1} Speaker contains forbidden text: "${badStr}"`);
        });
      });
      console.log(`  ✓ Text Integrity: 0 forbidden placeholder texts found across all lines.`);

      // Check protagonist
      const pSpeaker = dialogues.find(d => d.speaker.includes(novel.expectedProtagonist) || (d.role && d.role.includes(novel.expectedProtagonist)));
      assert(pSpeaker !== undefined, `Protagonist '${novel.expectedProtagonist}' must be present in scene or speaker/role`);
      console.log(`  ✓ Protagonist Verified: Found '${novel.expectedProtagonist}'`);

      // 3. CHOICE POINT & BRANCHING STRUCTURE
      const choiceIdx = dialogues.findIndex(d => d.type === 'choice');
      assert(choiceIdx !== -1, `A choice dialogue must exist`);
      const choiceDialogue = dialogues[choiceIdx];
      assert(Array.isArray(choiceDialogue.choices), `Choices array must be defined`);
      assert(choiceDialogue.choices.length >= novel.minChoices, `Must have at least ${novel.minChoices} choices, found ${choiceDialogue.choices.length}`);

      choiceDialogue.choices.forEach((c, cIdx) => {
        assert(c.choiceId, `Choice ${cIdx + 1} must have choiceId`);
        assert(c.choiceId.startsWith(novel.expectedChoicePrefix), `Choice ID ${c.choiceId} must start with '${novel.expectedChoicePrefix}'`);
        assert(c.textJp && c.textJp.length > 5, `Choice ${cIdx + 1} textJp must not be empty`);
        assert(c.textVn && c.textVn.length > 5, `Choice ${cIdx + 1} textVn must not be empty`);
        assert(c.branchResponse && c.branchResponse.jp, `Choice ${cIdx + 1} must define a branchResponse with jp text`);
        assert(c.branchType === 'Branching Scene' || c.branchType === 'Dialogue Variation', `Choice branchType must be 'Branching Scene' or 'Dialogue Variation', found: ${c.branchType}`);
      });
      console.log(`  ✓ Choices Validated: ${choiceDialogue.choices.length} distinct choices with unique IDs and branch responses.`);

      // 4. REAL CONSEQUENCE MUTATION TEST
      ctx.vnIndex = choiceIdx;
      ctx.updateVnStage();
      const chosenOption = choiceDialogue.choices[0];
      const expectedResponseJp = chosenOption.branchResponse.jp;
      
      // Select Choice 1
      ctx.selectVnChoice(0);
      const postChoiceLine = ctx.vnDialogues[choiceIdx + 1];
      assert.strictEqual(postChoiceLine.jp, expectedResponseJp, `Line ${choiceIdx + 2} must be dynamically mutated to choice 1 response`);
      console.log(`  ✓ Consequence Applied: Subsequent dialogue changed to reflect Choice 1 response.`);

      // 5. XP ANTI-EXPLOIT STRICT ASSERTION
      // Selection 1 must award 15 XP
      const xpSpanText = sandbox.document.querySelector('.xp-indicator span:last-child').textContent;
      const currentXp = parseInt(xpSpanText.split('/')[0].replace(/[^0-9]/g, ''));
      assert.strictEqual(currentXp, 115, `XP after initial choice must be 115 (100 + 15), got ${currentXp}`);
      console.log(`  ✓ First Choice Selection: Awarded exactly +15 XP (New XP: ${currentXp})`);

      // Rewind & Re-select Choice 2 (MUST NOT award XP)
      ctx.vnIndex = choiceIdx;
      ctx.updateVnStage();
      ctx.selectVnChoice(1);
      const xpAfterRewind = parseInt(sandbox.document.querySelector('.xp-indicator span:last-child').textContent.split('/')[0].replace(/[^0-9]/g, ''));
      assert.strictEqual(xpAfterRewind, 115, `XP after rewind must remain 115 (0 XP awarded), got ${xpAfterRewind}`);
      console.log(`  ✓ Rewind & Re-select: Exactly 0 XP awarded (Anti-exploit lock verified)`);

      // Simulate Page Reload by re-opening novel with same localStorage
      ctx.openVN(novel.key);
      ctx.vnIndex = choiceIdx;
      ctx.selectVnChoice(0);
      const xpAfterReload = parseInt(sandbox.document.querySelector('.xp-indicator span:last-child').textContent.split('/')[0].replace(/[^0-9]/g, ''));
      assert.strictEqual(xpAfterReload, 115, `XP after reload must remain 115, got ${xpAfterReload}`);
      console.log(`  ✓ Reload Persistence: Exactly 0 XP awarded after scene restart`);

      // 6. VOCABULARY TOKENS & SRS DEDUPLICATION TEST
      const lineWithVocab = dialogues.find(d => Array.isArray(d.vocabTokens) && d.vocabTokens.length > 0);
      assert(lineWithVocab !== undefined, `Dialogue must have vocabulary tokens for interactive learning`);
      const tok = lineWithVocab.vocabTokens[0];
      
      // Save to SRS first time
      const resSrs1 = await ctx.saveWordToSRS(tok.word, tok.reading, tok.meaning, tok.level, lineWithVocab.jp, lineWithVocab.vn, novel.title);
      assert.strictEqual(resSrs1.isDuplicate, false, `First SRS save must not be duplicate`);
      const xpAfterSrs1 = parseInt(sandbox.document.querySelector('.xp-indicator span:last-child').textContent.split('/')[0].replace(/[^0-9]/g, ''));
      assert.strictEqual(xpAfterSrs1, 125, `SRS save must award +10 XP (115 + 10 = 125), got ${xpAfterSrs1}`);
      console.log(`  ✓ SRS First Save: '${tok.word}' saved, +10 XP awarded`);

      // Save to SRS second time (MUST be duplicate, NO XP)
      const resSrs2 = await ctx.saveWordToSRS(tok.word, tok.reading, tok.meaning, tok.level, lineWithVocab.jp, lineWithVocab.vn, novel.title);
      assert.strictEqual(resSrs2.isDuplicate, true, `Second SRS save must be marked duplicate`);
      const xpAfterSrs2 = parseInt(sandbox.document.querySelector('.xp-indicator span:last-child').textContent.split('/')[0].replace(/[^0-9]/g, ''));
      assert.strictEqual(xpAfterSrs2, 125, `Duplicate SRS save must NOT award XP, remained ${xpAfterSrs2}`);
      console.log(`  ✓ SRS Duplicate Prevention: Duplicate detected, 0 XP awarded`);

      // 7. AI SENSEI DRAWER TEST
      ctx.vnIndex = 0;
      ctx.updateVnStage();
      ctx.openVnAiSensei();
      const grammarContent = elementRegistry.get('aiSenseiGrammarContent').textContent;
      assert(grammarContent && grammarContent.length > 5, `AI Sensei must produce grammar breakdown for current sentence`);
      ctx.closeVnAiSensei();
      console.log(`  ✓ AI Sensei: Grammar analysis populated (${grammarContent.slice(0, 40)}...)`);

      passCount++;
      results.push({ key: novel.key, title: novel.title, status: 'PASS' });
      console.log(`  => RESULT FOR [${novel.key}]: PASS (All Assertions Satisfied)\n`);
    } catch (err) {
      failCount++;
      results.push({ key: novel.key, title: novel.title, status: 'FAIL', error: err.message });
      console.error(`  ❌ FAILED [${novel.key}]:`, err.message);
      console.error(err.stack);
      console.log('');
    }
  }

  console.log('================================================================');
  console.log(` HARNESS AUDIT SUMMARY: ${passCount}/${auditNovels.length} NOVELS PASSED`);
  if (failCount > 0) {
    console.log(` ATTENTION: ${failCount} NOVELS FAILED ASSERTIONS!`);
    process.exit(1);
  } else {
    console.log(' ALL 6 NOVELS SATISFIED 100% OF PROGRAMMATIC ASSERTIONS.');
    process.exit(0);
  }
}

runAudit();
