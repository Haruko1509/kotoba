// ===================================================================
// KOTOBA ODYSSEY - VISUAL NOVEL THEATRE 2.0 ENGINE
// Immersive Japanese Visual Novel, Procedural Audio & AI Tutor
// Supports ALL 105 Library Novels + Curated Master Showcases
// ===================================================================

var vnDialogues = (typeof vnDialoguesByBook !== 'undefined' && vnDialoguesByBook.ginga) ? vnDialoguesByBook.ginga : [];
var vnIndex = 0;
var vnAutoTimer = null;
var vnBacklog = [];
var isTyping = false;
var typeTimer = null;
var currentFullJp = '';
var currentFullVn = '';
var currentTypingIndex = 0;

var currentVnBookKey = 'ginga';
if (typeof window !== 'undefined') window.currentVnBookKey = currentVnBookKey;
var isVnFocusMode = false;
var isVnTransVisible = true;
var isVnAutoPlay = false;
var vnAutoDelay = 4200; // ms
var isVnAudioEnabled = true;

var currentVnCharacters = [];
if (typeof window !== 'undefined') window.currentVnCharacters = currentVnCharacters;
var currentVnSelectedCharId = 'giovanni';
var currentVnChatHistory = [];
var currentVnMode = 'story'; // 'story' | 'chat'
var currentVnChatMode = 'free'; // 'free' | 'practice' | 'scenario'

let activeDrawerWord = null;
let currentActiveChoice = null;
let challengeAnswered = false;

let vnDropdownSearchQuery = '';
let vnDropdownJlptFilter = 'all';

// -------------------------------------------------------------------
// 1. PROCEDURAL AUDIO & SFX SYNTHESIS
// -------------------------------------------------------------------
function playVnTypewriterClick() {
  if (!isVnAudioEnabled || typeof soundEnabled !== 'undefined' && !soundEnabled) return;
  try {
    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (!AudioContextClass) return;
    if (!window.vnAudioCtx) window.vnAudioCtx = new AudioContextClass();
    if (window.vnAudioCtx.state === 'suspended') window.vnAudioCtx.resume();

    const osc = window.vnAudioCtx.createOscillator();
    const gain = window.vnAudioCtx.createGain();
    const now = window.vnAudioCtx.currentTime;

    osc.type = 'triangle';
    osc.frequency.setValueAtTime(650 + Math.random() * 250, now);
    osc.frequency.exponentialRampToValueAtTime(120, now + 0.035);

    gain.gain.setValueAtTime(0.04, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.035);

    osc.connect(gain);
    gain.connect(window.vnAudioCtx.destination);
    osc.start(now);
    osc.stop(now + 0.035);
  } catch (e) {}
}

function playVnChoiceChime() {
  if (!isVnAudioEnabled) return;
  try {
    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (!AudioContextClass) return;
    if (!window.vnAudioCtx) window.vnAudioCtx = new AudioContextClass();
    if (window.vnAudioCtx.state === 'suspended') window.vnAudioCtx.resume();

    const now = window.vnAudioCtx.currentTime;
    [523.25, 659.25, 783.99, 1046.50].forEach((freq, idx) => {
      const osc = window.vnAudioCtx.createOscillator();
      const gain = window.vnAudioCtx.createGain();
      const t = now + idx * 0.07;
      osc.type = 'sine';
      osc.frequency.setValueAtTime(freq, t);
      gain.gain.setValueAtTime(0.07, t);
      gain.gain.exponentialRampToValueAtTime(0.001, t + 0.35);
      osc.connect(gain);
      gain.connect(window.vnAudioCtx.destination);
      osc.start(t);
      osc.stop(t + 0.35);
    });
  } catch (e) {}
}

// -------------------------------------------------------------------
// 2. ENTRY & NAVIGATION CONTROLLER (ANY OF 105 NOVELS)
// -------------------------------------------------------------------
function openVNFromCurrentBook() {
  const currentKey = currentInspectedId || currentVnBookKey || 'ginga';
  openVN(currentKey);
}

function openVN(bookKeyOrId) {
  let targetKey = bookKeyOrId || currentInspectedId || currentVnBookKey || 'ginga';
  if (typeof booksData !== 'undefined') {
    if (booksData[targetKey]) {
      // Key directly exists in booksData (e.g. 'mushoku', 'rezero', 'momotaro')
    } else {
      const numId = parseInt(targetKey);
      const match = Object.keys(booksData).find(k => 
        (numId && booksData[k].dbId === numId) || 
        booksData[k].id === targetKey ||
        k.toLowerCase() === String(targetKey).toLowerCase() ||
        k.toLowerCase().startsWith(String(targetKey).toLowerCase()) ||
        String(targetKey).toLowerCase().startsWith(k.toLowerCase())
      );
      if (match) targetKey = match;
      else if (!booksData[targetKey]) targetKey = 'ginga';
    }
  }
  currentVnBookKey = targetKey;
  if (typeof window !== 'undefined') window.currentVnBookKey = targetKey;
  currentInspectedId = targetKey;
  if (typeof window !== 'undefined') window.currentInspectedId = targetKey;

  // Retrieve curated scene or dynamically adapt from novel's chapter!
  vnDialogues = getOrCreateVnStory(currentVnBookKey);
  vnIndex = 0;

  switchPane('vn');
  renderVnStoryMenu();
  updateVnStage();

  const novelId = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]?.dbId) || 1;
  loadVnCharacters(novelId);
}

