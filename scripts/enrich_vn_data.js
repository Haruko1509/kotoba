const fs = require('fs');

const vnStoryRegistry = {
  ginga: {
    id: 'ginga',
    title: '銀河鉄道の夜',
    vnTitle: 'Chuyến tàu Ngân Hà',
    author: 'Miyazawa Kenji (宮沢賢治)',
    theme: 'theme-ginga',
    bgAmbient: 'rgba(30, 58, 138, 0.25)',
    scenes: [
      { id: 'station', name: 'Cảnh 1: Bờ cát sông sao Thiên Nga', startIdx: 0, bg: 'assets/bg_shinkai_ginga.jpg' },
      { id: 'carriage', name: 'Cảnh 2: Bên trong khoang tàu đêm', startIdx: 4, bg: 'assets/bg_shinkai_carriage.jpg' }
    ]
  },
  merosu: {
    id: 'merosu',
    title: '走れメロス',
    vnTitle: 'Chạy đi Melos',
    author: 'Dazai Osamu (太宰治)',
    theme: 'theme-merosu',
    bgAmbient: 'rgba(180, 83, 9, 0.25)',
    scenes: [
      { id: 'throne', name: 'Cảnh 1: Ngai vàng bạo chúa Syracuse', startIdx: 0, bg: 'assets/covers/cover_anime_castle.jpg' },
      { id: 'road', name: 'Cảnh 2: Sa trường hoàng hôn rực lửa', startIdx: 4, bg: 'assets/covers/cover_anime_sunset.jpg' }
    ]
  },
  gon: {
    id: 'gon',
    title: 'ごんぎつね',
    vnTitle: 'Cáo nhỏ Gon',
    author: 'Niimi Nankichi (新美南吉)',
    theme: 'theme-gon',
    bgAmbient: 'rgba(16, 185, 129, 0.22)',
    scenes: [
      { id: 'stream', name: 'Cảnh 1: Bờ suối làng quê giăng lưới', startIdx: 0, bg: 'assets/covers/cover_anime_forest.jpg' },
      { id: 'cottage', name: 'Cảnh 2: Thềm nhà tranh mùa thu êm đềm', startIdx: 3, bg: 'assets/covers/cover_anime_night_shrine.jpg' }
    ]
  },
  rashomon: {
    id: 'rashomon',
    title: '羅生門',
    vnTitle: 'La Sinh Môn',
    author: 'Akutagawa Ryunosuke (芥川龍之介)',
    theme: 'theme-rashomon',
    bgAmbient: 'rgba(71, 85, 105, 0.32)',
    scenes: [
      { id: 'gate', name: 'Cảnh 1: Dưới cổng thành Kyoto chiều mưa', startIdx: 0, bg: 'assets/bg_shinkai_rashomon.jpg' },
      { id: 'attic', name: 'Cảnh 2: Tầng gác mái le lói ánh đuốc', startIdx: 3, bg: 'assets/rashomon.png' }
    ]
  },
  gakuen: {
    id: 'gakuen',
    title: '学園アイドルの彼氏役',
    vnTitle: 'Được Gakuen Idol Nhận Làm Bạn Trai',
    author: '佐々木 (Sasaki)',
    theme: 'theme-gakuen',
    bgAmbient: 'rgba(236, 72, 153, 0.22)',
    scenes: [
      { id: 'hallway', name: 'Cảnh 1: Hành lang trước tủ giày học viện', startIdx: 0, bg: 'assets/covers/cover_school_romcom.jpg' },
      { id: 'rooftop', name: 'Cảnh 2: Góc sân thượng bí mật hoàng hôn', startIdx: 3, bg: 'assets/covers/cover_anime_sunset.jpg' }
    ]
  },
  chuumon: {
    id: 'chuumon',
    title: '注文の多い料理店',
    vnTitle: 'Quán Ăn Nhiều Yêu Cầu',
    author: 'Miyazawa Kenji (宮沢賢治)',
    theme: 'theme-chuumon',
    bgAmbient: 'rgba(202, 138, 4, 0.25)',
    scenes: [
      { id: 'forest', name: 'Cảnh 1: Cửa hiệu Sơn Miêu Hiên giữa rừng', startIdx: 0, bg: 'assets/covers/cover_isekai_gourmet.jpg' },
      { id: 'dining', name: 'Cảnh 2: Căn phòng khách quý tộc sang trọng', startIdx: 2, bg: 'assets/covers/cover_anime_cafe.jpg' }
    ]
  }
};

