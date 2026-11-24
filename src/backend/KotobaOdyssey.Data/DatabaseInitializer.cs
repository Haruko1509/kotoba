using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KotobaOdyssey.Data;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly KotobaDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(KotobaDbContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring SQLite database exists and schema is up to date...");
            await _context.Database.EnsureCreatedAsync();

            try
            {
                await _context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
            }
            catch (Exception walEx)
            {
                _logger.LogWarning(walEx, "Could not set SQLite WAL journal mode.");
            }

            await SeedDataAsync();
            _logger.LogInformation("Database initialization and seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        // 1. Seed User Progress if none exists
        if (!await _context.UserProgresses.AnyAsync())
        {
            var progress = new UserProgress
            {
                UserName = "旅人 (Traveler)",
                Title = "Starlight Apprentice",
                Level = 3,
                CurrentXp = 160,
                RequiredXp = 390,
                StreakDays = 5,
                LastStudyDate = DateTime.UtcNow.Date,
                TodayStudyMinutes = 25,
                TotalStudyMinutes = 410,
                TotalWordsLearned = 48,
                TotalChaptersRead = 6
            };
            _context.UserProgresses.Add(progress);
        }

        // 2. Seed Daily Quests for today
        var today = DateTime.UtcNow.Date;
        if (!await _context.DailyQuests.AnyAsync(q => q.QuestDate == today))
        {
            _context.DailyQuests.AddRange(
                new DailyQuest
                {
                    Title = "Starlight Reading",
                    Description = "Đọc truyện tiếng Nhật ít nhất 15 phút hôm nay.",
                    Type = QuestType.ReadMinutes,
                    TargetCount = 15,
                    CurrentCount = 15,
                    RewardXp = 50,
                    IsClaimed = false,
                    QuestDate = today
                },
                new DailyQuest
                {
                    Title = "Memory Echoes",
                    Description = "Ôn tập 10 thẻ từ vựng đến hạn.",
                    Type = QuestType.ReviewCards,
                    TargetCount = 10,
                    CurrentCount = 8,
                    RewardXp = 60,
                    IsClaimed = false,
                    QuestDate = today
                },
                new DailyQuest
                {
                    Title = "Lexicon Explorer",
                    Description = "Lưu 3 từ vựng mới vào kho từ cá nhân.",
                    Type = QuestType.AddVocabulary,
                    TargetCount = 3,
                    CurrentCount = 3,
                    RewardXp = 40,
                    IsClaimed = true,
                    QuestDate = today
                }
            );
        }

        // 3. Seed Achievements
        if (!await _context.Achievements.AnyAsync())
        {
            _context.Achievements.AddRange(
                new Achievement
                {
                    Title = "First Odyssey Step",
                    Description = "Đọc xong chương truyện đầu tiên.",
                    IconKey = "BookOpen",
                    IsUnlocked = true,
                    UnlockedAt = DateTime.UtcNow.AddDays(-4),
                    RewardXp = 50
                },
                new Achievement
                {
                    Title = "Constellation Scholar",
                    Description = "Đạt chuỗi học tập 5 ngày liên tiếp.",
                    IconKey = "Flame",
                    IsUnlocked = true,
                    UnlockedAt = DateTime.UtcNow.AddDays(-1),
                    RewardXp = 80
                },
                new Achievement
                {
                    Title = "Silver Lexicon",
                    Description = "Lưu trên 50 từ vựng vào bộ sưu tập.",
                    IconKey = "Star",
                    IsUnlocked = false,
                    RewardXp = 100
                },
                new Achievement
                {
                    Title = "Cosmic Storyteller",
                    Description = "Hoàn thành một tiểu thuyết dạng Visual Novel.",
                    IconKey = "Trophy",
                    IsUnlocked = false,
                    RewardXp = 150
                }
            );
        }

        // 4. Seed / Expand Novels & Chapters
        var existingNovels = await _context.Novels.Include(n => n.Chapters).ToListAsync();

        // 4.1. 銀河鉄道の夜
        var ginga = existingNovels.FirstOrDefault(n => n.Title.Contains("銀河鉄道"));
        if (ginga == null)
        {
            ginga = new Novel
            {
                Title = "銀河鉄道の夜",
                JapaneseTitle = "銀河鉄道の夜 (Ginga Tetsudou no Yoru)",
                RomajiTitle = "Night on the Galactic Railroad",
                Author = "宮沢賢治 (Miyazawa Kenji)",
                Genre = "Cosmic Fantasy",
                Description = "Tác phẩm kinh điển huyền ảo của Miyazawa Kenji. Cậu bé Giovanni bước lên chuyến tàu lửa ngân hà chạy xuyên qua các chòm sao kỳ vĩ trên bầu trời đêm cùng người bạn thân Campanella.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_ginga.png",
                JlptLevel = JlptLevel.N3,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                LastReadAt = DateTime.UtcNow.AddMinutes(-30)
            };
            _context.Novels.Add(ginga);
        }

        EnsureChapter(ginga, 1, "一、午後の授業 (Giờ học buổi chiều)", 420,
            @"「ではみなさんは、そういうふうに川だと云われたり、乳の流れたあとだと云われたりしていたこのぼんやりと白いものがほんとうは何かご承知ですか」
先生は、黒板に吊した大きな黒い星座の図の、上から下へ白くけむった銀河の帯のようなところを指しながら、みんなに問いをかけました。
カムパネルラが手をあげました。それから四五人手をあげました。ジョバンニも手をあげようとして、急いでそのままやめました。たしかにあれがみんな星だと、いつか雑誌で読んだのでしたが、このごろはジョバンニはまるで毎日教室でもねむく、本を読むひまも読む本もないので、なんだかそんなことはみんなうすれてしまったように思われたのでした。
ところが先生は早くもそれを見附けたのでした。
「ジョバンニさん。あなたはわかっているのでしょう」
ジョバンニは勢いよく立ちあがりましたが、立って見るともうはっきりとそれを答えることができませんでした。");

        EnsureChapter(ginga, 2, "二、活版所 (Xưởng in)", 350,
            @"ジョバンニは学校の門を出るとき、同じ組の七八人は家へ帰らずカムパネルラをまん中にして校庭の隅の桜の木のところに集まっていました。今夜の星祭に青いあかりを網にして川へ流す烏瓜の相談をしているのでした。
ジョバンニは手を大きく振って町の方へ歩きました。
町の家々はもうあかりがついて、店々では今夜の祭のために色とりどりの旗を吊したり、銀の紙のすずらんを飾ったりしていました。
ジョバンニはある大きな活版所に入って行きました。入口の計算台にすわっている白い上着を着た人が、ジョバンニを見ると紙切れを一枚渡しました。");

        EnsureChapter(ginga, 3, "三、家 (Gia đình và người mẹ)", 460,
            @"ジョバンニが口笛を吹きながら、暗い路地を通って小さなわが家へ帰ってくると、入口にはまだランプがついていませんでした。
「お母さん、いま帰ったよ」
「おや、ジョバンニ、お帰り。今日はいいお天気でよかったね」
病気で臥せっている母のやさしい声が、奥の部屋から聞こえてきました。
「お母さん、牛乳はまだ来ないの？」
「ああ、まだなんだよ。牛乳屋さんの都合が悪いのかしらね」
「僕、いまから取りに行ってくるよ。ついでにケンタウル祭の烏瓜を川へ流すのを見てくる」
ジョバンニは母のために毛布を直してあげると、元気に家をとび出しました。");

        EnsureChapter(ginga, 4, "四、ケンタウル祭の夜 (Đêm lễ hội Centaurus)", 480,
            @"町はいつのまにか銀河のお祭りの光でいっぱいになっていました。
家々の軒下には青や赤やすみれ色のあかりが揺れ、子供たちはみんな新しい着物を着て、楽しそうに笑い声をあげて歩いていました。
川の岸辺では、烏瓜のあかりを水に浮かべる準備が進んでいました。
ジョバンニは牛乳屋へ向かう途中、橋のたもとで偶然カムパネルラの一行に出会いました。
ザネリが大きな声で言いました。
「ジョバンニ、らっこの上着が来るよ！」
ジョバンニは胸が熱くなって、思わず走り出しました。カムパネルラは申し訳なさそうに、黙ってジョバンニを見つめていました。");

        EnsureChapter(ginga, 5, "五、天気輪の柱 (Cột trụ thời tiết trên đồi)", 520,
            @"ジョバンニは町をはなれて、黒い丘の頂上へ登って行きました。
丘の上には風が冷たく吹き抜けて、草の波がさらさらと音を立てていました。
頂上には、一本の細長い天気輪の柱が立っていました。
ジョバンニは冷たい草の上に寝ころんで、星のきらめく夜空を見上げました。
黒い空のまん中を、銀河がまるで巨大な光の帯のように流れていました。
「ああ、僕のお父さんはきっと帰ってくる。お母さんもきっとよくなるんだ」
そのとき、どこからともなく、不思議な鐘の音が響きわたりました。
『銀河ステーション、銀河ステーション……』
あたり一面が、息をのむような青い光に包まれました。");

        EnsureChapter(ginga, 6, "六、銀河ステーション (Ga Ngân Hà)", 580,
            @"気がつくと、ジョバンニはごうごうと音を立てて走る小さな列車の中にすわっていました。
窓の外には、淡い天の川の銀色の水がどこまでも広がり、川底の砂利はすべて水晶やトパーズのように輝いていました。
「カムパネルラ！」
ジョバンニが叫ぶと、前の座席にぬれた黒い上着を着たカムパネルラが静かにすわっていました。
「ジョバンニ、僕たちとうとう一緒になったね」
カムパネルラは窓の外を指さしました。
「見てごらん。あの向こうの岸に立っている十字架の塔、まるでサファイアのようだね」
こうして二人の少年を乗せた銀河鉄道は、永遠の星空をめざして走り出したのでした。");

        // 4.2. 坊っちゃん
        var botchan = existingNovels.FirstOrDefault(n => n.Title.Contains("坊っちゃん"));
        if (botchan == null)
        {
            botchan = new Novel
            {
                Title = "坊っちゃん",
                JapaneseTitle = "坊っちゃん (Botchan)",
                RomajiTitle = "Master Darling",
                Author = "夏目漱石 (Natsume Soseki)",
                Genre = "School / Classic Drama",
                Description = "Tiểu thuyết châm biếm nổi tiếng của Natsume Soseki kể về một thầy giáo trẻ tính tình bộc trực, nóng nảy nhưng trọng chính nghĩa từ Tokyo đến dạy học tại trường trung học ở Shikoku.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_botchan.png",
                JlptLevel = JlptLevel.N4,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastReadAt = DateTime.UtcNow.AddDays(-2)
            };
            _context.Novels.Add(botchan);
        }

        EnsureChapter(botchan, 1, "第一章 (Bản tính bạt mạng thuở nhỏ)", 510,
            @"親譲りの無鉄砲で小供の時から損ばかりしている。小学校に居る時分学校の二階から飛び降りて一週間ほど腰を抜かした事がある。なぜそんな無闇をしたと聞く人があるかも知れぬ。別段深い理由でもない。新築の二階から首を出していたら、同級生の一人が冗談に、いくら威張っても、そこから飛び降りる事は出来まい。弱虫やーい。と囃したからである。小使に負ぶさって帰って来た時、おやじが大きな眼をして二階ぐらいから飛び降りて腰を抜かす奴があるかと云ったから、この次は抜かさずに飛んで見せますと答えた。");

        EnsureChapter(botchan, 2, "第二章 (Lên đường nhận việc tại Shikoku)", 470,
            @"物理学校を卒業してから八日目に、校長から四国の中学校へ数学の教師として行かないかと言われた。
月給は四十円だという。
おれは東京を離れたこともないし、四国が日本のどこにあるのかもよく知らなかったが、「行きましょう」と即座に引き受けた。
長年仕えてくれた下女の清（きよ）は、おれが遠い田舎へ行くのを聞いて大層悲しんだ。
「坊っちゃん、どうかお体をお大事になすって、早く立派になって東京へ帰ってきておくれ」
清はおれの荷物に、越後の笹飴と手作りの守り袋をそっと忍ばせてくれた。");

        EnsureChapter(botchan, 3, "第三章 (Biệt danh nơi phòng giáo viên)", 530,
            @"中学校へ着いて職員室へ行くと、実に奇妙な連中ばかりがそろっていた。
校長はまるで狸のような顔をして愛想笑いをしているので『狸』とあだ名をつけた。
教頭は年中赤いフランネルのシャツを着て偉そうにしているので『赤シャツ』だ。
英語の教師は青白い顔をしていつも弱々しい声を出しているから『うらなり』。
そして数学の主任教師は、頑丈な体をしていていが栗頭なので『山嵐』と呼ぶことにした。
この山嵐とおれは最初から馬が合いそうだったが、赤シャツの胡散臭い気取り屋ぶりには反吐が出そうだった。");

        // 4.3. ごんぎつね
        var gon = existingNovels.FirstOrDefault(n => n.Title.Contains("ごんぎつね"));
        if (gon == null)
        {
            gon = new Novel
            {
                Title = "ごんぎつね",
                JapaneseTitle = "ごんぎつね (Gongitsune)",
                RomajiTitle = "Gon, the Little Fox",
                Author = "新美南吉 (Niimi Nankichi)",
                Genre = "Folktale / Touching",
                Description = "Câu chuyện cổ tích cảm động sâu sắc về chú cáo nhỏ tinh nghịch Gongitsune tìm cách chuộc lỗi với anh nông dân Hyoju bằng những món quà vô danh từ rừng sâu.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_gon.png",
                JlptLevel = JlptLevel.N5,
                IsFavorite = false,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                LastReadAt = null
            };
            _context.Novels.Add(gon);
        }

        EnsureChapter(gon, 1, "第一話 (Chú cáo nhỏ cô đơn và nghịch ngợm)", 380,
            @"これは、わたしが小さいときに、村の茂平というおじいさんからきいたお話です。
むかしは、私たちの村のちかくの、中山というところに小さなお城があって、中山さまというおとのさまがおられたそうです。
その中山から、すこしはなれた山の中に、「ごんぎつね」という、いたずら好きの小ぎつねがいました。ごんは、ひとりぼっちのこぎつねで、しだのいっぱいしげった森の中に穴をほって住んでいました。そして、夜でも昼でも、ちかくの村へ出てきて、いたずらばかりしました。");

        EnsureChapter(gon, 2, "第二話 (Nỗi ân hận trước đám tang)", 420,
            @"秋の雨が二三日降りつづいたあとの、ある日のことでした。
ごんが森から出て村の小川のそばを通ると、兵十の家の前で村の人々が集まっていました。
「何かあったのかな」
ごんが木陰からこっそりのぞいてみると、兵十のお母さんの葬式が行われているところでした。
兵十は白い着物を着て、位牌をかかげて涙を流していました。
ごんは胸が痛くなりました。
「ああ、おれが川でウナギをとっていたずらしたから、兵十のおっ母さんはウナギを食べられずに死んでしまったのかもしれない……おれはなんて悪いことをしてしまったんだろう」");

        EnsureChapter(gon, 3, "第三話 (Những món quà thầm lặng từ rừng sâu)", 460,
            @"それからというもの、ごんは毎日山へ行って、栗や松茸をどっさり拾ってきました。
そして兵十が畑に出かけているすきに、そっと兵十の家の物置や土間に置いて帰るのでした。
兵十はある朝、栗が山のように積んであるのを見て不思議そうに首をかしげました。
「いったい誰が、こんな立派な栗や松茸を毎日届けてくれるんだろう。神様が恵んでくださっているに違いない」
ごんは物陰からその様子を見て、うれしくてたまりませんでした。
「神様じゃないよ、おいら、ごんなんだよ」と、心の中でつぶやきました。");

        // 4.4. 走れメロス (New Classic Novel: Run, Melos!)
        var melos = existingNovels.FirstOrDefault(n => n.Title.Contains("走れメロス") || n.Title.Contains("メロス"));
        if (melos == null)
        {
            melos = new Novel
            {
                Title = "走れメロス",
                JapaneseTitle = "走れメロス (Hashire Melos)",
                RomajiTitle = "Run, Melos!",
                Author = "太宰治 (Dazai Osamu)",
                Genre = "Heroic / Friendship",
                Description = "Kiệt tác bất hủ của Dazai Osamu về lòng quả cảm, đức hy sinh và niềm tin tuyệt đối vào tình bạn. Chàng mục đồng Melos chạy đua với tử thần để giữ lời hẹn ước cứu lấy người bạn thân.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_melos.png",
                JlptLevel = JlptLevel.N3,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                LastReadAt = DateTime.UtcNow.AddHours(-1)
            };
            _context.Novels.Add(melos);
        }

        EnsureChapter(melos, 1, "第一章：激怒と決意 (Sự phẫn nộ và quyết tâm)", 540,
            @"メロスは激怒した。必ず、かの邪悪の王を除かなければならぬと決意した。
メロスには政治がわからぬ。メロスは、村の牧人である。笛を吹き、羊と遊んで暮して来た。けれども邪悪に対しては、人一倍に敏感であった。
きょう未明メロスは村を出発し、野を越え山越え、十里はなれた此のシラクスの市にやって来た。
町を歩いてみると、人々の様子がおかしい。ひっそりとして、誰もが怯えている。
メロスはある老爺をつかまえて尋ねた。
「王様は、なぜ人々を殺すのか」
「人を信ずることができぬ、というのでございます。王は孤独で、自分の心をも疑っておられるのです」
メロスは怒りに胸を震わせ、王城へとまっすぐに乗り込んで行った。");

        EnsureChapter(melos, 2, "第二章：人質と三日間の約束 (Con tin và lời thề ước)", 510,
            @"王ディオニスは冷酷に笑った。
「処刑の前に、妹の結婚式を挙げてやりたいだと？ 戻ってくるはずがない。人間は嘘をつく生き物だ」
メロスは叫んだ。
「私には友がいる。石工のセリヌンティウスだ。彼を人質としてここに残そう。私が逃げたら彼を殺すがいい！」
こうして親友セリヌンティウスが捕らえられ、メロスは三日間の猶予を与えられて村へ駆け戻った。
妹の結婚式を無事に終えたメロスは、三日目の明け方、再び走り出した。
「待っていろ、セリヌンティウス！ 私は必ず戻る！」");

        EnsureChapter(melos, 3, "第三章：走れ、メロス！ (Chạy đi, Melos!)", 560,
            @"天候は悪化し、濁流が川の橋を押し流していた。
メロスは激流に飛び込み、腕一本で対岸へと渡り切った。
山道では山賊の襲撃を受けたが、死力を尽くして打ち倒した。
しかし真夏の太陽が照りつけ、メロスの体は極限に達した。喉は焼け、足は鉛のように重い。
「もうだめだ……ここまでか……」
草むらに倒れ込んだメロスの耳に、岩の裂け目からかすかに水の湧く音が聞こえた。
冷たい清水を一口飲むと、消えかけていた命の火が再び燃え上がった。
「信実を守るのだ！ 友との絆を証明するのだ！」
メロスは風のように大地を蹴り、沈みゆく夕日をめがけて走り続けた。");

        // 4.5. 羅生門 (Rashomon)
        var rashomon = existingNovels.FirstOrDefault(n => n.Title.Contains("羅生門"));
        if (rashomon == null)
        {
            rashomon = new Novel
            {
                Title = "羅生門",
                JapaneseTitle = "羅生門 (Rashomon)",
                RomajiTitle = "Rashomon Gate",
                Author = "芥川龍之介 (Akutagawa Ryunosuke)",
                Genre = "Dark Literature / Classic",
                Description = "Tác phẩm tâm lý văn học kinh điển của Akutagawa Ryunosuke tại kinh thành Kyoto hoang tàn, đào sâu vào bản ngã con người giữa ranh giới sinh tồn và đạo đức.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_rashomon.png",
                JlptLevel = JlptLevel.N2,
                IsFavorite = false,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastReadAt = null
            };
            _context.Novels.Add(rashomon);
        }

        EnsureChapter(rashomon, 1, "第一章：暮れ方の雨 (Mưa chiều dưới cổng La Sinh Môn)", 480,
            @"ある日の暮れ方の事である。一人の下人が、羅生門の下で雨やみを待っていた。
広い門の下には、この男のほかに誰もいない。ただ、所々丹塗の剥げた、大きな円柱に、蟋蟀が一匹とまっている。
京都の町はここ数年、辻風や火災や飢饉のために、荒れ果てるばかりであった。
下人は、数日前に仕えていた主人から暇を出されたばかりで、寝泊まりする所も今宵の糧もない。
「盗人になるよりほかに道はない……」
雨は夕闇の中に降りしきり、下人は思い悩みながら、上の楼へと続く階段へ足をかけた。");

        EnsureChapter(rashomon, 2, "第二章：楼の上の老婆 (Bà lão nhổ tóc trên gác cổng)", 520,
            @"下人は、梯子の途中に身をひそめて、上の様子をうかがった。
楼の上には、死骸が無造作に転がっており、その死臭が漂っている。
そして、その中に一人の白髪の老婆がかがみこみ、松明の火に照らされながら、死人の頭から一本ずつ髪の毛を抜いていた。
下人は、激しい恐怖と激しい憎悪を覚えた。
「あらゆる悪に対する反感」が、下人の胸に燃え上がった。
下人は刀に手をかけ、大股に楼の上へ踏み出した。
「己、何をしている！」");

        EnsureChapter(rashomon, 3, "第三章：下人の行方 (Sự lựa chọn và bóng tối)", 550,
            @"老婆は震え上がりながら答えた。
「この抜いた髪で鬘（かつら）を作って売るのじゃ。この死んだ女も、蛇を干して魚と偽って売っておった。生きるためには仕方のないことじゃ」
老婆の言葉を聞くうちに、下人の心に変化が起こった。
善悪の迷いが消え、冷酷な勇気が湧き出してきた。
「そうか、生きるためなら何をしても許されるのだな」
下人は老婆の首根っこを締め上げ、着ていた着物を剥ぎ取った。
「ならば、己の着物を奪うのも、生きるためだ。恨むなよ」
下人は黒い夜の闇の中へ、疾風のように駆け下りて行った。下人の行方は、誰も知らない。");

        // 4.6. 雪国 (Snow Country - Kawabata Yasunari)
        var yukiguni = existingNovels.FirstOrDefault(n => n.Title.Contains("雪国"));
        if (yukiguni == null)
        {
            yukiguni = new Novel
            {
                Title = "雪国",
                JapaneseTitle = "雪国 (Yukiguni)",
                RomajiTitle = "Snow Country",
                Author = "川端康成 (Kawabata Yasunari)",
                Genre = "Poetic / Nobel Masterpiece",
                Description = "Kiệt tác văn học đạt giải Nobel của Kawabata Yasunari. Khắc họa vẻ đẹp u hoài, mong manh của tình yêu giữa chàng lãng tử Shimamura và nàng geisha Komako nơi suối nước nóng ngập tràn tuyết trắng.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_yukiguni.png",
                JlptLevel = JlptLevel.N2,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                LastReadAt = null
            };
            _context.Novels.Add(yukiguni);
        }

        EnsureChapter(yukiguni, 1, "第一章：国境の長いトンネル (Đường hầm ranh giới tỉnh)", 490,
            @"国境の長いトンネルを抜けると雪国であった。夜の底が白くなった。信号所に汽車が止まった。
向側の座席から娘が立って来て、島村の前のガラス窓を落とした。雪の冷気が流れこんだ。
娘は窓いっぱいに乗り出して、遠くへ叫ぶように、
「駅長さあん、駅長さあん」
明かりをさげて、雪を踏んで来た男は、襟巻で鼻の上まで包み、耳に帽子の毛を垂れていた。
島村は、鏡にうつる夕景の中で、その娘の瞳の美しさに心を奪われていた。");

        EnsureChapter(yukiguni, 2, "第二章：鏡の中の夕景 (Nàng Komako và tiếng đàn sầu)", 530,
            @"温泉宿の静かな障子を開けると、外は一面の銀世界であった。
芸者の駒子は、白く化粧した首筋を微かに震わせながら、島村の部屋を訪れた。
「島村さん、また来てくださったのね」
駒子の指先は氷のように冷たかったが、その瞳には熱い情熱が宿っていた。
彼女が三味線を爪弾くと、澄んだ音が雪の積もった夜の空気に凛と響き渡った。
「私、日記をずっとつけているの。あなたが東京へ帰ったあとも、毎日……」
虚無感を抱える島村の心に、駒子の哀しくも純粋な生が深く沁み入った。");

        EnsureChapter(yukiguni, 3, "第三章：銀河と雪の炎 (Dải Ngân Hà và ánh lửa trắng)", 540,
            @"ある夜、町はずれの繭倉庫で火災が起こった。
空を見上げると、巨大な天の川が、凍りついた夜空を白く照らし出しながら、まるで島村の体の中へ流れ落ちてくるようであった。
「天の川だ……」
島村は息をのんだ。天の川の光が雪の野原を青く染め、遠くの火事の炎が赤く揺らめいている。
群衆の中で、駒子が狂おしく叫びながら誰かを助け出そうとしていた。
冷酷な美しさと生への渇望が、雪国の夜空の下で一つの幻影となって溶け合っていた。");

        // 4.7. 蜘蛛の糸 (The Spider's Thread - Akutagawa Ryunosuke)
        var spider = existingNovels.FirstOrDefault(n => n.Title.Contains("蜘蛛の糸"));
        if (spider == null)
        {
            spider = new Novel
            {
                Title = "蜘蛛の糸",
                JapaneseTitle = "蜘蛛の糸 (Kumo no Ito)",
                RomajiTitle = "The Spider's Thread",
                Author = "芥川龍之介 (Akutagawa Ryunosuke)",
                Genre = "Buddhist Parable / Classic",
                Description = "Truyện ngắn ngụ ngôn triết lý kinh điển kể về cơ hội giải thoát duy nhất của đại ác nhân Kandata từ địa ngục bằng một sợi tơ nhện mong manh thả xuống từ Cực Lạc.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_spider.png",
                JlptLevel = JlptLevel.N3,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastReadAt = null
            };
            _context.Novels.Add(spider);
        }

        EnsureChapter(spider, 1, "第一章：極楽の蓮池 (Hồ sen cõi Cực Lạc)", 420,
            @"ある日の事でございます。御釈迦様は極楽の蓮池のふちを、独りでぶらぶら御歩きになっていらっしゃいました。
池の中に咲いている蓮の花は、みんな玉のようにまっ白で、真中にある金色の蕊からは、何とも云えない好い匂いが、絶間なくあたりへ溢れて居ります。
極楽は丁度朝でございましょう。
やがて御釈迦様はその池のふちに御佇みになって、水の面を蔽っている蓮の葉の間から、ふと下の様子を御覧になりました。
この極楽の蓮池の下は、丁度地獄の底に当って居ります。");

        EnsureChapter(spider, 2, "第二章：血の池と銀の糸 (Biển máu và sợi tơ bạc)", 470,
            @"地獄の底を覗き見ますと、犍陀多（カンダタ）という男が、ほかの罪人と一緒に蠢いているのが見えました。
このカンダタは、人を殺したり家に火をつけたりした大泥棒でございますが、生前、道端の小さな蜘蛛を踏み殺さずに助けてやったことがありました。
御釈迦様は、その善行を思い出され、地獄から救い出してやろうとお考えになりました。
ふと見ますと、翡翠の葉の上に、一匹の蜘蛛が美しい銀色の糸をかけております。
御釈迦様はその蜘蛛の糸をそっと御手に取って、はるか下界の地獄の底を目がけて、まっすぐに降ろされました。");

        EnsureChapter(spider, 3, "第三章：切れた糸 (Lòng vị kỷ và sợi tơ đứt đoạn)", 520,
            @"カンダタは血の池の暗闇の中で、天井から銀の糸がするすると降りてくるのを見つけました。
「しめた！ この糸をよじ登れば、地獄から抜け出せるぞ！」
カンダタは必死になって蜘蛛の糸を手繰り登りました。
ふと下を見下ろすと、何百何千という罪人たちが、自分の後をつけて登ってくるではありませんか。
「こら、罪人ども！ この蜘蛛の糸はおれのものだ！ 誰に聞いて登ってきた！ 降りろ、降りろ！」
その途端でございます。糸はカンダタの握っているところから、ぷつりと音を立てて切れました。
カンダタはあっという間に、暗黒の地獄の底へ真っ逆さまに落ちて行きました。
御釈迦様は悲しそうな顔配りで、蓮池のふちから静かに歩み去られました。");

        // 4.8. こころ (Kokoro - Natsume Soseki)
        var kokoro = existingNovels.FirstOrDefault(n => n.Title.Contains("こころ"));
        if (kokoro == null)
        {
            kokoro = new Novel
            {
                Title = "こころ",
                JapaneseTitle = "こころ (Kokoro)",
                RomajiTitle = "Kokoro (Heart)",
                Author = "夏目漱石 (Natsume Soseki)",
                Genre = "Psychological / Classic",
                Description = "Kiệt tác tiểu thuyết tâm lý sâu sắc nhất của văn học cận đại Nhật Bản, khắc họa nỗi cô đơn định mệnh và bí mật u tối đằng sau vẻ điềm tĩnh của Tiên sinh.",
                CoverImagePath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/cover_kokoro.png",
                JlptLevel = JlptLevel.N2,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                LastReadAt = null
            };
            _context.Novels.Add(kokoro);
        }

        EnsureChapter(kokoro, 1, "第一章：先生と私 (Tiên sinh và tôi tại Kamakura)", 510,
            @"私はその人を常に先生と呼んでいた。だからここでもただ先生と書くだけで本名は明かさない。
私が先生と知り合ったのは鎌倉であった。
その時私はまだ若々しい学生であった。暑中休暇を利用して友達に誘われて海へ泳ぎに行ったのである。
由比ヶ浜の砂浜で、私は毎日海に入っていたが、ある日西洋人と一緒に波に入って行く一人の日本人を見かけた。
それが先生であった。
先生の姿には、どこか世間を避けているような、近寄りがたい寂しさが漂っていた。
私はなぜかその姿に強く惹かれ、やがて先生の東京の家を訪ねるようになったのである。");

        EnsureChapter(kokoro, 2, "第二章：雑司ヶ谷の墓地 (Nghĩa trang Zoshigaya và bí mật)", 540,
            @"先生は毎月一度、決まって雑司ヶ谷の墓地へ墓参りに出かけていた。
私はある日、その墓地で偶然先生にお会いした。
「先生、どなたのお墓なのですか」
私が尋ねると、先生は暗い影をその顔に浮かべて言った。
「友人の墓だよ。君、君にはまだ死というものの意味がわからないだろうね」
先生は決して友人の名を明かそうとはしなかった。
美しい奥さんと二人で静かに暮らしている先生の胸の奥底には、誰にも触れさせない深い孤独と後悔が横たわっていた。
「人間はね、いざという時には誰でも悪人になるものだよ」
先生の言葉が、私の胸に重く突き刺さった。");

        // 4.9. 注文の多い料理店 (The Restaurant of Many Orders - Miyazawa Kenji)
        var chuumon = existingNovels.FirstOrDefault(n => n.Title.Contains("注文の多い料理店"));
        if (chuumon == null)
        {
            chuumon = new Novel
            {
                Title = "注文の多い料理店",
                JapaneseTitle = "注文の多い料理店 (Chuumon no Ooi Ryouriten)",
                RomajiTitle = "The Restaurant of Many Orders",
                Author = "宮沢賢治 (Miyazawa Kenji)",
                Genre = "Cosmic Mystery / Dark Fairy Tale",
                Description = "Tác phẩm trào phúng kinh điển của Miyazawa Kenji. Hai chàng thanh niên quý tộc thành thị vào rừng sâu săn bắn, bất ngờ lạc vào quán ăn Tây phương kỳ lạ mang tên 'Sơn Miêu Hiên', nơi những vị khách ngỡ ngàng nhận ra chính họ mới là món ăn chuẩn bị lên bàn.",
                CoverImagePath = "assets/covers/cover_isekai_gourmet.jpg",
                JlptLevel = JlptLevel.N3,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastReadAt = null
            };
            _context.Novels.Add(chuumon);
        }

        EnsureChapter(chuumon, 1, "第一章：山奥の山猫軒 (Quán ăn Sơn Miêu Hiên giữa rừng sâu)", 450,
            @"二人の若い紳士が、すっかりイギリスの兵隊の形をして、ぴかぴかする鉄砲をかついで、白くまのような大きな犬を二匹連れて、だいぶ山奥の木の葉のかさかさしたところを歩いていました。
「ぜんたい、ここらは怪しからん山だね。鳥も獣も一ぴきもいやがらん。なんでも構わないから、早く何かを撃ってみたいもんだな」
案内人の猟師もどこかへ逃げてしまい、連れていた二匹の白犬も寒さのために倒れて死んでしまいました。
二人はお腹が空いて、途方に暮れながら風の吹きすさぶ薄暗い森を進みました。
そのとき、ふと前を見ると、立派な白い煉瓦造りの西洋館が建っていました。
玄関には青いガラスの看板が掲げてありました。
『西洋料理店　山猫軒』
二人は顔を見合わせて大喜びしました。
「おい、こんな山奥に西洋料理店があるぞ！ ここでうまいものでも食べて暖まろう！」");

        EnsureChapter(chuumon, 2, "第二章：次々と重なる注文 (Những yêu cầu kỳ lạ bên cánh cửa)", 480,
            @"扉をあけて中へ入ると、そこは明るい廊下になっていて、次の扉の金色の文字でこう書いてありました。
『当軒は注文の多い料理店ですからどうかそこはご承知ください』
「へえ、注文が多いというのは、客がたくさん来て繁盛しているという意味だろう」
二人は頷き合ってさらに奥へと進みました。
ところが、次の青い扉には不思議なことが書いてありました。
『鉄砲と弾丸をここに置いてください』
二人は鉄砲を棚に置きました。
その奥の扉には、
『どうか帽子と外套と靴をお脱ぎください』
『ネクタイピン、カフスボタン、眼鏡、財布、その他金属のものはすべてこの台の上にお置きください』
二人は不思議に思いながらも、言われた通りにすべてを外して身軽になりました。
「料理の電気仕掛けの火鉢に金属が触ると危ないからだろうね」
二人は自分たちに都合の良いように解釈しながら、笑い合って次の扉を開けました。");

        EnsureChapter(chuumon, 3, "第三章：塩とクリームの罠 (Cái bẫy muối giấm và sự thật kinh hoàng)", 520,
            @"次の扉の前に来ると、そこにはガラスの壺と壺が置いてあり、札が立っていました。
『壺の中のクリームを、顔や手足にすっかり塗り込んでください』
「おいおい、どうしてクリームなんか塗るんだい？」
「部屋の中が暖かくて皮膚がひび割れないようにという、店側の親切な心遣いだよ」
二人は壺のクリームを顔から耳の後ろまでたっぷりと塗り合いました。
ところが、その次の扉にはこう書いてあったのです。
『壺の中の塩と酢を、残らず頭からよく揉み込んでください』
二人は青ざめました。
「おい……塩と酢を揉み込めって……これじゃあまるで……」
「僕たちがお客として料理を食べるんじゃなくて……僕たちが料理されて食べられる側なんじゃないか！？」
ガタガタと震えながら後ろを振り返ると、今通ってきたはずの扉はすべて固く閉ざされ、二度と開かなくなっていました。
そして奥の扉の鍵穴から、爛々と光る青い二対の目がじっと二人を見つめていたのでした。");

        // 4.10. 幼馴染の妹の家庭教師を始めたら (Tutoring My Childhood Friend's Sister - Aisaka)
        var katei = existingNovels.FirstOrDefault(n => n.Title.Contains("家庭教師"));
        if (katei == null)
        {
            katei = new Novel
            {
                Title = "幼馴染の妹の家庭教師を始めたら",
                JapaneseTitle = "幼馴染の妹の家庭教師を始めたら (Osananajimi no Imouto no Kateikyoushi)",
                RomajiTitle = "Tutoring My Childhood Friend's Sister",
                Author = "逢坂 (Aisaka)",
                Genre = "School Life / Romantic Comedy",
                Description = "Light Novel học đường nhẹ nhàng và gần gũi. Nam sinh trung học nhận lời làm gia sư phụ đạo tiếng Nhật và toán học cho cô em gái nghịch ngợm nhưng thông minh của người bạn thân thanh mai trúc mã.",
                CoverImagePath = "assets/covers/cover_school_romcom.jpg",
                JlptLevel = JlptLevel.N4,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                LastReadAt = null
            };
            _context.Novels.Add(katei);
        }

        EnsureChapter(katei, 1, "第一章：思いがけない依頼 (Lời đề nghị bất ngờ sau giờ học)", 390,
            @"放課後の静かな教室で、幼馴染の亮太が真剣な顔で僕の席の前に立った。
「頼む！ お前の頭脳を貸してくれ！ うちの妹の家庭教師をやってほしいんだ！」
「は？ お前の妹って、あの生意気盛りの陽菜（ひな）ちゃんのことか？」
亮太の話によると、陽菜は次の期末テストで赤点を取ると、大好きな軽音部の活動を禁止されてしまうらしい。
僕と亮太は保育園からの付き合いで、陽菜とも昔からよく遊んでいた。
だが、高校生になってからは少し距離ができていた。
「わかったよ。昔遊んでやった恩返しだ、引き受けるよ」
夕方、亮太の家を訪ねると、部屋のドアからちょこんと顔を出した陽菜が、少し頬を赤らめながら僕を見上げていた。
「……先輩、本当に来てくれたんだ」");

        EnsureChapter(katei, 2, "第二章：初めての放課後勉強会 (Buổi học đầu tiên tại phòng khách)", 420,
            @"陽菜の部屋のローテーブルにノートと参考書を広げて、勉強会が始まった。
「いいかい、陽菜ちゃん。古典の助動詞は丸暗記するんじゃなくて、文脈の意味から推測するんだ」
「うう、先輩の教え方、学校の先生よりずっと分かりやすいかも……」
陽菜はシャーペンをくるくると回しながら、真剣な眼差しでノートに書き込んでいく。
普段は少しツンツンしている陽菜だが、勉強に集中すると時折見せる素直な笑顔がやけに眩しかった。
「ねえ先輩、もし次のテストで平均点以上取れたら、ご褒美にどこか連れて行ってくれる？」
夕暮れのオレンジ色の光が窓から差し込み、二人のノートを暖かく照らしていた。");

        EnsureChapter(katei, 3, "第三章：縮まる距離とテスト対策 (Thu hẹp khoảng cách và kỳ thi giữa kỳ)", 450,
            @"テスト前日、最後の追い込み勉強会が終わった。
「これで準備は万全だ。陽菜ちゃん、よく頑張ったね」
僕が頭にぽんと手を置くと、陽菜は驚いたように目を丸くしたあと、嬉しそうにはにかんだ。
「先輩のおかげで、苦手だった古文も現代文もすっかり自信がついたよ」
帰り際、玄関先まで見送りに来てくれた陽菜が、僕のシャツの袖をきゅっと掴んだ。
「テストが終わったら、絶対に約束のご褒美、忘れないでね」
街灯の光に照らされた陽菜の瞳は、昔の小さな妹ではなく、一人の魅力的な女の子として輝いていた。");

        // 4.11. 杜子春 (Toshishun - Akutagawa Ryunosuke)
        var toshi = existingNovels.FirstOrDefault(n => n.Title.Contains("杜子春"));
        if (toshi == null)
        {
            toshi = new Novel
            {
                Title = "杜子春",
                JapaneseTitle = "杜子春 (Toshishun)",
                RomajiTitle = "Toshishun",
                Author = "芥川龍之介 (Akutagawa Ryunosuke)",
                Genre = "Eastern Mythology / Parable",
                Description = "Truyện ngụ ngôn đậm chất Á Đông của Akutagawa Ryunosuke phỏng theo truyền kỳ Đường triều. Chàng trai Đỗ Tử Xuân nếm trải giàu sang phú quý rồi trắng tay, trước khi bước vào cuộc khảo nghiệm tu tiên trên đỉnh Nga Mi sơn.",
                CoverImagePath = "assets/covers/cover_anime_night_shrine.jpg",
                JlptLevel = JlptLevel.N3,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                LastReadAt = null
            };
            _context.Novels.Add(toshi);
        }

        EnsureChapter(toshi, 1, "第一章：洛陽の夕暮れ (Hoàng hôn thành Lạc Dương và ông lão kỳ lạ)", 460,
            @"ある春の日の暮れ方のことである。唐の都、洛陽の西の門の前に、杜子春という一人の若者が、途方に暮れて佇んでいた。
彼はもともと大富豪の息子であったが、放蕩を尽くして財産をすっかり使い果たし、今宵泊まる宿どころか、今日食べる米粒ひとつない有様であった。
「いっそ川へ身を投げて死んでしまおうか」
杜子春が空を見上げてため息をついたとき、どこからともなく片目の不思議な老人が現れ、こう言った。
「お前は何を思い悩んでいるのだ」
杜子春が事情を打ち明けると、老人は地面を杖で指さした。
「今夜、夕日の影が落ちるこの場所を掘ってみるがよい。黄金が荷車に三杯分、埋まっているぞ」
翌朝、杜子春はその通りにして莫大な富を手に入れた。しかし三年が過ぎると、その財宝もまた贅沢のためにすべて消え去ってしまった。");

        EnsureChapter(toshi, 2, "第二章：峨眉山の仙術修行 (Khảo nghiệm tu tiên nơi đỉnh Nga Mi)", 490,
            @"再び文無しになった杜子春は、西門の老人が実は峨眉山の鉄冠子という仙人であることを知った。
「世間の人間は、金がある時は群がり、金がなくなれば冷たく背を向ける。私はもう人間の世界に嫌気がさしました。どうか私を仙人にしてください」
仙人は杜子春を連れて峨眉山の断崖絶壁に登ると、静かに言った。
「これから私は天へ昇る。お前はここにすわって、何が起きても決して声を出してはならぬ。一言でも口を利けば、仙人にはなれんぞ」
仙人が雲に乗って去ると、たちまち空が暗転し、大蛇や虎が牙を剥いて杜子春に襲いかかった。
さらに雷神が天空から火の玉を投げつけ、杜子春の体を焼き焦がした。
しかし杜子春は歯を食いしばり、頑として一言も声を発しなかった。");

        EnsureChapter(toshi, 3, "第三章：母の声と真の人間 (Tiếng gọi mẹ hiền và giá trị làm người)", 520,
            @"やがて魔神が現れ、杜子春の亡くなった父母を地獄から引きずり出してきた。
魔神は鉄の鞭を振り上げ、痩せ衰えた父母の体を容赦なく打ち据えた。
肉が裂け、血が吹き飛んでも、父母は愛する我が子のために叫んだ。
「子春よ、私たちはどうなっても構わない。お前は仙人になるために、決して声を出してはならないよ」
母の慈愛に満ちたその優しい声を聞いた瞬間、杜子春の目から熱い涙が溢れ出た。
「お母さん！」
杜子春は思わず大声で叫んで母に抱きついた。
気がつくと、あたりは穏やかな春の夕暮れに戻り、目の前には鉄冠子が微笑んで立っていた。
「もしお前があのまま黙っていたら、私はその命を絶っていたところだ。親を思う真心がある限り、お前は仙人などにならず、真の人間として生きるがよい」
杜子春の心には、清々しい生きる喜びが満ちていた。");

        // 4.12. 舞姫 (Maihime - The Dancing Girl - Mori Ogai)
        var maihime = existingNovels.FirstOrDefault(n => n.Title.Contains("舞姫"));
        if (maihime == null)
        {
            maihime = new Novel
            {
                Title = "舞姫",
                JapaneseTitle = "舞姫 (Maihime)",
                RomajiTitle = "The Dancing Girl",
                Author = "森鴎外 (Mori Ogai)",
                Genre = "Classic Literature / Romantic Tragedy",
                Description = "Tác phẩm tiêu biểu mở đầu cho chủ nghĩa lãng mạn thời Minh Trị của Mori Ogai. Chàng du học sinh Ota Toyotaro tại Berlin thế kỷ 19 đối diện với sự giằng xé nghiệt ngã giữa con đường công danh tiền đồ và mối tình cùng nàng vũ công Elise.",
                CoverImagePath = "assets/covers/cover_anime_sunset.jpg",
                JlptLevel = JlptLevel.N2,
                IsFavorite = true,
                IsDemoData = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                LastReadAt = null
            };
            _context.Novels.Add(maihime);
        }

        EnsureChapter(maihime, 1, "第一章：ベルリンの冬と古寺 (Mùa đông Berlin và cuộc tao ngộ)", 520,
            @"石炭をば早や焚き果てつ。中点航路の船客は皆下甲板に集まりて、夜の更くるを待つ。
余が洋行の命を奉じてより、はや三年余りの歳月が流れた。
ドイツの帝都ベルリンの冬は厳しく、ウンテル・デン・リンデンの並木道には冷たい風が吹き荒れていた。
余は官命を帯びたエリート留学生として、法律と政治の学問に励んでいた。
ある日の夕暮れ、クロスター街の古い教会の門前を通りかかったとき、一人の金髪の少女が教会の扉に縋り付いて激しく泣き崩れているのを見かけた。
少女の名はエリス。貧しい劇場の若き踊り子であった。
父の葬儀を出す金もなく途方に暮れていたエリスを、余は懐の金を投じて救ってやった。
それが、余の運命を大きく狂わせる甘美で哀しい日々の始まりであった。");

        EnsureChapter(maihime, 2, "第二章：名誉と純愛の狭間 (Sự giằng xé giữa tiền đồ và ái tình)", 560,
            @"余とエリスの同棲は、やがて公使館の同僚たちの嫉妬と非難の的となった。
官職を免ぜられ、祖国からの俸給を断たれた余は、新聞社の通信員として糊口をしのぎつつ、エリスの真実の愛に安らぎを見出していた。
エリスの胎内には、新しい命が宿っていた。
しかしそこへ、余の親友である相沢謙吉が現れ、天方大臣の随員としての復官の好機を持ちかけてきた。
「トヨロー、君は一人の踊り子のために、己の輝かしい将来と国家への責務を捨てるつもりか」
大臣の信頼を取り戻し、故国への凱旋を約束されたその夜、余は自責と苦悩のあまり雪の降るベルリンの街を彷徨い、意識を失って倒れた。
余の裏切りを知ったエリスは狂乱の淵へと沈み、正気を失ってしまった。
愛と名誉の狭間で魂を引き裂かれた余は、深い悔恨を抱えたまま、一人帰国の途につく船の甲板に立つのであった。");

        // 5. Expand Vocabulary items with rich database (100+ items across N5-N1)
        var existingWords = await _context.VocabularyItems.Select(v => v.Word).ToListAsync();
        var now = DateTime.UtcNow;

        var vocabList = new List<VocabularyItem>
        {
            // Core literary words
            new() { Word = "銀河", Reading = "ぎんが", Meaning = "Ngân hà, dải thiên hà (Milky Way)", JlptLevel = JlptLevel.N3, ContextSentence = "上から下へ白くけむった銀河の帯のようなところを指しながら。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "星座", Reading = "せいざ", Meaning = "Chòm sao, tinh tú", JlptLevel = JlptLevel.N3, ContextSentence = "黒板に吊した大きな黒い星座の図。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddHours(-1), IntervalDays = 1, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "無鉄砲", Reading = "むてっぽう", Meaning = "Hấp tấp, liều lĩnh, không suy tính", JlptLevel = JlptLevel.N1, ContextSentence = "親譲りの無鉄砲で小供の時から損ばかりしている。", SourceNovelTitle = "坊っちゃん", NextReviewDate = now.AddHours(-4), IntervalDays = 3, EaseFactor = 2.3, Repetitions = 2 },
            new() { Word = "国境", Reading = "こっきょう", Meaning = "Biên giới quốc gia, ranh giới tỉnh", JlptLevel = JlptLevel.N3, ContextSentence = "国境の長いトンネルを抜けると雪国であった。", SourceNovelTitle = "雪国", NextReviewDate = now.AddDays(2), IntervalDays = 4, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "いたずら", Reading = "いたずら", Meaning = "Nghịch ngợm, trêu chọc", JlptLevel = JlptLevel.N4, ContextSentence = "夜でも昼でも、ちかくの村へ出てきて、いたずらばかりしました。", SourceNovelTitle = "ごんぎつね", NextReviewDate = now.AddDays(1), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "激怒", Reading = "げきど", Meaning = "Phẫn nộ, nổi giận đùng đùng", JlptLevel = JlptLevel.N2, ContextSentence = "メロスは激怒した。必ず、かの邪悪の王を除かなければならぬと決意した。", SourceNovelTitle = "走れメロス", NextReviewDate = now.AddHours(-3), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "邪悪", Reading = "じゃあく", Meaning = "Tà ác, độc ác, gian tà", JlptLevel = JlptLevel.N1, ContextSentence = "邪悪に対しては、人一倍に敏感であった。", SourceNovelTitle = "走れメロス", NextReviewDate = now.AddHours(-1), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "信実", Reading = "しんじつ", Meaning = "Lòng thành thật, chân thực, tín nghĩa", JlptLevel = JlptLevel.N1, ContextSentence = "信実を守るのだ！ 友との絆を証明するのだ！", SourceNovelTitle = "走れメロス", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "人質", Reading = "ひとじち", Meaning = "Con tin", JlptLevel = JlptLevel.N2, ContextSentence = "石工のセリヌンティウスを人質としてここに残そう。", SourceNovelTitle = "走れメロス", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "約束", Reading = "やくそく", Meaning = "Lời hứa, hẹn ước", JlptLevel = JlptLevel.N4, ContextSentence = "友との約束を果たすために走る。", SourceNovelTitle = "走れメロス", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "夜空", Reading = "よぞら", Meaning = "Bầu trời đêm", JlptLevel = JlptLevel.N3, ContextSentence = "冷たい草の上に寝ころんで、星のきらめく夜空を見上げました。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddHours(-5), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "燐光", Reading = "りんこう", Meaning = "Ánh lân tinh, lân quang xanh biếc", JlptLevel = JlptLevel.N1, ContextSentence = "天の川の水が、青い燐光のようにきらきら光っているんだ。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "切符", Reading = "きっぷ", Meaning = "Vé xe, vé tàu", JlptLevel = JlptLevel.N5, ContextSentence = "どこまででも行ける、不思議な切符を持っていた。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddHours(-1), IntervalDays = 2, EaseFactor = 2.7, Repetitions = 3 },
            new() { Word = "停車場", Reading = "ていしゃじょう", Meaning = "Ga tàu, bến đỗ", JlptLevel = JlptLevel.N2, ContextSentence = "白鳥の停車場や、鷲の停車場が地図に描いてある。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "幸い", Reading = "さいわい", Meaning = "Hạnh phúc, điều may mắn tốt lành", JlptLevel = JlptLevel.N2, ContextSentence = "本当の幸いって何だろうね。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddHours(-6), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "すずらん", Reading = "すずらん", Meaning = "Hoa linh lan (Lily of the valley)", JlptLevel = JlptLevel.N2, ContextSentence = "銀の紙のすずらんを飾ったりしていました。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "栗", Reading = "くり", Meaning = "Hạt dẻ", JlptLevel = JlptLevel.N3, ContextSentence = "毎日山へ行って、栗や松茸をどっさり拾ってきました。", SourceNovelTitle = "ごんぎつね", NextReviewDate = now.AddHours(-3), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "松茸", Reading = "まつたけ", Meaning = "Nấm matsutake quý hiếm", JlptLevel = JlptLevel.N1, ContextSentence = "こんな立派な栗や松茸を毎日届けてくれるんだろう。", SourceNovelTitle = "ごんぎつね", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "葬式", Reading = "そうしき", Meaning = "Tang lễ, đám tang", JlptLevel = JlptLevel.N2, ContextSentence = "兵十のお母さんの葬式が行われているところでした。", SourceNovelTitle = "ごんぎつね", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "あだ名", Reading = "あだな", Meaning = "Biệt danh, tên hiệu gọi vui", JlptLevel = JlptLevel.N2, ContextSentence = "校長は狸のような顔をしているので狸とあだ名をつけた。", SourceNovelTitle = "坊っちゃん", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "下女", Reading = "げじょ", Meaning = "Người giúp việc nữ (thời xưa)", JlptLevel = JlptLevel.N1, ContextSentence = "長年仕えてくれた下女の清はおれを可愛がってくれた。", SourceNovelTitle = "坊っちゃん", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "雨宿り", Reading = "あまやどり", Meaning = "Trú mưa", JlptLevel = JlptLevel.N2, ContextSentence = "一人の下人が、羅生門の下で雨やみを待っていた。", SourceNovelTitle = "羅生門", NextReviewDate = now.AddHours(-4), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "盗人", Reading = "ぬすびと", Meaning = "Kẻ trộm, tên đạo tặc", JlptLevel = JlptLevel.N1, ContextSentence = "盗人になるよりほかに道はない。", SourceNovelTitle = "羅生門", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "冷気", Reading = "れいき", Meaning = "Hơi lạnh, khí lạnh buốt", JlptLevel = JlptLevel.N2, ContextSentence = "窓を開けると雪の冷気が流れこんだ。", SourceNovelTitle = "雪国", NextReviewDate = now.AddDays(4), IntervalDays = 5, EaseFactor = 2.6, Repetitions = 3 },

            // Additional Rich Literary Vocabulary
            new() { Word = "蓮", Reading = "はす", Meaning = "Hoa sen cõi Phật thanh tịnh", JlptLevel = JlptLevel.N1, ContextSentence = "御釈迦様は極楽の蓮池のふちを、独りでぶらぶら御歩きになっていらっしゃいました。", SourceNovelTitle = "蜘蛛の糸", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "罪人", Reading = "ざいにん", Meaning = "Tội nhân, người mang trọng tội", JlptLevel = JlptLevel.N2, ContextSentence = "犍陀多という男が、ほかの罪人と一緒に蠢いているのが見えました。", SourceNovelTitle = "蜘蛛の糸", NextReviewDate = now.AddHours(-1), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "蠢く", Reading = "うごめく", Meaning = "Ngọ nguậy, giãy giụa, lúc nhúc", JlptLevel = JlptLevel.N1, ContextSentence = "地獄の底で何千もの罪人が蠢いている。", SourceNovelTitle = "蜘蛛の糸", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "善行", Reading = "ぜんこう", Meaning = "Hành vi thiện lành, công đức tốt", JlptLevel = JlptLevel.N1, ContextSentence = "御釈迦様はその善行を思い出され、地獄から救い出してやろうとお考えになりました。", SourceNovelTitle = "蜘蛛の糸", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "地獄", Reading = "じごく", Meaning = "Địa ngục, chốn u ám khốn cùng", JlptLevel = JlptLevel.N3, ContextSentence = "この蓮池の下は、丁度地獄の底に当って居ります。", SourceNovelTitle = "蜘蛛の糸", NextReviewDate = now.AddHours(-5), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "極楽", Reading = "ごくらく", Meaning = "Cõi Cực Lạc, cõi tịnh độ thiên đường", JlptLevel = JlptLevel.N2, ContextSentence = "極楽は丁度朝でございましょう。", SourceNovelTitle = "蜘蛛の糸", NextReviewDate = now.AddHours(-3), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "手繰る", Reading = "たぐる", Meaning = "Kéo từng đoạn, lần tay leo lên", JlptLevel = JlptLevel.N1, ContextSentence = "カンダタは必死になって蜘蛛の糸を手繰り登りました。", SourceNovelTitle = "蜘蛛の糸", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "温泉", Reading = "おんせん", Meaning = "Suối nước nóng thiên nhiên", JlptLevel = JlptLevel.N4, ContextSentence = "温泉宿の静かな障子を開けると、外は一面の銀世界であった。", SourceNovelTitle = "雪国", NextReviewDate = now.AddHours(-2), IntervalDays = 2, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "三味線", Reading = "しゃみせん", Meaning = "Đàn ba dây Shamisen truyền thống", JlptLevel = JlptLevel.N2, ContextSentence = "彼女が三味線を爪弾くと、澄んだ音が雪の積もった夜の空気に凛と響き渡った。", SourceNovelTitle = "雪国", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "情熱", Reading = "じょうねつ", Meaning = "Nhiệt huyết, niềm đam mê nồng cháy", JlptLevel = JlptLevel.N2, ContextSentence = "その瞳には熱い情熱が宿っていた。", SourceNovelTitle = "雪国", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "虚無", Reading = "きょむ", Meaning = "Hư vô, cảm giác rỗng không tuyệt vọng", JlptLevel = JlptLevel.N1, ContextSentence = "虚無感を抱える島村の心に、駒子の哀しくも純粋な生が深く沁み入った。", SourceNovelTitle = "雪国", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.3, Repetitions = 1 },
            new() { Word = "孤独", Reading = "こどく", Meaning = "Sự cô đơn, cô độc lặng lẽ", JlptLevel = JlptLevel.N2, ContextSentence = "誰にも触れさせない深い孤独と後悔が横たわっていた。", SourceNovelTitle = "こころ", NextReviewDate = now.AddHours(-4), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "後悔", Reading = "こうかい", Meaning = "Nỗi hối hận, cắn rứt lương tâm", JlptLevel = JlptLevel.N2, ContextSentence = "先生の胸の奥底には深い後悔があった。", SourceNovelTitle = "こころ", NextReviewDate = now.AddHours(-1), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "休暇", Reading = "きゅうか", Meaning = "Kỳ nghỉ hè, ngày phép", JlptLevel = JlptLevel.N3, ContextSentence = "暑中休暇を利用して友達に誘われて海へ泳ぎに行ったのである。", SourceNovelTitle = "こころ", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "墓参り", Reading = "はかまいり", Meaning = "Tảo mộ, viếng thăm phần mộ", JlptLevel = JlptLevel.N2, ContextSentence = "先生は毎月一度、決まって雑司ヶ谷の墓地へ墓参りに出かけていた。", SourceNovelTitle = "こころ", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "悪人", Reading = "あくにん", Meaning = "Kẻ bất lương, con người độc ác", JlptLevel = JlptLevel.N2, ContextSentence = "人間はね、いざという時には誰でも悪人になるものだよ。", SourceNovelTitle = "こころ", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "死骸", Reading = "しがい", Meaning = "Thi thể, xác chết", JlptLevel = JlptLevel.N1, ContextSentence = "楼の上には、死骸が無造作に転がっており、その死臭が漂っている。", SourceNovelTitle = "羅生門", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "老婆", Reading = "ろうば", Meaning = "Bà lão, bà già còng lưng", JlptLevel = JlptLevel.N2, ContextSentence = "一人の白髪の老婆がかがみこみ、死人の頭から一本ずつ髪の毛を抜いていた。", SourceNovelTitle = "羅生門", NextReviewDate = now.AddHours(-3), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "憎悪", Reading = "ぞうお", Meaning = "Sự căm thù, căm ghét tột cùng", JlptLevel = JlptLevel.N1, ContextSentence = "激しい恐怖と激しい憎悪を覚えた。", SourceNovelTitle = "羅生門", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "階段", Reading = "かいだん", Meaning = "Cầu thang gác", JlptLevel = JlptLevel.N5, ContextSentence = "上の楼へと続く階段へ足をかけた。", SourceNovelTitle = "羅生門", NextReviewDate = now.AddHours(-1), IntervalDays = 2, EaseFactor = 2.7, Repetitions = 3 },
            new() { Word = "飢饉", Reading = "ききん", Meaning = "Nạn đói, mùa màng thất bát", JlptLevel = JlptLevel.N1, ContextSentence = "京都の町は火災や飢饉のために荒れ果てるばかりであった。", SourceNovelTitle = "羅生門", NextReviewDate = now.AddDays(4), IntervalDays = 5, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "水晶", Reading = "すいしょう", Meaning = "Thạch anh pha lê sáng lấp lánh", JlptLevel = JlptLevel.N2, ContextSentence = "川底の砂利はすべて水晶やトパーズのように輝いていました。", SourceNovelTitle = "銀河鉄道の夜", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "濁流", Reading = "だくりゅう", Meaning = "Dòng nước lũ cuồn cuộn bùn đục", JlptLevel = JlptLevel.N1, ContextSentence = "濁流が川の橋を押し流していた。", SourceNovelTitle = "走れメロス", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "笛", Reading = "ふえ", Meaning = "Chiếc sáo, ống sáo", JlptLevel = JlptLevel.N3, ContextSentence = "笛を吹き、羊と遊んで暮して来た。", SourceNovelTitle = "走れメロス", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },

            // Rich Yomitan & Literature Vocabulary Expansion (35+ Items)
            // 注文の多い料理店 (Chuumon no Ooi Ryouriten)
            new() { Word = "紳士", Reading = "しんし", Meaning = "Thân sĩ, quý ông bảnh bao (Gentleman)", JlptLevel = JlptLevel.N2, ContextSentence = "二人の若い紳士が、ぴかぴかする鉄砲をかついで歩いていました。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddHours(-3), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "鉄砲", Reading = "てっぽう", Meaning = "Khẩu súng, súng săn cổ", JlptLevel = JlptLevel.N3, ContextSentence = "鉄砲と弾丸をここに置いてください。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "西洋館", Reading = "せいようかん", Meaning = "Tòa nhà kiểu Tây phương cổ kính", JlptLevel = JlptLevel.N2, ContextSentence = "立派な白い煉瓦造りの西洋館が建っていました。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "看板", Reading = "かんばん", Meaning = "Bảng hiệu, biển quảng cáo quán xá", JlptLevel = JlptLevel.N3, ContextSentence = "玄関には青いガラスの看板が掲げてありました。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "注文", Reading = "ちゅうもん", Meaning = "Đơn đặt hàng, gọi món, yêu cầu", JlptLevel = JlptLevel.N4, ContextSentence = "当軒は注文の多い料理店ですからどうかご承知ください。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddHours(-1), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "承知", Reading = "しょうち", Meaning = "Thấu hiểu, chấp nhận, hiểu rõ", JlptLevel = JlptLevel.N3, ContextSentence = "どうかそこはご承知ください。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "金属", Reading = "きんぞく", Meaning = "Kim loại", JlptLevel = JlptLevel.N3, ContextSentence = "その他金属のものはすべて台の上にお置きください。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "壺", Reading = "つぼ", Meaning = "Cái bình, cái hũ, chum vại", JlptLevel = JlptLevel.N2, ContextSentence = "壺の中のクリームを、顔や手足にすっかり塗り込んでください。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddHours(-4), IntervalDays = 1, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "酢", Reading = "す", Meaning = "Giấm ăn, vị chua lên men", JlptLevel = JlptLevel.N2, ContextSentence = "壺の中の塩と酢を、残らず頭からよく揉み込んでください。", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "罠", Reading = "わな", Meaning = "Cạm bẫy, bẫy sập khôn lường", JlptLevel = JlptLevel.N1, ContextSentence = "僕たちは恐ろしい罠にかかってしまったんだ！", SourceNovelTitle = "注文の多い料理店", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.3, Repetitions = 1 },

            // 幼馴染の妹の家庭教師を始めたら (Kateikyoushi)
            new() { Word = "家庭教師", Reading = "かていきょうし", Meaning = "Gia sư tại nhà, người dạy kèm cá nhân", JlptLevel = JlptLevel.N3, ContextSentence = "うちの妹の家庭教師をやってほしいんだ！", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "幼馴染", Reading = "おさななじみ", Meaning = "Bạn thanh mai trúc mã, bạn từ thời thơ ấu", JlptLevel = JlptLevel.N2, ContextSentence = "放課後の教室で幼馴染の亮太が真剣な顔で立った。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "放課後", Reading = "ほうかご", Meaning = "Sau giờ học, lúc tan trường", JlptLevel = JlptLevel.N4, ContextSentence = "放課後の静かな教室で勉強会が始まった。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddHours(-1), IntervalDays = 2, EaseFactor = 2.7, Repetitions = 3 },
            new() { Word = "参考書", Reading = "さんこうしょ", Meaning = "Sách tham khảo học tập", JlptLevel = JlptLevel.N3, ContextSentence = "ローテーブルにノートと参考書を広げた。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "文脈", Reading = "ぶんみゃく", Meaning = "Ngữ cảnh, văn cảnh của câu văn", JlptLevel = JlptLevel.N1, ContextSentence = "古典の助動詞は文脈の意味から推測するんだ。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "推測", Reading = "すいそく", Meaning = "Suy đoán, ước đoán logic", JlptLevel = JlptLevel.N2, ContextSentence = "前後の関係から正解を推測してみよう。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "笑顔", Reading = "えがお", Meaning = "Nụ cười rạng rỡ tươi tắn", JlptLevel = JlptLevel.N4, ContextSentence = "素直な笑顔がやけに眩しかった。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddHours(-3), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "ご褒美", Reading = "ごほうび", Meaning = "Phần thưởng khích lệ, quà tán dương", JlptLevel = JlptLevel.N3, ContextSentence = "ご褒美にどこか連れて行ってくれる？", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "自信", Reading = "じしん", Meaning = "Tự tin, niềm tin vào bản thân", JlptLevel = JlptLevel.N4, ContextSentence = "苦手だった現代文もすっかり自信がついたよ。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddHours(-1), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "夕暮れ", Reading = "ゆうぐれ", Meaning = "Hoàng hôn, chiều tà nhá nhem", JlptLevel = JlptLevel.N3, ContextSentence = "夕暮れのオレンジ色の光が窓から差し込んだ。", SourceNovelTitle = "幼馴染の妹の家庭教師を始めたら", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },

            // 杜子春 (Toshishun)
            new() { Word = "富豪", Reading = "ふごう", Meaning = "Phú hào, người giàu có cự vạn", JlptLevel = JlptLevel.N2, ContextSentence = "彼はもともと大富豪の息子であった。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "黄金", Reading = "おうごん", Meaning = "Vàng ròng, tiền vàng sáng chói", JlptLevel = JlptLevel.N3, ContextSentence = "黄金が荷車に三杯分、埋まっているぞ。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "仙人", Reading = "せんにん", Meaning = "Tiên nhân, bậc tu tiên thoát tục", JlptLevel = JlptLevel.N2, ContextSentence = "西門の老人が実は峨眉山の仙人であった。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddHours(-5), IntervalDays = 1, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "断崖絶壁", Reading = "だんがいぜっぺき", Meaning = "Vách đá dựng đứng cheo leo hiểm trở", JlptLevel = JlptLevel.N1, ContextSentence = "仙人は杜子春を連れて峨眉山の断崖絶壁に登った。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.3, Repetitions = 1 },
            new() { Word = "沈黙", Reading = "ちんもく", Meaning = "Trầm mặc, im lặng không nói một lời", JlptLevel = JlptLevel.N2, ContextSentence = "何が起きても決して声を出してはならぬ、沈黙を守れ。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "慈愛", Reading = "じあい", Meaning = "Tình thương từ ái, lòng bao dung nhân hậu", JlptLevel = JlptLevel.N1, ContextSentence = "母の慈愛に満ちたその優しい声を聞いた瞬間。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddDays(4), IntervalDays = 5, EaseFactor = 2.5, Repetitions = 2 },
            new() { Word = "涙", Reading = "なみだ", Meaning = "Giọt nước mắt, lệ", JlptLevel = JlptLevel.N4, ContextSentence = "杜子春の目から熱い涙が溢れ出た。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddHours(-1), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "真心", Reading = "まごころ", Meaning = "Tấm lòng chân thành, lòng tốt thuần khiết", JlptLevel = JlptLevel.N1, ContextSentence = "親を思う真心がある限り、真の人間として生きるがよい。", SourceNovelTitle = "杜子春", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.4, Repetitions = 1 },

            // 舞姫 (Maihime)
            new() { Word = "舞姫", Reading = "まいひめ", Meaning = "Vũ nữ, nàng vũ công múa ba lê kiều diễm", JlptLevel = JlptLevel.N2, ContextSentence = "少女の名はエリス、劇場の若き舞姫であった。", SourceNovelTitle = "舞姫", NextReviewDate = now.AddHours(-3), IntervalDays = 1, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "留学生", Reading = "りゅうがくせい", Meaning = "Du học sinh nước ngoài", JlptLevel = JlptLevel.N5, ContextSentence = "余は官命を帯びたエリート留学生として学問に励んでいた。", SourceNovelTitle = "舞姫", NextReviewDate = now.AddHours(-1), IntervalDays = 2, EaseFactor = 2.7, Repetitions = 3 },
            new() { Word = "名誉", Reading = "めいよ", Meaning = "Danh dự, vinh hiển, thanh danh", JlptLevel = JlptLevel.N2, ContextSentence = "己の輝かしい将来と国家への名誉を捨てるつもりか。", SourceNovelTitle = "舞姫", NextReviewDate = now.AddDays(1), IntervalDays = 2, EaseFactor = 2.5, Repetitions = 1 },
            new() { Word = "教会", Reading = "きょうかい", Meaning = "Nhà thờ Thiên Chúa giáo", JlptLevel = JlptLevel.N4, ContextSentence = "教会の扉に縋り付いて激しく泣き崩れていた。", SourceNovelTitle = "舞姫", NextReviewDate = now.AddHours(-2), IntervalDays = 1, EaseFactor = 2.6, Repetitions = 2 },
            new() { Word = "苦悩", Reading = "くのう", Meaning = "Khổ não, dằn vặt tâm can, trăn trở", JlptLevel = JlptLevel.N1, ContextSentence = "自責と苦悩のあまり雪の降る街を彷徨った。", SourceNovelTitle = "舞姫", NextReviewDate = now.AddDays(3), IntervalDays = 4, EaseFactor = 2.4, Repetitions = 1 },
            new() { Word = "凱旋", Reading = "がいせん", Meaning = "Khải hoàn, thắng trận trở về vinh quang", JlptLevel = JlptLevel.N1, ContextSentence = "故国への輝かしい凱旋を約束された。", SourceNovelTitle = "舞姫", NextReviewDate = now.AddDays(2), IntervalDays = 3, EaseFactor = 2.3, Repetitions = 1 },
            new() { Word = "悔恨", Reading = "かいこん", Meaning = "Hối hận sâu sắc, cắn rứt ân hận suốt đời", JlptLevel = JlptLevel.N1, ContextSentence = "深い悔恨を抱えたまま、一人帰国の途についた。", SourceNovelTitle = "舞姫", NextReviewDate = now.AddDays(4), IntervalDays = 5, EaseFactor = 2.4, Repetitions = 1 }
        };

        foreach (var v in vocabList)
        {
            if (!existingWords.Contains(v.Word))
            {
                v.CreatedAt = now.AddDays(-3);
                _context.VocabularyItems.Add(v);
            }
        }

        await _context.SaveChangesAsync();
    }

    private static void EnsureChapter(Novel novel, int chapterNum, string title, int wordCount, string content)
    {
        novel.Chapters ??= new List<Chapter>();
        if (!novel.Chapters.Any(c => c.ChapterNumber == chapterNum))
        {
            novel.Chapters.Add(new Chapter
            {
                ChapterNumber = chapterNum,
                Title = title,
                WordCount = wordCount,
                Content = content,
                IsCompleted = chapterNum == 1,
                LastReadPosition = 0
            });
        }
    }
}