// -------------------------------------------------------------------
// 3. DYNAMIC VISUAL NOVEL ADAPTOR & BRANCHING ENGINE (ALL 105 NOVELS)
// -------------------------------------------------------------------
function buildNovelBranchingChoices(bookKey, book, pChar, cChar, leftSprite, rightSprite, bg, theme) {
  const g = (book?.genre || '').toLowerCase();

  // 1. BESPOKE STORY CHOICES FOR CELEBRATED WORKS
  const bespokeMap = {
    tebukuro: [
      {
        choiceId: 'tebukuro_c1',
        branchType: 'Branching Scene',
        textJp: '「母さんの言いつけを守り、化けた人間の手を出して銅貨を渡す」',
        textVn: 'Tuân theo lời mẹ: Đưa bàn tay người ra kèm hai đồng xu (+15 XP)',
        reactionText: 'Cáo con nín thở, cẩn thận đưa bàn tay người vào khe cửa tiệm!',
        xp: 15,
        branchResponse: {
          speaker: '帽子屋の主人 (Bác thợ may)',
          role: '✦ Chủ tiệm nón mũ ấm áp',
          charId: 'boushiya',
          avatar: 'assets/chars/char_hyoju.png',
          speakerPos: 'right',
          bg: bg,
          location: `❄️ ${book.title} • TIỆM MŨ NÓN MÙA ĐÔNG`,
          theme: theme,
          jp: '「はいはい、毎度あり。外は寒いだろう、この暖かくて柔らかな毛糸の手袋を持ってお行き。」',
          vn: '"Được rồi, cảm ơn cháu. Ngoài trời tuyết lạnh lắm, hãy mang đôi găng tay len ấm áp và mềm mại này về sưởi ấm nhé."',
          grammarInsight: 'Cấu trúc [〜てお行き]: Thể mệnh lệnh ân cần của động từ [行く] (hãy cầm lấy mà đi về).',
          nuanceInsight: 'Sự tiếp đãi dịu dàng, xóa tan đi nỗi e sợ ban đầu của chú cáo nhỏ giữa mùa đông tuyết rơi.',
          vocabTokens: [
            { word: '毎度', reading: 'まいど', meaning: 'Mỗi lần, lời chào cảm ơn khách', level: 'N3' },
            { word: '毛糸', reading: 'けいと', meaning: 'Sợi len, đồ len', level: 'N3' },
            { word: '手袋', reading: 'てぶくろ', meaning: 'Găng tay, bao tay ấm', level: 'N5' }
          ]
        }
      },
      {
        choiceId: 'tebukuro_c2',
        branchType: 'Branching Scene',
        textJp: '「緊張のあまり、間違えてモフモフの狐の手を差し出してしまう」',
        textVn: 'Hồi hộp vụng về: Vô tình giơ nhầm bàn tay cáo lông xù (+15 XP)',
        reactionText: 'Trái tim Cáo con đập thình thịch khi chủ tiệm sững sờ nhận ra bàn tay cáo tuyết!',
        xp: 15,
        branchResponse: {
          speaker: '帽子屋の主人 (Bác thợ may)',
          role: '✦ Người thợ may nhân hậu',
          charId: 'boushiya',
          avatar: 'assets/chars/char_hyoju.png',
          speakerPos: 'right',
          bg: bg,
          location: `❄️ ${book.title} • ÁNH SÁNG CỦA LÒNG NHÂN ÁI`,
          theme: theme,
          jp: '「おや……？ これは狐の手じゃないか。だが本物の白銅貨だ。寒さに震えているんだな、持っていきなさい。」',
          vn: '"Ủa...? Đây chẳng phải là bàn tay của một chú cáo sao? Nhưng đồng xu trả là tiền thật. Hẳn nhóc đang run lên vì lạnh buốt, cầm lấy đi nào."',
          grammarInsight: 'Cấu trúc [〜じゃないか]: Lời cảm thán ngạc nhiên (Chẳng phải là... đó sao!).',
          nuanceInsight: 'Khoảnh khắc đắt giá nhất của tác phẩm: Lòng trắc ẩn vượt qua ranh giới chủng loài của người thợ may nhân từ.',
          vocabTokens: [
            { word: '本物', reading: 'ほんもの', meaning: 'Đồ thật, hàng thật', level: 'N3' },
            { word: '震える', reading: 'ふるえる', meaning: 'Run rẩy vì lạnh', level: 'N3' }
          ]
        }
      },
      {
        choiceId: 'tebukuro_c3',
        branchType: 'Dialogue Variation',
        textJp: '「窓のすき間から、店内の暖炉と人間の親子の姿をそっと覗く」',
        textVn: 'Tò mò khám phá: Hé nhìn ánh lửa lò sưởi và lời ru ấm áp (+15 XP)',
        reactionText: 'Ánh sáng vàng dịu từ lò sưởi xua tan nỗi sợ hãi trong lòng Cáo con!',
        xp: 15,
        branchResponse: {
          speaker: '母ぎつね (Cáo mẹ)',
          role: '✦ Cáo mẹ hiền từ',
          charId: 'haha_gitsune',
          avatar: 'assets/chars/char_runa.png',
          speakerPos: 'right',
          bg: bg,
          location: `❄️ ${book.title} • TÌNH NGƯỜI SƯỞI ẤM MÙA ĐÔNG`,
          theme: theme,
          jp: '「人間は本当に怖ろしいものかしら。いいえ、人間の中にも心優しい人たちがたくさんいるのよ。」',
          vn: '"Loài người có thực sự đáng sợ như người ta đồn đại không nhỉ? Không đâu, giữa thế giới con người cũng có biết bao tấm lòng nhân hậu dịu hiền."',
          grammarInsight: 'Cấu trúc [〜かしら / いいえ〜]: Tự vấn với sự phân vân (Liệu có phải... Không đâu...).',
          nuanceInsight: 'Thông điệp nhân văn cốt lõi của tác giả Niimi Nankichi: Tình người xóa tan định kiến và sự lạnh giá.',
          vocabTokens: [
            { word: '怖ろしい', reading: 'おそろしい', meaning: 'Đáng sợ, kinh hoàng', level: 'N3' },
            { word: '心優しい', reading: 'こころやさしい', meaning: 'Hiền lành, nhân hậu', level: 'N2' }
          ]
        }
      }
    ],

    suho_uma: [
      {
        choiceId: 'suho_c1',
        branchType: 'Branching Scene',
        textJp: '「狼の群れから羊たちを守るため、白馬と共に闇を駆ける」',
        textVn: 'Bảo vệ đàn cừu: Cùng bạch mã lao vào đêm tối đẩy lùi đàn sói dữ (+15 XP)',
        reactionText: 'Bạch mã hí vang dũng mãnh, vó ngựa tung bay xua tan bầy sói hoang!',
        xp: 15,
        branchResponse: {
          speaker: 'スーホ (Suho)',
          role: '✦ Cậu bé thảo nguyên dũng cảm',
          charId: 'suho_hero',
          avatar: 'assets/chars/char_melos.png',
          speakerPos: 'left',
          bg: bg,
          location: `🐎 ${book.title} • ĐÊM BÃO TUYẾT THẢO NGUYÊN`,
          theme: theme,
          jp: '「よくやったぞ、白馬！ お前がいれば、どんな困難も怖くないんだ。」',
          vn: '"Làm tốt lắm, ngựa trắng của ta! Chỉ cần có bạn bên cạnh, ta sẽ chẳng còn sợ bất kỳ hiểm nguy nào nữa."',
          grammarInsight: 'Cấu trúc [〜ば、〜ない]: Câu điều kiện chỉ sự vững tâm (Hễ có... thì không...)',
          nuanceInsight: 'Tình bạn tri kỷ và niềm tin son sắt giữa cậu bé du mục và chú ngựa trắng.',
          vocabTokens: [
            { word: '白馬', reading: 'はくば', meaning: 'Ngựa trắng, bạch mã', level: 'N3' },
            { word: '困難', reading: 'こんなん', meaning: 'Gian nan, trắc trở', level: 'N3' }
          ]
        }
      },
      {
        choiceId: 'suho_c2',
        branchType: 'Dialogue Variation',
        textJp: '「領主の理不尽な命令を拒絶し、白馬の綱を固く握りしめる」',
        textVn: 'Kiên định chính trực: Khước từ lệnh cướp ngựa của tên bạo chúa (+15 XP)',
        reactionText: 'Suho ngẩng cao đầu, ánh mắt kiên trung không khuất phục trước uy quyền!',
        xp: 15,
        branchResponse: {
          speaker: '領主 (Lãnh chúa)',
          role: '✦ Tên bạo chúa tham tàn',
          charId: 'ryoushu',
          avatar: 'assets/chars/char_dionys.png',
          speakerPos: 'right',
          bg: bg,
          location: `🐎 ${book.title} • ĐỐI ĐẦU BẠO CHÚA`,
          theme: theme,
          jp: '「生意気な小僧め！ その名馬を渡せば銀貨をくれてやるというのに……！」',
          vn: '"Thằng nhóc ngạo mạn kia! Ta đã bảo nếu dâng con ngựa quý đó thì sẽ thưởng bạc trắng cơ mà...!"',
          grammarInsight: 'Cấu trúc [〜というのに]: Biểu thị sự hậm hực, bất bình trước kết quả ngoài dự kiến.',
          nuanceInsight: 'Sự đối lập gay gắt giữa lòng tham quyền lực và tình bạn thuần khiết.',
          vocabTokens: [
            { word: '生意気', reading: 'なまいき', meaning: 'Hỗn láo, kiêu ngạo', level: 'N2' },
            { word: '名馬', reading: 'めいば', meaning: 'Ngựa quý, thần mã', level: 'N1' }
          ]
        }
      },
      {
        choiceId: 'suho_c3',
        branchType: 'Branching Scene',
        textJp: '「白馬の形見の骨と皮で、魂を宿す『馬頭琴』を彫り上げる」',
        textVn: 'Khắc sâu bất tử: Chế tác cây đàn Mã Đầu Cầm từ kỷ vật của ngựa trắng (+15 XP)',
        reactionText: 'Tiếng đàn Mã Đầu Cầm ngân vang, mang linh hồn bạch mã bay khắp thảo nguyên bao la!',
        xp: 15,
        branchResponse: {
          speaker: '語り手 (Người dẫn chuyện)',
          role: '✦ Truyền thuyết thảo nguyên Mông Cổ',
          charId: 'narrator',
          avatar: 'assets/covers/our_last_summer.jpg',
          speakerPos: 'center',
          bg: bg,
          location: `🐎 ${book.title} • TIẾNG ĐÀN BẤT TỬ`,
          theme: theme,
          jp: '「琴を奏でるたび、白馬はスーホのそばに寄り添い、優しい風となって草原を吹き抜けていく。」',
          vn: '"Mỗi khi tiếng đàn cất lên, chú ngựa trắng như lại nép sát bên Suho, hóa thành làn gió xuân êm đềm thổi qua thảo nguyên bát ngát."',
          grammarInsight: 'Cấu trúc [〜たび(に)]: Cứ mỗi lần X diễn ra thì Y lại xảy đến.',
          nuanceInsight: 'Cái kết bi tráng và thiêng liêng, nguồn gốc của cây đàn Morin Khuur trứ danh.',
          vocabTokens: [
            { word: '奏でる', reading: 'かなでる', meaning: 'Tấu đàn, gảy nhạc', level: 'N1' },
            { word: '草原', reading: 'そうげん', meaning: 'Thảo nguyên bao la', level: 'N3' }
          ]
        }
      }
    ],

    mushoku: [
      {
        choiceId: 'mushoku_c1',
        branchType: 'Branching Scene',
        textJp: '「無詠唱魔術の感覚を研ぎ澄まし、手のひらに水球を生み出す」',
        textVn: 'Khai phá tài năng: Tập trung ma lực thi triển ma pháp không niệm chú (+15 XP)',
        reactionText: 'Quả cầu nước tinh khiết xoay tròn lơ lửng trong không trung!',
        xp: 15,
        branchResponse: {
          speaker: 'ロキシー (Roxy)',
          role: '✦ Sư phụ ma thuật Migurd',
          charId: 'roxy',
          avatar: 'assets/chars/char_runa.png',
          speakerPos: 'right',
          bg: bg,
          location: `✨ ${book.title.slice(0, 24)} • PHÒNG TẬP MA PHÁP`,
          theme: theme,
          jp: '「信じられません……。詠唱をまったく唱えずに中級水魔術を発動させたのですか？」',
          vn: '"Thật không thể tin nổi... Em đã thi triển ma thuật nước trung cấp mà không cần ngâm nga bất kỳ câu niệm chú nào sao?"',
          grammarInsight: 'Cấu trúc [〜ずに]: Không làm hành động X mà thực hiện Y (hoàn toàn không niệm chú).',
          nuanceInsight: 'Sự thán phục và chấn động tâm lý của vị sư phụ trẻ trước thiên bẩm của Rudeus.',
          vocabTokens: [
            { word: '無詠唱', reading: 'むえいしょう', meaning: 'Không niệm chú', level: 'N1' },
            { word: '発動', reading: 'はつどう', meaning: 'Kích hoạt, phát động kỹ năng', level: 'N2' }
          ]
        }
      },
      {
        choiceId: 'mushoku_c2',
        branchType: 'Dialogue Variation',
        textJp: '「この世界で今度こそ本気で生き抜く覚悟を、心に誓う」',
        textVn: 'Ý chí hồi sinh: Thề nguyện kiếp này sẽ sống hết mình không hối tiếc (+15 XP)',
        reactionText: 'Ý chí kiên định soi tỏ con đường tương lai của Rudeus!',
        xp: 15,
        branchResponse: {
          speaker: 'ルーデウス (Rudeus)',
          role: '✦ Nhân vật chính chuyển sinh',
          charId: 'rudeus',
          avatar: 'assets/chars/char_melos.png',
          speakerPos: 'left',
          bg: bg,
          location: `✨ ${book.title.slice(0, 24)} • BƯỚC NGOẶT Ý CHÍ`,
          theme: theme,
          jp: '「前の人生のような後悔は絶対にしない。この世界で、僕は前を向いて生きるんだ。」',
          vn: '"Mình tuyệt đối sẽ không lặp lại những hối tiếc của kiếp trước. Tại thế giới này, mình sẽ luôn ngẩng cao đầu mà tiến bước."',
          grammarInsight: 'Cấu trúc [〜のような / 決して〜しない]: Khẳng định quyết tâm dứt khoát.',
          nuanceInsight: 'Bước ngoặt biến chuyển nội tâm mạnh mẽ nhất của nhân vật chính Rudeus Greyrat.',
          vocabTokens: [
            { word: '後悔', reading: 'こうかい', meaning: 'Hối tiếc, ân hận', level: 'N3' },
            { word: '人生', reading: 'じんせい', meaning: 'Cuộc đời, kiếp người', level: 'N4' }
          ]
        }
      },
      {
        choiceId: 'mushoku_c3',
        branchType: 'Branching Scene',
        textJp: '「パウロと木剣を交え、剣士としての基礎体力を鍛える」',
        textVn: 'Rèn luyện toàn diện: Cầm kiếm gỗ luyện tập kiếm thuật cùng cha Paul (+15 XP)',
        reactionText: 'Tiếng kiếm gỗ va chạm chan chát vang vọng ngoài sân tập!',
        xp: 15,
        branchResponse: {
          speaker: 'ロキシー (Roxy)',
          role: '✦ Sư phụ dõi theo học trò',
          charId: 'roxy',
          avatar: 'assets/chars/char_runa.png',
          speakerPos: 'right',
          bg: bg,
          location: `✨ ${book.title.slice(0, 24)} • SÂN TẬP BUỔI SỚM`,
          theme: theme,
          jp: '「魔術だけでなく剣術にも励むなんて、ルディは本当に努力家ですね。」',
          vn: '"Không chỉ chăm chỉ luyện ma pháp mà còn dốc sức học kiếm thuật, Rudy quả thực là một cậu bé vô cùng nỗ lực đấy."',
          grammarInsight: 'Cấu trúc [〜だけでなく〜も]: Không chỉ X mà còn cả Y.',
          nuanceInsight: 'Sự công nhận ấm áp từ sư phụ đối với đức tính cần cù hiếm thấy.',
          vocabTokens: [
            { word: '励む', reading: 'はげむ', meaning: 'Chăm chỉ, dốc sức rèn luyện', level: 'N2' },
            { word: '努力家', reading: 'どりょくか', meaning: 'Người cần cù, người nỗ lực', level: 'N2' }
          ]
        }
      }
    ],

    slime: [
      {
        choiceId: 'slime_c1',
        branchType: 'Branching Scene',
        textJp: '「大賢者スキルを起動し、周囲の洞窟の魔素と構造を解析する」',
        textVn: 'Khởi động trí tuệ: Kích hoạt [Đại Hiền Giả] phân tích ma tố xung quanh (+15 XP)',
        reactionText: 'Giọng nói máy móc thông thái vang lên trong tâm trí Rimuru!',
        xp: 15,
        branchResponse: {
          speaker: '大賢者 (Đại Hiền Giả)',
          role: '✦ Kỹ năng độc nhất trí tuệ',
          charId: 'great_sage',
          avatar: 'assets/char_campanella.png',
          speakerPos: 'right',
          bg: bg,
          location: `🌀 ${book.title.slice(0, 24)} • HANG ĐỘNG PHONG ẤN`,
          theme: theme,
          jp: '「解。洞窟内の魔素濃度は極めて高濃度です。解析鑑定が完了しました。」',
          vn: '"Giải đáp: Nồng độ ma tố trong hang động ở mức cực kỳ đậm đặc. Quá trình phân tích và thẩm định đã hoàn tất."',
          grammarInsight: 'Cấu trúc [解（かい）〜]: Cú pháp báo cáo đặc trưng đầy tính logic của Đại Hiền Giả.',
          nuanceInsight: 'Khẳng định năng lực phân tích bá đạo sẽ trở thành cánh tay đắc lực của Rimuru.',
          vocabTokens: [
            { word: '濃度', reading: 'のうど', meaning: 'Nồng độ', level: 'N2' },
            { word: '解析', reading: 'かいせき', meaning: 'Phân tích, giải mã', level: 'N1' }
          ]
        }
      },
      {
        choiceId: 'slime_c2',
        branchType: 'Branching Scene',
        textJp: '「捕食者スキルで、周囲に生える希少なヒポクテ草を丸呑みする」',
        textVn: 'Tích lũy tài nguyên: Kích hoạt [Kẻ Săn Mồi] hấp thụ cỏ dược liệu quý (+15 XP)',
        reactionText: 'Thân hình Slime co giãn nuốt trọn toàn bộ dược liệu và tinh thể ma khoáng!',
        xp: 15,
        branchResponse: {
          speaker: 'リムル (Rimuru)',
          role: '✦ Slime chuyển sinh vui vẻ',
          charId: 'rimuru',
          avatar: 'assets/chars/char_mob.png',
          speakerPos: 'left',
          bg: bg,
          location: `🌀 ${book.title.slice(0, 24)} • THU THẬP NGUYÊN LIỆU`,
          theme: theme,
          jp: '「よし、これで完全回復薬（ポーション）のストックがたくさん作れるぞ！」',
          vn: '"Tuyệt vời, vậy là mình có thể chế tạo được cả một kho bình thuốc hồi phục hoàn hảo (Potion) rồi!"',
          grammarInsight: 'Cấu trúc [〜これで〜できるぞ]: Nhờ việc này mà có thể làm được điều gì đó (thể hiện sự phấn khởi).',
          nuanceInsight: 'Sự vô tư yêu đời và bản năng thu thập vật phẩm đậm chất game RPG của Rimuru.',
          vocabTokens: [
            { word: '捕食', reading: 'ほしょく', meaning: 'Săn mồi, nuốt chửng', level: 'N1' },
            { word: '回復', reading: 'かいふく', meaning: 'Hồi phục sức khỏe, trị liệu', level: 'N3' }
          ]
        }
      },
      {
        choiceId: 'slime_c3',
        branchType: 'Branching Scene',
        textJp: '「思念伝達を放ち、封印された暴風竜ヴェルドラに語りかける」',
        textVn: 'Giao lưu định mệnh: Phóng thần giao cách cảm kết bạn cùng Rồng Bão Phong (+15 XP)',
        reactionText: 'Một luồng uy áp chấn động hang động kèm theo tiếng cười sấm sét!',
        xp: 15,
        branchResponse: {
          speaker: 'ヴェルドラ (Veldora)',
          role: '✦ Bạo Phong Long kiêu hãnh',
          charId: 'veldora',
          avatar: 'assets/chars/char_dionys.png',
          speakerPos: 'right',
          bg: bg,
          location: `🌀 ${book.title.slice(0, 24)} • GẶP GỠ TRI KỶ RỒNG THƯỢNG CỔ`,
          theme: theme,
          jp: '「クアハハハ！ 我の声が聞こえるのか、小さなスライムよ！ 面白い奴だ！」',
          vn: '"Khà khà khà! Ngươi nghe được giọng nói của bổn tọa sao, chú Slime bé nhỏ kia! Thật là một kẻ thú vị đấy!"',
          grammarInsight: 'Cấu trúc [我（われ）/ 〜奴だ]: Đại từ nhân xưng cổ xưa và uy nghiêm của loài rồng thượng cổ.',
          nuanceInsight: 'Khởi đầu của mối quan hệ tri kỷ kết nghĩa anh em định hình toàn bộ thế giới Jura Tempest.',
          vocabTokens: [
            { word: '思念', reading: 'しねん', meaning: 'Ý niệm, tâm tư', level: 'N1' },
            { word: '面白い', reading: 'おもしろい', meaning: 'Thú vị, hấp dẫn', level: 'N5' }
          ]
        }
      }
    ],

    rezero: [
      {
        choiceId: 'rezero_c1',
        branchType: 'Branching Scene',
        textJp: '「前回の記憶を手がかりに、危険な路地裏を事前に回避する」',
        textVn: 'Trở về từ cõi chết: Vận dụng ký ức kiếp trước đổi hướng né cạm bẫy (+15 XP)',
        reactionText: 'Ký ức đau đớn biến thành chiếc la bàn dẫn lối chuẩn xác!',
        xp: 15,
        branchResponse: {
          speaker: 'ナツキ・スバル (Subaru)',
          role: '✦ Chàng trai kiên cường',
          charId: 'subaru',
          avatar: 'assets/chars/char_mob.png',
          speakerPos: 'left',
          bg: bg,
          location: `⏳ ${book.title.slice(0, 24)} • CON PHỐ LUÂN HỒI`,
          theme: theme,
          jp: '「二度と同じ過ちは犯さない。今度こそ、全員が笑える未来を掴み取ってやる！」',
          vn: '"Mình tuyệt đối sẽ không phạm phải sai lầm tương tự thêm một lần nào nữa. Lần này, mình nhất định sẽ nắm lấy một tương lai nơi tất cả mọi người đều có thể mỉm cười!"',
          grammarInsight: 'Cấu trúc [二度と〜ない]: Tuyệt đối không bao giờ làm lại điều gì lần thứ hai.',
          nuanceInsight: 'Ý chí sắt đá của Subaru khi chấp nhận gánh chịu mọi nỗi đau để đổi lấy hạnh phúc cho người thân.',
          vocabTokens: [
            { word: '過ち', reading: 'あやまち', meaning: 'Sai lầm, lầm lỡ', level: 'N2' },
            { word: '掴み取る', reading: 'つかみとる', meaning: 'Nắm chặt lấy, đoạt lấy bằng nỗ lực', level: 'N2' }
          ]
        }
      },
      {
        choiceId: 'rezero_c2',
        branchType: 'Dialogue Variation',
        textJp: '「銀髪の少女エミリアの瞳を見つめ、守り抜くことを心に誓う」',
        textVn: 'Lời thề hiệp sĩ: Nhìn thẳng vào mắt Emilia, nguyện làm chiếc khiên bảo vệ (+15 XP)',
        reactionText: 'Nụ cười dịu dàng của Emilia xua tan đi bóng tối u uất!',
        xp: 15,
        branchResponse: {
          speaker: 'エミリア (Emilia)',
          role: '✦ Nửa yêu tinh tóc bạc lương thiện',
          charId: 'emilia',
          avatar: 'assets/char_campanella.png',
          speakerPos: 'right',
          bg: bg,
          location: `⏳ ${book.title.slice(0, 24)} • ÁNH SÁNG CỦA EMILIA`,
          theme: theme,
          jp: '「ありがとう、スバル。あなたが信じてくれるなら、私、もう少し頑張れる気がする。」',
          vn: '"Cảm ơn bạn nhé, Subaru. Nếu như bạn tin tưởng mình, mình cảm thấy bản thân dường như có thể cố gắng thêm một chút nữa rồi."',
          grammarInsight: 'Cấu trúc [〜気がする]: Có cảm giác là, dường như cảm thấy X.',
          nuanceInsight: 'Sự kết nối chân tình và thuần khiết, xoa dịu nỗi tự ti của cô gái bị người đời xa lánh.',
          vocabTokens: [
            { word: '信じる', reading: 'しんじる', meaning: 'Tin tưởng, gửi gắm niềm tin', level: 'N4' },
            { word: '頑張る', reading: 'がんばる', meaning: 'Cố gắng hết sức', level: 'N5' }
          ]
        }
      },
      {
        choiceId: 'rezero_c3',
        branchType: 'Branching Scene',
        textJp: '「レムに助けを求め、背中を預けて二人で困難を突破する」',
        textVn: 'Đồng hành tri kỷ: Trao gửi lưng cho Rem, phối hợp vượt qua nghịch cảnh (+15 XP)',
        reactionText: 'Quả chùy gai xoay tít, luồng gió quỷ tộc bảo vệ tuyệt đối!',
        xp: 15,
        branchResponse: {
          speaker: 'レム (Rem)',
          role: '✦ Hầu gái quỷ tộc tận tụy',
          charId: 'rem',
          avatar: 'assets/chars/char_runa.png',
          speakerPos: 'right',
          bg: bg,
          location: `⏳ ${book.title.slice(0, 24)} • LỜI HỨA CỦA QUỶ TỘC`,
          theme: theme,
          jp: '「スバル君が諦めない限り、レムはいつでもスバル君の英雄（ヒーロー）を信じています。」',
          vn: '"Miễn là Subaru-kun không bỏ cuộc, Rem sẽ luôn luôn đặt trọn niềm tin vào người anh hùng của riêng Rem."',
          grammarInsight: 'Cấu trúc [〜限り（かぎり）]: Miễn là, trong chừng mực mà X còn tiếp diễn.',
          nuanceInsight: 'Câu thoại kinh điển lay động hàng triệu trái tim độc giả của Rem dành cho Subaru.',
          vocabTokens: [
            { word: '諦める', reading: 'あきらめる', meaning: 'Bỏ cuộc, buông xuôi', level: 'N3' },
            { word: '英雄', reading: 'えいゆう', meaning: 'Anh hùng, người quả cảm', level: 'N2' }
          ]
        }
      }
    ],

    kokoro: [
      {
        choiceId: 'kokoro_c1',
        branchType: 'Dialogue Variation',
        textJp: '「先生の沈黙を尊重し、静かに波の音に耳を傾ける」',
        textVn: 'Tôn trọng sự u hoài: Lặng lẽ cùng Tiên sinh lắng nghe tiếng sóng biển Kamakura (+15 XP)',
        reactionText: 'Tiếng sóng vỗ rì rào mang lại sự bình yên hiếm hoi trong ánh mắt Tiên sinh.',
        xp: 15,
        branchResponse: {
          speaker: '先生 (Tiên sinh)',
          role: '✦ Người trí thức cô độc',
          charId: 'sensei',
          avatar: 'assets/chars/char_genin.png',
          speakerPos: 'right',
          bg: bg,
          location: `🌊 ${book.title} • BỜ BIỂN KAMAKURA HOÀNG HÔN`,
          theme: theme,
          jp: '「君は純粋だな。だがね、人間の心というものは、そう簡単に割り切れるものではないよ。」',
          vn: '"Cậu thật thuần khiết. Nhưng này, cõi lòng của con người ta, vốn chẳng phải thứ có thể phân định rạch ròi dễ dàng như thế đâu."',
          grammarInsight: 'Cấu trúc [〜割り切れるものではない]: Không phải là thứ có thể giải quyết hay rạch ròi một cách đơn giản.',
          nuanceInsight: 'Sự chiêm nghiệm sâu sắc của Natsume Soseki về sự phức tạp và bí ẩn khôn lường của tâm can con người.',
          vocabTokens: [
            { word: '純粋', reading: 'じゅんすい', meaning: 'Thuần khiết, ngây thơ', level: 'N2' },
            { word: '割り切る', reading: 'わりきる', meaning: 'Phân định dứt khoát, rạch ròi', level: 'N1' }
          ]
        }
      },
      {
        choiceId: 'kokoro_c2',
        branchType: 'Branching Scene',
        textJp: '「勇気を出して、なぜ毎月雑司ヶ谷へ墓参りに行くのか尋ねてみる」',
        textVn: 'Can đảm thấu hiểu: Gặng hỏi Tiên sinh về lý do viếng mộ hàng tháng (+15 XP)',
        reactionText: 'Ánh mắt Tiên sinh khẽ dao động, một thoáng bi thương sâu thẳm lướt qua.',
        xp: 15,
        branchResponse: {
          speaker: '先生 (Tiên sinh)',
          role: '✦ Nỗi đau chôn giấu bao năm',
          charId: 'sensei',
          avatar: 'assets/chars/char_genin.png',
          speakerPos: 'right',
          bg: bg,
          location: `🌊 ${book.title} • NỖI U HOÀI TẬN ĐÁY LÒNG`,
          theme: theme,
          jp: '「いつか……時が来たら、私の過去のすべてを君に話す日が来るかもしれないね。」',
          vn: '"Một lúc nào đó... khi thời khắc chín muồi, có lẽ sẽ tới ngày tôi kể cho cậu nghe toàn bộ quá khứ của cuộc đời tôi."',
          grammarInsight: 'Cấu trúc [〜かもしれない]: Có lẽ là, có thể sẽ (dự cảm tương lai).',
          nuanceInsight: 'Lời báo hiệu định mệnh mở đầu cho bức thư tuyệt mệnh huyền thoại ở phần cuối tác phẩm Kokoro.',
          vocabTokens: [
            { word: '過去', reading: 'かこ', meaning: 'Quá khứ đã qua', level: 'N4' },
            { word: '話す', reading: 'はなす', meaning: 'Kể lại, giãi bày', level: 'N5' }
          ]
        }
      },
      {
        choiceId: 'kokoro_c3',
        branchType: 'Dialogue Variation',
        textJp: '「奥さんの優しいお茶をいただきながら、先生の日常について伺う」',
        textVn: 'Ấm áp gia đình: Uống chén trà của Phu nhân và hỏi thăm sinh hoạt của Tiên sinh (+15 XP)',
        reactionText: 'Nụ cười dịu dàng của phu nhân Shizu xua bớt đi không khí trầm mặc của ngôi nhà.',
        xp: 15,
        branchResponse: {
          speaker: '静 (Phu nhân Shizu)',
          role: '✦ Người vợ dịu dàng ân cần',
          charId: 'shizu',
          avatar: 'assets/chars/char_runa.png',
          speakerPos: 'right',
          bg: bg,
          location: `🌊 ${book.title} • PHÒNG TRÀ ẤM CÚNG`,
          theme: theme,
          jp: '「あの方はいつも考え事ばかり。でも、あなたがいらしてくださると、どこか嬉しそうなのですよ。」',
          vn: '"Nhà tôi lúc nào cũng chỉ toàn suy tư trầm ngâm. Nhưng mỗi khi có cậu ghé chơi, ông ấy dường như lại có chút gì đó vui vẻ đấy."',
          grammarInsight: 'Cấu trúc [〜てくださる]: Nhận được hành động từ ai đó với lòng biết ơn (Kính ngữ).',
          nuanceInsight: 'Sự ấm áp, tinh tế của người phụ nữ truyền thống Nhật Bản nâng niu cuộc sống gia đình.',
          vocabTokens: [
            { word: '考え事', reading: 'かんがえごと', meaning: 'Sự trầm ngâm suy nghĩ', level: 'N3' },
            { word: '嬉しい', reading: 'うれしい', meaning: 'Vui mừng, hoan hỷ', level: 'N5' }
          ]
        }
      }
    ]
  };

  if (bespokeMap[bookKey]) {
    return bespokeMap[bookKey];
  }

  // 2. GENRE-BASED 3-BRANCH ADAPTIVE CHOICES
  if (g.includes('isekai') || g.includes('fantasy') || g.includes('adventure') || g.includes('action')) {
    return [
      {
        choiceId: `${bookKey}_c1`,
        branchType: 'Branching Scene',
        textJp: '「『我が力を解き放て！』と果敢に覚醒能力を発動する」',
        textVn: `Khai phóng năng lực: ${pChar.name} dũng mãnh tung đòn quyết định (+15 XP)`,
        reactionText: 'Luồng ma lực bùng nổ rực rỡ xé toạc màn đêm phía trước!',
        xp: 15,
        branchResponse: {
          speaker: `${cChar.japaneseName} (${cChar.name})`,
          role: `✦ ${book.title.slice(0, 24)} • Đồng đội thán phục`,
          charId: cChar.characterId,
          avatar: rightSprite,
          speakerPos: 'right',
          bg: bg,
          location: `⚔️ ${book.title.slice(0, 24)} • CHIẾN TÍCH KHỞI ĐẦU`,
          theme: theme,
          jp: `「見事な一撃ね！ その力があれば、この先の難関もきっと突破できるわ！」`,
          vn: `"Đòn tấn công xuất sắc lắm! Chỉ cần có sức mạnh ấy, chúng ta nhất định sẽ vượt qua mọi ải gian nan phía trước!"`,
          grammarInsight: `Cấu trúc [〜があれば、〜できる]: Nếu có X thì nhất định có thể làm được Y.`,
          nuanceInsight: `Sự tin tưởng tuyệt đối của đồng đội tiếp thêm dũng khí cho hành trình phiêu lưu.`,
          vocabTokens: [
            { word: '見事', reading: 'みごと', meaning: 'Tuyệt vời, xuất sắc', level: 'N3' },
            { word: '突破', reading: 'とっぱ', meaning: 'Đột phá, vượt qua hiểm nguy', level: 'N2' }
          ]
        }
      },
      {
        choiceId: `${bookKey}_c2`,
        branchType: 'Dialogue Variation',
        textJp: '「『敵の弱点を見極めよう』と冷静に戦況を分析する」',
        textVn: `Chiến thuật cẩn trọng: ${pChar.name} phân tích kẽ hở của đối phương (+15 XP)`,
        reactionText: 'Ánh mắt tinh tường nhìn thấu quy luật vận động của chiến trường!',
        xp: 15,
        branchResponse: {
          speaker: `${cChar.japaneseName} (${cChar.name})`,
          role: `✦ ${book.title.slice(0, 24)} • Yểm trợ chiến thuật`,
          charId: cChar.characterId,
          avatar: rightSprite,
          speakerPos: 'right',
          bg: bg,
          location: `⚔️ ${book.title.slice(0, 24)} • KẾ SÁCH CHU TOÀN`,
          theme: theme,
          jp: `「冷静な判断ね。相手の隙を突いて、安全に陣形を立て直しましょう！」`,
          vn: `"Phán đoán thật điềm tĩnh. Hãy nhắm đúng sơ hở của đối phương và củng cố lại đội hình an toàn nào!"`,
          grammarInsight: `Cấu trúc [隙を突く（すきをつく）]: Đánh trúng sơ hở, chớp lấy thời cơ.`,
          nuanceInsight: `Phong thái của một chỉ huy dày dạn kinh nghiệm thay vì manh động xông pha.`,
          vocabTokens: [
            { word: '冷静', reading: 'れいせい', meaning: 'Bình tĩnh, điềm đạm', level: 'N3' },
            { word: '判断', reading: 'はんだん', meaning: 'Phán đoán, nhận định', level: 'N3' }
          ]
        }
      },
      {
        choiceId: `${bookKey}_c3`,
        branchType: 'Branching Scene',
        textJp: '「『背中を預けるぞ！』と仲間と陣形を整え連携する」',
        textVn: `Gắn kết đồng đội: Khắc sâu sợi dây hiệp lực cùng ${cChar.name} (+15 XP)`,
        reactionText: 'Sợi dây liên kết đồng đội hóa thành bức tường kiên cố không thể xuyên thủng!',
        xp: 15,
        branchResponse: {
          speaker: `${cChar.japaneseName} (${cChar.name})`,
          role: `✦ ${book.title.slice(0, 24)} • Kề vai sát cánh`,
          charId: cChar.characterId,
          avatar: rightSprite,
          speakerPos: 'right',
          bg: bg,
          location: `⚔️ ${book.title.slice(0, 24)} • TÌNH BẰNG HỮU BẤT DIỆT`,
          theme: theme,
          jp: `「任せて！ 私たちが力を合わせれば、倒せない敵なんていないんだから！」`,
          vn: `"Cứ giao cho mình! Chỉ cần chúng ta chung sức, sẽ chẳng có kẻ thù nào là không thể đánh bại!"`,
          grammarInsight: `Cấu trúc [〜なんていないんだから]: Làm gì có chuyện... kia chứ (lời động viên khẳng định).`,
          nuanceInsight: `Tinh thần đồng đội Shounen kinh điển: Sức mạnh tình bạn vượt qua mọi giới hạn.`,
          vocabTokens: [
            { word: '合わせる', reading: 'あわせる', meaning: 'Hợp nhất, chung sức', level: 'N4' },
            { word: '敵', reading: 'てき', meaning: 'Kẻ địch, đối thủ', level: 'N3' }
          ]
        }
      }
    ];
  }

  if (g.includes('folktale') || g.includes('fairy') || g.includes('children') || g.includes('fable')) {
    return [
      {
        choiceId: `${bookKey}_c1`,
        branchType: 'Branching Scene',
        textJp: '「『困っている者を見捨てられない』と真心で助けの手を差し伸べる」',
        textVn: `Lòng nhân ái: ${pChar.name} dốc lòng cứu giúp bằng thiện lương (+15 XP)`,
        reactionText: 'Tấm lòng thiện lương gieo mầm phúc báo ấm áp giữa thế gian!',
        xp: 15,
        branchResponse: {
          speaker: `${cChar.japaneseName} (${cChar.name})`,
          role: `✦ ${book.title.slice(0, 24)} • Ánh sáng thiện lương`,
          charId: cChar.characterId,
          avatar: rightSprite,
          speakerPos: 'right',
          bg: bg,
          location: `🌸 ${book.title.slice(0, 24)} • ÁNH SÁNG THIỆN LƯƠNG`,
          theme: theme,
          jp: `「あなたの優しい心が奇跡を呼んだのですね。きっと良い報いがありますよ。」`,
          vn: `"Tấm lòng nhân hậu của bạn đã gọi mời phép màu tới. Nhất định sẽ có phúc báo tốt lành đền đáp."`,
          grammarInsight: `Cấu trúc [〜が奇跡を呼ぶ]: Điều gì đó mang lại phép màu kỳ diệu.`,
          nuanceInsight: `Mô típ kinh điển của truyện cổ tích Nhật Bản: Thiện hữu thiện báo, lòng tốt luôn được đền đáp.`,
          vocabTokens: [
            { word: '優しい', reading: 'やさしい', meaning: 'Hiền lành, dịu dàng', level: 'N5' },
            { word: '奇跡', reading: 'きせき', meaning: 'Phép màu, điều kỳ diệu', level: 'N2' }
          ]
        }
      },
      {
        choiceId: `${bookKey}_c2`,
        branchType: 'Dialogue Variation',
        textJp: '「『知恵を絞って切り抜けよう』と機転を利かせて工夫する」',
        textVn: `Mưu trí thông minh: ${pChar.name} vận dụng trí khôn vượt qua thử thách (+15 XP)`,
        reactionText: 'Sự nhạy bén và mưu trí giúp tháo gỡ nút thắt hiểm nguy một cách tài tình!',
        xp: 15,
        branchResponse: {
          speaker: `${cChar.japaneseName} (${cChar.name})`,
          role: `✦ ${book.title.slice(0, 24)} • Khâm phục trí tuệ`,
          charId: cChar.characterId,
          avatar: rightSprite,
          speakerPos: 'right',
          bg: bg,
          location: `🌸 ${book.title.slice(0, 24)} • TRÍ KHÔN DÂN GIAN`,
          theme: theme,
          jp: `「なんと賢い知恵でしょう！ 力ずくではなく機転で解決するとは見事です。」`,
          vn: `"Thật là một mưu trí thông minh làm sao! Dùng sự khéo léo thay vì bạo lực để giải quyết quả là tuyệt vời."`,
          grammarInsight: `Cấu trúc [なんと〜でしょう]: Thán từ cảm thán khen ngợi sâu sắc.`,
          nuanceInsight: `Ca ngợi trí thông minh và sự sắc sảo của người bình dân trong văn học dân gian.`,
          vocabTokens: [
            { word: '知恵', reading: 'ちえ', meaning: 'Trí khôn, sự khôn ngoan', level: 'N3' },
            { word: '解決', reading: 'かいけつ', meaning: 'Giải quyết, tháo gỡ', level: 'N3' }
          ]
        }
      },
      {
        choiceId: `${bookKey}_c3`,
        branchType: 'Branching Scene',
        textJp: '「『親や長老の言いつけを守ろう』と慎重に足元を固めて歩む」',
        textVn: `Khiêm cung giữ lễ: ${pChar.name} tuân giữ lời dặn của người đi trước (+15 XP)`,
        reactionText: 'Sự cẩn trọng và lòng kính cẩn giữ cho từng bước chân được bình an!',
        xp: 15,
        branchResponse: {
          speaker: `${cChar.japaneseName} (${cChar.name})`,
          role: `✦ ${book.title.slice(0, 24)} • Lời khen ngợi`,
          charId: cChar.characterId,
          avatar: rightSprite,
          speakerPos: 'right',
          bg: bg,
          location: `🌸 ${book.title.slice(0, 24)} • BÌNH AN TRỞ VỀ`,
          theme: theme,
          jp: `「教えを大切にする謙虚さこそが、災いを遠ざけ幸福を招く鍵なのですね。」`,
          vn: `"Chính sự khiêm nhường biết trân trọng lời dạy mới là chìa khóa xua tan tai ương và đón nhận hạnh phúc."`,
          grammarInsight: `Cấu trúc [〜こそが〜鍵だ]: Chính X mới thực sự là chìa khóa then chốt dẫn tới Y.`,
          nuanceInsight: `Bài học giáo dục đạo đức truyền thống: Sự khiêm nhường là cội nguồn của mọi phước lành.`,
          vocabTokens: [
            { word: '謙虚', reading: 'けんきょ', meaning: 'Khiêm nhường, lễ phép', level: 'N1' },
            { word: '幸福', reading: 'こうふく', meaning: 'Hạnh phúc trọn vẹn', level: 'N3' }
          ]
        }
      }
    ];
  }

  // Fallback: General Literary / Contemporary Branching
  return [
    {
      choiceId: `${bookKey}_c1`,
      branchType: 'Dialogue Variation',
      textJp: `「『前へ進もう！』と自らの意志で一歩を踏み出す」`,
      textVn: `Kiên định dấn bước: ${pChar.name} dũng cảm tiến thẳng về phía trước (+15 XP)`,
      reactionText: `Ánh sáng soi tỏ con đường, ${pChar.name} vững vàng sải bước về phía chân trời!`,
      xp: 15,
      branchResponse: {
        speaker: `${cChar.japaneseName} (${cChar.name})`,
        role: `✦ ${book.title.slice(0, 24)} • Đồng hành tin cậy`,
        charId: cChar.characterId,
        avatar: rightSprite,
        speakerPos: 'right',
        bg: bg,
        location: `🌟 ${book.title.slice(0, 24)} • BƯỚC NGOẶT Ý CHÍ`,
        theme: theme,
        jp: `「あなたの決意、とても頼もしいわ。どんな旅路になっても一緒に行きましょう！」`,
        vn: `"Quyết tâm của bạn thật đáng tin cậy. Dẫu chặng đường phía trước có ra sao, chúng ta hãy cùng nhau tiến bước nhé!"`,
        grammarInsight: `Cấu trúc [どんな〜になっても]: Dù cho có trở nên thế nào đi chăng nữa.`,
        nuanceInsight: `Lời hứa đồng hành son sắt trước ngã rẽ cuộc đời.`,
        vocabTokens: [
          { word: '頼もしい', reading: 'たのもしい', meaning: 'Đáng tin cậy, vững lòng', level: 'N2' },
          { word: '旅路', reading: 'たびじ', meaning: 'Chặng đường phiêu lưu', level: 'N2' }
        ]
      }
    },
    {
      choiceId: `${bookKey}_c2`,
      branchType: 'Dialogue Variation',
      textJp: `「『仲間と共に進もう！』と絆を確かめ合う」`,
      textVn: `Đồng hành cùng nhau: Khắc sâu sợi dây gắn kết cùng ${cChar.name} (+15 XP)`,
      reactionText: `Tình bạn cùng ${cChar.name} tiếp thêm sức mạnh vô song vượt qua gian nan!`,
      xp: 15,
      branchResponse: {
        speaker: `${cChar.japaneseName} (${cChar.name})`,
        role: `✦ ${book.title.slice(0, 24)} • Mối liên kết tâm hồn`,
        charId: cChar.characterId,
        avatar: rightSprite,
        speakerPos: 'right',
        bg: bg,
        location: `🌟 ${book.title.slice(0, 24)} • SỢI DÂY GẮN KẾT`,
        theme: theme,
        jp: `「ありがとう。一人では届かない場所でも、二人ならきっと辿り着けるわ。」`,
        vn: `"Cảm ơn bạn. Nơi mà một người đơn độc không thể chạm tới, nhưng nếu là hai chúng ta thì nhất định sẽ đặt chân đến được."`,
        grammarInsight: `Cấu trúc [〜ならきっと辿り着ける]: Nếu là X thì ắt hẳn sẽ tìm đến được đích.`,
        nuanceInsight: `Sức mạnh của sự tương trợ lẫn nhau giữa bão giông cuộc đời.`,
        vocabTokens: [
          { word: '届く', reading: 'とどく', meaning: 'Chạm tới, vươn tới', level: 'N4' },
          { word: '辿り着く', reading: 'たどりつく', meaning: 'Tìm đến được đích đến sau muôn vàn gian truân', level: 'N2' }
        ]
      }
    },
    {
      choiceId: `${bookKey}_c3`,
      branchType: 'Branching Scene',
      textJp: `「『言葉の力を信じて』と静かに心の中で誓う」`,
      textVn: `Khắc sâu ngôn từ: Biến từng câu từ tiếng Nhật thành hành trang cuộc đời (+15 XP)`,
      reactionText: `Từng nét chữ và thanh âm tiếng Nhật in đậm vào tâm trí qua Kotoba Odyssey!`,
      xp: 15,
      branchResponse: {
        speaker: `${cChar.japaneseName} (${cChar.name})`,
        role: `✦ ${book.title.slice(0, 24)} • Khát vọng học hỏi`,
        charId: cChar.characterId,
        avatar: rightSprite,
        speakerPos: 'right',
        bg: bg,
        location: `🌟 ${book.title.slice(0, 24)} • KHO TÀNG TRI THỨC`,
        theme: theme,
        jp: `「日本語の言葉ひとつひとつが、物語の美しい記憶となって心に刻まれていくわね。」`,
        vn: `"Từng câu từ tiếng Nhật sẽ trở thành những kỷ niệm tuyệt đẹp in sâu vào tâm trí trên hành trình Kotoba Odyssey."`,
        grammarInsight: `Cấu trúc [〜となって刻まれる]: Trở thành điều gì đó và khắc sâu vào tâm khảm.`,
        nuanceInsight: `Hòa quyện trải nghiệm đọc tiểu thuyết với hành trình nâng cao năng lực ngôn ngữ.`,
        vocabTokens: [
          { word: '記憶', reading: 'きおく', meaning: 'Ký ức, kỷ niệm sâu sắc', level: 'N3' },
          { word: '刻まれる', reading: 'きざまれる', meaning: 'Được khắc sâu, ghi tạc', level: 'N2' }
        ]
      }
    }
  ];
}