const vnDialoguesByBook = {
  ginga: [
    {
      speaker: 'ジョバンニ (Giovanni)',
      role: '✦ 星めぐりの旅人 • Hành khách chuyến tàu Ngân Hà',
      charId: 'giovanni',
      type: 'dialogue',
      avatar: 'assets/char_giovanni.png',
      leftSprite: 'assets/char_giovanni.png',
      centerSprite: null,
      rightSprite: 'assets/char_campanella.png',
      speakerPos: 'left',
      bg: 'assets/bg_shinkai_ginga.jpg',
      location: '🌸 CHUYẾN TÀU NGÂN HÀ • GA SỐ 4: BỜ BÃI THIÊN NGA & DÒNG SÔNG SAO',
      theme: 'theme-ginga',
      jp: '「カムパネルラ、見てごらん！ あの川の向こうの光、白くけむった銀河の帯だよ」',
      vn: '"Campanella, cậu nhìn kìa! Vệt sáng bên kia dòng sông kìa, chính là dải Ngân Hà mờ trắng đấy."',
      grammarInsight: 'Cấu trúc [Động từ thể て + ごらん]: Mẫu câu rủ rê, khuyên nhủ một cách ân cần, trìu mến giữa bạn bè đồng trang lứa hoặc người lớn với trẻ nhỏ (Xem kìa!).',
      nuanceInsight: 'Khắc họa sự ngỡ ngàng tột bậc xen lẫn niềm vui thuần khiết của cậu bé Giovanni khi lần đầu chạm mắt vào dòng sông sao vũ trụ.',
      vocabTokens: [
        { word: '見てごらん', reading: 'みてごらん', meaning: 'Hãy nhìn xem này (lời rủ rê trìu mến)', level: 'N3' },
        { word: '向こう', reading: 'むこう', meaning: 'Phía bên kia, hướng đối diện', level: 'N4' },
        { word: '銀河', reading: 'ぎんが', meaning: 'Dải Ngân Hà, sông sao trời', level: 'N2' },
        { word: '帯', reading: 'おび', meaning: 'Dải, vệt dài trải ngang', level: 'N3' }
      ]
    },
    {
      speaker: 'カムパネルラ (Campanella)',
      role: '✦ 親友 • Người bạn đồng hành định mệnh',
      charId: 'campanella',
      type: 'dialogue',
      avatar: 'assets/char_campanella.png',
      leftSprite: 'assets/char_giovanni.png',
      centerSprite: null,
      rightSprite: 'assets/char_campanella.png',
      speakerPos: 'right',
      bg: 'assets/bg_shinkai_ginga.jpg',
      location: '🌸 CHUYẾN TÀU NGÂN HÀ • GA SỐ 4: BỜ BÃI THIÊN NGA & DÒNG SÔNG SAO',
      theme: 'theme-ginga',
      jp: '「本当だね、ジョバンニ。先生が午後の授業で黒板に吊した図とまったく同じだ」',
      vn: '"Đúng thật rồi, Giovanni. Y hệt như bản đồ chòm sao thầy giáo treo trên bảng đen trong giờ học ban chiều."',
      grammarInsight: 'Cấu trúc [A とまったく同じ]: Hoàn toàn y hệt, giống hệt như A. Phó từ まったく nhấn mạnh tính tuyệt đối không một chút sai lệch.',
      nuanceInsight: 'Campanella có phong thái điềm đạm, trí thức và ấm áp, kết nối trải nghiệm kỳ ảo trên trời sao với lớp học đời thực ở trần thế.',
      vocabTokens: [
        { word: '本当', reading: 'ほんとう', meaning: 'Thực sự, quả thật', level: 'N5' },
        { word: '黒板', reading: 'こくばん', meaning: 'Bảng đen lớp học', level: 'N4' },
        { word: '吊した', reading: 'つるした', meaning: 'Đã treo lơ lửng lên', level: 'N2' },
        { word: 'まったく同じ', reading: 'まったくおなじ', meaning: 'Hoàn toàn giống hệt', level: 'N3' }
      ]
    },
    {
      speaker: 'ジョバンニ (Giovanni)',
      role: '💭 心の声 • Độc thoại nội tâm của Giovanni',
      charId: 'giovanni',
      type: 'monologue',
      avatar: 'assets/char_giovanni.png',
      leftSprite: 'assets/char_giovanni.png',
      centerSprite: null,
      rightSprite: 'assets/char_campanella.png',
      speakerPos: 'left',
      bg: 'assets/bg_shinkai_ginga.jpg',
      location: '🌌 DÒNG SÔNG THIÊN NGA • KÝ ỨC VÀ ÁNH SÁNG KỲ ẢO',
      theme: 'theme-ginga',
      jp: '（僕たちはいつの間に、この銀河ステーションにやってきたのだろう。胸の奥が不思議な安らぎで満たされていく……）',
      vn: '(Chúng mình đã bước lên ga tàu Ngân Hà này từ lúc nào vậy nhỉ. Trong sâu thẳm lồng ngực mình đang ngập tràn một nỗi an yên kỳ lạ...)',
      grammarInsight: 'Cấu trúc [いつの間に]: Tự bao giờ, từ lúc nào mà không hề hay biết. Biểu thị sự chuyển đổi không gian vô thức đặc trưng của văn phong hư ảo Miyazawa Kenji.',
      nuanceInsight: 'Độc thoại nội tâm thể hiện tâm trạng bàng hoàng thoát tục của Giovanni khi rời xa những vất vả thiếu thốn trần thế để bước vào thế giới sao.',
      vocabTokens: [
        { word: 'いつの間に', reading: 'いつのまに', meaning: 'Từ lúc nào không hay', level: 'N3' },
        { word: '胸の奥', reading: 'むねのおく', meaning: 'Tận đáy lòng, sâu trong tim', level: 'N3' },
        { word: '安らぎ', reading: 'やすらぎ', meaning: 'Sự bình yên, an tĩnh nội tâm', level: 'N2' },
        { word: '満たされる', reading: 'みたされる', meaning: 'Được lấp đầy, ngập tràn', level: 'N2' }
      ]
    },
    {
      speaker: 'ジョバンニ (Giovanni)',
      role: '✦ Lựa chọn hành động của bạn',
      charId: 'giovanni',
      type: 'choice',
      avatar: 'assets/char_giovanni.png',
      leftSprite: 'assets/char_giovanni.png',
      centerSprite: null,
      rightSprite: 'assets/char_campanella.png',
      speakerPos: 'left',
      bg: 'assets/bg_shinkai_ginga.jpg',
      location: '🌸 NGÃ RẼ TRÊN BỜ SÔNG SAO • LỰA CHỌN CỦA GIOVANNI',
      theme: 'theme-ginga',
      jp: '「ねえ、カムパネルラ。僕たち、これからどうしようか？」',
      vn: '"Này Campanella. Bọn mình giờ nên làm gì tiếp theo đây?"',
      choices: [
        {
          textJp: '「あの川原の青い燐光を確かめに行こう！」',
          textVn: 'Xuống bãi cát sông sao tìm hiểu luồng lân quang biếc (+15 XP)',
          reactionText: 'Giovanni và Campanella hào hứng chạy xuống bờ cát pha lê lấp lánh!',
          xp: 15
        },
        {
          textJp: '「先生が話していた白鳥の停車場を探してみよう」',
          textVn: 'Đi tìm nhà ga Thiên Nga mà thầy giáo từng nhắc tới (+15 XP)',
          reactionText: 'Hai cậu bé lắng nghe tiếng chuông bạc ngân vang từ ngọn tháp thiên hà!',
          xp: 15
        }
      ],
      grammarInsight: 'Cấu trúc [〜ようか / どうしようか]: Đưa ra lời đề nghị hoặc tự hỏi xem bản thân nên làm thế nào trong ngữ cảnh thân mật.',
      nuanceInsight: 'Thời khắc tình bạn gắn kết sâu sắc trước khi chuyến tàu bắt đầu lao vút qua các chòm sao phía Nam.',
      vocabTokens: [
        { word: '川原', reading: 'かわら', meaning: 'Bãi cát ven sông', level: 'N3' },
        { word: '燐光', reading: 'りんこう', meaning: 'Ánh lân tinh lấp lánh trong đêm', level: 'N1' },
        { word: '確かめる', reading: 'たしかめる', meaning: 'Xác minh, làm cho rõ ràng', level: 'N3' }
      ]
    },
    {
      speaker: 'ジョバンニ (Giovanni)',
      role: '✦ 星めぐりの旅人 • Lời nguyện ước chân thành',
      charId: 'giovanni',
      type: 'dialogue',
      avatar: 'assets/char_giovanni.png',
      leftSprite: 'assets/char_giovanni.png',
      centerSprite: null,
      rightSprite: 'assets/char_campanella.png',
      speakerPos: 'left',
      bg: 'assets/bg_shinkai_carriage.jpg',
      location: '🚂 BÊN TRONG KHOANG TÀU ĐÊM • ÁNH ĐÈN DẦU ẤM ÁP',
      theme: 'theme-ginga',
      jp: '「ああ、どこまでも一緒に行こう！ 僕はみんなの本当の幸いのためなら、この体だって惜しくないよ」',
      vn: '"Phải rồi, chúng mình hãy cùng nhau đi tới muôn nơi nhé! Nếu là vì hạnh phúc đích thực của mọi người, tớ chẳng tiếc thân mình đâu."',
      grammarInsight: 'Cấu trúc [〜のためなら、〜だって惜しくない]: Nếu là vì mục đích cao cả X thì ngay cả bản thân Y cũng không mảy may tiếc nuối. [惜しい] = tiếc nuối.',
      nuanceInsight: 'Tuyên ngôn nhân văn cốt lõi của tác phẩm: "Hạnh phúc đích thực (本当の幸い)" chỉ có được khi con người biết hy sinh và chia sẻ cùng tha nhân.',
      vocabTokens: [
        { word: 'どこまでも', reading: 'どこまでも', meaning: 'Tới bất cứ phương trời nào, muôn nơi', level: 'N3' },
        { word: '幸い', reading: 'さいわい', meaning: 'Hạnh phúc, niềm phúc lành', level: 'N3' },
        { word: '惜しくない', reading: 'おしくない', meaning: 'Không màng tiếc nuối', level: 'N2' }
      ]
    },
    {
      speaker: '車掌 (Người soát vé)',
      role: '🎫 銀河鉄道の車掌 • Người soát vé tàu đêm bí ẩn',
      charId: 'conductor',
      type: 'dialogue',
      avatar: 'assets/char_conductor.png',
      leftSprite: 'assets/char_giovanni.png',
      centerSprite: 'assets/char_conductor.png',
      rightSprite: 'assets/char_campanella.png',
      speakerPos: 'center',
      bg: 'assets/bg_shinkai_carriage.jpg',
      location: '🚂 KHOANG TÀU ĐÊM • TIẾNG CHUÔNG SOÁT VÉ VANG LÊN',
      theme: 'theme-ginga',
      jp: '「切符を拝見いたします。……おや、坊っちゃん方の切符は、天上へでもどこへでも行ける本物の特別な切符ですね」',
      vn: '"Xin phép kiểm tra vé tàu. ...Ô kìa, vé của hai cậu bé là tấm vé đặc biệt đích thực, có thể lên tận thiên giới hay tới bất cứ chân trời nào."',
      grammarInsight: 'Kính ngữ khiêm nhường [拝見いたします]: Khiêm nhường ngữ của 見る (tôi xin phép được xem vé). Đi liền sau là dạng phủ định lửng tôn kính [天上へでもどこへでも行ける].',
      nuanceInsight: 'Người soát vé đóng vai trò người dẫn đường tâm linh, hé lộ chân tướng huyền ảo của tấm vé không gian đa chiều mà Giovanni mang theo.',
      vocabTokens: [
        { word: '拝見する', reading: 'はいけんする', meaning: 'Xin phép xem (Khiêm nhường ngữ)', level: 'N3' },
        { word: '坊っちゃん', reading: 'ぼっちゃん', meaning: 'Công tử, cậu bé nhà gia giáo', level: 'N3' },
        { word: '天上', reading: 'てんじょう', meaning: 'Thiên giới, cõi trời cao', level: 'N2' },
        { word: '特別', reading: 'とくべつ', meaning: 'Đặc biệt, phi thường', level: 'N4' }
      ]
    },
    {
      speaker: '語り手 (Người dẫn chuyện)',
      role: '📜 旅の終わりと始まり • Lời tự sự ngân hà',
      charId: 'narrator',
      type: 'narration',
      avatar: 'assets/char_campanella.png',
      leftSprite: null,
      centerSprite: null,
      rightSprite: null,
      speakerPos: 'center',
      bg: 'assets/bg_shinkai_carriage.jpg',
      location: '✨ VŨ TRỤ HUYỀN BÍ • KHÚC CA QUA CHÒM NAM THẬP TỰ',
      theme: 'theme-ginga',
      jp: '銀河の夜はどこまでも深く、車窓の外には金剛石の砂利とリンドウの花が静かに流れていくのでした。',
      vn: 'Đêm Ngân Hà tĩnh lặng thâm sâu vô cùng, ngoài cửa sổ toa tàu, những hạt sỏi kim cương và đóa hoa thạch thảo tím biếc cứ thế êm đềm lướt trôi.',
      grammarInsight: 'Dạng kết câu trần thuật cổ điển [〜のでした]: Dùng trong văn học thiếu nhi và truyện cổ tích để gợi âm hưởng hồi tưởng êm dịu, lắng đọng.',
      nuanceInsight: 'Miyazawa Kenji sử dụng những hình ảnh biểu tượng như kim cương và hoa Gentian tím để đặc tả cái đẹp thanh khiết của thế giới vĩnh hằng.',
      vocabTokens: [
        { word: '車窓', reading: 'しゃそう', meaning: 'Cửa sổ toa xe tàu hỏa', level: 'N2' },
        { word: '金剛石', reading: 'こんごうせき', meaning: 'Kim cương thiên nhiên', level: 'N1' },
        { word: '砂利', reading: 'じゃり', meaning: 'Sỏi đá nhỏ', level: 'N2' }
      ]
    }
  ],

  merosu: [
    {
      speaker: 'メロス (Melos)',
      role: '🗡️ シラクスの熱血漢 • Người chăn cừu xả thân vì công lý',
      charId: 'melos',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_melos.png',
      leftSprite: 'assets/chars/char_melos.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_dionys.png',
      speakerPos: 'left',
      bg: 'assets/covers/cover_anime_castle.jpg',
      location: '🏛️ ĐIỆN VÀNG SYRACUSE • ĐỐI MẶT BẠO CHÚA DIONYS',
      theme: 'theme-merosu',
      jp: '「暴君ディオニスよ！ 私を処刑する前に、三日間の猶予をくれ。妹の結婚式を故郷で挙げてやりたいのだ」',
      vn: '"Bạo chúa Dionys kia! Trước khi xử tử ta, hãy ban cho ta ba ngày ân hạn. Ta muốn về quê nhà hoàn tất hôn lễ cho em gái ta."',
      grammarInsight: 'Cấu trúc [〜前に]: Trước khi thực hiện hành động X. [〜てやりたい]: Thể hiện mong muốn làm điều gì đó tốt đẹp cho người thân nhỏ tuổi hơn (ở đây là em gái).',
      nuanceInsight: 'Dazai Osamu khắc họa Melos ngay từ đầu là người bộc trực, không toan tính, dám đối mặt bạo quyền vì nghĩa vụ gia đình.',
      vocabTokens: [
        { word: '暴君', reading: 'ぼうくん', meaning: 'Bạo chúa, kẻ cai trị tàn độc', level: 'N1' },
        { word: '処刑', reading: 'しょけい', meaning: 'Xử tử, thi hành án phạt', level: 'N2' },
        { word: '猶予', reading: 'ゆうよ', meaning: 'Sự ân hạn, hoãn lại', level: 'N1' },
        { word: '結婚式', reading: 'けっこんしき', meaning: 'Lễ cưới, hôn lễ', level: 'N4' }
      ]
    },
    {
      speaker: 'ディオニス王 (Vua Dionys)',
      role: '👑 猜疑心の暴君 • Bạo chúa chìm trong hoài nghi thế gian',
      charId: 'dionys',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_dionys.png',
      leftSprite: 'assets/chars/char_melos.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_dionys.png',
      speakerPos: 'right',
      bg: 'assets/covers/cover_anime_castle.jpg',
      location: '🏛️ ĐIỆN VÀNG SYRACUSE • TIẾNG CƯỜI KHẨY CỦA BẠO QUYỀN',
      theme: 'theme-merosu',
      jp: '「ふん、戯言を言うな。一度逃げ帰った者が再び戻ってくるはずがなかろう。人の心など信じられぬ」',
      vn: '"Hừ, đừng nói lời xằng bậy. Kẻ một khi đã trốn thoát nào có lý do gì tự quay lại nộp mạng. Lòng dạ con người trên đời này làm sao tin được!"',
      grammarInsight: 'Cấu trúc [〜はずがなかろう (はずがない)]: Tuyệt đối không thể nào có chuyện như vậy. Thể phủ định phỏng đoán trang trọng của [はずがない].',
      nuanceInsight: 'Bạo chúa Dionys tượng trưng cho sự hoài nghi cực độ của nhân loại thời bấy giờ, khi lòng tin giữa người với người đã hoàn toàn sụp đổ.',
      vocabTokens: [
        { word: '戯言', reading: 'ざれごと / たわごと', meaning: 'Lời nói nhảm, chuyện hão huyền', level: 'N1' },
        { word: '再び', reading: 'ふたたび', meaning: 'Lại một lần nữa', level: 'N3' },
        { word: '信じる', reading: 'しんじる', meaning: 'Tin tưởng, đặt niềm tin', level: 'N4' }
      ]
    },
    {
      speaker: 'メロス (Melos)',
      role: '🗡️ Lời thề son sắt',
      charId: 'melos',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_melos.png',
      leftSprite: 'assets/chars/char_melos.png',
      centerSprite: 'assets/chars/char_selinuntius.png',
      rightSprite: 'assets/chars/char_dionys.png',
      speakerPos: 'left',
      bg: 'assets/covers/cover_anime_castle.jpg',
      location: '🏛️ ĐIỆN VÀNG SYRACUSE • CON TIM LÀM CON TIN',
      theme: 'theme-merosu',
      jp: '「私には無二の友がいる！ 石工のセリヌンティウスだ。彼を身代わりの人質としてここに残す！」',
      vn: '"Ta có một người bạn chí cốt độc nhất vô nhị! Là người thợ đá Selinuntius. Ta sẽ để cậu ấy lại đây làm con tin thế mạng!"',
      grammarInsight: 'Cấu trúc [〜として]: Với tư cách là, đóng vai trò là. Cụm [無二の友] (người bạn độc nhất vô nhị) nhấn mạnh mối liên kết tri kỷ phi thường.',
      nuanceInsight: 'Hành động đặt cược sinh mệnh của người bạn thân nhất biểu thị niềm tin sắt đá tuyệt đối của tình bằng hữu chân chính.',
      vocabTokens: [
        { word: '無二', reading: 'むに', meaning: 'Có một không hai, vô song', level: 'N1' },
        { word: '石工', reading: 'いしく', meaning: 'Thợ điêu khắc đá', level: 'N2' },
        { word: '身代わり', reading: 'みがわり', meaning: 'Người thế mạng, gánh thay', level: 'N2' },
        { word: '人質', reading: 'ひとじち', meaning: 'Con tin', level: 'N2' }
      ]
    },
    {
      speaker: 'メロス (Melos)',
      role: '💭 Sa trường hoàng hôn rực lửa',
      charId: 'melos',
      type: 'choice',
      avatar: 'assets/chars/avatar_melos.png',
      leftSprite: 'assets/chars/char_melos.png',
      centerSprite: null,
      rightSprite: null,
      speakerPos: 'left',
      bg: 'assets/covers/cover_anime_sunset.jpg',
      location: '🏃 CON ĐƯỜNG VỀ SYRACUSE • MẶT TRỜI ĐANG DẦN CHÌM XUỐNG BIỂN',
      theme: 'theme-merosu',
      jp: '「待っていろ、セリヌンティウス！ 私は裏切り者として死ぬわけにはいかない……！」',
      vn: '"Hãy đợi ta, Selinuntius! Ta tuyệt đối không thể chết như một kẻ phản bội hèn nhát...!"',
      choices: [
        {
          textJp: '「喉が渇き果てても、立ち止まらずに走り続ける！」',
          textVn: 'Cắn răng chạy tiếp không ngừng dù cổ họng khô rát (+15 XP)',
          reactionText: 'Melos gạt đi giọt mồ hôi đẫm máu, lao mình qua con dốc như tên bắn!',
          xp: 15
        },
        {
          textJp: '「岩陰の湧き水を一口すくい、全身の力を取り戻す！」',
          textVn: 'Uống ngụm nước suối mát lành để đánh thức mọi giác quan (+15 XP)',
          reactionText: 'Dòng nước suối ngọt ngào cứu sống linh hồn Melos, thổi bùng lên ngọn lửa hy vọng!',
          xp: 15
        }
      ],
      grammarInsight: 'Cấu trúc [〜わけにはいかない]: Không thể nào làm chuyện X được (do đạo đức, lương tâm hoặc hoàn cảnh bắt buộc không cho phép).',
      nuanceInsight: 'Đỉnh điểm của ý chí kiên định: Thà kiệt sức ngã gục trên đường chạy còn hơn sống sót nhục nhã như một kẻ bội ước.',
      vocabTokens: [
        { word: '裏切り者', reading: 'うらぎりもの', meaning: 'Kẻ phản bội, kẻ nuốt lời thề', level: 'N2' },
        { word: 'わけにはいかない', reading: 'wake ni wa ikanai', meaning: 'Không thể nào cho phép bản thân làm thế', level: 'N3' },
        { word: '渇く', reading: 'かわく', meaning: 'Khát khô họng', level: 'N4' }
      ]
    },
    {
      speaker: 'セリヌンティウス (Selinuntius)',
      role: '🤝 親友 • Người tri kỷ trên đoạn đầu đài',
      charId: 'selinuntius',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_selinuntius.png',
      leftSprite: 'assets/chars/char_melos.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_selinuntius.png',
      speakerPos: 'right',
      bg: 'assets/covers/cover_anime_sunset.jpg',
      location: '⚔️ QUẢNG TRƯỜNG THI HÀNH ÁN • ĐÚNG KHOẢNH KHẮC HOÀNG HÔN TẮT',
      theme: 'theme-merosu',
      jp: '「メロスよ、お前が帰ってくることを少しも疑わなかった。さあ、私を殴れ！ 一瞬でも疑った私を！」',
      vn: '"Melos, ta chưa từng mảy may nghi ngờ việc cậu sẽ trở về. Nào, hãy đấm ta đi! Hãy trừng phạt kẻ từng có một thoáng dao động nghi ngờ này!"',
      grammarInsight: 'Cấu trúc [少しも〜ない]: Hoàn toàn không một chút nào... Mẫu câu [〜を一瞬でも]: Dẫu chỉ trong một tích tắc thoáng qua.',
      nuanceInsight: 'Cái tát lẫn nhau giữa hai người bạn là hành động gột rửa sạch sẽ những yếu lòng nhân tính, nâng tầm tình bạn thành biểu tượng thiêng liêng.',
      vocabTokens: [
        { word: '疑う', reading: 'うたがう', meaning: 'Nghi ngờ, ngờ vực', level: 'N3' },
        { word: '殴る', reading: 'なぐる', meaning: 'Đấm, giáng một cú đấm', level: 'N3' },
        { word: '一瞬', reading: 'いっしゅん', meaning: 'Một tích tắc, chớp mắt', level: 'N2' }
      ]
    },
    {
      speaker: 'ディオニス王 (Vua Dionys)',
      role: '👑 改心した王 • Nước mắt của vị vua thức tỉnh',
      charId: 'dionys',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_dionys.png',
      leftSprite: 'assets/chars/char_melos.png',
      centerSprite: 'assets/chars/char_dionys.png',
      rightSprite: 'assets/chars/char_selinuntius.png',
      speakerPos: 'center',
      bg: 'assets/covers/cover_anime_sunset.jpg',
      location: '🌅 QUẢNG TRƯỜNG SYRACUSE • TIẾNG HOAN HÔ RỢP TRỜI',
      theme: 'theme-merosu',
      jp: '「お前たちの勝利だ。真の信頼とは何か、今この目で確かに見た。どうか私をも仲間に入れてくれ」',
      vn: '"Các ngươi đã chiến thắng rồi. Lòng tin chân thực là gì, hôm nay chính mắt ta đã được chứng kiến. Xin hãy cho ta kết nạp làm một người bạn của các ngươi."',
      grammarInsight: 'Cấu trúc [〜てくれ / 入れてくれ]: Cầu xin một cách khẩn thiết, chân thành từ một bạo chúa kiêu ngạo nay hạ mình học bài học lòng tin.',
      nuanceInsight: 'Cái kết huy hoàng rực rỡ của văn học nhân văn lãng mạn: Ánh sáng của lòng trung thực và đức hy sinh đã làm tan chảy cả trái tim sắt đá nhất.',
      vocabTokens: [
        { word: '勝利', reading: 'しょうり', meaning: 'Chiến thắng vinh quang', level: 'N3' },
        { word: '信頼', reading: 'しんらい', meaning: 'Lòng tin cậy, tín nhiệm', level: 'N3' },
        { word: '仲間', reading: 'なかま', meaning: 'Bạn hữu, đồng đội cùng chí hướng', level: 'N4' }
      ]
    }
  ],

  gon: [
    {
      speaker: 'ごん (Chú cáo nhỏ Gon)',
      role: '🦊 いたずら好きの子狐 • Chú cáo tinh nghịch cô đơn',
      charId: 'gon',
      type: 'monologue',
      avatar: 'assets/chars/avatar_gon.png',
      leftSprite: 'assets/chars/char_gon.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_hyoju.png',
      speakerPos: 'left',
      bg: 'assets/covers/cover_anime_forest.jpg',
      location: '🍂 BỜ SUỐI LÀNG QUÊ • NẮNG CHIỀU VÀ TẤM LƯỚI CÁ',
      theme: 'theme-gon',
      jp: '（兵十が川でうなぎを捕っているぞ。ふふっ、ちょっといたずらして驚かせてやろうか）',
      vn: '(Anh Hyoju đang bắt lươn dưới suối kìa. Hì hì, hay là mình nghịch ngợm một tí cho anh ấy giật mình chơi nhỉ?)',
      grammarInsight: 'Cấu trúc [〜て驚かせてやろうか]: Thể ý chí [〜ようか] kết hợp thể sai khiến [驚かせる] biểu thị ý định tinh nghịch trêu chọc.',
      nuanceInsight: 'Gon bản chất không hề có ác ý, sự nghịch ngợm của chú cáo chỉ là khao khát được kết nối và chú ý trong nỗi cô đơn lẻ loi.',
      vocabTokens: [
        { word: 'うなぎ', reading: 'うなぎ', meaning: 'Con lươn suối', level: 'N3' },
        { word: 'いたずら', reading: 'いたずら', meaning: 'Trò nghịch ngợm, quậy phá', level: 'N3' },
        { word: '驚かせる', reading: 'おどろかせる', meaning: 'Làm cho kinh ngạc, giật mình', level: 'N3' }
      ]
    },
    {
      speaker: 'ごん (Chú cáo nhỏ Gon)',
      role: '🦊 Lựa chọn của Gon bên bờ suối',
      charId: 'gon',
      type: 'choice',
      avatar: 'assets/chars/avatar_gon.png',
      leftSprite: 'assets/chars/char_gon.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_hyoju.png',
      speakerPos: 'left',
      bg: 'assets/covers/cover_anime_forest.jpg',
      location: '🍂 BỤI CỎ VEN BỜ SUỐI • THỜI KHẮC ĐỊNH MỆNH',
      theme: 'theme-gon',
      jp: '「兵十のビクの中に、大きなうなぎが跳ねているぞ……」',
      vn: '"Trong giỏ cá của anh Hyoju, có con lươn béo tròn đang quẫy đạp..."',
      choices: [
        {
          textJp: '「ビクからうなぎを引っ張り出して川へ逃がす！」',
          textVn: 'Kéo con lươn ra thả về dòng nước suối (+15 XP)',
          reactionText: 'Con lươn trườn tuột vào dòng nước, Hyoju tức giận vung chài đuổi bắt!',
          xp: 15
        },
        {
          textJp: '「いたずらをぐっと我慢して、草むらから見守る」',
          textVn: 'Kìm nén trò đùa, lặng lẽ quan sát từ bụi cỏ dày (+15 XP)',
          reactionText: 'Gon cảm nhận được nỗi nhọc nhằn của người tiều phu đánh cá mưu sinh...',
          xp: 15
        }
      ],
      grammarInsight: 'Cấu trúc [〜が跳ねている]: Động từ miêu tả hành động đang diễn ra trước mắt (con lươn đang quẫy đạp).',
      nuanceInsight: 'Hành động tưởng chừng vô hại này chính là mấu chốt dẫn đến bi kịch khi mẹ Hyoju ốm nặng không kịp ăn bát cháo lươn.',
      vocabTokens: [
        { word: '引っ張り出す', reading: 'ひっぱりだす', meaning: 'Kéo tuột ra ngoài', level: 'N2' },
        { word: '我慢する', reading: 'がまんする', meaning: 'Nhẫn nhịn, chịu đựng kiềm chế', level: 'N3' },
        { word: '草むら', reading: 'くさむら', meaning: 'Bụi cỏ dại ven đường', level: 'N2' }
      ]
    },
    {
      speaker: '兵十 (Anh tiều phu Hyoju)',
      role: '🌾 村の若者 • Người tiều phu nghèo hiếu thảo',
      charId: 'hyoju',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_hyoju.png',
      leftSprite: 'assets/chars/char_gon.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_hyoju.png',
      speakerPos: 'right',
      bg: 'assets/covers/cover_anime_forest.jpg',
      location: '🍂 BỜ SUỐI LÀNG QUÊ • TIẾNG LA MẮNG CỦA HYOJU',
      theme: 'theme-gon',
      jp: '「こらっ、ごん狐！ また悪戯をしにきたな！ 母ちゃんに食べさせる大事な魚なんだぞ！」',
      vn: '"Này con cáo Gon kia! Mày lại đến đây quậy phá nữa hả! Đây là con cá quý để tao bồi bổ cho mẹ già đang ốm đấy!"',
      grammarInsight: 'Cấu trúc [〜に食べさせる]: Thể sai khiến [食べさせる] mang ý nghĩa hiếu thảo "cho ai đó ăn / phụng dưỡng".',
      nuanceInsight: 'Chi tiết hé lộ động cơ thiêng liêng của Hyoju, làm dậy sóng lương tâm và sự hối hận khôn nguôi trong lòng Gon sau này.',
      vocabTokens: [
        { word: '大事', reading: 'だいじ', meaning: 'Quan trọng, quý giá', level: 'N4' },
        { word: '食べさせる', reading: 'たべさせる', meaning: 'Cho ăn, dâng món ăn cho mẹ', level: 'N3' }
      ]
    },
    {
      speaker: 'ごん (Chú cáo nhỏ Gon)',
      role: '🌰 栗と松茸の贈り物 • Âm thầm chuộc lại lỗi lầm',
      charId: 'gon',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_gon.png',
      leftSprite: 'assets/chars/char_gon.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_hyoju.png',
      speakerPos: 'left',
      bg: 'assets/covers/cover_anime_night_shrine.jpg',
      location: '🏡 THỀM NHÀ TRANH MÙA THU • ÁNH TRĂNG CHIẾU XUỐNG RỔ HẠT DẺ',
      theme: 'theme-gon',
      jp: '「今夜も兵十の家の戸口に、山で拾った栗と松茸をどっさり置いていこう……許してくれよな」',
      vn: '"Đêm nay mình lại đem hạt dẻ và nấm tùng nhặt được trên núi đặt trước hiên nhà anh Hyoju... Xin anh hãy tha lỗi cho tôi nhé."',
      grammarInsight: 'Phó từ [どっさり]: Đầy ắp, chất đống nặng trĩu. Cấu trúc [〜ていこう]: Thể hiện hành động kiên trì tiếp diễn trong tương lai.',
      nuanceInsight: 'Tình cảm chuộc tội thầm lặng, thánh thiện của Gon diễn ra dưới ánh trăng mùa thu tịch mịch tạo nên dư âm buồn man mác của văn học Niimi Nankichi.',
      vocabTokens: [
        { word: '戸口', reading: 'とぐち', meaning: 'Ngưỡng cửa, cửa ngõ ra vào', level: 'N2' },
        { word: 'どっさり', reading: 'dossari', meaning: 'Đầy ắp, ê hề nhiều', level: 'N2' },
        { word: '松茸', reading: 'まつたけ', meaning: 'Nấm Tùng quý giá', level: 'N2' },
        { word: '許す', reading: 'ゆるす', meaning: 'Tha thứ, bao dung lỗi lầm', level: 'N3' }
      ]
    }
  ],

  rashomon: [
    {
      speaker: '下人 (Người hạ nhân)',
      role: '🌧️ 行き場を失った下人 • Đứng trước vực thẳm sinh tử',
      charId: 'genin',
      type: 'monologue',
      avatar: 'assets/chars/avatar_genin.png',
      leftSprite: 'assets/chars/char_genin.png',
      centerSprite: null,
      rightSprite: null,
      speakerPos: 'left',
      bg: 'assets/bg_shinkai_rashomon.jpg',
      location: '⛩️ DƯỚI CỔNG THÀNH KYOTO • CHIỀU MƯA RẢ RÍCH HOANG TÀN',
      theme: 'theme-rashomon',
      jp: '（雨はやみそうにない。このまま飢え死にするか、盗人になるか……おれにはもう道がない）',
      vn: '(Mưa chiều không có dấu hiệu tạnh. Cứ đà này thì hoặc là chết đói, hoặc là trở thành kẻ trộm... ta chẳng còn con đường nào khác.)',
      grammarInsight: 'Cấu trúc [〜そうにない]: Dường như tuyệt đối không thể nào... Mẫu diễn đạt [A か B か]: Lựa chọn giằng xé giữa hai ngả đường cùng quẫn.',
      nuanceInsight: 'Vấn đề hiện sinh cốt lõi của Akutagawa Ryunosuke: Khi xã hội suy đồi đến đáy, ranh giới giữa cái thiện và cái ác trở nên mỏng manh như sợi tóc.',
      vocabTokens: [
        { word: 'やみそうにない', reading: 'yamisou ni nai', meaning: 'Dường như không thể tạnh mưa', level: 'N3' },
        { word: '飢え死にする', reading: 'うえじにする', meaning: 'Chết vì đói lả', level: 'N2' },
        { word: '盗人', reading: 'ぬすびと', meaning: 'Kẻ trộm cắp, kẻ cướp', level: 'N2' }
      ]
    },
    {
      speaker: '語り手 (Người dẫn chuyện)',
      role: '📜 La Sinh Môn trong mưa chiều hoang tàn',
      charId: 'narrator',
      type: 'narration',
      avatar: 'assets/chars/avatar_genin.png',
      leftSprite: null,
      centerSprite: null,
      rightSprite: null,
      speakerPos: 'center',
      bg: 'assets/bg_shinkai_rashomon.jpg',
      location: '⛩️ CỔNG THÀNH LA SINH MÔN • ĐỐM LỬA TRÊN TẦNG GÁC',
      theme: 'theme-rashomon',
      jp: '荒れ果てた門の楼上からは、かすかに一筋の火の光が漏れ、動物のような物音が聞こえていた。',
      vn: 'Từ trên tầng gác mái hoang tàn của cổng thành, le lói hắt ra một vệt ánh lửa mờ nhạt, kèm theo những tiếng động sột soạt như loài thú dữ.',
      grammarInsight: 'Cụm [荒れ果てた]: Tan hoang, đổ nát hoàn toàn. [一筋の火の光]: Một vệt sáng le lói đơn độc xuyên qua bóng tối.',
      nuanceInsight: 'Bút pháp miêu tả mang không khí u ám, lạnh lẽo đặc trưng của văn học cận đại Nhật Bản.',
      vocabTokens: [
        { word: '荒れ果てた', reading: 'あれはてた', meaning: 'Hoang tàn, trơ trọi đổ nát', level: 'N2' },
        { word: '楼上', reading: 'ろうじょう', meaning: 'Tầng gác mái lầu cao', level: 'N1' },
        { word: '物音', reading: 'ものおと', meaning: 'Âm thanh lạ phát ra', level: 'N3' }
      ]
    },
    {
      speaker: '老婆 (Bà lão nhổ tóc)',
      role: '🕯️ 楼上の老婆 • Sinh tồn giữa cõi thây ma',
      charId: 'oldwoman',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_oldwoman.png',
      leftSprite: 'assets/chars/char_genin.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_oldwoman.png',
      speakerPos: 'right',
      bg: 'assets/rashomon.png',
      location: '🕯️ TẦNG GÁC MÁI LA SINH MÔN • ÁNH ĐUỐC SOI RỌI BỘ MẶT CỦA SỰ THẬT',
      theme: 'theme-rashomon',
      jp: '「この髪を抜いて鬘にするのじゃ。生きるためなら、死人の髪を抜くのも悪くはあるまい」',
      vn: '"Ta nhổ tóc này để làm tóc giả đem bán. Nếu vì miếng cơm sinh tồn, thì nhổ tóc người chết nào có phải điều ác đáng trách."',
      grammarInsight: 'Cấu trúc [〜悪くはあるまい]: Há lại là điều xấu sao (phủ định biểu thị ý khẳng định ngầm thanh minh cho bản thân).',
      nuanceInsight: 'Triết lý sinh tồn tàn nhẫn của bà lão: Bất kỳ hành vi nào, dẫu vô đạo đức tới đâu, đều được hợp thức hóa khi khoác lên chiếc áo "vì sinh tồn".',
      vocabTokens: [
        { word: '鬘', reading: 'かつら', meaning: 'Tóc giả', level: 'N1' },
        { word: '死人', reading: 'しびと', meaning: 'Người đã chết, tử thi', level: 'N2' },
        { word: '悪くはあるまい', reading: 'waruku wa arumai', meaning: 'Há lại là điều sai trái sao', level: 'N1' }
      ]
    },
    {
      speaker: '下人 (Người hạ nhân)',
      role: '⚡ Lựa chọn lương tri hay thú tính',
      charId: 'genin',
      type: 'choice',
      avatar: 'assets/chars/avatar_genin.png',
      leftSprite: 'assets/chars/char_genin.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_oldwoman.png',
      speakerPos: 'left',
      bg: 'assets/rashomon.png',
      location: '🕯️ LA SINH MÔN • RANH GIỚI BƯỚC VÀO ĐÊM TỐI',
      theme: 'theme-rashomon',
      jp: '「そうか……生きるためなら許されるのだな。ならば！」',
      vn: '"Hóa ra là vậy... nếu vì sinh tồn thì việc gì cũng được dung thứ sao. Thế thì!"',
      choices: [
        {
          textJp: '「『その言葉、忘れるな！』と老婆の着物を剥ぎ取る」',
          textVn: 'Lột phăng chiếc áo bào của mụ già để đem bán lấy tiền sống sót (+15 XP)',
          reactionText: 'Hạ nhân lột sạch áo của mụ già, đạp mụ ngã dúi dụi rồi lao mình vào màn đêm!',
          xp: 15
        },
        {
          textJp: '「良心の呵責を覚え、刀を収めて闇の中へ立ち去る」',
          textVn: 'Khựng lại trước lương tri, quay gót bước vào mưa chiều dằn vặt (+15 XP)',
          reactionText: 'Chàng hạ nhân cắn chặt môi, chọn giữ lại phần người còn sót lại trong tâm can...',
          xp: 15
        }
      ],
      grammarInsight: 'Cấu trúc [〜ならば]: Nếu quả thật là như thế thì... Liên từ mang tính quyết đoán cao sau khi đã thông tỏ chân tướng.',
      nuanceInsight: 'Bước ngoặt tâm lý chớp nhoáng: Lòng căm thù cái ác ban đầu biến tướng thành hành động cướp bóc nhân danh cùng một lý lẽ.',
      vocabTokens: [
        { word: '剥ぎ取る', reading: 'はぎとる', meaning: 'Lột phăng, tước đoạt', level: 'N2' },
        { word: '良心', reading: 'りょうしん', meaning: 'Lương tâm, thiên lương', level: 'N3' },
        { word: '呵責', reading: 'かしゃく', meaning: 'Sự dằn vặt, cắn rứt', level: 'N1' }
      ]
    }
  ],

  gakuen: [
    {
      speaker: '星野 瑠奈 (Hoshino Runa)',
      role: '⭐ 学園の絶対的アイドル • Gakuen Idol vạn người mê',
      charId: 'runa',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_runa.png',
      leftSprite: 'assets/chars/char_mob.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_runa.png',
      speakerPos: 'right',
      bg: 'assets/covers/cover_school_romcom.jpg',
      location: '🏫 HÀNH LANG HỌC VIỆN • CÚ TỎ TÌNH BẤT NGỜ CỦA GAKUEN IDOL',
      theme: 'theme-gakuen',
      jp: '「みんな、聞いて！ この人が私の彼氏なの。だからもう誰も告白してこないで！」',
      vn: '"Mọi người nghe đây! Người này chính là bạn trai của tớ. Thế nên từ giờ đừng ai đến tỏ tình với tớ nữa!"',
      grammarInsight: 'Cấu trúc [〜てこないで]: Xin đừng tiếp tục làm hành động hướng về phía tôi nữa. Cụm [告白してくる] = tiến tới tỏ tình với ai đó.',
      nuanceInsight: 'Tình huống kinh điển của Light Novel RomCom học đường: Cô nàng Idol viện cớ kéo một nam sinh bình thường làm lá chắn tình ái.',
      vocabTokens: [
        { word: '彼氏', reading: 'かれし', meaning: 'Bạn trai, người yêu', level: 'N4' },
        { word: '告白する', reading: 'こくはくする', meaning: 'Tỏ tình, bộc bạch lòng mình', level: 'N3' },
        { word: 'アイドル', reading: 'aidoru', meaning: 'Thần tượng học đường', level: 'N4' }
      ]
    },
    {
      speaker: 'モブ主人公 (Nam sinh bình phàm)',
      role: '💭 心の声 • Tiếng thét hoang mang trong lồng ngực',
      charId: 'mob',
      type: 'choice',
      avatar: 'assets/chars/avatar_mob.png',
      leftSprite: 'assets/chars/char_mob.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_runa.png',
      speakerPos: 'left',
      bg: 'assets/covers/cover_school_romcom.jpg',
      location: '🏫 HÀNH LANG HỌC VIỆN • TÂM ĐIỂM CỦA MỌI ÁNH NHÌN',
      theme: 'theme-gakuen',
      jp: '（学園一の美少女アイドルが、なぜ平凡なモブの俺の腕にしがみついているんだ……！？）',
      vn: '(Tại sao Idol xinh đẹp số một học viện lại ôm chặt lấy cánh tay của một tên nam sinh mờ nhạt như mình thế này...!?)',
      choices: [
        {
          textJp: '「『ええっ！？ 僕なんかで本当にいいの？』と戸惑う」',
          textVn: 'Bối rối hoang mang: "Hả!? Người như tớ mà được sao?" (+15 XP)',
          reactionText: 'Runa nháy mắt tinh nghịch rồi khẽ ghé sát tai bạn thì thầm lời cảm ơn!',
          xp: 15
        },
        {
          textJp: '「『よし、任せて！ 完璧に彼氏役を演じてみせるよ』と引き受ける」',
          textVn: 'Bản lĩnh nhận lời: "Được thôi, cứ để tớ diễn tròn vai!" (+15 XP)',
          reactionText: 'Gương mặt Runa thoáng ửng đỏ ngượng ngùng trước sự dứt khoát của bạn!',
          xp: 15
        }
      ],
      grammarInsight: 'Cấu trúc [なぜ〜ているんだ]: Câu hỏi chất vấn bản thân với cảm xúc kinh ngạc tột độ.',
      nuanceInsight: 'Sự tương phản hài hước giữa một nam sinh tự nhận là Mob và cô nàng Idol nổi tiếng nhất trường.',
      vocabTokens: [
        { word: '美少女', reading: 'びしょうじょ', meaning: 'Mỹ thiếu nữ, cô gái xinh đẹp', level: 'N3' },
        { word: '平凡', reading: 'へいぼん', meaning: 'Bình thường, mờ nhạt', level: 'N3' },
        { word: 'しがみつく', reading: 'shigamitsuku', meaning: 'Bám chặt lấy, ôm ghì lấy', level: 'N2' }
      ]
    },
    {
      speaker: '星野 瑠奈 (Hoshino Runa)',
      role: '🌇 Hoàng hôn trên sân thượng bí mật',
      charId: 'runa',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_runa.png',
      leftSprite: 'assets/chars/char_mob.png',
      centerSprite: null,
      rightSprite: 'assets/chars/char_runa.png',
      speakerPos: 'right',
      bg: 'assets/covers/cover_anime_sunset.jpg',
      location: '🌇 SÂN THƯỢNG HỌC VIỆN • GIÓ CHIỀU VÀ ÁNH MẮT THẸN THÙNG',
      theme: 'theme-gakuen',
      jp: '「ふふっ、さっきは助かったわ。でもね……ただのフリじゃなくて、本当に好きになっちゃったらどうする？」',
      vn: '"Hi hi, ban nãy may mà có cậu giải vây đấy. Nhưng mà này... nếu không chỉ là đóng kịch, mà tớ lỡ thích cậu thật thì cậu tính sao?"',
      grammarInsight: 'Cấu trúc [〜ちゃったら (てしまったら)]: Nếu lỡ như xảy ra điều gì ngoài dự tính. Cấu trúc [どうする]: Tính thế nào đây?',
      nuanceInsight: 'Cú twist ngọt ngào điển hình của RomCom: Khi ranh giới giữa thỏa thuận giả vờ và cảm xúc thật bắt đầu xóa nhòa.',
      vocabTokens: [
        { word: '助かった', reading: 'たすかった', meaning: 'Đã được cứu nguy, cảm ơn cậu', level: 'N4' },
        { word: 'フリ', reading: 'furi', meaning: 'Sự giả vờ, đóng kịch giả bộ', level: 'N3' },
        { word: '好きになる', reading: 'すきになる', meaning: 'Phải lòng, đem lòng yêu thích', level: 'N5' }
      ]
    }
  ],

  chuumon: [
    {
      speaker: '紳士の猟師 (Hai chàng thợ săn)',
      role: '🎩 東京から来た紳士 • Hai quý tộc thành thị lạc vào rừng sâu',
      charId: 'hunter',
      type: 'dialogue',
      avatar: 'assets/chars/avatar_mob.png',
      leftSprite: 'assets/chars/char_mob.png',
      centerSprite: null,
      rightSprite: null,
      speakerPos: 'left',
      bg: 'assets/covers/cover_isekai_gourmet.jpg',
      location: '🌲 RỪNG SÂU MÙ SƯƠNG • CỬA HIỆU SƠN MIÊU HIÊN KỲ BÍ',
      theme: 'theme-chuumon',
      jp: '「おや、こんな山奥に立派な西洋料理店があるぞ！『山猫軒・どなたもどうかお入りください』と書いてある」',
      vn: '"Kìa, giữa rừng sâu núi thẳm thế này mà lại có một nhà hàng Tây sang trọng! Biển hiệu đề: Nhà hàng Sơn Miêu - Xin mời bất cứ ai cũng đừng ngại bước vào."',
      grammarInsight: 'Cấu trúc [〜と書いてある]: Đang có ghi dòng chữ rằng... [どなたもどうかお入りください]: Lời mời kính ngữ trang trọng bậc nhất.',
      nuanceInsight: 'Vẻ ngoài hào nhoáng và hiếu khách của tiệm ăn phương Tây giữa chốn rừng sâu chính là cái bẫy tinh vi của loài mèo ma.',
      vocabTokens: [
        { word: '山奥', reading: 'やまおく', meaning: 'Sâu thẳm trong núi non', level: 'N3' },
        { word: '立派', reading: 'りっぱ', meaning: 'Lộng lẫy, tráng lệ', level: 'N4' },
        { word: 'どなたも', reading: 'donatamo', meaning: 'Bất kỳ ai (Kính ngữ của だれも)', level: 'N3' }
      ]
    },
    {
      speaker: '山猫軒の扉 (Lời nhắn trên cửa vàng)',
      role: '🚪 怪しき扉の指示 • Lời yêu cầu quái dị thứ 6',
      charId: 'chef',
      type: 'choice',
      avatar: 'assets/char_conductor.png',
      leftSprite: 'assets/chars/char_mob.png',
      centerSprite: 'assets/char_conductor.png',
      rightSprite: null,
      speakerPos: 'center',
      bg: 'assets/covers/cover_anime_cafe.jpg',
      location: '🍽️ PHÒNG KHÁCH SƠN MIÊU HIÊN • CÁNH CỬA SỐ 6 VÀ HŨ KEM TRẮNG',
      theme: 'theme-chuumon',
      jp: '「『壺の中のクリームを顔や手足にすっかり塗ってください』……なあ、これって本当に美味しい料理を食べるためなのか？」',
      vn: '"Hãy lấy kem trong hũ này bôi đều lên khắp mặt và tay chân... Này, việc này thực sự là để thưởng thức đồ ăn ngon sao?"',
      choices: [
        {
          textJp: '「指示に従い、顔や手足にクリームを塗りたくる」',
          textVn: 'Ngây thơ làm theo: Bôi đẫm kem béo lên khắp mặt và chân tay (+15 XP)',
          reactionText: 'Mùi bơ thơm lừng lan tỏa, phía sau cánh cửa vang lên tiếng mài dao xoèn xoẹt!',
          xp: 15
        },
        {
          textJp: '「何かおかしいと気付き、出口へ向かって脱出を図る！」',
          textVn: 'Nhận ra cạm bẫy: Quay đầu tìm cách thoát thân trong gang tấc! (+15 XP)',
          reactionText: 'Hai thợ săn giật bắn mình, hai con mắt mèo xanh lè sau khe cửa trừng trừng nhìn!',
          xp: 15
        }
      ],
      grammarInsight: 'Cấu trúc [〜にすっかり塗る]: Bôi trát toàn bộ không chừa chỗ nào. [壺]: Chiếc bình / hũ gốm đựng gia vị.',
      nuanceInsight: 'Nghệ thuật châm biếm sâu sắc của Miyazawa Kenji: Con người thành thị săn bắn thú rừng để tiêu khiển, nay lại chính là món mồi ngon được tẩm ướp gia vị.',
      vocabTokens: [
        { word: '壺', reading: 'つぼ', meaning: 'Hũ, bình gốm', level: 'N1' },
        { word: 'すっかり', reading: 'sukkari', meaning: 'Toàn bộ, hoàn toàn', level: 'N3' },
        { word: '塗る', reading: 'ぬる', meaning: 'Bôi, thoa lên bề mặt', level: 'N3' }
      ]
    }
  ]
};

// Now read web-preview/js/data.js and replace the section
let dataJs = fs.readFileSync('web-preview/js/data.js', 'utf8');

const startMarker = '// Visual Novel Registry: Story & Scene Definitions';
const endMarker = '// Satori Reader Sentence Analysis Sample Data';

const startIdx = dataJs.indexOf(startMarker);
const endIdx = dataJs.indexOf(endMarker);

if (startIdx === -1 || endIdx === -1) {
  console.error('Could not locate markers in data.js:', { startIdx, endIdx });
  process.exit(1);
}

const newVnSection = `${startMarker}
const vnStoryRegistry = ${JSON.stringify(vnStoryRegistry, null, 2)};

// Visual Novel Dialogues: Curated Multi-Scene Dialogues
const vnDialoguesByBook = ${JSON.stringify(vnDialoguesByBook, null, 2)};\n\n`;

dataJs = dataJs.substring(0, startIdx) + newVnSection + dataJs.substring(endIdx);
fs.writeFileSync('web-preview/js/data.js', dataJs, 'utf8');
console.log('✓ Successfully injected enhanced VN 2.0 stories into web-preview/js/data.js!');