function getOrCreateVnStory(bookKey) {
  const masterShowcases = ['ginga', 'merosu', 'gon', 'rashomon', 'gakuen', 'chuumon'];
  if (masterShowcases.includes(bookKey)) {
    if (typeof vnDialoguesByBook !== 'undefined' && vnDialoguesByBook[bookKey] && vnDialoguesByBook[bookKey].length > 0) {
      return vnDialoguesByBook[bookKey];
    }
  }

  const book = (typeof booksData !== 'undefined' && booksData[bookKey]) ? booksData[bookKey] : null;
  if (!book) {
    return (typeof vnDialoguesByBook !== 'undefined' && vnDialoguesByBook.ginga) ? vnDialoguesByBook.ginga : [];
  }

  // Curated Prologue texts for featured works to guarantee authentic immersion (NEVER generic template lines!)
  const curatedPrologueProse = {
    suho_uma: {
      title: '第1章：白馬との出会い',
      content: `大草原の果て、澄みきった青空の下。少年スーホは一頭の傷ついた白い子馬に出会った。\n「おい、大丈夫か？ もう怖がらなくてもいいんだよ。俺の家へおいで」\n冷たい風が吹きつける草原で、震える白い体を抱きしめると、小さな命のぬくもりが胸に伝わってきた。`
    },
    tebukuro: {
      title: '第1章：雪の朝と冷たい手',
      content: `雪の降る寒い朝、子狐は初めて見る銀世界に目を丸くし、母さん狐のそばに駆け寄った。\n「母ちゃん、おててが冷たい！ おててがちんちんするよ」\n可愛い坊やの冷え切った小さな前足を温めながら、母さん狐は町の手袋屋へ行かせる決意を固めた。`
    },
    mushoku: {
      title: '第1章：第二の人生の幕開け',
      content: `34歳引きこもりニートだった俺は、トラックに撥ねられ、気づけば異世界の赤ん坊になっていた。\n「今度こそ……今度こそこの世界で、本気で生きてみせる！」\n目の前に広がる未知なる魔法の世界を見つめながら、胸の奥で熱い決意が滾っていた。`
    },
    slime: {
      title: '第1章：転生したらスライムだった件',
      content: `通り魔から後輩をかばって刺された三上悟は、気がつくと洞窟の中でスライムの姿になっていた。\n「ちょ、待てよ！ なんで俺の体がぷるぷるのスライムになってんだよ！？」\n目も見えず声も出ない暗闇の中、大賢者の声が静かに頭の中に響き渡った。`
    },
    rezero: {
      title: '第1章：始まりの余熱',
      content: `コンビニ帰りに突如として異世界へと召喚されたナツキ・スバルは、路地裏で途方に暮れていた。\n「ファンタジー世界召喚キター！ って、美少女のお出迎えはどこだよ！？」\n右も左もわからない異国の喧騒の中で、運命の歯車が静かに狂気と共に回り始めていた。`
    },
    kokoro: {
      title: '第1章：先生と私',
      content: `鎌倉の海岸で出会ったあの人を、私はいつしか「先生」と呼ぶようになっていた。\n「先生、またお話を伺ってもよろしいでしょうか」\n先生の横顔には、世を避けて深く思索に沈む者の、寂しくも孤高な影が常に漂っていた。`
    }
  };

  // Retrieve chapters for this novel
  const crawled = (typeof crawledChapterProse !== 'undefined' && crawledChapterProse[bookKey]);
  const curated = curatedPrologueProse[bookKey];
  const chaps = (typeof getNovelChapters === 'function') ? getNovelChapters(bookKey) : [];
  
  const ch1 = curated || (crawled ? { title: crawled.title, content: crawled.content } : (chaps[0] || {
    title: '第1章：物語の発端',
    content: `『${book.title}』―― 静かに流れる時間の中で、物語の幕が開ける。\n「目の前に広がる未知なる世界……ここから新しい旅が始まるのか」\n胸の奥で、確かな決意が芽生えていくのを感じていた。もう迷いはしない。`
  }));

  const paragraphs = (ch1.content || '').split(/\r?\n/).filter(p => p.trim().length > 0);

  // Retrieve bespoke or genre-accurate characters for THIS specific novel
  const roster = getNovelFallbackCharacters(bookKey);
  const pChar = roster[0] || { name: 'Nhân vật chính', japaneseName: '主人公', avatarUrl: 'assets/chars/char_mob.png' };
  const cChar = roster[1] || { name: 'Người đồng hành', japaneseName: '相棒', avatarUrl: 'assets/chars/char_runa.png' };

  let leftSprite = pChar.avatarUrl || 'assets/chars/char_mob.png';
  let rightSprite = cChar.avatarUrl || 'assets/chars/char_runa.png';
  let centerSprite = null;
  let bg = book.cover || 'assets/covers/cover_anime_forest.jpg';
  let theme = 'theme-gakuen';

  const g = (book.genre || '').toLowerCase();
  if (g.includes('romcom') || g.includes('school') || g.includes('slice of life') || g.includes('gourmet')) {
    bg = 'assets/covers/cover_school_romcom.jpg';
    theme = 'theme-gakuen';
  } else if (g.includes('folktale') || g.includes('fable') || g.includes('children') || g.includes('fairy')) {
    bg = 'assets/covers/cover_anime_forest.jpg';
    theme = 'theme-gon';
  } else if (g.includes('dark') || g.includes('mystery') || g.includes('psychological') || g.includes('nobel') || g.includes('classic')) {
    bg = 'assets/bg_shinkai_rashomon.jpg';
    theme = 'theme-rashomon';
  } else if (g.includes('isekai') || g.includes('fantasy') || g.includes('adventure') || g.includes('dungeon') || g.includes('action')) {
    bg = 'assets/covers/cover_anime_castle.jpg';
    theme = 'theme-merosu';
  }

  const dynamicDialogues = [];

  // Line 1: Narration opening (both characters already established on stage)
  const line1Text = paragraphs[0] || `『${book.title}』―― 静かに流れる時間の中で、物語の幕が開ける。`;
  dynamicDialogues.push({
    speaker: '語り手 (Người dẫn chuyện)',
    role: `📜 ${book.title} • Mở đầu câu chuyện`,
    charId: 'narrator',
    type: 'narration',
    avatar: book.cover || leftSprite,
    leftSprite: leftSprite,
    centerSprite: null,
    rightSprite: rightSprite,
    speakerPos: 'center',
    bg: bg,
    location: `📖 ${book.title.slice(0, 24)} • ${ch1.title.slice(0, 24)}`,
    theme: theme,
    jp: line1Text,
    vn: `"${book.title}" - Khởi đầu câu chuyện. Trong dòng chảy êm đềm của thời gian, những bức màn vận mệnh bắt đầu hé mở.`,
    grammarInsight: `Cấu trúc [〜の中で]: Trong không gian, thời gian hoặc bối cảnh X. Bút pháp mở đầu kinh điển trong văn học Nhật Bản.`,
    nuanceInsight: `Gợi mở không gian và cảm xúc khởi đầu của tác phẩm, khơi dậy sự tò mò và niềm say mê khám phá của người đọc.`,
    vocabTokens: [
      { word: '物語', reading: 'ものがたり', meaning: 'Câu chuyện, thiên truyện', level: 'N4' },
      { word: '時間', reading: 'じかん', meaning: 'Thời gian, thời khắc', level: 'N5' },
      { word: '静かに', reading: 'しずかに', meaning: 'Yên lặng, êm đềm', level: 'N5' }
    ]
  });

  // Line 2: Protagonist speech (Bespoke character name & voice!)
  const rawP1 = paragraphs[1] || '「信じられない……。これが目の前に広がる新しい世界の光景なのか」';
  const line2Jp = rawP1.startsWith('「') ? rawP1 : `「${rawP1}」`;
  dynamicDialogues.push({
    speaker: `${pChar.japaneseName} (${pChar.name})`,
    role: `✦ ${book.title} • ${pChar.name}`,
    charId: pChar.characterId,
    type: 'dialogue',
    avatar: leftSprite,
    leftSprite: leftSprite,
    centerSprite: null,
    rightSprite: rightSprite,
    speakerPos: 'left',
    bg: bg,
    location: `🌸 ${book.title.slice(0, 24)} • ${ch1.title.slice(0, 24)}`,
    theme: theme,
    jp: line2Jp,
    vn: `"Thật không thể tin được... Chẳng lẽ đây chính là quang cảnh của thế giới mới đang mở ra trước mắt mình sao."`,
    grammarInsight: `Cấu trúc [〜なのか / なのだろうか]: Đưa ra câu hỏi tự vấn với tâm trạng ngỡ ngàng trước thực tại.`,
    nuanceInsight: `Cảm xúc rung động và khát khao dấn thân khám phá chân trời mới của nhân vật chính ${pChar.name}.`,
    vocabTokens: [
      { word: '信じる', reading: 'しんじる', meaning: 'Tin tưởng, tin là thật', level: 'N4' },
      { word: '新しい', reading: 'あたらしい', meaning: 'Mới mẻ, tươi mới', level: 'N5' },
      { word: '光景', reading: 'こうけい', meaning: 'Quang cảnh, cảnh tượng trước mắt', level: 'N2' }
    ]
  });

  // Line 3: Inner Monologue of the Protagonist
  const rawP2 = paragraphs[2] || '胸の奥で、確かな決意が芽生えていくのを感じていた。もう迷いはしない';
  dynamicDialogues.push({
    speaker: `${pChar.japaneseName} (Độc thoại nội tâm)`,
    role: `💭 Ý niệm sâu kín của ${pChar.name}`,
    charId: `${pChar.characterId}_inner`,
    type: 'monologue',
    avatar: leftSprite,
    leftSprite: leftSprite,
    centerSprite: null,
    rightSprite: rightSprite,
    speakerPos: 'left',
    bg: bg,
    location: `🌸 ${book.title.slice(0, 24)} • BƯỚC NGOẶT Ý CHÍ`,
    theme: theme,
    jp: `（${rawP2.replace(/[（）()]/g, '')}）`,
    vn: `(Tận sâu trong lòng, mình cảm nhận được một ý chí kiên định đang dần nảy nở. Sẽ không còn do dự hay nao núng nữa.)`,
    grammarInsight: `Cấu trúc [〜のを感じていた]: Cảm nhận được sự việc gì đó đang diễn ra. [迷いはしない]: Khẳng định dứt khoát không hề lưỡng lự.`,
    nuanceInsight: `Khắc họa khoảnh khắc biến chuyển tâm lý quan trọng: từ bỡ ngỡ bước sang quyết tâm kiên định của ${pChar.name}.`,
    vocabTokens: [
      { word: '決意', reading: 'けつい', meaning: 'Quyết tâm, ý chí sắt son', level: 'N3' },
      { word: '感じる', reading: 'かんじる', meaning: 'Cảm thấy, cảm nhận', level: 'N4' },
      { word: '迷い', reading: 'まよい', meaning: 'Sự băn khoăn, do dự', level: 'N2' }
    ]
  });

  // Line 4: Interactive Choice Point (3 Rich Branching Options)
  const choices = buildNovelBranchingChoices(bookKey, book, pChar, cChar, leftSprite, rightSprite, bg, theme);

  dynamicDialogues.push({
    speaker: `選択の刻 (Thời khắc lựa chọn)`,
    role: `✦ Quyết định hướng đi của ${pChar.name}`,
    charId: 'choice',
    type: 'choice',
    avatar: leftSprite,
    leftSprite: leftSprite,
    centerSprite: null,
    rightSprite: rightSprite,
    speakerPos: 'left',
    bg: bg,
    location: `✨ ${book.title.slice(0, 24)} • LỰA CHỌN CỦA BẠN`,
    theme: theme,
    jp: `「目の前に広がる未知なる道――さあ、どちらへ進むべきだろうか？」`,
    vn: `"Con đường phía trước đang rộng mở... Nào, ta nên tiến bước theo ngã rẽ nào đây?"`,
    choices: choices,
    grammarInsight: `Cấu trúc [〜べきだろうか]: Liệu nên làm hành động gì? [一歩を踏み出す]: Cất bước chân đầu tiên dấn thân.`,
    nuanceInsight: `Trao quyền quyết định cho độc giả để đồng hành cùng nhân vật ${pChar.name} trong cuộc phiêu lưu.`,
    vocabTokens: [
      { word: '未知', reading: 'みち', meaning: 'Chưa từng biết tới, bí ẩn', level: 'N2' },
      { word: '進む', reading: 'すすむ', meaning: 'Tiến lên, dấn bước', level: 'N4' },
      { word: '絆', reading: 'きずな', meaning: 'Sợi dây gắn kết, mối liên kết chân tình', level: 'N2' }
    ]
  });

  // Line 5: Companion Response / Chapter Resolution (Defaults to Choice 1, dynamically replaced upon user choice!)
  const defaultResolution = (choices[0] && choices[0].branchResponse) ? choices[0].branchResponse : {
    speaker: `${cChar.japaneseName} (${cChar.name})`,
    role: `✦ ${book.title} • ${cChar.name}`,
    charId: cChar.characterId,
    type: 'dialogue',
    avatar: rightSprite,
    leftSprite: leftSprite,
    centerSprite: null,
    rightSprite: rightSprite,
    speakerPos: 'right',
    bg: bg,
    location: `🌟 ${book.title.slice(0, 24)} • TIẾP NỐI HÀNH TRÌNH`,
    theme: theme,
    jp: (paragraphs[3] || '日本語の言葉ひとつひとつが、物語の美しい記憶となって心に刻まれていく。'),
    vn: `"${cChar.name} mỉm cười đồng hành: Từng câu từ tiếng Nhật trở thành những ký ức tuyệt đẹp in sâu vào tâm trí qua Kotoba Odyssey."`,
    grammarInsight: `Cấu trúc [〜となって刻まれていく]: Trở thành điều gì đó và khắc sâu vào tâm khảm theo thời gian.`,
    nuanceInsight: `Sự gắn bó mật thiết giữa ${pChar.name} và ${cChar.name}, khép lại phân cảnh mở đầu đầy cảm xúc.`,
    vocabTokens: [
      { word: '記憶', reading: 'きおく', meaning: 'Ký ức, kỷ niệm sâu sắc', level: 'N3' },
      { word: '心', reading: 'こころ', meaning: 'Trái tim, tâm hồn', level: 'N5' },
      { word: '刻まれる', reading: 'きざまれる', meaning: 'Được khắc sâu, in đậm', level: 'N2' }
    ]
  };

  dynamicDialogues.push(defaultResolution);

  if (typeof vnDialoguesByBook !== 'undefined') {
    vnDialoguesByBook[bookKey] = dynamicDialogues;
  }

  return dynamicDialogues;
}

// -------------------------------------------------------------------
// 4. STAGE & SCENE CONTROLLER (Characters, Backgrounds, Lighting)
// -------------------------------------------------------------------
function updateVnStage() {
  if (!vnDialogues || vnDialogues.length === 0) return;
  if (vnIndex >= vnDialogues.length) vnIndex = 0;

  const d = vnDialogues[vnIndex];
  if (!d) return;

  hideVnChoices();
  closeVnWordPopup();

  // 1. Theme class on Theatre Wrap
  const theatreWrap = document.querySelector('.vn-theatre-wrap');
  if (theatreWrap) {
    theatreWrap.classList.remove('theme-ginga', 'theme-merosu', 'theme-gon', 'theme-rashomon', 'theme-gakuen', 'theme-chuumon');
    if (d.theme) theatreWrap.classList.add(d.theme);
  }

  // 2. Background Crossfade Transition
  const bgArt = document.getElementById('vnBgArt');
  const bgArtNext = document.getElementById('vnBgArtNext');
  if (bgArt && d.bg) {
    if (bgArt.getAttribute('src') !== d.bg) {
      if (bgArtNext) {
        bgArtNext.src = d.bg;
        bgArtNext.style.opacity = '1';
        setTimeout(() => {
          bgArt.src = d.bg;
          bgArtNext.style.opacity = '0';
        }, 400);
      } else {
        bgArt.src = d.bg;
      }
    }
  }

  // 3. Location and Scene tags
  const curBook = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]);
  const locTag = document.getElementById('vnLocationTag');
  if (locTag) locTag.innerHTML = `<span>${escapeHtml(d.location || (curBook ? `🌸 ${curBook.title}` : 'PHÂN CẢNH VĂN HỌC'))}</span>`;

  const sceneBadge = document.getElementById('currentSceneTag');
  if (sceneBadge) {
    sceneBadge.textContent = `Cảnh ${vnIndex + 1}/${vnDialogues.length}`;
  }

  const navTitle = document.getElementById('currentStoryNavTitle');
  if (navTitle) {
    navTitle.textContent = curBook ? (curBook.title.length > 20 ? curBook.title.slice(0, 18) + '...' : curBook.title) : 'Visual Novel';
  }

  // 4. Standees (Left, Center, Right slots)
  updateStandeeSlots(d);

  // 5. Nameplate & Speech Type Styling
  updateNameplateAndType(d);

  // 6. Record to Dialogue Backlog
  if (!vnBacklog.some(b => b.jp === d.jp && b.idx === vnIndex && b.bookKey === currentVnBookKey)) {
    vnBacklog.push({
      bookKey: currentVnBookKey,
      idx: vnIndex,
      speaker: d.speaker,
      role: d.role,
      avatar: d.avatar,
      jp: d.jp,
      vn: d.vn,
      type: d.type || 'dialogue'
    });
  }

  // 7. Start Typewriter Rendering
  startVnTypewriter(d.jp, d.vn, d);

  // 8. If AI Sensei drawer is open, automatically refresh analysis for current line
  if (document.getElementById('vnAiSenseiDrawer')?.classList.contains('open')) {
    populateAiSenseiContent(d);
  }
}

function updateStandeeSlots(d) {
  const slotLeft = document.getElementById('slotLeft');
  const slotCenter = document.getElementById('slotCenter');
  const slotRight = document.getElementById('slotRight');

  const charLeft = document.getElementById('charLeft');
  const charCenter = document.getElementById('charCenter');
  const charRight = document.getElementById('charRight');

  // Left Slot
  if (charLeft) {
    if (d.leftSprite) {
      charLeft.src = d.leftSprite;
      charLeft.style.display = 'block';
      if (slotLeft) slotLeft.style.display = 'block';
      if (d.speakerPos === 'left') {
        charLeft.className = 'vn-char-sprite active-speaker';
      } else {
        charLeft.className = 'vn-char-sprite dimmed-listener';
      }
    } else {
      charLeft.style.display = 'none';
      if (slotLeft) slotLeft.style.display = 'none';
    }
  }

  // Center Slot
  if (charCenter) {
    if (d.centerSprite) {
      charCenter.src = d.centerSprite;
      charCenter.style.display = 'block';
      if (slotCenter) slotCenter.style.display = 'block';
      if (d.speakerPos === 'center') {
        charCenter.className = 'vn-char-sprite active-speaker center-standee';
      } else {
        charCenter.className = 'vn-char-sprite dimmed-listener center-standee';
      }
    } else {
      charCenter.style.display = 'none';
      if (slotCenter) slotCenter.style.display = 'none';
    }
  }

  // Right Slot
  if (charRight) {
    if (d.rightSprite) {
      charRight.src = d.rightSprite;
      charRight.style.display = 'block';
      if (slotRight) slotRight.style.display = 'block';
      if (d.speakerPos === 'right') {
        charRight.className = 'vn-char-sprite active-speaker';
      } else {
        charRight.className = 'vn-char-sprite dimmed-listener';
      }
    } else {
      charRight.style.display = 'none';
      if (slotRight) slotRight.style.display = 'none';
    }
  }
}

function updateNameplateAndType(d) {
  const nameplate = document.getElementById('vnNameplate');
  const avatarImg = document.getElementById('vnSpeakerAvatar');
  const speakerName = document.getElementById('vnSpeaker');
  const roleEl = document.getElementById('vnSpeakerRole');
  const storyBox = document.getElementById('vnStoryBox');

  if (!storyBox) return;

  storyBox.classList.remove('speech-type-dialogue', 'speech-type-monologue', 'speech-type-narration', 'speech-type-choice');

  if (d.type === 'narration') {
    storyBox.classList.add('speech-type-narration');
    if (nameplate) nameplate.style.display = 'none';
  } else if (d.type === 'monologue') {
    storyBox.classList.add('speech-type-monologue');
    if (nameplate) {
      nameplate.style.display = 'flex';
      nameplate.className = 'vn-nameplate monologue-ghost';
    }
    if (avatarImg) avatarImg.src = d.avatar || d.leftSprite || 'assets/chars/char_mob.png';
    if (speakerName) speakerName.textContent = d.speaker;
    if (roleEl) roleEl.textContent = '💭 Độc thoại nội tâm';
  } else {
    storyBox.classList.add('speech-type-dialogue');
    if (nameplate) {
      nameplate.style.display = 'flex';
      nameplate.className = 'vn-nameplate';
    }
    if (avatarImg) avatarImg.src = d.avatar || d.leftSprite || 'assets/chars/char_mob.png';
    if (speakerName) speakerName.textContent = d.speaker;
    if (roleEl) roleEl.textContent = d.role || '';
  }
}

// -------------------------------------------------------------------
// 5. TYPEWRITER & INTERACTIVE TOKENS ENGINE
// -------------------------------------------------------------------
function startVnTypewriter(jpText, vnText, d) {
  if (typeTimer) clearInterval(typeTimer);
  if (vnAutoTimer) clearTimeout(vnAutoTimer);

  currentFullJp = jpText || '';
  currentFullVn = vnText || '';
  currentTypingIndex = 0;
  isTyping = true;

  const jpEl = document.getElementById('vnJp');
  const vnEl = document.getElementById('vnVn');
  const advancePrompt = document.getElementById('vnAdvancePrompt');

  if (jpEl) jpEl.textContent = '';
  if (vnEl) {
    vnEl.textContent = currentFullVn;
    vnEl.style.opacity = '0';
  }
  if (advancePrompt) advancePrompt.classList.remove('ready');

  const speed = 26; // ms per char

  typeTimer = setInterval(() => {
    if (!isTyping) {
      clearInterval(typeTimer);
      return;
    }

    currentTypingIndex++;
    if (jpEl) {
      jpEl.textContent = currentFullJp.slice(0, currentTypingIndex);
    }

    if (currentTypingIndex % 2 === 0) {
      playVnTypewriterClick();
    }

    if (currentTypingIndex >= currentFullJp.length) {
      finishTypewriterInstantly(d);
    }
  }, speed);
}

function finishTypewriterInstantly(optionalDialogData) {
  if (typeTimer) clearInterval(typeTimer);
  isTyping = false;

  const d = optionalDialogData || vnDialogues[vnIndex];
  const jpEl = document.getElementById('vnJp');
  const vnEl = document.getElementById('vnVn');
  const advancePrompt = document.getElementById('vnAdvancePrompt');

  if (jpEl) {
    jpEl.innerHTML = formatVnDialogueTokens(currentFullJp, d);
  }

  if (vnEl && isVnTransVisible) {
    vnEl.textContent = currentFullVn;
    vnEl.style.opacity = '1';
    vnEl.style.display = 'block';
  }

  if (advancePrompt) {
    advancePrompt.classList.add('ready');
  }

  if (d && d.choices && d.choices.length > 0) {
    renderVnChoices(d.choices);
  } else if (isVnAutoPlay) {
    vnAutoTimer = setTimeout(() => {
      advanceVnDialogue();
    }, vnAutoDelay);
  }
}

function handleVnDialogueBoxClick(event) {
  if (event.target.closest('.vn-word-tok') || event.target.closest('.vn-choice-card') || event.target.closest('.shinkai-advance-trigger')) {
    return;
  }

  if (isTyping) {
    finishTypewriterInstantly();
  } else {
    advanceVnDialogue();
  }
}

function advanceVnDialogue() {
  if (currentVnMode === 'chat') return;
  if (currentActiveChoice) return;
  if (!vnDialogues || vnDialogues.length === 0) return;

  if (isTyping) {
    finishTypewriterInstantly();
    return;
  }

  vnIndex++;
  if (vnIndex >= vnDialogues.length) {
    vnIndex = 0;
    showToastNotification('🌸 Đã hoàn thành phân cảnh! Bắt đầu lại từ đầu.', 'info');
  }
  updateVnStage();
}

// -------------------------------------------------------------------
// 6. INTERACTIVE JAPANESE WORD TOKENS & INSPECTOR
// -------------------------------------------------------------------
function formatVnDialogueTokens(rawJp, d) {
  const regex = /([\u4e00-\u9faf\u3400-\u4dbf]+|[\u30a0-\u30ff]+|[\u3040-\u309f]{1,4}|[a-zA-Z0-9]+|[^\s\w\u3040-\u30ff\u4e00-\u9faf]+)/gu;
  const tokens = rawJp.match(regex) || [rawJp];

  return tokens.map(tok => {
    if (/^[\s、。！？「」『』（）…—\.,!?\(\)"]+$/.test(tok)) {
      return `<span class="vn-punct-tok">${escapeHtml(tok)}</span>`;
    }

    let vocabInfo = null;
    if (d && d.vocabTokens) {
      vocabInfo = d.vocabTokens.find(v => tok.includes(v.word) || v.word.includes(tok));
    }

    const levelAttr = vocabInfo ? `data-level="${vocabInfo.level}"` : '';
    const meaningAttr = vocabInfo ? `data-meaning="${escapeHtml(vocabInfo.meaning)}"` : '';
    const readAttr = vocabInfo ? `data-reading="${escapeHtml(vocabInfo.reading)}"` : '';

    return `<span class="vn-word-tok" ${levelAttr} ${meaningAttr} ${readAttr} onclick="handleVnWordTokenClick(event, this, '${escapeHtml(tok)}')">${escapeHtml(tok)}</span>`;
  }).join('');
}

function handleVnWordTokenClick(event, element, word) {
  event.stopPropagation();

  const reading = element.getAttribute('data-reading') || word;
  const meaning = element.getAttribute('data-meaning') || 'Đang tra cứu từ điển ngữ cảnh...';
  const level = element.getAttribute('data-level') || 'N3';

  activeDrawerWord = {
    word: word,
    reading: reading,
    meaning: meaning,
    jlpt: level,
    sentence: currentFullJp,
    sentenceVn: currentFullVn
  };

  showVnWordPopup(element, activeDrawerWord);
}

function showVnWordPopup(anchorEl, wordData) {
  let popup = document.getElementById('vnWordPopup');
  if (!popup) {
    popup = document.createElement('div');
    popup.id = 'vnWordPopup';
    popup.className = 'vn-word-popup';
    document.body.appendChild(popup);
  }

  popup.innerHTML = `
    <div class="vn-popup-inner">
      <div class="vn-popup-head">
        <div class="vn-popup-term">
          <span class="popup-kanji">${escapeHtml(wordData.word)}</span>
          <span class="popup-reading">${escapeHtml(wordData.reading)}</span>
        </div>
        <span class="popup-jlpt-tag jlpt-badge-${(wordData.jlpt || 'N3').toLowerCase()}">${wordData.jlpt || 'JLPT'}</span>
        <button class="popup-close-btn" onclick="closeVnWordPopup(event)">&times;</button>
      </div>

      <div class="vn-popup-meaning">${escapeHtml(wordData.meaning)}</div>

      <div class="vn-popup-actions">
        <button class="btn-popup-srs" onclick="saveVnWordPopupToSrs(this)">
          <span>➕ Lưu SRS (+10 XP)</span>
        </button>
        <button class="btn-popup-ai" onclick="openAiSenseiFromPopup()">
          <span>🧠 Hỏi AI Sensei</span>
        </button>
      </div>
    </div>
  `;

  const rect = anchorEl.getBoundingClientRect();
  const top = rect.top - 140 > 10 ? rect.top - 140 : rect.bottom + 12;
  const left = Math.max(16, Math.min(window.innerWidth - 320, rect.left - 40));

  popup.style.top = `${top}px`;
  popup.style.left = `${left}px`;
  popup.style.display = 'block';
  popup.classList.add('visible');
}

function closeVnWordPopup(event) {
  if (event) event.stopPropagation();
  const popup = document.getElementById('vnWordPopup');
  if (popup) {
    popup.style.display = 'none';
    popup.classList.remove('visible');
  }
}

async function saveVnWordPopupToSrs(btn) {
  if (!activeDrawerWord) return;
  const novelTitle = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]?.title) || 'Visual Novel';

  if (btn) {
    btn.innerHTML = '<span>⏳ Đang lưu...</span>';
    btn.disabled = true;
  }

  if (typeof saveWordToSRS === 'function') {
    const result = await saveWordToSRS(
      activeDrawerWord.word, 
      activeDrawerWord.reading, 
      activeDrawerWord.meaning, 
      activeDrawerWord.jlpt || 'N3', 
      activeDrawerWord.sentence || '', 
      activeDrawerWord.sentenceVn || '', 
      novelTitle
    );

    if (btn) {
      if (result && result.isDuplicate) {
        btn.innerHTML = '<span>✓ Đã có trong SRS</span>';
        btn.style.color = '#60A5FA';
      } else {
        btn.innerHTML = '<span>✓ Đã lưu SRS (+10 XP)!</span>';
        btn.style.color = '#10B981';
      }
      btn.disabled = true;
    }
  }
}

function openAiSenseiFromPopup() {
  closeVnWordPopup();
  openVnAiSensei();
}

// -------------------------------------------------------------------
// 7. CHOICE SELECTION SYSTEM
// -------------------------------------------------------------------
function renderVnChoices(choices) {
  currentActiveChoice = choices;
  const overlay = document.getElementById('vnChoiceOverlay');
  const container = document.getElementById('vnChoiceContainer');
  if (!overlay || !container) return;

  container.innerHTML = choices.map((c, idx) => `
    <div class="vn-choice-card" onclick="selectVnChoice(${idx})">
      <div class="vn-choice-jp">${escapeHtml(c.textJp)}</div>
      <div class="vn-choice-vn">${escapeHtml(c.textVn)}</div>
      <div class="vn-choice-xp-chip">+15 XP</div>
    </div>
  `).join('');

  overlay.style.display = 'flex';
  overlay.classList.add('active');
  playVnChoiceChime();
}

function selectVnChoice(idx) {
  const choices = currentActiveChoice || (vnDialogues && vnDialogues[vnIndex] && vnDialogues[vnIndex].choices);
  if (!choices || !choices[idx]) return;
  const chosen = choices[idx];

  playSuccessChime();

  // Anti-Exploit XP Tracking: One reward per choice event
  const choiceRewardKey = `vn_choice_xp_${currentVnBookKey || 'scene'}_ch1`;
  let alreadyRewarded = false;
  try {
    alreadyRewarded = localStorage.getItem(choiceRewardKey) === '1';
  } catch(e) {}

  if (!alreadyRewarded) {
    addLocalXp(chosen.xp || 15);
    try { localStorage.setItem(choiceRewardKey, '1'); } catch(e) {}
    showToastNotification(`✨ Đã lựa chọn: ${chosen.textVn} (+15 XP)`, 'success');
  } else {
    showToastNotification(`✨ Đã lựa chọn: ${chosen.textVn} (Đã nhận thưởng trước đó)`, 'info');
  }

  hideVnChoices();

  if (chosen.reactionText) {
    showToastNotification(`✦ ${chosen.reactionText}`, 'info');
  }

  // Real Story Branching Consequence: dynamically update subsequent resolution dialogue!
  if (chosen.branchResponse && vnDialogues && vnDialogues[vnIndex + 1]) {
    Object.assign(vnDialogues[vnIndex + 1], chosen.branchResponse);
  }

  setTimeout(() => {
    advanceVnDialogue();
  }, 400);
}

function hideVnChoices() {
  currentActiveChoice = null;
  const overlay = document.getElementById('vnChoiceOverlay');
  if (overlay) {
    overlay.style.display = 'none';
    overlay.classList.remove('active');
  }
}

// -------------------------------------------------------------------
// 8. AI SENSEI SIDE DRAWER & GRAMMAR BREAKDOWN
// -------------------------------------------------------------------
function toggleVnAiSensei() {
  const drawer = document.getElementById('vnAiSenseiDrawer');
  if (!drawer) return;
  if (drawer.classList.contains('open')) {
    closeVnAiSensei();
  } else {
    openVnAiSensei();
  }
}

function openVnAiSensei() {
  const drawer = document.getElementById('vnAiSenseiDrawer');
  if (!drawer) return;
  drawer.classList.add('open');

  const d = vnDialogues[vnIndex];
  if (d) populateAiSenseiContent(d);
}

function closeVnAiSensei() {
  const drawer = document.getElementById('vnAiSenseiDrawer');
  if (drawer) drawer.classList.remove('open');
}

function populateAiSenseiContent(d) {
  const titleEl = document.getElementById('aiSenseiSentenceJp');
  const vnEl = document.getElementById('aiSenseiSentenceVn');
  const grammarEl = document.getElementById('aiSenseiGrammarContent');
  const nuanceEl = document.getElementById('aiSenseiNuanceContent');
  const vocabList = document.getElementById('aiSenseiVocabList');

  if (titleEl) titleEl.textContent = d.jp;
  if (vnEl) vnEl.textContent = d.vn;

  if (grammarEl) {
    grammarEl.innerHTML = d.grammarInsight || 'Đang phân tích cấu trúc cú pháp câu thoại...';
  }

  if (nuanceEl) {
    nuanceEl.innerHTML = d.nuanceInsight || 'Sắc thái cảm xúc và bối cảnh văn học Nhật Bản.';
  }

  if (vocabList) {
    if (d.vocabTokens && d.vocabTokens.length > 0) {
      vocabList.innerHTML = d.vocabTokens.map(v => `
        <div class="sensei-vocab-card">
          <div class="sensei-vocab-header">
            <span class="vocab-word">${escapeHtml(v.word)}</span>
            <span class="vocab-reading">(${escapeHtml(v.reading)})</span>
            <span class="vocab-badge">${escapeHtml(v.level || 'N3')}</span>
          </div>
          <div class="vocab-meaning">${escapeHtml(v.meaning)}</div>
          <button class="btn-save-mini" onclick="saveDirectSenseiVocab('${escapeHtml(v.word)}', '${escapeHtml(v.reading)}', '${escapeHtml(v.meaning)}', '${escapeHtml(v.level)}')">
            ➕ Lưu SRS
          </button>
        </div>
      `).join('');
    } else {
      vocabList.innerHTML = '<div style="font-size:12px;color:#94A3B8;padding:8px;">Bấm trực tiếp vào các từ vựng trong câu thoại để lưu nhanh vào SRS.</div>';
    }
  }
}

function saveDirectSenseiVocab(word, reading, meaning, level) {
  saveWordToSRS(word, reading, meaning, level || 'N3', currentFullJp, currentFullVn, currentVnBookKey);
  playSuccessChime();
  showToastNotification(`Đã lưu "${word}" vào SRS! (+10 XP)`, 'success');
  addLocalXp(10);
}

// -------------------------------------------------------------------
// 9. DIALOGUE BACKLOG / HISTORY
// -------------------------------------------------------------------
function toggleVnLog() {
  const modal = document.getElementById('vnLogModal');
  if (!modal) return;
  modal.classList.toggle('open');

  if (modal.classList.contains('open')) {
    const list = document.getElementById('vnLogList');
    if (list) {
      if (vnBacklog.length === 0) {
        list.innerHTML = '<div style="padding: 24px; text-align: center; color: #94A3B8;">Chưa có câu thoại nào trong lịch sử.</div>';
      } else {
        list.innerHTML = vnBacklog.map((item, idx) => `
          <div class="vn-backlog-item">
            <div class="backlog-head">
              <img src="${item.avatar || 'assets/char_giovanni.png'}" class="backlog-avatar">
              <span class="backlog-speaker">${escapeHtml(item.speaker)}</span>
              <button class="backlog-speak-btn" onclick="speakBacklogLine('${escapeHtml(item.jp).replace(/'/g, "\\'")}')" title="Phát âm câu này">
                🔊 Nghe lại
              </button>
            </div>
            <div class="backlog-jp">${escapeHtml(item.jp)}</div>
            <div class="backlog-vn">${escapeHtml(item.vn)}</div>
          </div>
        `).join('');
        list.scrollTop = list.scrollHeight;
      }
    }
  }
}

function speakBacklogLine(jp) {
  const clean = jp.replace(/[「」]/g, '').trim();
  speakJapanese(clean);
}

function speakVnLine() {
  const d = vnDialogues[vnIndex];
  if (!d || !d.jp) return;
  const clean = d.jp.replace(/[「」]/g, '').trim();
  speakJapanese(clean);
}

// -------------------------------------------------------------------
// 10. QUICK HUD & SETTINGS CONTROLLERS
// -------------------------------------------------------------------
function toggleVnTransVisibility() {
  isVnTransVisible = !isVnTransVisible;
  const vnEl = document.getElementById('vnVn');
  const chatVnEl = document.getElementById('vnChatVnText');
  const label = document.getElementById('labelToggleSub');

  const displayVal = isVnTransVisible ? 'block' : 'none';
  if (vnEl) {
    vnEl.style.display = displayVal;
    vnEl.style.opacity = isVnTransVisible ? '1' : '0';
  }
  if (chatVnEl) chatVnEl.style.display = displayVal;
  if (label) label.textContent = isVnTransVisible ? 'Dịch: Bật' : 'Dịch: Tắt';
}

function toggleVnAuto() {
  isVnAutoPlay = !isVnAutoPlay;
  const btn = document.getElementById('btnVnAuto');

  if (isVnAutoPlay) {
    if (btn) {
      btn.innerHTML = '⏸ Tự động (Bật)';
      btn.classList.add('active');
    }
    showToastNotification('▶ Đã bật chế độ Tự động chuyển câu thoại', 'info');
    if (!isTyping) {
      vnAutoTimer = setTimeout(() => {
        advanceVnDialogue();
      }, vnAutoDelay);
    }
  } else {
    if (btn) {
      btn.innerHTML = '▶ Tự động';
      btn.classList.remove('active');
    }
    showToastNotification('⏸ Đã tắt tự động', 'info');
    if (vnAutoTimer) clearTimeout(vnAutoTimer);
  }
}

function toggleVnAudio() {
  isVnAudioEnabled = !isVnAudioEnabled;
  const btn = document.getElementById('btnVnAudio');
  if (btn) {
    btn.innerHTML = isVnAudioEnabled ? '🔊 Âm thanh' : '🔇 Âm thanh';
    btn.classList.toggle('muted', !isVnAudioEnabled);
  }
  showToastNotification(isVnAudioEnabled ? '🔊 Đã bật âm thanh Visual Novel' : '🔇 Đã tắt âm thanh', 'info');
}

function toggleVnFocusMode() {
  isVnFocusMode = !isVnFocusMode;
  const theatre = document.querySelector('.vn-theatre-wrap');
  const btn = document.getElementById('btnVnFullscreen');
  if (theatre) {
    theatre.classList.toggle('focus-fullscreen', isVnFocusMode);
  }
  if (btn) {
    btn.innerHTML = isVnFocusMode ? '⛶ Thoát rạp chiếu' : '⛶ Toàn màn hình';
  }
}

// -------------------------------------------------------------------
// 11. ENHANCED STORY SELECTION MENU (105 NOVELS + SEARCH + JLPT FILTERS)
// -------------------------------------------------------------------
function toggleVnStoryMenu(event) {
  if (event) event.stopPropagation();
  const dropdown = document.getElementById('vnStoryDropdown');
  if (dropdown) dropdown.classList.toggle('open');
}

function handleVnStorySearch(val) {
  vnDropdownSearchQuery = (val || '').toLowerCase().trim();
  renderVnStoryMenuItems();
}

function setVnDropdownJlpt(level, btn) {
  vnDropdownJlptFilter = level;
  const tabs = document.querySelectorAll('.vn-filter-pill-mini');
  tabs.forEach(t => t.classList.remove('active'));
  if (btn) btn.classList.add('active');
  renderVnStoryMenuItems();
}

function renderVnStoryMenu() {
  const dropdown = document.getElementById('vnStoryDropdown');
  if (!dropdown) return;

  dropdown.innerHTML = `
    <div class="vn-dropdown-controls">
      <div class="vn-search-box-wrap">
        <input type="text" id="vnSearchStoryInput" placeholder="🔍 Tìm tác phẩm trong 105 bộ..." oninput="handleVnStorySearch(this.value)">
      </div>
      <div class="vn-filter-pills-row">
        <button class="vn-filter-pill-mini ${vnDropdownJlptFilter === 'all' ? 'active' : ''}" onclick="setVnDropdownJlpt('all', this)">Tất cả (105)</button>
        <button class="vn-filter-pill-mini ${vnDropdownJlptFilter === 'N5' ? 'active' : ''}" onclick="setVnDropdownJlpt('N5', this)">N5 (45)</button>
        <button class="vn-filter-pill-mini ${vnDropdownJlptFilter === 'N4' ? 'active' : ''}" onclick="setVnDropdownJlpt('N4', this)">N4 (32)</button>
        <button class="vn-filter-pill-mini ${vnDropdownJlptFilter === 'N3' ? 'active' : ''}" onclick="setVnDropdownJlpt('N3', this)">N3 (18)</button>
        <button class="vn-filter-pill-mini ${vnDropdownJlptFilter === 'N2' ? 'active' : ''}" onclick="setVnDropdownJlpt('N2', this)">N2 (7)</button>
        <button class="vn-filter-pill-mini ${vnDropdownJlptFilter === 'N1' ? 'active' : ''}" onclick="setVnDropdownJlpt('N1', this)">N1 (3)</button>
      </div>
    </div>
    <div class="vn-dropdown-scroll-list" id="vnStoryScrollList">
      <!-- Populated dynamically via renderVnStoryMenuItems() -->
    </div>
  `;

  renderVnStoryMenuItems();

  const navTitle = document.getElementById('currentStoryNavTitle');
  if (navTitle) {
    const curBook = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]);
    navTitle.textContent = curBook ? (curBook.title.length > 20 ? curBook.title.slice(0, 18) + '...' : curBook.title) : 'Visual Novel';
  }
}

function renderVnStoryMenuItems() {
  const listEl = document.getElementById('vnStoryScrollList');
  if (!listEl) return;

  let html = '';

  // 1. Showcase Classics Section (if no query or query matches)
  const showcases = [
    { key: 'ginga', title: '銀河鉄道の夜', vnTitle: 'Chuyến tàu Ngân Hà', author: '宮沢賢治', level: 'N3', cover: 'assets/covers/cover_anime_forest.jpg' },
    { key: 'merosu', title: '走れメロス', vnTitle: 'Chạy đi Melos', author: '太宰治', level: 'N3', cover: 'assets/covers/cover_anime_castle.jpg' },
    { key: 'gon', title: 'ごんぎつね', vnTitle: 'Cáo nhỏ Gon', author: '新美南吉', level: 'N5', cover: 'assets/cover_gon.svg' },
    { key: 'rashomon', title: '羅生門', vnTitle: 'La Sinh Môn', author: '芥川龍之介', level: 'N2', cover: 'assets/bg_shinkai_rashomon.jpg' },
    { key: 'gakuen', title: '学園アイドルの彼氏役', vnTitle: 'Được Gakuen Idol Nhận Làm Bạn Trai', author: '佐々木', level: 'N4', cover: 'assets/covers/cover_school_romcom.jpg' },
    { key: 'chuumon', title: '注文の多い料理店', vnTitle: 'Quán Ăn Nhiều Yêu Cầu', author: '宮沢賢治', level: 'N4', cover: 'assets/covers/cover_isekai_gourmet.jpg' }
  ];

  const filteredShowcases = showcases.filter(s => {
    if (vnDropdownJlptFilter !== 'all' && s.level !== vnDropdownJlptFilter) return false;
    if (!vnDropdownSearchQuery) return true;
    return s.title.toLowerCase().includes(vnDropdownSearchQuery) || 
           s.vnTitle.toLowerCase().includes(vnDropdownSearchQuery) || 
           s.author.toLowerCase().includes(vnDropdownSearchQuery);
  });

  if (filteredShowcases.length > 0) {
    html += '<div class="vn-menu-section-tag">🌟 Tuyển Tập Phân Cảnh Kinh Điển (Master Showcase)</div>';
    filteredShowcases.forEach(s => {
      const isCurrent = s.key === currentVnBookKey;
      html += `
        <div class="vn-story-item-row ${isCurrent ? 'active' : ''}" onclick="selectVnStory('${s.key}', 0)">
          <img src="${s.cover}" class="story-item-thumb">
          <div class="story-item-body">
            <div class="story-item-title">${escapeHtml(s.vnTitle)}</div>
            <div class="story-item-meta">${escapeHtml(s.title)} • ${escapeHtml(s.author)}</div>
          </div>
          <span class="story-item-badge badge-${s.level.toLowerCase()}">${s.level}</span>
        </div>
      `;
    });
  }

  // 2. All 105 Library Novels
  if (typeof booksData !== 'undefined') {
    const allNovels = Object.values(booksData);
    const filteredNovels = allNovels.filter(n => {
      if (vnDropdownJlptFilter !== 'all' && n.level !== vnDropdownJlptFilter) return false;
      if (!vnDropdownSearchQuery) return true;
      return (n.title && n.title.toLowerCase().includes(vnDropdownSearchQuery)) ||
             (n.romaji && n.romaji.toLowerCase().includes(vnDropdownSearchQuery)) ||
             (n.author && n.author.toLowerCase().includes(vnDropdownSearchQuery)) ||
             (n.genre && n.genre.toLowerCase().includes(vnDropdownSearchQuery));
    });

    if (filteredNovels.length > 0) {
      html += `<div class="vn-menu-section-tag" style="margin-top: 14px;">📚 Thư Viện 105 Bộ Tiểu Thuyết (${filteredNovels.length} bộ)</div>`;
      filteredNovels.forEach(n => {
        const isCurrent = n.id === currentVnBookKey;
        html += `
          <div class="vn-story-item-row ${isCurrent ? 'active' : ''}" onclick="selectVnStory('${n.id}', 0)">
            <img src="${n.cover || 'assets/cover_gon.svg'}" class="story-item-thumb">
            <div class="story-item-body">
              <div class="story-item-title">${escapeHtml(n.title)}</div>
              <div class="story-item-meta">${escapeHtml(n.romaji || n.author)} • ${escapeHtml(n.genre || 'Light Novel')}</div>
            </div>
            <span class="story-item-badge badge-${(n.level || 'N3').toLowerCase()}">${n.level || 'N3'}</span>
          </div>
        `;
      });
    } else {
      html += '<div style="padding: 20px; text-align: center; color: #94A3B8; font-size: 13px;">Không tìm thấy tác phẩm phù hợp với từ khóa.</div>';
    }
  }

  listEl.innerHTML = html;
}

function selectVnStory(bookKey, sceneIdx = 0) {
  currentVnBookKey = bookKey;
  currentInspectedId = bookKey;
  vnDialogues = getOrCreateVnStory(bookKey);
  vnIndex = sceneIdx || 0;

  const dropdown = document.getElementById('vnStoryDropdown');
  if (dropdown) dropdown.classList.remove('open');

  const navTitle = document.getElementById('currentStoryNavTitle');
  if (navTitle) {
    const curBook = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]);
    navTitle.textContent = curBook ? (curBook.title.length > 20 ? curBook.title.slice(0, 18) + '...' : curBook.title) : 'Visual Novel';
  }

  const novelId = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]?.dbId) || 1;
  loadVnCharacters(novelId);

  if (currentVnMode === 'chat') {
    switchVnMode('chat');
  } else {
    updateVnStage();
  }
}

// Global click to close story dropdown & word popup
document.addEventListener('click', (e) => {
  const dropdown = document.getElementById('vnStoryDropdown');
  if (dropdown && dropdown.classList.contains('open') && !e.target.closest('.vn-story-dropdown-wrap')) {
    dropdown.classList.remove('open');
  }

  const popup = document.getElementById('vnWordPopup');
  if (popup && popup.classList.contains('visible') && !e.target.closest('#vnWordPopup') && !e.target.closest('.vn-word-tok')) {
    closeVnWordPopup();
  }
});

// -------------------------------------------------------------------
// 12. AI CHARACTER CHAT MODE 2.0 (DISTINCT ROSTER FOR ALL NOVELS)
// -------------------------------------------------------------------
const novelCharacterRoster = {
  // 1. Classic Masterpieces
  ginga: [
    { characterId: 'giovanni', name: 'Giovanni', japaneseName: 'ジョバンニ', avatarUrl: 'assets/char_giovanni.png', personality: 'Cậu bé giàu cảm xúc, trân trọng tình bạn và chuyến tàu Ngân Hà kỳ bí.', suggestedPrompts: ['カンパネルラと一緒にどこへ行くの？', '銀河の川原の光はどう見えますか？', '本当の幸いって何だと思いますか？'] },
    { characterId: 'campanella', name: 'Campanella', japaneseName: 'カムパネルラ', avatarUrl: 'assets/char_campanella.png', personality: 'Người bạn đồng hành tri kỷ, điềm đạm, ấm áp và thông thái.', suggestedPrompts: ['午後の授業の黒板の図を覚えていますか？', '白鳥の停車場について教えて！'] },
    { characterId: 'conductor', name: 'Conductor', japaneseName: '車掌', avatarUrl: 'assets/char_conductor.png', personality: 'Người soát vé tàu vũ trụ bí ẩn, nghiêm trang và trầm mặc.', suggestedPrompts: ['銀河鉄道の切符を見せてください！', '次の停車駅はどこですか？'] }
  ],
  merosu: [
    { characterId: 'melos', name: 'Melos', japaneseName: 'メロス', avatarUrl: 'assets/chars/char_melos.png', personality: 'Người chăn cừu dũng cảm, trọng danh dự và trung thành tuyệt đối với lời hứa.', suggestedPrompts: ['なぜ命を賭けて走るのですか？', 'セリヌンティウスとの絆を教えて！', 'シラクスの暴君に言いたいことは？'] },
    { characterId: 'dionys', name: 'Dionys', japaneseName: 'ディオニス王', avatarUrl: 'assets/chars/char_dionys.png', personality: 'Bạo chúa đa nghi, cô độc và lạnh lùng nhưng bị lay chuyển bởi đức tin.', suggestedPrompts: ['なぜ人を信じることができないのですか？', '王様の孤独について聞かせてください'] },
    { characterId: 'selinuntius', name: 'Selinuntius', japaneseName: 'セリヌンティウス', avatarUrl: 'assets/chars/char_selinuntius.png', personality: 'Người bạn thợ đá trung kiên, sẵn sàng hy sinh thân mình chờ đợi bạn.', suggestedPrompts: ['メロスが戻ってくると信じていましたか？', '石工の仕事について教えて！'] }
  ],
  gon: [
    { characterId: 'gon', name: 'Gon', japaneseName: 'ごん', avatarUrl: 'assets/chars/char_gon.png', personality: 'Chú cáo nhỏ tinh nghịch nhưng giàu trắc ẩn, luôn muốn chuộc lại lỗi lầm.', suggestedPrompts: ['兵十の家に毎日何を届けているの？', 'いたずらをして後悔したことはある？'] },
    { characterId: 'hyoju', name: 'Hyoju', japaneseName: '兵十', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Người nông dân mồ côi mẹ, chất phác và sống đơn độc giữa núi rừng.', suggestedPrompts: ['うなぎを盗まれた時の気持ちは？', '栗や松茸を届けてくれるのは誰だと思う？'] }
  ],
  rashomon: [
    { characterId: 'genin', name: 'Genin (Hạ nhân)', japaneseName: '下人', avatarUrl: 'assets/chars/char_genin.png', personality: 'Kẻ lưu lạc đứng trước ranh giới mong manh giữa đạo đức và sự sống còn.', suggestedPrompts: ['羅生門の雨宿りで何を考えていますか？', '生き延びるために盗人になる覚悟はありますか？'] },
    { characterId: 'oldwoman', name: 'Bà lão nhổ tóc', japaneseName: '老婆', avatarUrl: 'assets/chars/char_oldwoman.png', personality: 'Bà lão gầy còm nhổ tóc người chết làm tóc giả để sinh tồn giữa thời loạn.', suggestedPrompts: ['なぜ死人の髪の毛を抜くのですか？', '悪事と生きることの違いは何ですか？'] }
  ],
  gakuen: [
    { characterId: 'mob_kun', name: 'Mob-kun', japaneseName: 'モブ男', avatarUrl: 'assets/chars/char_mob.png', personality: 'Nam sinh bình thường bất ngờ bước vào tình huống dở khóc dở cười.', suggestedPrompts: ['学園アイドルと付き合うプレッシャーは？', '放課後のデートはどこに行くの？'] },
    { characterId: 'runa', name: 'Runa-chan', japaneseName: 'ルナ', avatarUrl: 'assets/chars/char_runa.png', personality: 'Thần tượng trường học xinh đẹp, rạng rỡ và luôn tràn đầy năng lượng.', suggestedPrompts: ['どうして私を彼氏役に選んだの？', 'アイドルの秘密を教えて！'] }
  ],
  chuumon: [
    { characterId: 'hunter', name: 'Thợ săn trẻ', japaneseName: '若い紳士', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Nhà quý tộc trẻ lạc bước vào rừng sâu trong một ngày tuyết trắng.', suggestedPrompts: ['山猫軒の注文の多さに怪しまなかったの？', '扉の奥に何があると思いますか？'] },
    { characterId: 'restaurant_master', name: 'Sơn Miêu Chủ Quán', japaneseName: '山猫軒の主', avatarUrl: 'assets/char_conductor.png', personality: 'Chủ nhân kỳ bí của nhà hàng Tây dương ẩn hiện giữa rừng hoang.', suggestedPrompts: ['お客様に塩と酢を塗らせる理由は何ですか？', 'どなたでも歓迎する本当の意味は？'] }
  ],

  // 2. Folk Legends (N5 - N4)
  momotaro: [
    { characterId: 'momotaro_hero', name: 'Momotaro', japaneseName: '桃太郎', avatarUrl: 'assets/chars/char_melos.png', personality: 'Cậu bé sinh ra từ quả đào, dũng cảm và chính trực.', suggestedPrompts: ['鬼ヶ島へ行く決意を教えて！', 'きび団子の味はどうですか？', '犬や猿とのチームワークの秘訣は？'] },
    { characterId: 'dog_companion', name: 'Inu (Chú chó trung thành)', japaneseName: '忠犬シロ', avatarUrl: 'assets/chars/char_gon.png', personality: 'Bạn đồng hành bốn chân trung dũng, sẵn sàng tiên phong diệt quỷ.', suggestedPrompts: ['きび団子ひとつで命を預けたの？', '鬼ヶ島で一番怖かったことは何？'] },
    { characterId: 'grandma', name: 'Bà lão nhân hậu', japaneseName: 'おばあさん', avatarUrl: 'assets/chars/char_oldwoman.png', personality: 'Người mẹ nuôi hiền từ đã vớt quả đào trên dòng suối mùa xuân.', suggestedPrompts: ['川から流れてきた桃を見た時の気持ちは？', '旅立つ桃太郎にどんな言葉をかけましたか？'] }
  ],
  urashima: [
    { characterId: 'urashima_taro', name: 'Urashima Taro', japaneseName: '浦島太郎', avatarUrl: 'assets/chars/char_mob.png', personality: 'Chàng đánh cá lương thiện cứu rùa biển và đặt chân tới Thủy Cung.', suggestedPrompts: ['竜宮城の毎日はどんな様子でしたか？', '玉手箱を開けた時の気持ちを教えて！'] },
    { characterId: 'otohime', name: 'Công chúa Otohime', japaneseName: '乙姫', avatarUrl: 'assets/chars/char_runa.png', personality: 'Nàng công chúa Thủy Cung kiều diễm, đón tiếp Taro ân cần.', suggestedPrompts: ['なぜ玉手箱を開けてはならなかったのですか？', '海の底の時間の流れについて教えて！'] }
  ],
  kaguyahime: [
    { characterId: 'kaguya_hime', name: 'Công chúa Kaguya', japaneseName: 'かぐや姫', avatarUrl: 'assets/chars/char_runa.png', personality: 'Nàng tiên sinh ra từ ống tre phát sáng, mang vẻ đẹp huyền ảo ánh trăng.', suggestedPrompts: ['月の都へ帰らなければならないのですか？', '求婚者たちに出した難題の意味は何ですか？'] },
    { characterId: 'taketori', name: 'Ông lão đốn tre', japaneseName: '竹取の翁', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Người cha nuôi hết mực thương yêu và bảo vệ Kaguya.', suggestedPrompts: ['竹の中で光る小さな姫を見つけた瞬間は？', '姫とのお別れの悲しさをどう乗り越えましたか？'] }
  ],
  tsuru: [
    { characterId: 'otsuru', name: 'Nàng Hạc Otsuru', japaneseName: 'おつる', avatarUrl: 'assets/chars/char_runa.png', personality: 'Nàng tiên biến từ chim hạc được cứu mạng, dệt lụa tơ vàng đáp ơn.', suggestedPrompts: ['なぜ機織りの部屋を見てはいけなかったの？', '恩返しのために命の羽を抜く痛みは？'] },
    { characterId: 'yohei', name: 'Yohei (Chàng tiều phu)', japaneseName: '与平', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Chàng thanh niên nghèo khó, ân hận vì trót hé nhìn bí mật của vợ.', suggestedPrompts: ['雪の中で罠にかかった鶴を助けた時のことは？', '飛び去る鶴の姿を見て何を思いましたか？'] }
  ],
  botchan: [
    { characterId: 'botchan_lead', name: 'Botchan', japaneseName: '坊っちゃん', avatarUrl: 'assets/chars/char_mob.png', personality: 'Chàng giáo viên trẻ thẳng tính, ghét sự giả tạo và dối trá.', suggestedPrompts: ['四国の学校での生徒たちのいたずらはどう？', '清（きよ）からの手紙を読んでどう思いましたか？'] },
    { characterId: 'yamaarashi', name: 'Yamaarashi (Nhím biển)', japaneseName: '山嵐', avatarUrl: 'assets/chars/char_melos.png', personality: 'Thầy giáo dạy toán nghĩa khí, đồng minh đáng tin cậy của Botchan.', suggestedPrompts: ['赤シャツの策略にどう立ち向かいますか？', '正義を貫く勇気について聞かせて！'] }
  ],

  // 3. Modern Light Novels (Isekai / Fantasy / Action)
  mushoku: [
    { characterId: 'rudeus', name: 'Rudeus Greyrat', japaneseName: 'ルーデウス', avatarUrl: 'assets/chars/char_melos.png', personality: 'Chàng pháp sư tài năng, luôn nỗ lực sống hết mình ở thế giới mới.', suggestedPrompts: ['無詠唱魔術のコツを教えて！', 'ロキシー師匠はどこにいるの？', 'この世界での暮らしはどう？'] },
    { characterId: 'roxy', name: 'Roxy Migurdia', japaneseName: 'ロキシー', avatarUrl: 'assets/chars/char_runa.png', personality: 'Thầy dạy ma thuật tộc Migurd điềm tĩnh, dễ thương và uyên bác.', suggestedPrompts: ['水聖級魔術を教えてください！', '旅の途中で何を見ましたか？'] },
    { characterId: 'eris', name: 'Eris Boreas Greyrat', japaneseName: 'エリス', avatarUrl: 'assets/chars/char_runa.png', personality: 'Đại tiểu thư quý tộc tóc đỏ hiếu động, mạnh mẽ và thẳng thắn.', suggestedPrompts: ['一緒に剣の特訓をしよう！', 'ボレアス家の掟って何？'] }
  ],
  rezero: [
    { characterId: 'subaru', name: 'Natsuki Subaru', japaneseName: 'ナツキ・スバル', avatarUrl: 'assets/chars/char_mob.png', personality: 'Chàng trai nhiệt huyết, kiên cường không chịu đầu hàng số phận.', suggestedPrompts: ['エミリアたんを守る決意を聞かせて！', '死に戻りの辛さを誰かに話したことはある？'] },
    { characterId: 'emilia', name: 'Emilia', japaneseName: 'エミリア', avatarUrl: 'assets/char_campanella.png', personality: 'Nửa yêu tinh tóc bạc lương thiện, dịu dàng và ấm áp.', suggestedPrompts: ['パックは今どこにいるの？', 'ルグニカ王選で叶えたい夢は何？'] },
    { characterId: 'rem', name: 'Rem', japaneseName: 'レム', avatarUrl: 'assets/chars/char_runa.png', personality: 'Hầu gái tộc quỷ tận tụy, hết lòng vì người mình yêu thương.', suggestedPrompts: ['スバル君の好きなところを教えて！', 'ロズワール邸の朝食は何を作るの？'] }
  ],
  slime: [
    { characterId: 'rimuru', name: 'Rimuru Tempest', japaneseName: 'リムル＝テンペスト', avatarUrl: 'assets/chars/char_mob.png', personality: 'Lãnh tụ vĩ đại của liên bang Jura Tempest, dễ thương và thấu hiểu.', suggestedPrompts: ['テンペストの町を案内して！', '大賢者さん、今の状態を教えて！'] },
    { characterId: 'shizue', name: 'Shizue Izawa', japaneseName: 'シズ', avatarUrl: 'assets/chars/char_runa.png', personality: 'Chiến binh định mệnh dịu dàng, người truyền cảm hứng cho Rimuru.', suggestedPrompts: ['日本の故郷の思い出を教えて', '子どもたちへの想いを聞かせて'] },
    { characterId: 'veldora', name: 'Veldora', japaneseName: 'ヴェルドラ', avatarUrl: 'assets/chars/char_dionys.png', personality: 'Bạo phong long kiêu hãnh, tri kỷ cùng Rimuru kết nghĩa anh em.', suggestedPrompts: ['洞窟での孤独はどうだった？', 'リムルの胃袋の中の居心地は？'] }
  ],
  honzuki: [
    { characterId: 'myne', name: 'Myne (Urano)', japaneseName: 'マイン', avatarUrl: 'assets/chars/char_runa.png', personality: 'Cô bé đam mê sách bất tận, thông minh và kiên trì chế tạo sách.', suggestedPrompts: ['本のない世界でどうやって本を作るの？', 'ルッツと一緒に作った和紙を見せて！'] },
    { characterId: 'ferdinand', name: 'Ferdinand', japaneseName: 'フェルディナンド', avatarUrl: 'assets/char_conductor.png', personality: 'Thần quan trưởng nghiêm nghị, uyên bác và bảo bọc Myne chu đáo.', suggestedPrompts: ['神殿の図書館の利用許可をください！', '魔力制御の訓練を教えてください'] }
  ],
  kokoro: [
    { characterId: 'sensei', name: 'Sensei (Tiên sinh)', japaneseName: '先生', avatarUrl: 'assets/chars/char_genin.png', personality: 'Người trí thức cô độc, trầm ngâm với những bí mật dằn vặt khôn nguôi.', suggestedPrompts: ['なぜ雑司ヶ谷の墓参りを続けるのですか？', '人間の心とは何だと思いますか？'] },
    { characterId: 'watashi', name: 'Watashi (Tôi)', japaneseName: '私', avatarUrl: 'assets/chars/char_mob.png', personality: 'Chàng sinh viên trẻ ngưỡng mộ và tha thiết thấu hiểu tâm can Tiên sinh.', suggestedPrompts: ['鎌倉の浜辺での出会いを覚えていますか？', '先生からの手紙を読んでどう感じましたか？'] },
    { characterId: 'shizu', name: 'Shizu (Phu nhân)', japaneseName: '静', avatarUrl: 'assets/chars/char_runa.png', personality: 'Người vợ dịu dàng, luôn âm thầm lo lắng cho sự u sầu của Tiên sinh.', suggestedPrompts: ['先生の昔の様子を教えてください', 'ふたりの暮らしで大切にしていることは？'] }
  ],
  suho: [
    { characterId: 'suho_hero', name: 'Suho', japaneseName: 'スーホ', avatarUrl: 'assets/chars/char_melos.png', personality: 'Cậu bé du mục Mông Cổ nghèo khó nhưng dũng cảm, hết lòng yêu thương chú ngựa trắng.', suggestedPrompts: ['白い馬との出会いを教えて！', '馬頭琴（モリンホール）の音色はどうですか？', '領主の理不尽な命令にどう立ち向かいましたか？'] },
    { characterId: 'white_horse', name: 'Chú ngựa trắng', japaneseName: '白馬', avatarUrl: 'assets/chars/char_gon.png', personality: 'Chú ngựa dũng mãnh, trung thành quên mình vì chủ nhân.', suggestedPrompts: ['スーホのもとへ駆けてきた時の気持ちは？', '大草原を走る喜びについて教えて！'] },
    { characterId: 'ryoushu', name: 'Lãnh chúa', japaneseName: '領主', avatarUrl: 'assets/chars/char_dionys.png', personality: 'Tên bạo chúa tham lam, hống hách cướp ngựa trắng của Suho.', suggestedPrompts: ['なぜ力づくで馬を奪ったのですか？', '貧しい人々への慈悲はないのですか？'] }
  ],
  suho_uma: [
    { characterId: 'suho_hero', name: 'Suho', japaneseName: 'スーホ', avatarUrl: 'assets/chars/char_melos.png', personality: 'Cậu bé du mục Mông Cổ nghèo khó nhưng dũng cảm, hết lòng yêu thương chú ngựa trắng.', suggestedPrompts: ['白い馬との出会いを教えて！', '馬頭琴（モリンホール）の音色はどうですか？', '領主の理不尽な命令にどう立ち向かいましたか？'] },
    { characterId: 'white_horse', name: 'Chú ngựa trắng', japaneseName: '白馬', avatarUrl: 'assets/chars/char_gon.png', personality: 'Chú ngựa dũng mãnh, trung thành quên mình vì chủ nhân.', suggestedPrompts: ['スーホのもとへ駆けてきた時の気持ちは？', '大草原を走る喜びについて教えて！'] },
    { characterId: 'ryoushu', name: 'Lãnh chúa', japaneseName: '領主', avatarUrl: 'assets/chars/char_dionys.png', personality: 'Tên bạo chúa tham lam, hống hách cướp ngựa trắng của Suho.', suggestedPrompts: ['なぜ力づくで馬を奪ったのですか？', '貧しい人々への慈悲はないのですか？'] }
  ],
  tate_yuusha: [
    { characterId: 'naofumi', name: 'Iwatani Naofumi', japaneseName: '岩谷尚文', avatarUrl: 'assets/chars/char_genin.png', personality: 'Khiên hiệp sĩ kiên định, thực tế và luôn bảo vệ đồng đội.', suggestedPrompts: ['盾の勇者としての苦悩を聞かせて！', 'ラフタリアとの出会いは？'] },
    { characterId: 'raphtalia', name: 'Raphtalia', japaneseName: 'ラフタリア', avatarUrl: 'assets/chars/char_runa.png', personality: 'Cô bé á nhân tộc gấu mèo trung thành, thanh kiếm bảo vệ Naofumi.', suggestedPrompts: ['尚文様の力になれていますか？', '故郷への思いを教えて！'] }
  ],
  spider: [
    { characterId: 'kumoko', name: 'Kumoko (Bạch Chức)', japaneseName: '白織（蜘蛛子）', avatarUrl: 'assets/chars/char_runa.png', personality: 'Nữ sinh chuyển sinh thành nhện, hài hước, kiên cường sinh tồn trong mê cung.', suggestedPrompts: ['エルロー大迷宮のサバイバル術は？', '鑑定スキルの感想を教えて！'] },
    { characterId: 'ariel', name: 'Ma vương Ariel', japaneseName: 'アリエル', avatarUrl: 'assets/chars/char_dionys.png', personality: 'Ma vương cổ đại mạnh mẽ, bí ẩn và giàu tình cảm.', suggestedPrompts: ['世界の崩壊を防ぐ方法は？', '蜘蛛子との関係はどうですか？'] }
  ],
  kage_jitsuryoku: [
    { characterId: 'shadow_cid', name: 'Cid Kagenou (Shadow)', japaneseName: 'シャドウ', avatarUrl: 'assets/chars/char_genin.png', personality: 'Chúa tể bóng tối bí ẩn, thích diễn xuất sau cánh gà.', suggestedPrompts: ['アイ・アム・アトミックの極意は？', 'シャドウガーデンの目的は何ですか？'] },
    { characterId: 'alpha', name: 'Alpha', japaneseName: 'アルファ', avatarUrl: 'assets/chars/char_runa.png', personality: 'Người đứng đầu Thất Âm, tuyệt đối sùng bái và tuân lệnh Shadow.', suggestedPrompts: ['シャドウ様の叡智について教えて！', '教団の企みはどうなっていますか？'] }
  ],
  kachikachi: [
    { characterId: 'usagi', name: 'Thỏ trắng nghĩa hiệp', japaneseName: 'ウサギ', avatarUrl: 'assets/chars/char_melos.png', personality: 'Chú thỏ thông minh, mưu trí trừng trị kẻ gian ác để báo thù cho bà lão.', suggestedPrompts: ['カチカチ鳥の正体は何ですか？', 'タヌキへの仕返しはどう思いついたの？'] },
    { characterId: 'tanuki', name: 'Tanuki (Con lửng gian xảo)', japaneseName: 'タヌキ', avatarUrl: 'assets/chars/char_gon.png', personality: 'Con lửng tinh quái nhưng ngốc nghếch, bị lừa bởi các mưu kế của thỏ.', suggestedPrompts: ['泥の舟に乗った時の気持ちは？', 'おじいさんにいたずらした理由は？'] }
  ],
  issun: [
    { characterId: 'issun_boushi', name: 'Issun Boushi (Cậu bé Tí hon)', japaneseName: '一寸法師', avatarUrl: 'assets/chars/char_melos.png', personality: 'Cậu bé chỉ cao một đốt ngón tay, dùng kim làm kiếm, chèo bát vượt sóng đến kinh đô.', suggestedPrompts: ['お椀の舟での川下りは怖くなかった？', '鬼の腹の中でどう戦ったのですか？'] },
    { characterId: 'haruhime', name: 'Công chúa Haruhime', japaneseName: '春姫', avatarUrl: 'assets/chars/char_runa.png', personality: 'Nàng công chúa kiều diễm được Issun Boushi xả thân bảo vệ khỏi quỷ dữ.', suggestedPrompts: ['打ち出の小槌を振った時の願いは何？', '一寸法師の勇気について教えて！'] }
  ],
  yukionna: [
    { characterId: 'oyuki', name: 'Yuki Onna (Oyuki)', japaneseName: '雪女（お雪）', avatarUrl: 'assets/chars/char_runa.png', personality: 'Nàng tiên tuyết xinh đẹp ma mị, vì tình yêu mà kết duyên cùng chàng tiều phu.', suggestedPrompts: ['なぜあの吹雪の夜、巳之吉を助けたのですか？', '約束を破られた時、どんな気持ちでしたか？'] },
    { characterId: 'minokichi', name: 'Minokichi (Chàng tiều phu)', japaneseName: '巳之吉', avatarUrl: 'assets/chars/char_mob.png', personality: 'Chàng tiều phu chất phác, ôm nỗi ân hận vì vô tình thốt ra bí mật tuyết trắng.', suggestedPrompts: ['お雪の正体に気づいた瞬間の気持ちは？', '子どもたちと一緒に暮らした日々は？'] }
  ],
  tebukuro: [
    { characterId: 'kogitsune', name: 'Cáo con', japaneseName: '子ぎつね', avatarUrl: 'assets/chars/char_gon.png', personality: 'Chú cáo con ngây thơ lần đầu đi vào làng loài người trong ngày tuyết trắng để mua găng tay.', suggestedPrompts: ['雪の森での暮らしはどう？', '人間の町は怖くなかった？', '手袋の温かさを教えて！'] },
    { characterId: 'haha_gitsune', name: 'Cáo mẹ', japaneseName: '母ぎつね', avatarUrl: 'assets/chars/char_runa.png', personality: 'Cáo mẹ hiền từ, hóa phép một bàn tay con thành tay người để đi mua găng.', suggestedPrompts: ['どうして人間の手を化かしてあげたの？', '子ぎつねへの想いを聞かせて！'] },
    { characterId: 'boushiya', name: 'Bác thợ may mũ nón', japaneseName: '帽子屋の主人', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Người chủ tiệm nhân hậu, sưởi ấm tâm hồn chú cáo nhỏ giữa mùa đông lạnh giá.', suggestedPrompts: ['狐の手だと気づいた時の気持ちは？', '冬の町のお客さんについて教えて！'] }
  ],
  hanasaka: [
    { characterId: 'hanasaka_jii', name: 'Ông lão nhân hậu', japaneseName: '正直なおじいさん', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Ông lão chất phác, hết lòng yêu thương chú chó Shiro và được trao tặng phép màu hoa anh đào.', suggestedPrompts: ['シロとの毎日はどんな様子でしたか？', '枯れ木に花を咲かせた瞬間の気持ちは？'] },
    { characterId: 'shiro_inu', name: 'Chú chó Shiro', japaneseName: '白犬シロ', avatarUrl: 'assets/chars/char_gon.png', personality: 'Chú chó trung thành thông minh, luôn báo tin vui cho ông bà lão.', suggestedPrompts: ['「ここ掘れワンワン」と鳴いた理由は？', 'おじいさんへの恩返しについて教えて！'] },
    { characterId: 'tonosama', name: 'Lãnh chúa', japaneseName: '殿様', avatarUrl: 'assets/chars/char_dionys.png', personality: 'Vị quan liêm chính yêu chuộng cái đẹp của hoa xuân.', suggestedPrompts: ['満開の桜を見た感動を聞かせて！'] }
  ],
  kasajizou: [
    { characterId: 'kasa_jii', name: 'Ông lão đan nón', japaneseName: '笠売りのおじいさん', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Ông lão nghèo khó nhưng có tấm lòng bồ tát bao la giữa trời bão tuyết.', suggestedPrompts: ['なぜ売り物の笠をお地蔵様にかぶせたのですか？', '雪の日の帰り道は寒くありませんでしたか？'] },
    { characterId: 'kasa_baa', name: 'Bà lão hiền từ', japaneseName: 'おばあさん', avatarUrl: 'assets/chars/char_oldwoman.png', personality: 'Người vợ nhân hậu cùng chung tấm lòng thiện lương với ông lão.', suggestedPrompts: ['笠が売れずに帰ってきたおじいさんに何を言いましたか？'] }
  ],
  bunbuku: [
    { characterId: 'chagama_tanuki', name: 'Tanuki ấm trà Bunbuku', japaneseName: '分福茶釜（タヌキ）', avatarUrl: 'assets/chars/char_gon.png', personality: 'Chú lửng biến thành ấm trà biết đi dây xiếc để báo ân cho chàng trai tốt bụng.', suggestedPrompts: ['綱渡りの芸はどうやって覚えたの？', '屑屋の男との友情を教えて！'] },
    { characterId: 'kuzuya_man', name: 'Bác ve chai', japaneseName: '屑屋の男', avatarUrl: 'assets/chars/char_hyoju.png', personality: 'Chàng trai thu mua đồ cũ lương thiện, chăm sóc và yêu quý chú lửng ấm trà.', suggestedPrompts: ['茶釜から手足が生えた時の驚きは？'] }
  ],
  sarukani: [
    { characterId: 'kani_oya', name: 'Cua mẹ', japaneseName: 'カニ', avatarUrl: 'assets/chars/char_runa.png', personality: 'Cua mẹ hiền lành, gieo hạt hồng để trái ngọt cho đàn con.', suggestedPrompts: ['おにぎりと柿の種を交換した時の気持ちは？'] },
    { characterId: 'kani_nakama', name: 'Hội bạn nghĩa hiệp', japaneseName: '栗・蜂・臼', avatarUrl: 'assets/chars/char_gon.png', personality: 'Những người bạn dũng cảm liên minh cùng trừng phạt kẻ xấu.', suggestedPrompts: ['猿を懲らしめる作戦はどう立てたの？'] }
  ],
  kusuriya: [
    { characterId: 'maomao', name: 'Maomao (Miêu Miêu)', japaneseName: '猫猫', avatarUrl: 'assets/chars/char_runa.png', personality: 'Dược sư kỳ tài, mê độc dược và thảo mộc, sắc sảo phá giải các bí ẩn chốn hậu cung.', suggestedPrompts: ['後宮での毒味役の毎日は？', '一番好きな薬草は何ですか？', '壬氏様をどう思っていますか？'] },
    { characterId: 'jinshi', name: 'Jinshi (Nhâm Thị)', japaneseName: '壬氏', avatarUrl: 'assets/char_campanella.png', personality: 'Đại thần tài mạo song toàn, vẻ đẹp hoa nhường nguyệt thẹn và luôn quan tâm đến Maomao.', suggestedPrompts: ['猫猫の薬草への執着をどう見ていますか？', '後宮の秘密について教えてください'] }
  ],
  sao: [
    { characterId: 'kirito', name: 'Kirito (Hắc kiếm sĩ)', japaneseName: 'キリト', avatarUrl: 'assets/chars/char_genin.png', personality: 'Kiếm sĩ solo kiên cường, gánh vác sứ mệnh giải cứu người chơi khỏi Aincrad.', suggestedPrompts: ['二刀流スキルに目覚めた瞬間は？', 'アインクラッド75層の戦いについて教えて！', 'アスナとの絆はどうやって生まれたの？'] },
    { characterId: 'asuna', name: 'Asuna (Tia chớp)', japaneseName: 'アスナ', avatarUrl: 'assets/chars/char_runa.png', personality: 'Phó đoàn trưởng Huyết Kỵ Sĩ đoàn, kiếm kỹ thần tốc và nấu ăn tuyệt đỉnh.', suggestedPrompts: ['キリト君と一緒に作った料理は？', '血盟騎士団での毎日はどう？'] }
  ],
  overlord: [
    { characterId: 'ainz', name: 'Ainz Ooal Gown', japaneseName: 'アインズ・ウール・ゴウン', avatarUrl: 'assets/chars/char_dionys.png', personality: 'Chúa tể tối cao của Lăng mộ Nazarick, ma đạo vương uy nghiêm nhưng luôn thận trọng.', suggestedPrompts: ['ナザリック地下大墳墓の統治はどうですか？', '守護者たちへの本音を聞かせてください'] },
    { characterId: 'albedo', name: 'Albedo', japaneseName: 'アルベド', avatarUrl: 'assets/chars/char_runa.png', personality: 'Tổng quản các thủ hộ giả, tuyệt đối trung thành và si mê Ainz vô bờ bến.', suggestedPrompts: ['アインズ様への想いを語ってください！', 'ナザリックの防衛計画はどうなっていますか？'] }
  ],
  konosuba: [
    { characterId: 'kazuma', name: 'Satou Kazuma', japaneseName: 'サトウ・カズマ', avatarUrl: 'assets/chars/char_mob.png', personality: 'Mạo hiểm giả thực tế, đầy bất mãn với tổ đội dị biệt nhưng luôn gánh team.', suggestedPrompts: ['駄女神アクアへの苦情を聞かせて！', 'スティールスキルの使い道は？'] },
    { characterId: 'aqua', name: 'Nữ thần Aqua', japaneseName: 'アクア', avatarUrl: 'assets/chars/char_runa.png', personality: 'Nữ thần nước ngây ngô, kiêu ngạo nhưng mít ướt và luôn gây rắc rối dở khóc dở cười.', suggestedPrompts: ['花鳥風月の芸を見せて！', 'アンデッド退散の祈りを捧げて！'] }
  ],
  otonari_tenshi: [
    { characterId: 'amane', name: 'Fujimiya Amane', japaneseName: '藤宮周', avatarUrl: 'assets/chars/char_mob.png', personality: 'Nam sinh bình thường, thích sự yên bình và dần bị sự dịu dàng của Mahiru sưởi ấm.', suggestedPrompts: ['真昼の作ってくれるご飯で一番好きなのは？', '雨の日に傘を渡した時の気持ちは？'] },
    { characterId: 'mahiru', name: 'Shiina Mahiru (Thiên sứ)', japaneseName: '椎名真昼', avatarUrl: 'assets/chars/char_runa.png', personality: 'Cô bé hàng xóm hoàn hảo, dịu dàng, đảm đang và vụng về che giấu sự ngại ngùng.', suggestedPrompts: ['周君のお世話をするようになったきっかけは？', '学校と家での違いについて教えて！'] }
  ]
};

function getNovelFallbackCharacters(bookKey) {
  if (novelCharacterRoster[bookKey]) return novelCharacterRoster[bookKey];
  const b = (typeof booksData !== 'undefined' && booksData[bookKey]) ? booksData[bookKey] : null;
  const title = b ? b.title : 'Tiểu thuyết';
  const author = b ? b.author : 'Tác giả';
  const g = (b?.genre || '').toLowerCase();

  // Pick bespoke aesthetic sprites based on genre (NEVER default to Giovanni!)
  let heroSprite = 'assets/chars/char_mob.png';
  let heroineSprite = 'assets/chars/char_runa.png';
  let mentorSprite = 'assets/char_conductor.png';

  if (g.includes('folktale') || g.includes('fairy') || g.includes('children') || g.includes('fable') || g.includes('legend') || g.includes('touching') || g.includes('mythology')) {
    heroSprite = 'assets/chars/char_melos.png';
    heroineSprite = 'assets/chars/char_runa.png';
    mentorSprite = 'assets/chars/char_hyoju.png';
  } else if (g.includes('dark') || g.includes('mystery') || g.includes('psychological') || g.includes('classic') || g.includes('nobel')) {
    heroSprite = 'assets/chars/char_genin.png';
    heroineSprite = 'assets/chars/char_runa.png';
    mentorSprite = 'assets/chars/char_oldwoman.png';
  } else if (g.includes('animal')) {
    heroSprite = 'assets/chars/char_gon.png';
    heroineSprite = 'assets/chars/char_runa.png';
    mentorSprite = 'assets/chars/char_hyoju.png';
  } else if (g.includes('isekai') || g.includes('fantasy') || g.includes('adventure') || g.includes('action')) {
    heroSprite = 'assets/chars/char_melos.png';
    heroineSprite = 'assets/chars/char_runa.png';
    mentorSprite = 'assets/char_conductor.png';
  }

  // Derive clean protagonist name - NEVER use raw English romaji words!
  const cleanTitle = (b?.title || 'Nhân vật').replace(/（.*?）|\(.*?\)/g, '').trim();
  let heroJp = '主人公';
  let heroName = 'Nhân vật chính';
  let companionJp = 'ヒロイン・相棒';
  let companionName = 'Bạn đồng hành';

  if (g.includes('folktale') || g.includes('fairy') || g.includes('children') || g.includes('fable')) {
    heroJp = cleanTitle.length <= 6 ? cleanTitle : '主人公';
    heroName = cleanTitle.length <= 8 ? cleanTitle : 'Nhân vật';
    companionJp = '旅の仲間';
    companionName = 'Bạn đồng hành';
  } else if (g.includes('animal')) {
    heroJp = '主人公（森の仲間）';
    heroName = 'Nhân vật chính';
    companionJp = '森の仲間';
    companionName = 'Bạn đồng hành';
  } else if (g.includes('isekai') || g.includes('fantasy') || g.includes('action')) {
    heroJp = '冒険者';
    heroName = 'Nhà thám hiểm';
    companionJp = 'パーティの仲間';
    companionName = 'Đồng đội';
  } else if (g.includes('romcom') || g.includes('school')) {
    heroJp = '主人公';
    heroName = 'Nam chính';
    companionJp = 'ヒロイン';
    companionName = 'Nữ chính';
  }

  return [
    {
      characterId: `${bookKey}_protagonist`,
      name: heroName,
      japaneseName: heroJp,
      avatarUrl: heroSprite,
      personality: `Nhân vật chính của tác phẩm "${title}". ${b?.synopsis ? b.synopsis.slice(0, 70) + '...' : ''}`,
      suggestedPrompts: [
        `『${cleanTitle}』の物語のきっかけを教えて！`,
        'あなたの世界での一番の目的は何ですか？',
        'これからの旅の展開はどうなりますか？'
      ]
    },
    {
      characterId: `${bookKey}_companion`,
      name: companionName,
      japaneseName: companionJp,
      avatarUrl: heroineSprite,
      personality: `Người bạn đồng hành tin cậy và ấm áp trong thế giới "${title}".`,
      suggestedPrompts: [
        '一緒にこの世界を冒険しよう！',
        'あなたが一番好きな日本語の言葉を教えて！'
      ]
    },
    {
      characterId: `${bookKey}_author`,
      name: `${author} (Tác giả)`,
      japaneseName: author,
      avatarUrl: mentorSprite,
      personality: `Văn hào sáng tác tác phẩm "${title}". Sẵn sàng chia sẻ vẻ đẹp câu chữ tiếng Nhật.`,
      suggestedPrompts: [
        `なぜ『${cleanTitle}』という物語を描いたのですか？`,
        '日本語を学ぶ読者へアドバイスをください！'
      ]
    }
  ];
}

async function loadVnCharacters(novelId) {
  // 1. Immediately initialize with the distinct character roster for this novel
  currentVnCharacters = getNovelFallbackCharacters(currentVnBookKey);

  // 2. Query backend only if novelId exists, and avoid generic Ginga fallback pollution
  try {
    const curBook = typeof booksData !== 'undefined' ? booksData[currentVnBookKey] : null;
    const dbId = curBook?.dbId || (typeof novelId === 'number' && novelId > 1 ? novelId : null);
    if (dbId) {
      const res = await fetch(`${API_BASE}/characters?novelId=${dbId}`);
      if (res.ok) {
        const data = await res.json();
        // Only accept if backend characters actually belong to this novel
        if (data && data.length > 0 && (currentVnBookKey === 'ginga' || !data.some(c => c.characterId === 'giovanni'))) {
          currentVnCharacters = data;
        }
      }
    }
  } catch (err) {
    // Keep local distinct roster
  }

  if (currentVnCharacters.length > 0) {
    currentVnSelectedCharId = currentVnCharacters[0].characterId;
    renderVnCharPills();
    selectVnChatCharacter(currentVnSelectedCharId);
  }
}

function switchVnMode(mode) {
  currentVnMode = mode;
  const btnStory = document.getElementById('btnModeVnStory');
  const btnChat = document.getElementById('btnModeVnChat');
  const storyBox = document.getElementById('vnStoryBox');
  const chatBox = document.getElementById('vnAiChatBox');
  const strip = document.getElementById('vnCharSelectorStrip');

  if (mode === 'chat') {
    if (btnStory) btnStory.classList.remove('active');
    if (btnChat) btnChat.classList.add('active');
    if (storyBox) storyBox.style.display = 'none';
    if (chatBox) chatBox.style.display = 'flex';
    if (strip) strip.style.display = 'flex';

    const novelId = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]?.dbId) || (typeof currentNovelDbId !== 'undefined' ? currentNovelDbId : 1);
    loadVnCharacters(novelId);
  } else {
    if (btnStory) btnStory.classList.add('active');
    if (btnChat) btnChat.classList.remove('active');
    if (storyBox) storyBox.style.display = 'block';
    if (chatBox) chatBox.style.display = 'none';
    if (strip) strip.style.display = 'none';
    updateVnStage();
  }
}

function renderVnCharPills() {
  const container = document.getElementById('vnCharPillsList');
  if (!container) return;

  container.innerHTML = currentVnCharacters.map(char => {
    const isActive = char.characterId === currentVnSelectedCharId ? 'active' : '';
    return `
      <button class="vn-char-pill ${isActive}" onclick="selectVnChatCharacter('${char.characterId}')">
        <img src="${char.avatarUrl || 'assets/char_giovanni.png'}" class="char-pill-avatar">
        <span>${escapeHtml(char.japaneseName || char.name)}</span>
      </button>
    `;
  }).join('');
}

function selectVnChatCharacter(charId) {
  currentVnSelectedCharId = charId;
  renderVnCharPills();

  const char = currentVnCharacters.find(c => c.characterId === charId);
  if (!char) return;

  const avatar = document.getElementById('vnChatAvatar');
  if (avatar) avatar.src = char.avatarUrl || 'assets/char_giovanni.png';

  const nameEl = document.getElementById('vnChatSpeakerName');
  if (nameEl) nameEl.textContent = `${char.japaneseName} (${char.name})`;

  const bioEl = document.getElementById('vnChatSpeakerBio');
  if (bioEl) bioEl.textContent = char.personality || '';

  const startersList = document.getElementById('vnPromptStartersList');
  if (startersList && char.suggestedPrompts) {
    startersList.innerHTML = char.suggestedPrompts.map(prompt => `
      <button class="starter-chip" onclick="sendVnChatMessage('${escapeHtml(prompt).replace(/'/g, "\\'")}')">
        ✦ ${escapeHtml(prompt)}
      </button>
    `).join('');
  }

  // Update Standee to reflect the speaking character in 1-on-1 Chat Mode
  const charLeft = document.getElementById('charLeft');
  const slotLeft = document.getElementById('slotLeft');
  if (charLeft && char.avatarUrl) {
    charLeft.src = char.avatarUrl;
    charLeft.style.display = 'block';
    if (slotLeft) slotLeft.style.display = 'block';
    charLeft.className = 'vn-char-sprite active-speaker';
  }

  // In 1-on-1 Chat Mode, hide other slots to focus on the active conversational partner
  const slotCenter = document.getElementById('slotCenter');
  const slotRight = document.getElementById('slotRight');
  if (slotCenter) slotCenter.style.display = 'none';
  if (slotRight) slotRight.style.display = 'none';
}

function setVnChatMode(mode) {
  currentVnChatMode = mode;
  ['free', 'practice', 'scenario'].forEach(m => {
    const el = document.getElementById(`btnChatMode${m.charAt(0).toUpperCase() + m.slice(1)}`);
    if (el) el.classList.toggle('active', m === mode);
  });
  const modeNames = { free: 'Hội thoại tự do', practice: 'Luyện tập tiếng Nhật', scenario: 'Nhập vai tình huống' };
  showToastNotification(`Đã chuyển sang: ${modeNames[mode]}`, 'info');
}

async function sendVnChatMessage(customText) {
  const inputEl = document.getElementById('vnChatTextInput');
  const userText = (customText || (inputEl ? inputEl.value : '')).trim();
  if (!userText) return;

  if (inputEl) inputEl.value = '';

  const btnSend = document.getElementById('btnSendVnChat');
  if (btnSend) btnSend.disabled = true;

  const emotionPill = document.getElementById('vnChatEmotionPill');
  if (emotionPill) emotionPill.textContent = '🤔 Đang suy nghĩ...';

  const novelId = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]?.dbId) || (typeof currentNovelDbId !== 'undefined' ? currentNovelDbId : 1);

  try {
    const res = await fetch(`${API_BASE}/ai/character-chat`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        characterId: currentVnSelectedCharId,
        novelId: novelId,
        chapterNumber: currentChapterNum || 1,
        userMessage: userText,
        jlptLevel: 'N3',
        conversationMode: currentVnChatMode,
        history: currentVnChatHistory
      })
    });

    if (res.ok) {
      const data = await res.json();
      if (data.success) {
        document.getElementById('vnChatJpText').textContent = data.replyJapanese;
        document.getElementById('vnChatVnText').textContent = data.replyVietnamese;

        const emotions = {
          happy: '✨ Tươi cười',
          gentle: '🌸 Dịu dàng',
          thinking: '🤔 Trầm tư',
          surprised: '😲 Kinh ngạc',
          sad: '💧 Thoáng buồn',
          neutral: '✦ Bình thản'
        };
        if (emotionPill) emotionPill.textContent = emotions[data.expression] || '✨ ' + data.expression;

        const feedbackBox = document.getElementById('vnChatFeedbackContent');
        if (feedbackBox) feedbackBox.textContent = data.grammarFeedback || 'Bạn giao tiếp rất tự nhiên!';

        // Gentle Correction Box
        const corrBox = document.getElementById('vnChatCorrectionBox');
        const corrContent = document.getElementById('vnChatCorrectionContent');
        if (corrBox && corrContent) {
          if (data.correction && data.correction.hasCorrection) {
            corrBox.style.display = 'block';
            corrContent.innerHTML = `<div><strong>Câu gợi ý tự nhiên hơn:</strong> <span style="color:#FBBF24;">${escapeHtml(data.correction.correctedSentence)}</span></div><div style="font-size:11.5px;color:#CBD5E1;margin-top:2px;">${escapeHtml(data.correction.explanation)}</div>`;
          } else {
            corrBox.style.display = 'none';
          }
        }

        // Key Vocab chips
        const vocabContainer = document.getElementById('vnChatVocabChips');
        if (vocabContainer && data.keyVocabulary) {
          vocabContainer.innerHTML = data.keyVocabulary.map(v => `
            <span class="vocab-chip" onclick="addVnChatVocabToSrs('${escapeHtml(v.word)}', '${escapeHtml(v.reading)}', '${escapeHtml(v.meaning)}', '${escapeHtml(v.jlptLevel)}')" title="Bấm để lưu vào Flashcard SRS">
              ➕ ${escapeHtml(v.word)} (${escapeHtml(v.reading)}): ${escapeHtml(v.meaning)}
            </span>
          `).join('');
        }

        // Sliding window history
        currentVnChatHistory.push({ role: 'user', text: userText });
        currentVnChatHistory.push({ role: 'character', text: data.replyJapanese });
        if (currentVnChatHistory.length > 8) currentVnChatHistory = currentVnChatHistory.slice(-8);

        playSuccessChime();
        showToastNotification('✦ +5 XP! Bạn vừa giao tiếp tiếng Nhật với nhân vật!', 'success');
        addLocalXp(5);
      }
    }
  } catch (err) {
    console.error('Chat error', err);
    showToastNotification('Không thể kết nối máy chủ AI nhân vật', 'error');
  } finally {
    if (btnSend) btnSend.disabled = false;
  }
}

function handleVnChatInputKey(event) {
  if (event.key === 'Enter') {
    sendVnChatMessage();
  }
}

function speakCurrentChatReply() {
  const jpText = document.getElementById('vnChatJpText');
  if (!jpText || !jpText.textContent) return;
  const clean = jpText.textContent.replace(/[「」]/g, '').trim();
  speakJapanese(clean);
}

function addVnChatVocabToSrs(word, reading, meaning, jlpt) {
  const jpEl = document.getElementById('vnChatJpText');
  const contextSentence = (jpEl && jpEl.textContent) ? jpEl.textContent.trim() : word;
  const novelTitle = (typeof booksData !== 'undefined' && booksData[currentVnBookKey]?.title) || 'Visual Novel';
  saveWordToSRS(word, reading, meaning, jlpt || 'N3', contextSentence, meaning, novelTitle);
  playSuccessChime();
  showToastNotification(`Đã lưu "${word}" vào Flashcard SRS! (+10 XP)`, 'success');
  addLocalXp(10);
}

// -------------------------------------------------------------------
// 13. KEYBOARD SHORTCUTS CONTROLLER
// -------------------------------------------------------------------
document.addEventListener('keydown', (e) => {
  const vnPane = document.getElementById('pane-vn');
  if (!vnPane || !vnPane.classList.contains('active')) return;

  if (['INPUT', 'TEXTAREA'].includes(e.target.tagName)) return;

  switch (e.key) {
    case ' ':
    case 'Enter':
      e.preventDefault();
      if (currentVnMode === 'story') {
        if (isTyping) finishTypewriterInstantly();
        else advanceVnDialogue();
      }
      break;
    case 't':
    case 'T':
      toggleVnTransVisibility();
      break;
    case 'a':
    case 'A':
      toggleVnAuto();
      break;
    case 's':
    case 'S':
      speakVnLine();
      break;
    case 'l':
    case 'L':
      toggleVnLog();
      break;
    case 'f':
    case 'F':
      toggleVnFocusMode();
      break;
    case 'Escape':
      closeVnAiSensei();
      closeVnWordPopup();
      const modal = document.getElementById('vnLogModal');
      if (modal && modal.classList.contains('open')) modal.classList.remove('open');
      break;
  }
});
